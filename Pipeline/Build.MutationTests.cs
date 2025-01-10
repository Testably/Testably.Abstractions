using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Project = Nuke.Common.ProjectModel.Project;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	/// <summary>
	///     The markdown texts for each project.
	/// </summary>
	static readonly Dictionary<string, string> MutationCommentBodies = new();

	Target MutationComment => _ => _
		.After(MutationTestsLinux)
		.After(MutationTestsWindows)
		.OnlyWhenDynamic(() => GitHubActions.IsPullRequest)
		.Executes(async () =>
		{
			int? prId = GitHubActions.PullRequestNumber;
			Log.Debug("Pull request number: {PullRequestId}", prId);
			if (MutationCommentBodies.Count == 0)
			{
				return;
			}

			if (prId != null)
			{
				GitHubClient gitHubClient = new(new ProductHeaderValue("Nuke"));
				Credentials tokenAuth = new(GithubToken);
				gitHubClient.Credentials = tokenAuth;
				IReadOnlyList<IssueComment> comments =
					await gitHubClient.Issue.Comment.GetAllForIssue("Testably",
						"Testably.Abstractions", prId.Value);
				IssueComment? existingComment = null;
				Log.Information($"Found {comments.Count} comments");
				foreach (IssueComment comment in comments)
				{
					if (comment.Body.Contains("## :alien: Mutation Results"))
					{
						Log.Information($"Found comment: {comment.Body}");
						existingComment = comment;
					}
				}

				if (existingComment == null)
				{
					string body = "## :alien: Mutation Results"
					              + Environment.NewLine
					              + $"[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FTestably.Abstractions%2Fpull/{prId}/merge)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/Testably.Abstractions/pull/{prId}/merge)"
					              + Environment.NewLine
					              + string.Join(Environment.NewLine, MutationCommentBodies.Values);

					Log.Information($"Create comment:\n{body}");
					await gitHubClient.Issue.Comment.Create("Testably", "Testably.Abstractions",
						prId.Value, body);
				}
				else
				{
					string body = existingComment.Body;
					foreach ((string project, string value) in MutationCommentBodies)
					{
						body = ReplaceProject(body, project, value);
					}

					Log.Information($"Update comment:\n{body}");
					await gitHubClient.Issue.Comment.Update("Testably", "Testably.Abstractions",
						existingComment.Id, body);
				}
			}
		});

	Target MutationTestPreparation => _ => _
		.Executes(() =>
		{
			StrykerToolPath.CreateOrCleanDirectory();

			DotNetToolInstall(_ => _
				.SetPackageName("dotnet-stryker")
				.SetToolInstallationPath(StrykerToolPath));

			StrykerOutputDirectory.CreateOrCleanDirectory();
		});

	Target MutationTests => _ => _
		.DependsOn(MutationTestsWindows)
		.DependsOn(MutationTestsLinux)
		.DependsOn(MutationComment);

	Target MutationTestsLinux => _ => _
		.DependsOn(Compile)
		.DependsOn(MutationTestPreparation)
		.Executes(() =>
		{
			AbsolutePath configFile = StrykerToolPath / "Stryker.Config.json";
			Dictionary<Project, Project[]> projects = new()
			{
				{
					Solution.Testably_Abstractions_Testing, [
						Solution.Tests.Testably_Abstractions_Testing_Tests,
						Solution.Tests.Testably_Abstractions_Tests
					]
				},
			};

			foreach (KeyValuePair<Project, Project[]> project in projects)
			{
				string? branchName = GitVersion?.BranchName;
				if (GitHubActions?.Ref.StartsWith("refs/tags/",
					StringComparison.OrdinalIgnoreCase) == true)
				{
					string version = GitHubActions.Ref.Substring("refs/tags/".Length);
					branchName = "release/" + version;
					Log.Information("Use release branch analysis for '{BranchName}'", branchName);
				}

				string configText = $$"""
				                      {
				                      	"stryker-config": {
				                      		"project-info": {
				                      			"name": "github.com/Testably/Testably.Abstractions",
				                      			"module": "{{project.Key.Name}}",
				                      			"version": "{{branchName}}"
				                      		},
				                      		"test-projects": [
				                      			{{string.Join(",\n\t\t\t", project.Value.Select(PathForJson))}}
				                      		],
				                      		"project": {{PathForJson(project.Key)}},
				                      		"target-framework": "net8.0",
				                      		"since": {
				                      			"target": "main",
				                      			"enabled": {{(GitVersion?.BranchName != "main").ToString().ToLowerInvariant()}},
				                      			"ignore-changes-in": [
				                      				"**/.github/**/*.*"
				                      			]
				                      		},
				                      		"reporters": [
				                      			"html",
				                      			"progress",
				                      			"cleartext"
				                      		],
				                      		"mutation-level": "Advanced",
				                      		"mutate": [
				                      			// The enumeration options helper is a wrapper around Microsoft code
				                      			"!**/Testably.Abstractions.Testing/Helpers/EnumerationOptionsHelper.cs",
				                      			// The exception type is checked, but not the message, as this could be language dependent
				                      			"!**/Testably.Abstractions.Testing/Helpers/ExceptionFactory.cs",
				                      			// Indicates operating system specific code
				                      			"!**/Testably.Abstractions.Testing/Helpers/Execute.cs",
				                      			// The directory cleaner should cleanup the real file system
				                      			"!**/Testably.Abstractions.Testing/FileSystemInitializer/DirectoryCleaner.cs"
				                      		],
				                      		"ignore-methods": [
				                      			// The exception type is checked, but not the message, as this could be language dependent
				                      			"ExceptionFactory.*",
				                      			// Some checks are redundant but there for performance improvements
				                      			"InMemoryLocation.Equals",
				                      			// Indicates operating system specific code
				                      			"Testably.Abstractions.Testing.Helpers.Execute.*On*",
				                      			// Drives are not used in Linux
				                      			"ValidateDriveLetter",
				                      			// The encryption helper is only valid for testing purposes
				                      			"CreateDummyEncryptionAlgorithm",
				                      			// Triggered by invalid chars which don't exist in Linux
				                      			"ThrowCommonExceptionsIfPathIsInvalid",
				                      			// Calls to Thread.Sleep cannot be detected by a test
				                      			"System.Threading.Thread.Sleep",
				                      			// Ensures that an expectation from developers is met
				                      			"Debug.Assert"
				                      		]
				                      	}
				                      }
				                      """;
				File.WriteAllText(configFile, configText);
				Log.Debug($"Created '{configFile}':{Environment.NewLine}{configText}");

				string arguments = IsServerBuild
					? $"-f \"{configFile}\" -O \"{StrykerOutputDirectory}\" -r \"Markdown\" -r \"Dashboard\" -r \"cleartext\""
					: $"-f \"{configFile}\" -O \"{StrykerOutputDirectory}\" -r \"Markdown\" -r \"cleartext\"";

				string executable = EnvironmentInfo.IsWin ? "dotnet-stryker.exe" : "dotnet-stryker";
				IProcess process = ProcessTasks.StartProcess(
						Path.Combine(StrykerToolPath, executable),
						arguments,
						Solution.Directory)
					.AssertWaitForExit();
				if (process.ExitCode != 0)
				{
					Assert.Fail(
						$"Stryker did not execute successfully for {project.Key.Name}: (exit code {process.ExitCode}).");
				}

				MutationCommentBodies.Add(project.Key.Name,
					CreateMutationCommentBody(project.Key.Name));
			}
		});

	Target MutationTestsWindows => _ => _
		.DependsOn(Compile)
		.DependsOn(MutationTestPreparation)
		.Executes(() =>
		{
			AbsolutePath configFile = StrykerToolPath / "Stryker.Config.json";
			Dictionary<Project, Project[]> projects = new()
			{
				{
					Solution.Testably_Abstractions_AccessControl,
					[Solution.Tests.Testably_Abstractions_AccessControl_Tests]
				},
				{
					Solution.Testably_Abstractions_Compression,
					[Solution.Tests.Testably_Abstractions_Compression_Tests]
				},
				{
					Solution.Testably_Abstractions, [
						Solution.Tests.Testably_Abstractions_Testing_Tests,
						Solution.Tests.Testably_Abstractions_Tests
					]
				}
			};

			foreach (KeyValuePair<Project, Project[]> project in projects)
			{
				string? branchName = GitVersion?.BranchName;
				if (GitHubActions?.Ref.StartsWith("refs/tags/",
					StringComparison.OrdinalIgnoreCase) == true)
				{
					string version = GitHubActions.Ref.Substring("refs/tags/".Length);
					branchName = "release/" + version;
					Log.Information("Use release branch analysis for '{BranchName}'", branchName);
				}

				string configText = $$"""
				                      {
				                      	"stryker-config": {
				                      		"project-info": {
				                      			"name": "github.com/Testably/Testably.Abstractions",
				                      			"module": "{{project.Key.Name}}",
				                      			"version": "{{branchName}}"
				                      		},
				                      		"test-projects": [
				                      			{{string.Join(",\n\t\t\t", project.Value.Select(PathForJson))}}
				                      		],
				                      		"project": {{PathForJson(project.Key)}},
				                      		"target-framework": "net8.0",
				                      		"since": {
				                      			"target": "main",
				                      			"enabled": {{(GitVersion?.BranchName != "main").ToString().ToLowerInvariant()}},
				                      			"ignore-changes-in": [
				                      				"**/.github/**/*.*"
				                      			]
				                      		},
				                      		"reporters": [
				                      			"html",
				                      			"progress",
				                      			"cleartext"
				                      		],
				                      		"mutation-level": "Advanced"
				                      	}
				                      }
				                      """;
				File.WriteAllText(configFile, configText);
				Log.Debug($"Created '{configFile}':{Environment.NewLine}{configText}");

				string arguments = IsServerBuild
					? $"-f \"{configFile}\" -O \"{StrykerOutputDirectory}\" -r \"Markdown\" -r \"Dashboard\" -r \"cleartext\""
					: $"-f \"{configFile}\" -O \"{StrykerOutputDirectory}\" -r \"Markdown\" -r \"cleartext\"";

				string executable = EnvironmentInfo.IsWin ? "dotnet-stryker.exe" : "dotnet-stryker";
				IProcess process = ProcessTasks.StartProcess(
						Path.Combine(StrykerToolPath, executable),
						arguments,
						Solution.Directory)
					.AssertWaitForExit();
				if (process.ExitCode != 0)
				{
					Assert.Fail(
						$"Stryker did not execute successfully for {project.Key.Name}: (exit code {process.ExitCode}).");
				}

				MutationCommentBodies.Add(project.Key.Name,
					CreateMutationCommentBody(project.Key.Name));
			}
		});

	AbsolutePath StrykerOutputDirectory => ArtifactsDirectory / "Stryker";
	AbsolutePath StrykerToolPath => TestResultsDirectory / "dotnet-stryker";

	string CreateMutationCommentBody(string projectName)
	{
		string[] fileContent =
			File.ReadAllLines(ArtifactsDirectory / "Stryker" / "reports" / "mutation-report.md");
		StringBuilder sb = new();
		sb.AppendLine($"<!-- START {projectName} -->");
		sb.AppendLine($"### {projectName}");
		sb.AppendLine("<details>");
		sb.AppendLine("<summary>Details</summary>");
		sb.AppendLine();
		int count = 0;
		foreach (string line in fileContent.Skip(1))
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			if (line.StartsWith("#"))
			{
				if (++count == 1)
				{
					sb.AppendLine();
					sb.AppendLine("</details>");
					sb.AppendLine();
				}

				sb.AppendLine("##" + line);
				continue;
			}

			if (count == 0 &&
			    line.StartsWith("|") &&
			    line.Contains("| N\\/A"))
			{
				continue;
			}

			sb.AppendLine(line);
		}

		sb.AppendLine($"<!-- END {projectName} -->");
		string body = sb.ToString();
		return body;
	}

	static string PathForJson(Project project)
		=> $"\"{project.Path.ToString().Replace(@"\", @"\\")}\"";

	string ReplaceProject(string body, string project, string value)
	{
		int startIndex =
			body.IndexOf($"<!-- START {project} -->", StringComparison.OrdinalIgnoreCase);
		int endIndex = body.IndexOf($"<!-- END {project} -->", StringComparison.OrdinalIgnoreCase);
		if (startIndex >= 0 && endIndex > startIndex)
		{
			string prefix = body.Substring(0, startIndex);
			string suffix = body.Substring(endIndex + $"<!-- END {project} -->".Length);
			return prefix + value + suffix;
		}

		return body + Environment.NewLine + value;
	}
}

using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Fallout.Common.Tools.DotNet.DotNetTasks;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Project = Fallout.Solutions.Project;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	private const long MaxMutationReportSize = 100 * 1024 * 1024;

	Dictionary<Project, Project[]> LinuxMutationTestProjects => new()
	{
		{
			Solution.Testably_Abstractions_Testing, [
				Solution.Tests.Testably_Abstractions_Testing_Tests,
				Solution.Tests.Testably_Abstractions_Tests
			]
		},
	};

	Dictionary<Project, Project[]> WindowsMutationTestProjects => new()
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
			Solution.Testably_Abstractions_MemoryMappedFiles,
			[Solution.Tests.Testably_Abstractions_MemoryMappedFiles_Tests]
		},
		{
			Solution.Core.Testably_Abstractions, [
				Solution.Tests.Testably_Abstractions_Testing_Tests,
				Solution.Tests.Testably_Abstractions_Tests
			]
		}
	};

	Target MutationTestsComment => _ => _
		.After(MutationTestsLinux)
		.After(MutationTestsWindows)
		.DependsOn(MutationTestsDashboard)
		.OnlyWhenDynamic(() => BuildScope == BuildScope.Default)
		.Executes(async () =>
		{
			// The artifacts are untrusted, because the pull request controls the code that produces them, so the pull
			// request to comment on comes from the event of the workflow run.
			int? prId = await BuildExtensions.ResolvePullRequestOfWorkflowRun(GithubToken);
			if (prId == null)
			{
				Log.Information(
					"The workflow run belongs to no pull request, so there is no mutation comment to write");
				return;
			}

			Dictionary<string, string> mutationCommentBodies = new();
			foreach (AbsolutePath file in ArtifactsDirectory.GetFiles("MutationTest_*.md", 2))
			{
				string project = Path.GetFileNameWithoutExtension(file).Substring("MutationTest_".Length);
				mutationCommentBodies[project] = (await File.ReadAllTextAsync(file)).AsUntrustedCommentContent();
			}

			if (mutationCommentBodies.Count == 0)
			{
				Log.Warning("No files matching \"MutationTest_*.md\" found");
				return;
			}

			GitHubClient gitHubClient = new(new ProductHeaderValue("Fallout"));
			Credentials tokenAuth = new(GithubToken);
			gitHubClient.Credentials = tokenAuth;
			IReadOnlyList<IssueComment> comments =
				await gitHubClient.Issue.Comment.GetAllForIssue("Testably",
					"Testably.Abstractions", prId.Value);
			IssueComment? existingComment = null;
			Log.Information($"Found {comments.Count} comments");
			foreach (IssueComment comment in comments)
			{
				if (comment.IsPipelineComment("## :alien: Mutation Results"))
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
				              + string.Join(Environment.NewLine, mutationCommentBodies.Values);

				Log.Information($"Create comment:\n{body}");
				await gitHubClient.Issue.Comment.Create("Testably", "Testably.Abstractions",
					prId.Value, body);
			}
			else
			{
				string body = existingComment.Body;
				foreach ((string project, string value) in mutationCommentBodies)
				{
					body = ReplaceProject(body, project, value);
				}

				Log.Information($"Update comment:\n{body}");
				await gitHubClient.Issue.Comment.Update("Testably", "Testably.Abstractions",
					existingComment.Id, body);
			}
		});

	Target MutationTestsDashboard => _ => _
		.After(MutationTestsLinux)
		.After(MutationTestsWindows)
		.OnlyWhenDynamic(() => BuildScope == BuildScope.Default)
		.Executes(async () =>
		{
			string? version = await GetMutationDashboardVersion();
			if (version == null)
			{
				Log.Information("The run has no version on the mutation dashboard, so there is nothing to publish");
				return;
			}

			await "MutationTests-Linux".DownloadArtifactTo(ArtifactsDirectory, GithubToken);
			await "MutationTests-Windows".DownloadArtifactTo(ArtifactsDirectory, GithubToken);

			string? apiKey = Environment.GetEnvironmentVariable("STRYKER_DASHBOARD_API_KEY");
			foreach (Project project in LinuxMutationTestProjects.Keys.Concat(WindowsMutationTestProjects.Keys))
			{
				AbsolutePath reportFile = StrykerOutputDirectory / project.Name / "reports" / "mutation-report.json";
				if (!File.Exists(reportFile))
				{
					Log.Information("Skip uploading the mutation report of {ProjectName}, as there is none",
						project.Name);
					continue;
				}

				if (new FileInfo(reportFile).Length > MaxMutationReportSize)
				{
					Assert.Fail($"The {project.Name} mutation report is larger than {MaxMutationReportSize} bytes");
				}

				string reportComment = File.ReadAllText(reportFile);
				if (JsonNode.Parse(reportComment) is not JsonObject)
				{
					Assert.Fail($"The {project.Name} mutation report is no JSON object");
				}

				using HttpClient client = new();
				client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
				// https://stryker-mutator.io/docs/General/dashboard/#send-a-report-via-curl
				HttpResponseMessage response = await client.PutAsync(
					$"https://dashboard.stryker-mutator.io/api/reports/github.com/Testably/Testably.Abstractions/{version}?module={project.Name}",
					new StringContent(reportComment, new MediaTypeHeaderValue("application/json")));
				string responseContent = await response.Content.ReadAsStringAsync();
				if (response.IsSuccessStatusCode)
				{
					Log.Information("Uploaded the {Module} mutation report ({Size} bytes): {Response}",
						project.Name, reportComment.Length, responseContent);
				}
				else
				{
					// Without this the job stays green while the dashboard keeps showing the previous score.
					Assert.Fail(
						$"Could not upload the {project.Name} mutation report ({reportComment.Length} bytes), " +
						$"the dashboard answered {(int)response.StatusCode} {response.ReasonPhrase}: {responseContent}");
				}
			}
		});

	Target MutationTestPreparation => _ => _
		.OnlyWhenDynamic(() => BuildScope == BuildScope.Default)
		.Executes(() =>
		{
			StrykerToolPath.CreateOrCleanDirectory();

			DotNetToolInstall(_ => _
				.SetPackageName("dotnet-stryker")
				.SetVersion(GetStrykerVersion())
				.SetToolInstallationPath(StrykerToolPath));

			StrykerOutputDirectory.CreateOrCleanDirectory();
		});

	Target MutationTests => _ => _
		.DependsOn(MutationTestsWindows)
		.DependsOn(MutationTestsLinux)
		.OnlyWhenDynamic(() => BuildScope == BuildScope.Default);

	Target MutationTestsLinux => _ => _
		.DependsOn(Compile)
		.DependsOn(MutationTestPreparation)
		.OnlyWhenDynamic(() => BuildScope == BuildScope.Default)
		.Executes(() =>
		{
			AbsolutePath configFile = StrykerToolPath / "Stryker.Config.json";
			foreach (KeyValuePair<Project, Project[]> project in LinuxMutationTestProjects)
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
				                      		"target-framework": "net10.0",
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

				string arguments =
					$"-f \"{configFile}\" -O \"{StrykerOutputDirectory / project.Key.Name}\" -r \"Markdown\" -r \"json\" -r \"cleartext\" --test-runner mtp";

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

				if (!File.Exists(StrykerOutputDirectory / project.Key.Name / "reports" / "mutation-report.md"))
				{
					Log.Information("Stryker found nothing to mutate in {ProjectName}", project.Key.Name);
					continue;
				}

				File.WriteAllText(ArtifactsDirectory / $"MutationTest_{project.Key.Name}.md",
					CreateMutationCommentBody(project.Key.Name));
			}
		});

	Target MutationTestsWindows => _ => _
		.DependsOn(Compile)
		.DependsOn(MutationTestPreparation)
		.OnlyWhenDynamic(() => BuildScope == BuildScope.Default)
		.Executes(() =>
		{
			AbsolutePath configFile = StrykerToolPath / "Stryker.Config.json";
			foreach (KeyValuePair<Project, Project[]> project in WindowsMutationTestProjects)
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
				                      		"target-framework": "net10.0",
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

				string arguments =
					$"-f \"{configFile}\" -O \"{StrykerOutputDirectory / project.Key.Name}\" -r \"Markdown\" -r \"json\" -r \"cleartext\" --test-runner mtp";

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

				if (!File.Exists(StrykerOutputDirectory / project.Key.Name / "reports" / "mutation-report.md"))
				{
					Log.Information("Stryker found nothing to mutate in {ProjectName}", project.Key.Name);
					continue;
				}

				File.WriteAllText(ArtifactsDirectory / $"MutationTest_{project.Key.Name}.md",
					CreateMutationCommentBody(project.Key.Name));
			}
		});

	AbsolutePath StrykerOutputDirectory => ArtifactsDirectory / "Stryker";
	AbsolutePath StrykerToolPath => TestResultsDirectory / "dotnet-stryker";

	/// <summary>
	///     The version under which the dashboard stores the mutation reports of the analysed run, or
	///     <see langword="null" /> when it has none.
	/// </summary>
	/// <remarks>
	///     The artifacts are untrusted, because a pull request controls the code that produces them, so the version
	///     comes from the event alone, and a run that a pull request triggered can only publish to the version of
	///     that pull request.
	/// </remarks>
	private async Task<string?> GetMutationDashboardVersion()
	{
		string? version = null;
		if (GitHubActions?.EventName != "workflow_run")
		{
			// The reports come from this very run, so its own ref identifies them.
			string gitRef = GitHubActions?.Ref ?? "";
			if (gitRef.StartsWith("refs/tags/", StringComparison.Ordinal))
			{
				version = "release/" + gitRef.Substring("refs/tags/".Length);
			}
			else if (gitRef.StartsWith("refs/heads/", StringComparison.Ordinal))
			{
				version = gitRef.Substring("refs/heads/".Length);
			}
		}
		else if (Environment.GetEnvironmentVariable("WorkflowRunEvent") == "pull_request")
		{
			int? prId = await BuildExtensions.ResolvePullRequestOfWorkflowRun(GithubToken);
			return prId == null ? null : $"pull/{prId}/merge";
		}
		else
		{
			version = Environment.GetEnvironmentVariable("WorkflowRunHeadBranch");
		}

		// The version becomes a part of the path that the report is sent to.
		return version != null &&
		       Regex.IsMatch(version, @"\A[A-Za-z0-9][A-Za-z0-9._-]*(/[A-Za-z0-9][A-Za-z0-9._-]*)*\z")
			? version
			: null;
	}

	string CreateMutationCommentBody(string projectName)
	{
		string[] fileContent =
			File.ReadAllLines(StrykerOutputDirectory / projectName / "reports" / "mutation-report.md");
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

	/// <summary>
	///     The version of Stryker to install, which is the one that the <c>PackageDownload</c> of the build project
	///     pins, so that Dependabot keeps it up to date.
	/// </summary>
	static string GetStrykerVersion()
		=> XDocument.Load(RootDirectory / "Pipeline" / "Build.csproj")
			.Descendants("PackageDownload")
			.Single(package => package.Attribute("Include")?.Value == "dotnet-stryker")
			.Attribute("Version")!.Value.Trim('[', ']');

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

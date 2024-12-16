using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	Target MutationTests => _ => _
		.DependsOn(MutationTestsWindows)
		.DependsOn(MutationTestsLinux);

	Target MutationTestsWindows => _ => _
		.DependsOn(Compile)
		.Executes(() =>
		{
			AbsolutePath toolPath = TestResultsDirectory / "dotnet-stryker";
			AbsolutePath configFile = toolPath / "Stryker.Config.json";
			toolPath.CreateOrCleanDirectory();

			DotNetToolInstall(_ => _
				.SetPackageName("dotnet-stryker")
				.SetToolInstallationPath(toolPath));

			Dictionary<Project, Project[]> projects = new()
			{
				{ Solution.Testably_Abstractions_AccessControl, [Solution.Tests.Testably_Abstractions_AccessControl_Tests] },
				{ Solution.Testably_Abstractions_Compression, [Solution.Tests.Testably_Abstractions_Compression_Tests] },
				{ Solution.Testably_Abstractions, [ Solution.Tests.Testably_Abstractions_Testing_Tests, Solution.Tests.Testably_Abstractions_Tests ] }
			};

			foreach (KeyValuePair<Project, Project[]> project in projects)
			{
				string branchName = GitVersion.BranchName;
				if (GitHubActions?.Ref.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase) == true)
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
				                      			"enabled": {{(GitVersion.BranchName != "main").ToString().ToLowerInvariant()}},
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
					? $"-f \"{configFile}\" -r \"Dashboard\" -r \"cleartext\""
					: $"-f \"{configFile}\" -r \"cleartext\"";

				string executable = EnvironmentInfo.IsWin ? "dotnet-stryker.exe" : "dotnet-stryker";
				IProcess process = ProcessTasks.StartProcess(
						Path.Combine(toolPath, executable),
						arguments,
						Solution.Directory)
					.AssertWaitForExit();
				if (process.ExitCode != 0)
				{
					Assert.Fail(
						$"Stryker did not execute successfully for {project.Key.Name}: (exit code {process.ExitCode}).");
				}
			}
		});

	Target MutationTestsLinux => _ => _
		.DependsOn(Compile)
		.Executes(() =>
		{
			AbsolutePath toolPath = TestResultsDirectory / "dotnet-stryker";
			AbsolutePath configFile = toolPath / "Stryker.Config.json";
			toolPath.CreateOrCleanDirectory();

			DotNetToolInstall(_ => _
				.SetPackageName("dotnet-stryker")
				.SetToolInstallationPath(toolPath));

			Dictionary<Project, Project[]> projects = new()
			{
				{ Solution.Testably_Abstractions_Testing, [ Solution.Tests.Testably_Abstractions_Testing_Tests, Solution.Tests.Testably_Abstractions_Tests ] },
			};

			foreach (KeyValuePair<Project, Project[]> project in projects)
			{
				string branchName = GitVersion.BranchName;
				if (GitHubActions?.Ref.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase) == true)
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
				                      			"enabled": {{(GitVersion.BranchName != "main").ToString().ToLowerInvariant()}},
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
					? $"-f \"{configFile}\" -r \"Dashboard\" -r \"cleartext\""
					: $"-f \"{configFile}\" -r \"cleartext\"";

				string executable = EnvironmentInfo.IsWin ? "dotnet-stryker.exe" : "dotnet-stryker";
				IProcess process = ProcessTasks.StartProcess(
						Path.Combine(toolPath, executable),
						arguments,
						Solution.Directory)
					.AssertWaitForExit();
				if (process.ExitCode != 0)
				{
					Assert.Fail(
						$"Stryker did not execute successfully for {project.Key.Name}: (exit code {process.ExitCode}).");
				}
			}
		});
	
	static string PathForJson(Project project) => $"\"{project.Path.ToString().Replace(@"\", @"\\")}\"";
}

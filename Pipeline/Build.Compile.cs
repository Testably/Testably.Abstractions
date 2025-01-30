using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using System;
using System.Linq;
using Nuke.Common.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	string? BranchName;
	AssemblyVersion? CoreVersion;
	AssemblyVersion? MainVersion;
	string? SemVer;
	Project[] MainProjects = [];
	Project[] CoreProjects = [];

	Target CalculateNugetVersion => _ => _
		.Unlisted()
		.Executes(() =>
		{
			MainProjects =
			[
				Solution.Testably_Abstractions_Testing,
				Solution.Testably_Abstractions_AccessControl,
				Solution.Testably_Abstractions_Compression,
			];
			
			CoreProjects =
			[
				Solution.Testably_Abstractions,
				Solution.Testably_Abstractions_Interface,
				Solution.Testably_Abstractions_FileSystem_Interface,
			];
			
			CoreVersion = AssemblyVersion.FromGitVersion(GitVersionTasks.GitVersion(s => s
					.SetFramework("net8.0")
					.SetNoFetch(true)
					.SetNoCache(true)
					.DisableProcessOutputLogging()
					.SetUpdateAssemblyInfo(false)
					.AddProcessAdditionalArguments("/overrideconfig", "tag-prefix=core/v"))
				.Result);

			GitVersion gitVersion = GitVersionTasks.GitVersion(s => s
					.SetFramework("net8.0")
					.SetNoFetch(true)
					.SetNoCache(true)
					.DisableProcessOutputLogging()
					.SetUpdateAssemblyInfo(false))
				.Result;

			MainVersion = AssemblyVersion.FromGitVersion(gitVersion);
			SemVer = gitVersion.SemVer;
			BranchName = gitVersion.BranchName;

			if (GitHubActions?.IsPullRequest == true && GitVersion != null)
			{
				string buildNumber = GitHubActions.RunNumber.ToString();
				Console.WriteLine(
					$"Branch spec is a pull request. Adding build number {buildNumber}");

				SemVer = string.Join('.', GitVersion.SemVer.Split('.').Take(3).Union([buildNumber]));
			}

			Console.WriteLine($"SemVer = {SemVer}");
		});

	Target Clean => _ => _
		.Unlisted()
		.Before(Restore)
		.Executes(() =>
		{
			ArtifactsDirectory.CreateOrCleanDirectory();
			Log.Information("Cleaned {path}...", ArtifactsDirectory);

			TestResultsDirectory.CreateOrCleanDirectory();
			Log.Information("Cleaned {path}...", TestResultsDirectory);
		});

	Target Restore => _ => _
		.Unlisted()
		.DependsOn(Clean)
		.Executes(() =>
		{
			DotNetRestore(s => s
				.SetProjectFile(Solution)
				.EnableNoCache()
				.SetConfigFile(RootDirectory / "nuget.config"));
		});

	Target Compile => _ => _
		.DependsOn(Restore)
		.DependsOn(CalculateNugetVersion)
		.Executes(() =>
		{
			ReportSummary(s => s
				.WhenNotNull(SemVer, (summary, semVer) => summary
					.AddPair("Version", semVer)));

			foreach (var mainProject in MainProjects)
			{
				ClearNugetPackages(mainProject.Directory / "bin");
			}
			
			UpdateReadme(MainVersion.FileVersion, false);
			DotNetBuild(s => s
				.SetProjectFile(Solution)
				.SetConfiguration(Configuration)
				.EnableNoLogo()
				.EnableNoRestore()
				.SetVersion(MainVersion!.FileVersion)
				.SetAssemblyVersion(MainVersion!.FileVersion)
				.SetFileVersion(MainVersion!.FileVersion));
			
			UpdateReadme(CoreVersion.FileVersion, true);
			foreach (var coreProject in CoreProjects)
			{
				ClearNugetPackages(coreProject.Directory / "bin");
				DotNetBuild(s => s
					.SetProjectFile(coreProject)
					.SetConfiguration(Configuration)
					.EnableNoLogo()
					.EnableNoRestore()
					.SetProcessAdditionalArguments($"/p:SolutionDir={RootDirectory}/")
					.SetVersion(CoreVersion!.FileVersion)
					.SetAssemblyVersion(CoreVersion!.FileVersion)
					.SetFileVersion(CoreVersion!.FileVersion));
			}
		});
	
	public record AssemblyVersion(string FileVersion, string InformationalVersion)
	{
		[return: NotNullIfNotNull(nameof(gitVersion))]
		public static AssemblyVersion? FromGitVersion(GitVersion gitVersion)
		{
			if (gitVersion is null)
			{
				return null;
			}

			return new AssemblyVersion(gitVersion.AssemblySemVer, gitVersion.InformationalVersion);
		}
	}

	private void UpdateReadme(string fileVersion, bool forCore)
	{
		string version;
		if (GitHubActions?.Ref.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase) == true)
		{
			version = GitHubActions.Ref.Substring("refs/tags/".Length);
		}
		else
		{
			version = string.Join('.', fileVersion.Split('.').Take(3));
			if (version.IndexOf('-') != -1)
			{
				version = "v" + version.Substring(0, version.IndexOf('-'));
			}
		}
		
		Log.Information("Update readme using '{Version}' as version", version);

		StringBuilder sb = new();
		string[] lines = File.ReadAllLines(Solution.Directory / "README.md");
		sb.AppendLine(lines[0]);
		sb.AppendLine(
			$"[![Changelog](https://img.shields.io/badge/Changelog-v{version}-blue)](https://github.com/Testably/Testably.Abstractions/releases/tag/v{version})");
		foreach (string line in lines.Skip(1))
		{
			if (line.StartsWith(
				    "[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml") ||
			    line.StartsWith(
				    "[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure"))
			{
				continue;
			}

			if (line.StartsWith(
				"[![Coverage](https://sonarcloud.io/api/project_badges/measure"))
			{
				sb.AppendLine(line
					.Replace(")", $"&branch=release/v{version})"));
				continue;
			}

			if (line.StartsWith("[![Mutation testing badge](https://img.shields.io/endpoint"))
			{
				sb.AppendLine(line
					.Replace("%2Fmain)", $"%2Frelease%2Fv{version})")
					.Replace("/main)", $"/release/v{version})"));
				continue;
			}

			sb.AppendLine(line);
		}

		File.WriteAllText(ArtifactsDirectory / "README.md", sb.ToString());
	}

	private static void ClearNugetPackages(string binPath)
	{
		if (Directory.Exists(binPath))
		{
			foreach (string package in Directory.EnumerateFiles(binPath, "*nupkg", SearchOption.AllDirectories))
			{
				File.Delete(package);
			}
		}
	}
}

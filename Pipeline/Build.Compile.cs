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
using System.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	string? BranchName;
	AssemblyVersion? CoreVersion;
	AssemblyVersion? MainVersion;
	string? SemVer;
	Project[] MainProjects;
	Project[] CoreProjects;

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
			
			DotNetBuild(s => s
				.SetProjectFile(Solution)
				.SetConfiguration(Configuration)
				.EnableNoLogo()
				.EnableNoRestore()
				.SetVersion(MainVersion.FileVersion)
				.SetAssemblyVersion(MainVersion.FileVersion)
				.SetFileVersion(MainVersion.FileVersion));
			
			foreach (var coreProject in CoreProjects)
			{
				ClearNugetPackages(coreProject.Directory / "bin");
				DotNetBuild(s => s
					.SetProjectFile(coreProject)
					.SetConfiguration(Configuration)
					.EnableNoLogo()
					.EnableNoRestore()
					.SetProcessAdditionalArguments($"/p:SolutionDir={RootDirectory}")
					.SetVersion(CoreVersion.FileVersion)
					.SetAssemblyVersion(CoreVersion.FileVersion)
					.SetFileVersion(CoreVersion.FileVersion));
			}
		});
	
	public record AssemblyVersion(string FileVersion, string InformationalVersion)
	{
		public static AssemblyVersion FromGitVersion(GitVersion gitVersion)
		{
			if (gitVersion is null)
			{
				return null;
			}

			return new AssemblyVersion(gitVersion.AssemblySemVer, gitVersion.InformationalVersion);
		}
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

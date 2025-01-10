using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using System;
using System.Linq;
using Nuke.Common.Utilities;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	string? SemVer;

	Target CalculateNugetVersion => _ => _
		.Unlisted()
		.Executes(() =>
		{
			SemVer = GitVersion?.SemVer;

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

			DotNetBuild(s => s
				.SetProjectFile(Solution)
				.SetConfiguration(Configuration)
				.EnableNoLogo()
				.EnableNoRestore()
				.SetVersion(SemVer)
				.SetAssemblyVersion(GitVersion?.AssemblySemVer)
				.SetFileVersion(GitVersion?.AssemblySemFileVer)
				.SetInformationalVersion(GitVersion?.InformationalVersion));
		});
}

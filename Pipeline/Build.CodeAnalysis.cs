using Nuke.Common;
using Nuke.Common.Tools.SonarScanner;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	[Parameter("The key to push to sonarcloud")] [Secret] readonly string? SonarToken;

	Target CodeAnalysisBegin => _ => _
		.Unlisted()
		.Before(Compile)
		.Before(CodeCoverage)
		.Executes(() =>
		{
			SonarScannerTasks.SonarScannerBegin(s => s
				.SetOrganization("testably")
				.SetProjectKey("Testably_Testably.Abstractions")
				.AddVSTestReports(TestResultsDirectory / "*.trx")
				.AddOpenCoverPaths(TestResultsDirectory / "reports" / "OpenCover.xml")
				.SetPullRequestOrBranchName(GitHubActions, BranchName)
				.SetVersion(SemVer)
				.SetToken(SonarToken));
		});

	Target CodeAnalysisEnd => _ => _
		.Unlisted()
		.DependsOn(Compile)
		.DependsOn(CodeCoverage)
		.OnlyWhenDynamic(() => IsServerBuild)
		.Executes(() =>
		{
			SonarScannerTasks.SonarScannerEnd(s => s
				.SetToken(SonarToken));
		});

	// ReSharper disable once UnusedMember.Local
	Target CodeAnalysis => _ => _
		.DependsOn(CodeAnalysisBegin)
		.DependsOn(CodeAnalysisEnd);
}

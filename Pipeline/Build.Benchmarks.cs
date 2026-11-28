using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.Git;
using Octokit;
using Serilog;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	private const string BenchmarkBranch = "benchmarks";
	private const string BenchmarkDataPath = "Docs/pages/static/js/data.js";
	private const string BenchmarkLimitedDataPath = "Docs/pages/static/js/limited-data.js";
	private const int BenchmarkLimit = 50;

	[Parameter("Filter for BenchmarkDotNet - Default is '*'")] readonly string BenchmarkFilter = "*";

	AbsolutePath BenchmarkDirectory => ArtifactsDirectory / "Benchmarks";
	AbsolutePath MainBenchmarkDirectory => ArtifactsDirectory / "MainBenchmarks";

	Target Benchmarks => _ => _
		.Description("Runs the benchmarks (--benchmark-filter, default '*') and prints the reports.")
		.Executes(() =>
		{
			BenchmarkDirectory.CreateOrCleanDirectory();
			RunBenchmarks(Solution.Benchmarks.Testably_Abstractions_Benchmarks.Directory, BenchmarkFilter,
				BenchmarkDirectory);

			foreach (string file in Directory.GetFiles(BenchmarkDirectory / "results", "*-report-github.md"))
			{
				Log.Information("Report ({FileName}):\n {FileContent}", Path.GetFileName(file), File.ReadAllText(file));
			}
		});

	Target BenchmarkMain => _ => _
		.Description("Runs the Testably benchmarks of the 'main' that the current commit is based on, so that the " +
		             "benchmark comment compares results from the same runner.")
		.Executes(() =>
		{
			MainBenchmarkDirectory.CreateOrCleanDirectory();
			string mainSha = GitTasks.Git("merge-base HEAD origin/main").Single().Text;
			File.WriteAllText(MainBenchmarkDirectory / "sha.txt", mainSha);
			// Outside the repository, so that MSBuild does not pick up the props files of the current commit.
			AbsolutePath mainDirectory =
				AbsolutePath.Create(Path.Combine(Path.GetTempPath(), $"Testably.Abstractions-main-{mainSha}"));
			GitTasks.Git($"worktree add --force --detach \"{mainDirectory}\" {mainSha}");
			try
			{
				// System.IO.Abstractions does not change between 'main' and the pull request.
				RunBenchmarks(
					mainDirectory / Path.GetRelativePath(RootDirectory,
						Solution.Benchmarks.Testably_Abstractions_Benchmarks.Directory),
					$"{BenchmarkFilter}_Testably", MainBenchmarkDirectory);
			}
			finally
			{
				GitTasks.Git($"worktree remove --force \"{mainDirectory}\"");
			}
		});

	Target BenchmarkComment => _ => _
		.Executes(async () =>
		{
			// The artifacts are untrusted, because the pull request controls the code that produces them, so the pull
			// request to comment on comes from the event of the workflow run.
			int? prId = await BuildExtensions.ResolvePullRequestOfWorkflowRun(GithubToken);
			if (prId == null)
			{
				Log.Information("Skip writing a comment, as the workflow run belongs to no pull request.");
				return;
			}

			await "Benchmarks-".DownloadArtifactsStartingWith(ArtifactsDirectory, GithubToken);
			string[] files = Directory.Exists(BenchmarkDirectory / "results")
				? Directory.GetFiles(BenchmarkDirectory / "results", "*-report-github.md")
				: [];
			if (files.Length == 0)
			{
				Log.Information("Skip benchmark comment, because no report file was generated.");
				return;
			}

			List<BenchmarkReportFile> reportFiles = new();
			foreach (string file in files.Order(StringComparer.Ordinal))
			{
				string prefix = file.Substring(0, file.Length - "-report-github.md".Length);
				string mainFile = MainBenchmarkDirectory / "results" /
				                  (Path.GetFileName(prefix) + "-report-full-compressed.json");
				reportFiles.Add(new BenchmarkReportFile(
					prefix.Substring(prefix.LastIndexOf('.') + 1),
					await File.ReadAllLinesAsync(file),
					await File.ReadAllTextAsync(prefix + "-report-full-compressed.json"),
					File.Exists(mainFile) ? await File.ReadAllTextAsync(mainFile) : null));
			}

			AbsolutePath mainShaFile = MainBenchmarkDirectory / "sha.txt";
			string? mainSha = File.Exists(mainShaFile) ? (await File.ReadAllTextAsync(mainShaFile)).Trim() : null;
			// The artifact is untrusted, so only a commit hash may become part of the link.
			string? mainCommitUrl = mainSha != null && Regex.IsMatch(mainSha, "^[0-9a-f]{40}$")
				? $"https://github.com/Testably/Testably.Abstractions/commit/{mainSha}"
				: null;
			string body = BenchmarkReport.BuildBody(reportFiles, mainCommitUrl, BenchmarkReport.DefaultColumnsToRemove)
				.AsUntrustedCommentContent();
			GitHubClient gitHubClient = new(new ProductHeaderValue("Fallout"))
			{
				Credentials = new Credentials(GithubToken),
			};
			IReadOnlyList<IssueComment> comments =
				await gitHubClient.Issue.Comment.GetAllForIssue("Testably", "Testably.Abstractions", prId.Value);
			IssueComment? existing = comments.LastOrDefault(c => c.IsPipelineComment(BenchmarkReport.Heading));
			if (existing == null)
			{
				await gitHubClient.Issue.Comment.Create("Testably", "Testably.Abstractions", prId.Value, body);
			}
			else
			{
				await gitHubClient.Issue.Comment.Update("Testably", "Testably.Abstractions", existing.Id, body);
			}
		});

	Target PublishBenchmarkReport => _ => _
		.Description("Aggregates the Benchmarks-* artifacts of the current workflow run into data.js and " +
		             $"limited-data.js on the '{BenchmarkBranch}' branch, which the docs site reads.")
		.Requires(() => GithubToken)
		.Executes(async () =>
		{
			await "Benchmarks-".DownloadArtifactsStartingWith(ArtifactsDirectory, GithubToken);
			List<string> benchmarkReports = Directory.Exists(BenchmarkDirectory / "results")
				? Directory.GetFiles(BenchmarkDirectory / "results", "*-report-full-compressed.json")
					.Select(File.ReadAllText).ToList()
				: [];
			if (benchmarkReports.Count == 0)
			{
				Log.Warning("Skip benchmark report: no benchmark JSON reports found.");
				return;
			}

			PageBenchmarkReportGenerator.CommitInfo commitInfo = ReadCurrentCommitInfo();
			BuildExtensions.GithubFile dataFile =
				await BuildExtensions.ReadBranchFileAsync(BenchmarkDataPath, BenchmarkBranch, GithubToken)
				?? throw new InvalidOperationException($"'{BenchmarkDataPath}' is missing on '{BenchmarkBranch}'.");
			BuildExtensions.GithubFile limitedFile =
				await BuildExtensions.ReadBranchFileAsync(BenchmarkLimitedDataPath, BenchmarkBranch, GithubToken)
				?? throw new InvalidOperationException($"'{BenchmarkLimitedDataPath}' is missing on '{BenchmarkBranch}'.");

			(string? updated, string? limited) = PageBenchmarkReportGenerator.Append(
				commitInfo, dataFile.Content, benchmarkReports, BenchmarkLimit);
			if (string.IsNullOrWhiteSpace(updated) || limited == null)
			{
				Log.Information("No changes to publish (commit already recorded).");
				return;
			}

			string commitMessage =
				$"Update benchmark for {commitInfo.Sha.Substring(0, 8)}: {commitInfo.Message} by {commitInfo.Author}";
			await BuildExtensions.WriteBranchFileAsync(BenchmarkDataPath, BenchmarkBranch, commitMessage, updated,
				dataFile.Sha, GithubToken);
			await BuildExtensions.WriteBranchFileAsync(BenchmarkLimitedDataPath, BenchmarkBranch, commitMessage,
				limited, limitedFile.Sha, GithubToken);
		});

	void RunBenchmarks(AbsolutePath projectDirectory, string filter, AbsolutePath artifactsDirectory)
	{
		DotNetBuild(s => s
			.SetProjectFile(projectDirectory)
			.SetConfiguration(Configuration.Release)
			.EnableNoLogo());

		DotNet(
			$"Testably.Abstractions.Benchmarks.dll --exporters json --filter {filter} --artifacts \"{artifactsDirectory}\"",
			projectDirectory / "bin" / "Release");
	}

	static PageBenchmarkReportGenerator.CommitInfo ReadCurrentCommitInfo()
	{
		string[] fields = GitTasks.Git("log -1 --format=%H%n%an%n%ad%n%s").Select(x => x.Text).ToArray();
		return new PageBenchmarkReportGenerator.CommitInfo(fields[0], fields[1], fields[2], fields[3]);
	}
}

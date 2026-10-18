using Fallout.Common.CI.GitHubActions;
using Fallout.Common.Tools.SonarScanner;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Build;

public static class BuildExtensions
{
	public static SonarScannerBeginSettings SetPullRequestOrBranchName(
		this SonarScannerBeginSettings settings,
		GitHubActions? gitHubActions,
		string? branchName)
	{
		if (gitHubActions?.IsPullRequest == true)
		{
			Log.Information("Use pull request analysis");
			return settings
				.SetPullRequestKey(gitHubActions.PullRequestNumber.ToString())
				.SetPullRequestBranch(gitHubActions.Ref)
				.SetPullRequestBase(gitHubActions.BaseRef);
		}

		if (gitHubActions?.Ref.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase) == true)
		{
			string version = gitHubActions.Ref.Substring("refs/tags/".Length);
			branchName = "release/" + version;
			Log.Information("Use release branch analysis for '{BranchName}'", branchName);
			return settings.SetBranchName(branchName);
		}

		Log.Information("Use branch analysis for '{BranchName}'", branchName);
		return settings.SetBranchName(branchName);
	}

	/// <summary>
	///     Resolves the pull request that triggered the workflow run under analysis, or <see langword="null" />
	///     when there is none.
	/// </summary>
	/// <remarks>
	///     The artifacts of that run are untrusted, because the pull request controls the code that produces them, so
	///     the pull request is identified by the <c>workflow_run</c> event alone. The event lists no pull requests for
	///     a fork, which is why the number is looked up by the head branch and verified against the head commit.
	///     <para />
	///     A pull request that was merged or closed while its run was analysed is still found. When the same commit
	///     of the same branch belongs to several pull requests, the open one is taken.
	/// </remarks>
	public static async Task<int?> ResolvePullRequestOfWorkflowRun(string? githubToken)
	{
		string? workflowRunEvent = Environment.GetEnvironmentVariable("WorkflowRunEvent");
		string? headOwner = Environment.GetEnvironmentVariable("WorkflowRunHeadOwner");
		string? headBranch = Environment.GetEnvironmentVariable("WorkflowRunHeadBranch");
		string? headSha = Environment.GetEnvironmentVariable("WorkflowRunHeadSha");
		if (workflowRunEvent != "pull_request" ||
		    string.IsNullOrEmpty(headOwner) || string.IsNullOrEmpty(headBranch) || string.IsNullOrEmpty(headSha))
		{
			Log.Information("The workflow run was not triggered by a pull request");
			return null;
		}

		GitHubClient gitHubClient = new(new ProductHeaderValue("Fallout"))
		{
			Credentials = new Credentials(githubToken),
		};
		IReadOnlyList<PullRequest> pullRequests = await gitHubClient.PullRequest.GetAllForRepository(
			"Testably", "Testably.Abstractions",
			new PullRequestRequest
			{
				State = ItemStateFilter.All,
				Head = $"{headOwner}:{headBranch}",
			});
		// GitHub ignores a head filter it cannot interpret and then lists every pull request.
		PullRequest[] matches = pullRequests
			.Where(pullRequest => pullRequest.Head.Sha == headSha &&
			                      pullRequest.Head.Ref == headBranch &&
			                      string.Equals(pullRequest.Head.User?.Login, headOwner,
				                      StringComparison.OrdinalIgnoreCase))
			.ToArray();
		if (matches.Length > 1)
		{
			matches = matches.Where(pullRequest => pullRequest.State.Value == ItemState.Open).ToArray();
		}

		int[] numbers = matches.Select(pullRequest => pullRequest.Number).ToArray();
		if (numbers.Length != 1)
		{
			Log.Information("Found {Count} pull requests for the commit {Sha} of the workflow run",
				numbers.Length, headSha);
			return null;
		}

		Log.Information("The workflow run was triggered by the pull request #{PullRequest}", numbers[0]);
		return numbers[0];
	}

	/// <summary>
	///     Whether the <paramref name="comment" /> is the one that the pipeline wrote under the
	///     <paramref name="heading" />.
	/// </summary>
	/// <remarks>
	///     Anyone can write a comment that contains the heading, so the author has to match too.
	/// </remarks>
	public static bool IsPipelineComment(this IssueComment comment, string heading)
		=> comment.User?.Login == "github-actions[bot]" &&
		   comment.Body?.StartsWith(heading, StringComparison.Ordinal) == true;

	/// <summary>
	///     Prepares the <paramref name="markdown" /> of an artifact for a comment of the pipeline.
	/// </summary>
	/// <remarks>
	///     The artifact is untrusted, so the markdown is bound to what a comment can hold, and a zero-width space
	///     after every <c>@</c> keeps it from notifying users and teams.
	/// </remarks>
	public static string AsUntrustedCommentContent(this string markdown)
	{
		const int maxLength = 60000;
		markdown = markdown.Replace("@", "@​");
		if (markdown.Length > maxLength)
		{
			markdown = markdown.Substring(0, maxLength) + Environment.NewLine + Environment.NewLine +
			           "_The report was truncated._";
		}

		return markdown;
	}

	public static async Task DownloadArtifactTo(this string artifactName, string artifactsDirectory,
		string? githubToken)
	{
		string? runId = Environment.GetEnvironmentVariable("WorkflowRunId");
		if (string.IsNullOrEmpty(runId))
		{
			Log.Information("Skip downloading artifacts, because no 'WorkflowRunId' environment variable is set.");
			return;
		}

		using HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Testably.Abstractions");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
		HttpResponseMessage response = await client.GetAsync(
			$"https://api.github.com/repos/Testably/Testably.Abstractions/actions/runs/{runId}/artifacts");

		string responseContent = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
			throw new InvalidOperationException(
				$"Could not find artifacts for run #{runId}': {responseContent}");
		}

		try
		{
			JsonDocument jsonDocument = JsonDocument.Parse(responseContent);
			foreach (JsonElement artifact in jsonDocument.RootElement.GetProperty("artifacts").EnumerateArray())
			{
				string name = artifact.GetProperty("name").GetString()!;
				if (name.Equals(artifactName, StringComparison.OrdinalIgnoreCase))
				{
					long artifactId = artifact.GetProperty("id").GetInt64();
					HttpResponseMessage fileResponse = await client.GetAsync(
						$"https://api.github.com/repos/Testably/Testably.Abstractions/actions/artifacts/{artifactId}/zip");
					if (fileResponse.IsSuccessStatusCode)
					{
						using ZipArchive archive = new(await fileResponse.Content.ReadAsStreamAsync());
						// The artifact can be untrusted, and a link in it could point a later read at any file of
						// the runner. The upper half of the attributes is the Unix mode, where 0xA000 marks a link.
						if (archive.Entries.Any(entry => ((entry.ExternalAttributes >> 16) & 0xF000) == 0xA000))
						{
							throw new InvalidOperationException(
								$"The artifact #{artifactId} contains a symbolic link");
						}

						archive.ExtractToDirectory(artifactsDirectory, true);
						Log.Information(
							$"Extracted artifact #{artifactId} with {archive.Entries.Count} entries to {artifactsDirectory}:\n - {string.Join("\n - ", archive.Entries.Select(entry => $"{entry.Name} ({entry.Length})"))}");
					}
					else
					{
						string fileResponseContent = await fileResponse.Content.ReadAsStringAsync();
						throw new InvalidOperationException(
							$"Could not download the artifacts with id #{artifactId}': {fileResponseContent}");
					}
				}
			}
		}
		catch (JsonException e)
		{
			Log.Error($"Could not parse JSON: {e.Message}\n{responseContent}");
		}
	}
}

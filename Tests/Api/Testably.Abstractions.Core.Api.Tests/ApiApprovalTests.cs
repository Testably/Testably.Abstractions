using aweXpect;
using aweXpect.Core;
using aweXpect.Results;
using aweXpect.Snapshots;
using aweXpect.Snapshots.PublicApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable ExplicitCallerInfoArgument

namespace Testably.Abstractions.Core.Api.Tests;

/// <summary>
///     Whenever a test fails, this means that the public API surface changed.
///     If the change was intentional, execute <see cref="AcceptApiChanges" />, rename the pending snapshot next to this
///     file or run the tests with the environment variable <c>AWEXPECT_ACCEPT_SNAPSHOTS</c> set to <c>all</c> to take
///     over the current public API surface. The changes will become part of the pull request and will be reviewed
///     accordingly.
/// </summary>
public sealed class ApiApprovalTests
{
	private static readonly string[] AssemblyNames =
	[
		"Testably.Abstractions.FileSystem.Interface",
		"Testably.Abstractions.Interface",
	];

	[Test]
	[MethodDataSource(nameof(Builds))]
	public async Task VerifyPublicApi(string assemblyName, string framework)
		=> await MatchesPublicApiSnapshot(assemblyName, framework);

	/// <summary>
	///     Execute this test to update the expected public API to the current API surface.
	/// </summary>
	[Test]
	[Explicit]
	public async Task AcceptApiChanges()
	{
		foreach ((string assemblyName, string framework) in Builds())
		{
			try
			{
				await MatchesPublicApiSnapshot(assemblyName, framework).Accepting(SnapshotAcceptance.All);
			}
			catch (Exception exception) when (exception.Message.Contains("Accepted snapshot"))
			{
				// An accepted snapshot still fails the expectation.
			}
		}
	}

	public static IEnumerable<(string, string)> Builds()
		=> AssemblyNames.SelectMany(assemblyName => Helper.GetTargetFrameworks()
			.Select(framework => (assemblyName, framework)));

	/// <summary>
	///     The snapshot is named after the assembly and the target framework, so that both tests use the same one.
	/// </summary>
	private static SnapshotResult<string?, IThat<string?>> MatchesPublicApiSnapshot(string assemblyName,
		string framework)
		=> Expect.That(Helper.GetAssemblyFile(framework, assemblyName))
			.MatchesPublicApiSnapshot(callerMemberName: assemblyName).Named(framework);
}

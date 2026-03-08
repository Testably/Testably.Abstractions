using aweXpect;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Testably.Abstractions.Core.Api.Tests;

/// <summary>
///     Whenever a test fails, this means that the public API surface changed.
///     If the change was intentional, execute the <see cref="ApiAcceptance.AcceptApiChanges()" /> test to take over the
///     current public API surface. The changes will become part of the pull request and will be reviewed accordingly.
/// </summary>
public sealed class ApiApprovalTests
{
	public static IEnumerable<string> TargetFrameworks()
		=> Helper.GetTargetFrameworks();

	[Test]
	[MethodDataSource(nameof(TargetFrameworks))]
	public async Task VerifyPublicApiForTestablyAbstractionsFileSystemInterface(string framework)
	{
		const string assemblyName = "Testably.Abstractions.FileSystem.Interface";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await Expect.That(publicApi).IsEqualTo(expectedApi);
	}

	[Test]
	[MethodDataSource(nameof(TargetFrameworks))]
	public async Task VerifyPublicApiForTestablyAbstractionsInterface(string framework)
	{
		const string assemblyName = "Testably.Abstractions.Interface";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await Expect.That(publicApi).IsEqualTo(expectedApi);
	}
}

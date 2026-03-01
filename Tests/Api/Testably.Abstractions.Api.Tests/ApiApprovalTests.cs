using aweXpect;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Testably.Abstractions.Api.Tests;

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
	public async Task VerifyPublicApiForTestablyAbstractions(string framework)
	{
		const string assemblyName = "Testably.Abstractions";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await Expect.That(publicApi).IsEqualTo(expectedApi);
	}

	[Test]
	[MethodDataSource(nameof(TargetFrameworks))]
	public async Task VerifyPublicApiForTestablyAbstractionsAccessControl(string framework)
	{
		const string assemblyName = "Testably.Abstractions.AccessControl";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await Expect.That(publicApi).IsEqualTo(expectedApi);
	}

	[Test]
	[MethodDataSource(nameof(TargetFrameworks))]
	public async Task VerifyPublicApiForTestablyAbstractionsCompression(string framework)
	{
		const string assemblyName = "Testably.Abstractions.Compression";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await Expect.That(publicApi).IsEqualTo(expectedApi);
	}

	[Test]
	[MethodDataSource(nameof(TargetFrameworks))]
	public async Task VerifyPublicApiForTestablyAbstractionsTesting(string framework)
	{
		const string assemblyName = "Testably.Abstractions.Testing";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await Expect.That(publicApi).IsEqualTo(expectedApi);
	}
}

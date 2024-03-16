using NUnit.Framework;
using System.Collections;

namespace Testably.Abstractions.Api.Tests;

/// <summary>
///     Whenever a test fails, this means that the public API surface changed.
///     If the change was intentional, execute the <see cref="ApiAcceptance.AcceptApiChanges()" /> test to take over the
///     current public API surface. The changes will become part of the pull request and will be reviewed accordingly.
/// </summary>
public sealed class ApiApprovalTests
{
	[TestCaseSource(typeof(TargetFrameworksTheoryData))]
	public void VerifyPublicApiForTestablyAbstractions(string framework)
	{
		const string assemblyName = "Testably.Abstractions";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		Assert.That(publicApi, Is.EqualTo(expectedApi));
	}

	[TestCaseSource(typeof(TargetFrameworksTheoryData))]
	public void VerifyPublicApiForTestablyAbstractionsAccessControl(string framework)
	{
		const string assemblyName = "Testably.Abstractions.AccessControl";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		Assert.That(publicApi, Is.EqualTo(expectedApi));
	}

	[TestCaseSource(typeof(TargetFrameworksTheoryData))]
	public void VerifyPublicApiForTestablyAbstractionsCompression(string framework)
	{
		const string assemblyName = "Testably.Abstractions.Compression";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		Assert.That(publicApi, Is.EqualTo(expectedApi));
	}

	[TestCaseSource(typeof(TargetFrameworksTheoryData))]
	public void VerifyPublicApiForTestablyAbstractionsInterface(string framework)
	{
		const string assemblyName = "Testably.Abstractions.Interface";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		Assert.That(publicApi, Is.EqualTo(expectedApi));
	}

	[TestCaseSource(typeof(TargetFrameworksTheoryData))]
	public void VerifyPublicApiForTestablyAbstractionsTesting(string framework)
	{
		const string assemblyName = "Testably.Abstractions.Testing";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		Assert.That(publicApi, Is.EqualTo(expectedApi));
	}

	private sealed class TargetFrameworksTheoryData : IEnumerable
	{
		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			foreach (string targetFramework in Helper.GetTargetFrameworks())
			{
				yield return new object[]
				{
					targetFramework
				};
			}
		}

		#endregion
	}
}

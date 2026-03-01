using aweXpect;
using TUnit;
using System;
using System.Threading.Tasks;

namespace Testably.Abstractions.Api.Tests;

public sealed class ApiAcceptance
{
	/// <summary>
	///     Execute this test to update the expected public API to the current API surface.
	/// </summary>
	[Test]
	[Explicit]
	public async Task AcceptApiChanges()
	{
		string[] assemblyNames =
		[
			"Testably.Abstractions.AccessControl",
			"Testably.Abstractions.Compression",
			"Testably.Abstractions.Testing",
			"Testably.Abstractions",
		];

		foreach (string assemblyName in assemblyNames)
		{
			foreach (string framework in Helper.GetTargetFrameworks())
			{
				string publicApi = Helper.CreatePublicApi(framework, assemblyName)
					.Replace("\n", Environment.NewLine);
				Helper.SetExpectedApi(framework, assemblyName, publicApi);
			}
		}

		await Expect.That(assemblyNames).IsNotEmpty();
	}
}

using NUnit.Framework;

namespace Testably.Abstractions.Api.Tests;

public sealed class ApiAcceptance
{
	[TestCase]
	[Explicit]
	public void AcceptApiChanges()
	{
		string[] assemblyNames = new[]
		{
			"Testably.Abstractions",
			"Testably.Abstractions.AccessControl",
			"Testably.Abstractions.Compression",
			"Testably.Abstractions.Interface",
			"Testably.Abstractions.Testing"
		};
		foreach (string assemblyName in assemblyNames)
		{
			foreach (string framework in Helper.GetTargetFrameworks())
			{
				string publicApi = Helper.CreatePublicApi(framework, assemblyName);
				Helper.SetExpectedApi(framework, assemblyName, publicApi);
			}
		}
	}
}

using System.Reflection;
using Testably.Abstractions.Testing.Helpers;
using Assembly = System.Reflection.Assembly;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class AssemblyNameExtensionsTests
{
	[Test]
	public async Task GetNameOrDefault_ExecutingAssembly_ShouldReturnCorrectString()
	{
		AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();

		string result = assemblyName.GetNameOrDefault();

		await That(result).IsEqualTo("Testably.Abstractions.Testing.Tests");
	}

	[Test]
	public async Task GetNameOrDefault_NullName_ShouldReturnEmptyString()
	{
		AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
		assemblyName.Name = null;

		string result = assemblyName.GetNameOrDefault();

		await That(result).IsEqualTo("");
	}
}

using System.Reflection;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class AssemblyNameExtensionsTests
{
	[Fact]
	public async Task GetNameOrDefault_ExecutingAssembly_ShouldReturnCorrectString()
	{
		AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();

		string result = assemblyName.GetNameOrDefault();

		await That(result).IsEqualTo("Testably.Abstractions.Testing.Tests");
	}

	[Fact]
	public async Task GetNameOrDefault_NullName_ShouldReturnEmptyString()
	{
		AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
		assemblyName.Name = null;

		string result = assemblyName.GetNameOrDefault();

		await That(result).IsEqualTo("");
	}
}

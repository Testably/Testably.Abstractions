using System.Reflection;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class AssemblyNameExtensionsTests
{
	[Fact]
	public void GetNameOrDefault_ExecutingAssembly_ShouldReturnCorrectString()
	{
		AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();

		string result = assemblyName.GetNameOrDefault();

		result.Should().Be("Testably.Abstractions.Testing.Tests");
	}

	[Fact]
	public void GetNameOrDefault_NullName_ShouldReturnEmptyString()
	{
		AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
		assemblyName.Name = null;

		string result = assemblyName.GetNameOrDefault();

		result.Should().Be("");
	}
}

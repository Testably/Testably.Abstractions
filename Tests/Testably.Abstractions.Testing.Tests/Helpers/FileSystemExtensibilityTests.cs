using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class FileSystemExtensibilityTests
{
	[Fact]
	public void ToString_Empty_ShouldBeEmptyArray()
	{
		FileSystemExtensibility extensibility = new();

		string result = extensibility.ToString();

		result.Should().Be("[]");
	}

	[Fact]
	public void ToString_WithMetadata_Should()
	{
		FileSystemExtensibility extensibility = new();
		extensibility.StoreMetadata("foo1", "bar1");
		extensibility.StoreMetadata("foo2", 42);

		string result = extensibility.ToString();

		result.Should().Be("[foo1: bar1, foo2: 42]");
	}
}

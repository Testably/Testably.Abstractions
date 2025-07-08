using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class FileSystemExtensibilityTests
{
	[Fact]
	public async Task ToString_Empty_ShouldBeEmptyArray()
	{
		FileSystemExtensibility extensibility = new();

		string result = extensibility.ToString();

		await That(result).IsEqualTo("[]");
	}

	[Fact]
	public async Task ToString_WithMetadata_Should()
	{
		FileSystemExtensibility extensibility = new();
		extensibility.StoreMetadata("foo1", "bar1");
		extensibility.StoreMetadata("foo2", 42);

		string result = extensibility.ToString();

		await That(result).IsEqualTo("[foo1: bar1, foo2: 42]");
	}
}

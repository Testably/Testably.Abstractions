using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DefaultAccessControlStrategyTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Fact]
	public void Constructor_NullCallback_ShouldThrowArgumentNullException()
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = new DefaultAccessControlStrategy(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("callback");
	}

	[Fact]
	public void IsAccessGranted_ShouldUseCallback()
	{
		DefaultAccessControlStrategy sut = new((p, _) => p.StartsWith('a'));

		sut.IsAccessGranted("abc", new FileSystemExtensibility()).Should().BeTrue();
		sut.IsAccessGranted("xyz", new FileSystemExtensibility()).Should().BeFalse();
	}
}

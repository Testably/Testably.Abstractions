using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DefaultAccessControlStrategyTests
{
	public MockFileSystem FileSystem { get; }

	public DefaultAccessControlStrategyTests()
	{
		FileSystem = new MockFileSystem();
	}

	[SkippableFact]
	public void Constructor_NullCallback_ShouldThrowArgumentNullException()
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = new DefaultAccessControlStrategy(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("callback");
	}

	[SkippableFact]
	public void IsAccessGranted_ShouldUseCallback()
	{
		DefaultAccessControlStrategy sut = new((p, _) => p.StartsWith("a"));

		sut.IsAccessGranted("abc", new FileSystemExtensionContainer()).Should().BeTrue();
		sut.IsAccessGranted("xyz", new FileSystemExtensionContainer()).Should().BeFalse();
	}
}

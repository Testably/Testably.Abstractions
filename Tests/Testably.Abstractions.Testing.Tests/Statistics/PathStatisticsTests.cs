using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class PathStatisticsTests
{
	[Fact]
	public void Key_AbsoluteAndRelativePathsShouldMatch()
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("/foo");
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		IStatistics absolutPath = sut["/foo"];
		IStatistics relativePath = sut["."];

		absolutPath.Should().Be(relativePath);
	}

	[Fact]
	public void Key_NullShouldBeSameAsEmptyKey()
	{
		MockFileSystem fileSystem = new();
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		IStatistics nullKey = sut[null!];
		IStatistics emptyKey = sut[""];

		nullKey.Should().Be(emptyKey);
	}

	[Fact]
	public void Key_ShouldSimplifyRelativePaths()
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("foo/bar");
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		IStatistics absolutPath = sut["/foo"];
		IStatistics relativePath = sut[".."];

		absolutPath.Should().Be(relativePath);
	}

	[Fact]
	public void Key_WithNull_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		Exception? exception = Record.Exception(() => _ = sut[null!]);

		exception.Should().BeNull();
	}
}

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
	public void Key_DifferentDrives_ShouldBeConsideredDifferent()
	{
		MockFileSystem fileSystem = new();
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[@"C:\"];
		IStatistics result2 = sut[@"D:\"];

		result1.Should().NotBe(result2);
	}

	[Fact]
	public void Key_DifferentUncRootPaths_ShouldBeConsideredDifferent()
	{
		MockFileSystem fileSystem = new();
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[@"\\foo1"];
		IStatistics result2 = sut[@"\\foo2"];

		result1.Should().NotBe(result2);
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

	[Theory]
	[InlineData("/")]
	[InlineData("\\")]
	public void Key_WithDrives_ShouldIgnoreTrailingSeparator(string separator)
	{
		const string key = @"C:";
		MockFileSystem fileSystem = new();
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[key];
		IStatistics result2 = sut[key + separator];

		result1.Should().Be(result2);
	}

	[Theory]
	[InlineData("/")]
	[InlineData("\\")]
	public void Key_WithFolderInDrives_ShouldIgnoreTrailingSeparator(string separator)
	{
		const string key = @"C:\foo";
		MockFileSystem fileSystem = new();
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[key];
		IStatistics result2 = sut[key + separator];

		result1.Should().Be(result2);
	}

	[Theory]
	[InlineData("/")]
	[InlineData("\\")]
	public void Key_WithFolderInUncRootPaths_ShouldIgnoreTrailingSeparator(string separator)
	{
		const string key = @"\\server1\foo";
		MockFileSystem fileSystem = new();
		IPathStatistics sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[key];
		IStatistics result2 = sut[key + separator];

		result1.Should().Be(result2);
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

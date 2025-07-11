using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class PathStatisticsTests
{
	[Fact]
	public async Task Key_AbsoluteAndRelativePathsShouldMatch()
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("/foo");
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		IStatistics absolutPath = sut["/foo"];
		IStatistics relativePath = sut["."];

		await That(absolutPath).IsEqualTo(relativePath);
	}

	[Fact]
	public async Task Key_DifferentDrives_ShouldBeConsideredDifferent()
	{
		MockFileSystem fileSystem = new();
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[@"C:\"];
		IStatistics result2 = sut[@"D:\"];

		await That(result1).IsNotEqualTo(result2);
	}

	[Fact]
	public async Task Key_DifferentUncRootPaths_ShouldBeConsideredDifferent()
	{
		MockFileSystem fileSystem = new();
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[@"\\foo1"];
		IStatistics result2 = sut[@"\\foo2"];

		await That(result1).IsNotEqualTo(result2);
	}

	[Fact]
	public async Task Key_NullShouldBeSameAsEmptyKey()
	{
		MockFileSystem fileSystem = new();
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		IStatistics nullKey = sut[null!];
		IStatistics emptyKey = sut[""];

		await That(nullKey).IsEqualTo(emptyKey);
	}

	[Fact]
	public async Task Key_ShouldSimplifyRelativePaths()
	{
		MockFileSystem fileSystem = new(o => o.UseCurrentDirectory());
		fileSystem.InitializeIn("/foo/bar");
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		IStatistics absolutPath = sut["/foo"];
		IStatistics relativePath = sut[".."];

		await That(absolutPath).IsEqualTo(relativePath);
	}

	[Theory]
	[InlineData("/")]
	[InlineData("\\")]
	public async Task Key_WithDrives_ShouldIgnoreTrailingSeparator(string separator)
	{
		const string key = @"C:";
		MockFileSystem fileSystem = new();
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[key];
		IStatistics result2 = sut[key + separator];

		await That(result1).IsEqualTo(result2);
	}

	[Theory]
	[InlineData("/")]
	[InlineData("\\")]
	public async Task Key_WithFolderInDrives_ShouldIgnoreTrailingSeparator(string separator)
	{
		const string key = @"C:\foo";
		MockFileSystem fileSystem = new();
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[key];
		IStatistics result2 = sut[key + separator];

		await That(result1).IsEqualTo(result2);
	}

	[Theory]
	[InlineData("/")]
	[InlineData("\\")]
	public async Task Key_WithFolderInUncRootPaths_ShouldIgnoreTrailingSeparator(string separator)
	{
		const string key = @"\\server1\foo";
		MockFileSystem fileSystem = new();
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		IStatistics result1 = sut[key];
		IStatistics result2 = sut[key + separator];

		await That(result1).IsEqualTo(result2);
	}

	[Fact]
	public async Task Key_WithNull_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IPathStatistics<IFileInfoFactory, IFileInfo> sut = fileSystem.Statistics.FileInfo;

		Exception? exception = Record.Exception(() => _ = sut[null!]);

		await That(exception).IsNull();
	}
}

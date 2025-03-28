namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class PathTests
{
	[Fact]
	public void Path_Empty_ShouldNotThrowException()
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.Path = "";
		});

		exception.Should().BeNull();
	}

	[Fact]
	public void Path_Null_ShouldNotThrowException()
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.Path = null!;
		});

		exception.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void Path_SetToNotExistingPath_ShouldThrowArgumentException(string path)
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.Path = path;
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			messageContains: path);
	}

	[Fact]
	public void Path_Whitespace_ShouldThrowArgumentException()
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.Path = "  ";
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: Test.IsNetFramework ? null : "Path");
	}
}

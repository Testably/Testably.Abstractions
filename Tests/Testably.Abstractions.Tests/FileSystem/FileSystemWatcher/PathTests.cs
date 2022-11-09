using Moq;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class PathTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Path_SetToNotExistingPath_ShouldThrowArgumentException(string path)
	{
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			fileSystemWatcher.Path = path;
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain(path);
	}

	[SkippableFact]
	public void Path_Null_ShouldNotThrowException()
	{
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			fileSystemWatcher.Path = null!;
		});

		exception.Should().BeNull();
	}

	[SkippableFact]
	public void Path_Empty_ShouldNotThrowException()
	{
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			fileSystemWatcher.Path = "";
		});

		exception.Should().BeNull();
	}

	[SkippableFact]
	public void Path_Whitespace_ShouldThrowArgumentException()
	{
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			fileSystemWatcher.Path = "  ";
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("Path");
	}
}
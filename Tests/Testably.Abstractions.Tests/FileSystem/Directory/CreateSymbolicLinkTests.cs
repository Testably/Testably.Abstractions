#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateSymbolicLinkTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);

		FileSystem.DirectoryInfo.New(path).Attributes
			.HasFlag(FileAttributes.ReparsePoint)
			.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_SourceDirectoryAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(-2147024713);
		}
		else
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(17);
		}

		exception.Should().BeOfType<IOException>()
			.Which.Message.Should().Contain($"'{path}'");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_TargetDirectoryMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithEmptyPath_ShouldThrowArgumentException(
		string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(string.Empty, pathToTarget);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.ParamName.Should().Be("path");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithEmptyTarget_ShouldThrowArgumentException(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, string.Empty);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.ParamName.Should().Be("pathToTarget");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithIllegalCharactersInPath_ShouldThrowIOException(
		string pathToTarget)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(pathToTarget);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink("bar_?_", pathToTarget);
		});

		exception.Should().BeOfType<IOException>()
			.Which.HResult.Should().Be(-2147024773);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithIllegalCharactersInTarget_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, "bar_?_");
		});

		exception.Should().BeOfType<IOException>()
			.Which.HResult.Should().Be(-2147024713);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithIllegalPath_ShouldThrowArgumentExceptioncOnWindows(
		string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(" ", pathToTarget);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<ArgumentException>()
				.Which.ParamName.Should().Be("path");
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithIllegalTarget_ShouldNotThrowException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, " ");
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithNullPath_ShouldThrowArgumentNullException(
		string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(null!, pathToTarget);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("path");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithNullTarget_ShouldThrowArgumentNullException(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("pathToTarget");
	}
}
#endif
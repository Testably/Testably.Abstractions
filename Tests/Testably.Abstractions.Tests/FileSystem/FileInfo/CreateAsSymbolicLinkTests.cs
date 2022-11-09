#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateAsSymbolicLinkTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);

		FileSystem.FileInfo.New(path).CreateAsSymbolicLink(pathToTarget);

		FileSystem.File.GetAttributes(path)
		   .HasFlag(FileAttributes.ReparsePoint)
		   .Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_SourceFileAlreadyExists_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(pathToTarget);
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
	public void CreateAsSymbolicLink_TargetFileMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithEmptyPath_ShouldThrowArgumentException(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(string.Empty).CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("path");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithEmptyTarget_ShouldThrowArgumentException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(string.Empty);
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("pathToTarget");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithIllegalCharactersInPath_ShouldThrowIOException(
		string pathToTarget)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New("bar_?_").CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeOfType<IOException>()
		   .Which.HResult.Should().Be(-2147024773);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithIllegalCharactersInTarget_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink("bar_?_");
		});

		exception.Should().BeOfType<IOException>()
		   .Which.HResult.Should().Be(-2147024713);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithIllegalPath_ShouldThrowArgumentExceptionOnWindows(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(" ").CreateAsSymbolicLink(pathToTarget);
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
	public void CreateAsSymbolicLink_WithIllegalTarget_ShouldNotThrowException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(" ");
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithNullPath_ShouldThrowArgumentNullException(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(null!).CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("fileName");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithNullTarget_ShouldThrowArgumentNullException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("pathToTarget");
	}
}
#endif
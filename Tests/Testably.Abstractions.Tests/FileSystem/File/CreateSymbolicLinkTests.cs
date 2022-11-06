#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

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
		FileSystem.File.WriteAllText(pathToTarget, null);

		FileSystem.File.CreateSymbolicLink(path, pathToTarget);

		FileSystem.File.GetAttributes(path)
		   .HasFlag(FileAttributes.ReparsePoint)
		   .Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_SourceFileAlreadyExists_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(path, pathToTarget);
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
	public void CreateSymbolicLink_TargetFileMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(path, pathToTarget);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithEmptyPath_ShouldThrowArgumentException(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(string.Empty, pathToTarget);
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("path");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithEmptyTarget_ShouldThrowArgumentException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(path, string.Empty);
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

		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink("bar_?_", pathToTarget);
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

		FileSystem.File.WriteAllText(path, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(path, "bar_?_");
		});

		exception.Should().BeOfType<IOException>()
		   .Which.HResult.Should().Be(-2147024713);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithIllegalPath_ShouldThrowArgumentException(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(" ", pathToTarget);
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("path");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithIllegalTarget_ShouldNotThrowException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(path, " ");
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithNullPath_ShouldThrowArgumentNullException(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(null!, pathToTarget);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("path");
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_WithNullTarget_ShouldThrowArgumentNullException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(path, null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("pathToTarget");
	}
}
#endif
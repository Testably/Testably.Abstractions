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
	public void CreateAsSymbolicLink_SourceFileAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeException<IOException>($"'{path}'",
			hResult: Test.RunsOnWindows ? -2147024713 : 17);
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

		exception.Should().BeException<ArgumentException>(paramName: "path");
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

		exception.Should().BeException<ArgumentException>(paramName: "pathToTarget");
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

		exception.Should().BeException<IOException>(hResult: -2147024773);
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

		exception.Should().BeException<IOException>(hResult: -2147024713);
	}

	[SkippableTheory]
	[AutoData]
	public void
		CreateAsSymbolicLink_WithIllegalPath_ShouldThrowArgumentException_OnWindows(
			string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(" ").CreateAsSymbolicLink(pathToTarget);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<ArgumentException>(paramName: "path");
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithIllegalTarget_ShouldNotThrowException(
		string path)
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

#if NET7_0
		// https://github.com/dotnet/runtime/issues/78224
		exception.Should().BeException<ArgumentNullException>();
#else
		exception.Should().BeException<ArgumentNullException>(paramName: "fileName");
#endif
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

		exception.Should().BeException<ArgumentNullException>(paramName: "pathToTarget");
	}
}
#endif
#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class CreateAsSymbolicLinkTests
{
	[Theory]
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

	[Theory]
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

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_TargetFileMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		});

		await That(exception).IsNull();
	}

	[Theory]
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

	[Theory]
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

	[Theory]
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

	[Theory]
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

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalPath_ShouldThrowArgumentException_OnWindows(
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
			await That(exception).IsNull();
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalTarget_ShouldNotThrowException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(" ");
		});

		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public void CreateAsSymbolicLink_WithNullPath_ShouldThrowArgumentNullException(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(null!).CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeException<ArgumentNullException>(paramName: "fileName");
	}

	[Theory]
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

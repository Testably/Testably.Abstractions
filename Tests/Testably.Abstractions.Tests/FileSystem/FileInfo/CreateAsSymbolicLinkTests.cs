#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class CreateAsSymbolicLinkTests
{
	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);

		FileSystem.FileInfo.New(path).CreateAsSymbolicLink(pathToTarget);

		await That(FileSystem.File.GetAttributes(path))
			.HasFlag(FileAttributes.ReparsePoint);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_SourceFileAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");

		void Act()
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessageContaining($"'{path}'").And
			.WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_TargetFileMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		void Act()
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithEmptyPath_ShouldThrowArgumentException(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		void Act()
		{
			FileSystem.FileInfo.New(string.Empty).CreateAsSymbolicLink(pathToTarget);
		}

		await That(Act).Throws<ArgumentException>().WithParamName("path");
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithEmptyTarget_ShouldThrowArgumentException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		void Act()
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(string.Empty);
		}

		await That(Act).Throws<ArgumentException>().WithParamName("pathToTarget");
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalCharactersInPath_ShouldThrowIOException(
		string pathToTarget)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(pathToTarget, "some content");

		void Act()
		{
			FileSystem.FileInfo.New("bar_?_").CreateAsSymbolicLink(pathToTarget);
		}

		await That(Act).Throws<IOException>().WithHResult(-2147024773);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalCharactersInTarget_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "some content");

		void Act()
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink("bar_?_");
		}

		await That(Act).Throws<IOException>().WithHResult(-2147024713);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalPath_ShouldThrowArgumentException_OnWindows(
			string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		void Act()
		{
			FileSystem.FileInfo.New(" ").CreateAsSymbolicLink(pathToTarget);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<ArgumentException>().WithParamName("path");
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalTarget_ShouldNotThrowException(
		string path)
	{
		void Act()
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(" ");
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithNullPath_ShouldThrowArgumentNullException(
		string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "some content");

		void Act()
		{
			FileSystem.FileInfo.New(null!).CreateAsSymbolicLink(pathToTarget);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("fileName");
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithNullTarget_ShouldThrowArgumentNullException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		void Act()
		{
			FileSystem.FileInfo.New(path).CreateAsSymbolicLink(null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("pathToTarget");
	}
}
#endif

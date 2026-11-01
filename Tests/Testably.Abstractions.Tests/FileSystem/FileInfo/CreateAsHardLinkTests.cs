#if FEATURE_FILESYSTEM_HARDLINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class CreateAsHardLinkTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task CreateAsHardLink_EmptyPathToTarget_ShouldThrowArgumentException(
		string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			sut.CreateAsHardLink("");
		}

		await That(Act).Throws<ArgumentException>().WithParamName("pathToTarget");
	}

	[Test]
	[AutoArguments]
	public async Task CreateAsHardLink_NullPathToTarget_ShouldThrowArgumentNullException(
		string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			sut.CreateAsHardLink(null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("pathToTarget");
	}

	[Test]
	[AutoArguments]
	public async Task CreateAsHardLink_PathAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			sut.CreateAsHardLink(pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessage().Containing($"'{path}'").And
			.WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
	}

	[Test]
	[AutoArguments]
	public async Task CreateAsHardLink_ShouldRefreshState(
		string path, string pathToTarget, string contents)
	{
		FileSystem.File.WriteAllText(pathToTarget, contents);
		IFileInfo sut = FileSystem.FileInfo.New(path);
		bool existsBefore = sut.Exists;

		sut.CreateAsHardLink(pathToTarget);

		await That(existsBefore).IsFalse();
		await That(sut.Exists).IsTrue();
		await That(sut.Length).IsEqualTo(FileSystem.FileInfo.New(pathToTarget).Length);
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(contents);
	}
}
#endif

using NSubstitute.ExceptionExtensions;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class MoveTests
{
	[Theory]
	[AutoData]
	public async Task Move_CaseOnlyChange_ShouldMoveFileWithContent(
		string name, string contents)
	{
		string sourceName = name.ToLowerInvariant();
		string destinationName = name.ToUpperInvariant();
		FileSystem.File.WriteAllText(sourceName, contents);

		FileSystem.File.Move(sourceName, destinationName);

		if (Test.RunsOnLinux)
		{
			// sourceName and destinationName are considered different only on Linux
			await That(FileSystem.File.Exists(sourceName)).IsFalse();
		}

		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
		await That(FileSystem.Directory.GetFiles(".")).HasSingle().Matching(d => d.Contains(destinationName, StringComparison.Ordinal));
	}

	[Theory]
	[AutoData]
	public async Task
		Move_DestinationDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException(
			string source)
	{
		FileSystem.Initialize()
			.WithFile(source);
		string destination = FileTestHelper.RootDrive(Test, "not-existing/path");

		void Act()
		{
			FileSystem.File.Move(source, destination);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Move_DestinationExists_ShouldThrowIOException_AndNotMoveFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		void Act()
		{
			FileSystem.File.Move(sourceName, destinationName);
		}

		await That(Act).Throws<IOException>().WithHResult(Test.RunsOnWindows ? -2147024713 : 17);

		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[Theory]
	[AutoData]
	public async Task Move_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		FileSystem.File.Move(sourceName, destinationName, true);

		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
	}
#endif

	[Theory]
	[AutoData]
	public async Task Move_ReadOnly_ShouldMoveFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		FileSystem.File.Move(sourceName, destinationName);

		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
		await That(FileSystem.File.GetAttributes(destinationName)).HasFlag(FileAttributes.ReadOnly);
	}

	[Theory]
	[AutoData]
	public async Task Move_ShouldMoveFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);

		FileSystem.File.Move(sourceName, destinationName);

		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task Move_ShouldNotAdjustTimes(string source, string destination)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(source, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.File.Move(source, destination);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(destination);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(destination);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(destination);

		await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

	[Theory]
	[AutoData]
	public async Task Move_SourceAndDestinationIdentical_ShouldNotThrowException(string path)
	{
		FileSystem.Initialize()
			.WithFile(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(path, path);
		});

		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task Move_SourceDirectoryMissing_ShouldThrowFileNotFoundException(
		string missingDirectory,
		string sourceName,
		string destinationName)
	{
		string sourcePath = FileSystem.Path.Combine(missingDirectory, sourceName);
		void Act()
		{
			FileSystem.File.Move(sourcePath, destinationName);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(sourcePath)}'").And
			.WithHResult(-2147024894);
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}

	[Theory]
	[InlineAutoData(FileAccess.Read, FileShare.None)]
	[InlineAutoData(FileAccess.Read, FileShare.Read)]
	[InlineAutoData(FileAccess.Read, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.Read, FileShare.Write)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.None)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.Read)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.Write)]
	[InlineAutoData(FileAccess.Write, FileShare.None)]
	[InlineAutoData(FileAccess.Write, FileShare.Read)]
	[InlineAutoData(FileAccess.Write, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.Write, FileShare.Write)]
	public async Task Move_SourceLocked_ShouldThrowIOException_OnWindows(
		FileAccess fileAccess,
		FileShare fileShare,
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(
			sourceName, FileMode.Open, fileAccess, fileShare);

		void Act()
		{
			FileSystem.File.Move(sourceName, destinationName);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>().WithHResult(-2147024864);
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.Exists(destinationName)).IsFalse();
		}
		else
		{
			// https://github.com/dotnet/runtime/issues/52700
			await That(Act).DoesNotThrow();
			await That(FileSystem.File.Exists(sourceName)).IsFalse();
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
		}
	}

	[Theory]
	[AutoData]
	public async Task Move_SourceMissing_CopyToItself_ShouldThrowFileNotFoundException(
		string sourceName)
	{
		void Act()
		{
			FileSystem.File.Move(sourceName, sourceName);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(sourceName)}'").And
			.WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task Move_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		void Act()
		{
			FileSystem.File.Move(sourceName, destinationName);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(sourceName)}'").And
			.WithHResult(-2147024894);
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}
}

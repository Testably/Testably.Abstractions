using NSubstitute.ExceptionExtensions;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class CopyTests
{
	[Theory]
	[AutoData]
	public async Task Copy_CaseOnlyChange_ShouldThrowIOException_ExceptOnLinux(
		string name, string contents)
	{
		string sourceName = name.ToLowerInvariant();
		string destinationName = name.ToUpperInvariant();
		FileSystem.File.WriteAllText(sourceName, contents);

		void Act()
		{
			FileSystem.File.Copy(sourceName, destinationName);
		}

		if (Test.RunsOnLinux)
		{
			await That(Act).DoesNotThrow();
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
		}
		else
		{
			await That(Act).Throws<IOException>().WithHResult(Test.RunsOnWindows ? -2147024816 : 17);
		}
	}

	[Theory]
	[AutoData]
	public async Task
		Copy_DestinationDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException(
			string source)
	{
		FileSystem.Initialize()
			.WithFile(source);
		string destination = FileTestHelper.RootDrive(Test, "not-existing/path/foo.txt");

		void Act()
		{
			FileSystem.File.Copy(source, destination);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Copy_DestinationExists_ShouldThrowIOException_AndNotCopyFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		void Act()
		{
			FileSystem.File.Copy(sourceName, destinationName);
		}

		await That(Act).Throws<IOException>().WithHResult(Test.RunsOnWindows ? -2147024816 : 17);

		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[Theory]
	[AutoData]
	public async Task Copy_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		FileSystem.File.Copy(sourceName, destinationName, true);

		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
	}
#endif

	[Theory]
	[InlineData(@"0:\something\demo.txt", @"C:\elsewhere\demo.txt")]
	[InlineData(@"C:\something\demo.txt", @"^:\elsewhere\demo.txt")]
	[InlineData(@"C:\something\demo.txt", @"C:\elsewhere:\demo.txt")]
	public async Task
		Copy_InvalidDriveName_ShouldThrowNotSupportedException(
			string source, string destination)
	{
		Skip.IfNot(Test.IsNetFramework);

		void Act()
		{
			FileSystem.File.Copy(source, destination);
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}

	[Theory]
	[InlineData(@"C::\something\demo.txt", @"C:\elsewhere\demo.txt")]
	public async Task
		Copy_InvalidPath_ShouldThrowCorrectException(
			string source, string destination)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			FileSystem.File.Copy(source, destination);
		}

		if (Test.IsNetFramework)
		{
			await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
		}
		else
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining("The filename, directory name, or volume label syntax is incorrect").And
				.WithHResult(-2147024773);
		}
	}

	[Theory]
	[AutoData]
	public async Task Copy_ReadOnly_ShouldCopyFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		FileSystem.File.Copy(sourceName, destinationName);

		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(contents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
		await That(FileSystem.File.GetAttributes(destinationName)).HasFlag(FileAttributes.ReadOnly);
	}

	[Theory]
	[AutoData]
	public async Task Copy_ShouldAdjustTimes(
		string source, string destination)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(source, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.Copy(source, destination);

		DateTime sourceCreationTime = FileSystem.File.GetCreationTimeUtc(source);
		DateTime sourceLastAccessTime = FileSystem.File.GetLastAccessTimeUtc(source);
		DateTime sourceLastWriteTime = FileSystem.File.GetLastWriteTimeUtc(source);
		DateTime destinationCreationTime =
			FileSystem.File.GetCreationTimeUtc(destination);
		DateTime destinationLastAccessTime =
			FileSystem.File.GetLastAccessTimeUtc(destination);
		DateTime destinationLastWriteTime =
			FileSystem.File.GetLastWriteTimeUtc(destination);

		await That(sourceCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		if (Test.RunsOnMac)
		{
#if NET8_0_OR_GREATER
			await That(sourceLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
#else
			await That(sourceLastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
#endif
		}
		else if (Test.RunsOnWindows)
		{
			await That(sourceLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(sourceLastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(sourceLastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		if (Test.RunsOnWindows)
		{
			await That(destinationCreationTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(destinationCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		if (!Test.RunsOnMac)
		{
			await That(destinationLastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(destinationLastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

	[Theory]
	[AutoData]
	public async Task Copy_ShouldCloneBinaryContent(
		string source, string destination, byte[] original)
	{
		FileSystem.File.WriteAllBytes(source, original);

		FileSystem.File.Copy(source, destination);

		using (FileSystemStream stream =
			FileSystem.File.Open(source, FileMode.Open, FileAccess.ReadWrite))
		{
			BinaryWriter binaryWriter = new(stream);

			binaryWriter.Seek(0, SeekOrigin.Begin);
			binaryWriter.Write("Some text");
		}

		await That(FileSystem.File.Exists(destination)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(destination)).IsEqualTo(original);
		await That(FileSystem.File.ReadAllBytes(destination)).IsNotEqualTo(FileSystem.File.ReadAllBytes(source));
	}

	[Theory]
	[AutoData]
	public async Task Copy_ShouldCloneTextContent(
		string source, string destination, string contents)
	{
		FileSystem.File.WriteAllText(source, contents);

		FileSystem.File.Copy(source, destination);

		using (FileSystemStream stream =
			FileSystem.File.Open(source, FileMode.Open, FileAccess.ReadWrite))
		{
			BinaryWriter binaryWriter = new(stream);

			binaryWriter.Seek(0, SeekOrigin.Begin);
			binaryWriter.Write("Some text");
		}

		await That(FileSystem.File.ReadAllText(source)).IsNotEqualTo(FileSystem.File.ReadAllText(destination));
	}

	[Theory]
	[AutoData]
	public async Task Copy_ShouldCopyFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText(sourceName, contents);

		TimeSystem.Thread.Sleep(EnsureTimeout);

		FileSystem.File.Copy(sourceName, destinationName);
		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(contents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
	}

	[Theory]
	[InlineAutoData(FileAccess.Read, FileShare.Read)]
	[InlineAutoData(FileAccess.Read, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.Read)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.Write, FileShare.Read)]
	[InlineAutoData(FileAccess.Write, FileShare.ReadWrite)]
	public async Task Copy_SourceAccessedWithReadShare_ShouldNotThrow(
		FileAccess fileAccess,
		FileShare fileShare,
		string sourcePath,
		string destinationPath,
		string sourceContents)
	{
		FileSystem.Initialize().WithFile(sourcePath)
			.Which(f => f.HasStringContent(sourceContents));
		using (FileSystem.FileStream
			.New(sourcePath, FileMode.Open, fileAccess, fileShare))
		{
			FileSystem.File.Copy(sourcePath, destinationPath);
		}

		await That(FileSystem.File.Exists(destinationPath)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationPath)).IsEqualTo(sourceContents);
	}

	[Theory]
	[InlineAutoData(FileAccess.Read)]
	[InlineAutoData(FileAccess.ReadWrite)]
	[InlineAutoData(FileAccess.Write)]
	public async Task Copy_SourceAccessedWithWriteShare_ShouldNotThrowOnLinuxOrMac(
		FileAccess fileAccess,
		string sourcePath,
		string destinationPath,
		string sourceContents)
	{
		Skip.If(Test.RunsOnWindows, "see https://github.com/dotnet/runtime/issues/52700");

		FileSystem.Initialize().WithFile(sourcePath)
			.Which(f => f.HasStringContent(sourceContents));
		using (FileSystem.FileStream
			.New(sourcePath, FileMode.Open, fileAccess, FileShare.Write))
		{
			FileSystem.File.Copy(sourcePath, destinationPath);
		}

		await That(FileSystem.File.Exists(destinationPath)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationPath)).IsEqualTo(sourceContents);
	}

	[Theory]
	[AutoData]
	public async Task Copy_SourceDirectoryMissing_ShouldThrowDirectoryNotFoundException(
		string missingDirectory,
		string sourceName,
		string destinationName)
	{
		string source = FileSystem.Path.Combine(missingDirectory, sourceName);
		void Act()
		{
			FileSystem.File.Copy(source, destinationName);
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining(Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.GetFullPath(source)}'").And
			.WithHResult(-2147024893);
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Copy_SourceIsDirectory_ShouldThrowUnauthorizedAccessException_AndNotCopyFile(
		string sourceName,
		string destinationName)
	{
		FileSystem.Directory.CreateDirectory(sourceName);

		void Act()
		{
			FileSystem.File.Copy(sourceName, destinationName);
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithMessageContaining(Test.IsNetFramework
				? $"'{sourceName}'"
				: $"'{FileSystem.Path.GetFullPath(sourceName)}'").And
			.WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}

	[Theory]
	[InlineAutoData(FileShare.None)]
	[InlineAutoData(FileShare.Write)]
	public async Task Copy_SourceLocked_ShouldThrowIOException(
		FileShare fileShare,
		string sourceName,
		string destinationName)
	{
		Skip.If(!Test.RunsOnWindows && fileShare == FileShare.Write,
			"see https://github.com/dotnet/runtime/issues/52700");

		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(
			sourceName, FileMode.Open, FileAccess.Read, fileShare);

		void Act()
		{
			FileSystem.File.Copy(sourceName, destinationName);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>().WithHResult(-2147024864);
			await That(FileSystem.File.Exists(destinationName)).IsFalse();
		}
		else
		{
			await That(Act).ThrowsException();
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.Exists(destinationName)).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task Copy_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		void Act()
		{
			FileSystem.File.Copy(sourceName, destinationName);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining(Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.GetFullPath(sourceName)}'").And
			.WithHResult(-2147024894);
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}
}

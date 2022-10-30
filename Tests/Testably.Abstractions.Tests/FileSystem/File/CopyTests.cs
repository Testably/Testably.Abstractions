using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CopyTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
		Copy_DestinationDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException(
			string source)
	{
		FileSystem.Initialize()
		   .WithFile(source);
		string destination = FileTestHelper.RootDrive("not-existing/path/foo.txt");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Copy(source, destination);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>();
	}

	[SkippableTheory]
	[AutoData]
	public void Copy_DestinationExists_ShouldThrowIOExceptionAndNotCopyFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Copy(sourceName, destinationName);
		});

		exception.Should().BeOfType<IOException>();
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableTheory]
	[AutoData]
	public void Copy_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		FileSystem.File.Copy(sourceName, destinationName, true);

		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void Copy_ReadOnly_ShouldCopyFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		FileSystem.File.Copy(sourceName, destinationName);

		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.GetAttributes(destinationName)
		   .Should().HaveFlag(FileAttributes.ReadOnly);
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void Copy_ShouldAdjustTimes(
		string source, string destination)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

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

		sourceCreationTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		if (Test.RunsOnWindows)
		{
			sourceLastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}
		else
		{
			sourceLastAccessTime.Should()
			   .BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		sourceLastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		if (Test.RunsOnWindows)
		{
			destinationCreationTime.Should()
			   .BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			destinationCreationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		destinationLastAccessTime.Should()
		   .BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		destinationLastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}

	[SkippableTheory]
	[AutoData]
	public void Copy_ShouldCopyFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.File.WriteAllText(sourceName, contents);

		TimeSystem.Thread.Sleep(1000);

		FileSystem.File.Copy(sourceName, destinationName);
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(contents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void Copy_ShouldNotUseAReferenceToFileContent(
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

		FileSystem.File.ReadAllText(source).Should()
		   .NotBe(FileSystem.File.ReadAllText(destination));
	}

	[SkippableTheory]
	[AutoData]
	public void Copy_SourceIsDirectory_ShouldThrowIOExceptionAndNotCopyFile(
		string sourceName,
		string destinationName)
	{
		FileSystem.Directory.CreateDirectory(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Copy(sourceName, destinationName);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<UnauthorizedAccessException>()
		   .Which.Message.Should().Contain($"'{sourceName}'");
#else
		exception.Should().BeOfType<UnauthorizedAccessException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
#endif
		FileSystem.Directory.Exists(sourceName).Should().BeTrue();
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Copy_SourceLocked_ShouldThrowIOException(
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
			FileAccess.Read, FileShare.None);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Copy(sourceName, destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>();
			FileSystem.File.Exists(destinationName).Should().BeFalse();
		}
		else
		{
			FileSystem.File.Exists(sourceName).Should().BeTrue();
			FileSystem.File.Exists(destinationName).Should().BeFalse();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void Copy_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Copy(sourceName, destinationName);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<FileNotFoundException>();
#else
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
#endif
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}
}
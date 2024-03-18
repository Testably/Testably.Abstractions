using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateDirectoryTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_AlreadyExisting_ShouldDoNothing(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		exception.Should().BeNull();
		FileSystem.Should().HaveDirectory(path);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ReadOnlyParent_ShouldStillCreateDirectoryUnderWindows(string parent,
		string subdirectory)
	{
		string subdirectoryPath = FileSystem.Path.Combine(parent, subdirectory);
		FileSystem.Directory.CreateDirectory(parent);
		FileSystem.DirectoryInfo.New(parent).Attributes = FileAttributes.ReadOnly;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(subdirectoryPath);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeNull();
			FileSystem.Should().HaveDirectory(subdirectoryPath);
			FileSystem.DirectoryInfo.New(parent).Attributes
				.Should().HaveFlag(FileAttributes.ReadOnly);
		}
		else
		{
			exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
			FileSystem.Should().NotHaveDirectory(subdirectoryPath);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_FileWithSameNameAlreadyExists_ShouldThrowIOException(string name)
	{
		FileSystem.File.WriteAllText(name, "");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(name);
		});

		exception.Should().BeException<IOException>(
			hResult: Test.RunsOnWindows ? -2147024713 : 17);
		FileSystem.Should().NotHaveDirectory(name);
	}

	[SkippableFact]
	public void CreateDirectory_Root_ShouldNotThrowException()
	{
		string path = FileTestHelper.RootDrive(Test);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		exception.Should().BeNull();
		FileSystem.Should().HaveDirectory(path);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ShouldTrimTrailingSpaces_OnWindows(string path)
	{
		string pathWithSpaces = path + "  ";

		IDirectoryInfo result = FileSystem.Directory.CreateDirectory(pathWithSpaces);

		if (Test.RunsOnWindows)
		{
			result.Name.Should().Be(path);
		}
		else
		{
			result.Name.Should().Be(pathWithSpaces);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ShouldAdjustTimes(string path, string subdirectoryName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectoryName);
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Directory.CreateDirectory(path);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.CreateDirectory(subdirectoryPath);

		DateTime creationTime = FileSystem.Directory.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.Directory.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.Directory.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
				.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
				.BeOnOrBefore(creationTimeEnd);
			lastAccessTime.Should()
				.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
				.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
				.BeOnOrBefore(creationTimeEnd);
		}

		lastWriteTime.Should()
			.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ShouldAdjustTimesOnlyForDirectParentDirectory(
		string rootPath)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		string subdirectoryLevel1Path =
			FileSystem.Path.Combine(rootPath, "lvl1");
		string subdirectoryLevel2Path =
			FileSystem.Path.Combine(subdirectoryLevel1Path, "lvl2");
		string subdirectoryLevel3Path =
			FileSystem.Path.Combine(subdirectoryLevel2Path, "lvl3");
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Directory.CreateDirectory(subdirectoryLevel2Path);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.Directory.CreateDirectory(subdirectoryLevel3Path);

		foreach (string path in new[]
			{
				rootPath, subdirectoryLevel1Path
			})
		{
			DateTime lastAccessTime =
				FileSystem.Directory.GetLastAccessTimeUtc(path);
			DateTime lastWriteTime =
				FileSystem.Directory.GetLastWriteTimeUtc(path);

			lastAccessTime.Should()
				.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
				.BeOnOrBefore(creationTimeEnd);
			lastWriteTime.Should()
				.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
				.BeOnOrBefore(creationTimeEnd);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ShouldSetCreationTime(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetCreationTime(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
		result.Kind.Should().Be(DateTimeKind.Local);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ShouldSetCreationTimeUtc(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetCreationTimeUtc(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
		result.Kind.Should().Be(DateTimeKind.Utc);
	}

	[SkippableFact]
	public void CreateDirectory_NullCharacter_ShouldThrowArgumentException()
	{
		string path = "foo\0bar";
		Exception? exception =
			Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[SkippableFact]
	public void CreateDirectory_ShouldCreateDirectoryInBasePath()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateDirectory("foo");

		FileSystem.Should().HaveDirectory("foo");
		result.FullName.Should().StartWith(BasePath);
	}

	[SkippableFact]
	public void CreateDirectory_ShouldCreateParentDirectories()
	{
		string directoryLevel1 = "lvl1";
		string directoryLevel2 = "lvl2";
		string directoryLevel3 = "lvl3";
		string path =
			FileSystem.Path.Combine(directoryLevel1, directoryLevel2, directoryLevel3);

		IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

		result.Name.Should().Be(directoryLevel3);
		result.Parent!.Name.Should().Be(directoryLevel2);
		result.Parent.Parent!.Name.Should().Be(directoryLevel1);
		result.Should().Exist();
		result.Parent.Should().Exist();
		result.Parent.Parent.Should().Exist();
	}

#if NETFRAMEWORK
	[SkippableTheory]
	[InlineData("/")]
	[InlineData("\\")]
	public void CreateDirectory_TrailingDirectorySeparator_ShouldNotBeTrimmed(
		string suffix)
	{
		string nameWithSuffix = "foobar" + suffix;
		string expectedName = nameWithSuffix;
		expectedName = expectedName.TrimEnd(' ');

		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(nameWithSuffix);

		result.Name.Should().Be(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		result.FullName.Should().Be(System.IO.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		FileSystem.Should().HaveDirectory(nameWithSuffix);
	}
#endif

#if NETFRAMEWORK
	[SkippableTheory]
	[InlineData("")]
	[InlineData(" ")]
	public void CreateDirectory_EmptyOrWhitespace_ShouldReturnEmptyString(
		string suffix)
	{
		string nameWithSuffix = "foobar" + suffix;
		string expectedName = nameWithSuffix;
		expectedName = expectedName.TrimEnd(' ');

		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(nameWithSuffix);

		result.Name.Should().Be(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		result.FullName.Should().Be(System.IO.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		FileSystem.Should().HaveDirectory(nameWithSuffix);
	}
#else
	[SkippableTheory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("/")]
	[InlineData("\\")]
	public void CreateDirectory_TrailingDirectorySeparator_ShouldNotBeTrimmed(
		string suffix)
	{
		string nameWithSuffix = "foobar" + suffix;
		string expectedName = nameWithSuffix;
		if (Test.RunsOnWindows)
		{
			expectedName = expectedName.TrimEnd(' ');
		}
		else if (suffix == "\\")
		{
			//This case is only supported on Windows
			return;
		}

		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(nameWithSuffix);

		result.Name.Should().Be(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		result.FullName.Should().Be(
			$"{BasePath}{FileSystem.Path.DirectorySeparatorChar}{expectedName}"
				.Replace(FileSystem.Path.AltDirectorySeparatorChar,
					FileSystem.Path.DirectorySeparatorChar));
		FileSystem.Should().HaveDirectory(nameWithSuffix);
	}
#endif
}

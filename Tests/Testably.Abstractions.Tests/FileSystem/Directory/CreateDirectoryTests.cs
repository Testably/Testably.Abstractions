using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class CreateDirectoryTests
{
	[Theory]
	[AutoData]
	public void CreateDirectory_AlreadyExisting_ShouldDoNothing(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		exception.Should().BeNull();
		FileSystem.Directory.Exists(path).Should().BeTrue();
	}

	[Theory]
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
			FileSystem.Directory.Exists(subdirectoryPath).Should().BeTrue();
			FileSystem.DirectoryInfo.New(parent).Attributes
				.Should().HaveFlag(FileAttributes.ReadOnly);
		}
		else
		{
			exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
			FileSystem.Directory.Exists(subdirectoryPath).Should().BeFalse();
		}
	}

	[Fact]
	public async Task CreateDirectory_ShouldSupportExtendedLengthPaths()
	{
		Skip.If(!Test.RunsOnWindows);

		FileSystem.DirectoryInfo.New(@"\\?\c:\bar").Create();

		await That(FileSystem.Directory.Exists(@"\\?\c:\bar")).IsTrue();
	}

	[Theory]
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
		FileSystem.Directory.Exists(name).Should().BeFalse();
	}

	[Fact]
	public void CreateDirectory_Root_ShouldNotThrowException()
	{
		string path = FileTestHelper.RootDrive(Test);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		exception.Should().BeNull();
		FileSystem.Directory.Exists(path).Should().BeTrue();
	}

	[Theory]
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

	[Theory]
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
				.BeBetween(creationTimeStart, creationTimeEnd);
			lastAccessTime.Should()
				.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}

		lastWriteTime.Should()
			.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
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
				rootPath, subdirectoryLevel1Path,
			})
		{
			DateTime lastAccessTime =
				FileSystem.Directory.GetLastAccessTimeUtc(path);
			DateTime lastWriteTime =
				FileSystem.Directory.GetLastWriteTimeUtc(path);

			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
			lastWriteTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}
	}

	[Theory]
	[AutoData]
	public void CreateDirectory_ShouldSetCreationTime(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.Directory.CreateDirectory(path);

		DateTime end = TimeSystem.DateTime.Now;
		DateTime result = FileSystem.Directory.GetCreationTime(path);
		result.Should().BeBetween(start, end);
		result.Kind.Should().Be(DateTimeKind.Local);
	}

	[Theory]
	[AutoData]
	public void CreateDirectory_ShouldSetCreationTimeUtc(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.CreateDirectory(path);

		DateTime end = TimeSystem.DateTime.UtcNow;
		DateTime result = FileSystem.Directory.GetCreationTimeUtc(path);
		result.Should().BeBetween(start, end);
		result.Kind.Should().Be(DateTimeKind.Utc);
	}

	[Fact]
	public void CreateDirectory_NullCharacter_ShouldThrowArgumentException()
	{
		string path = "foo\0bar";
		Exception? exception =
			Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[Fact]
	public void CreateDirectory_ShouldCreateDirectoryInBasePath()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateDirectory("foo");

		FileSystem.Directory.Exists("foo").Should().BeTrue();
		result.FullName.Should().StartWith(BasePath);
	}

	[Fact]
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
		result.Exists.Should().BeTrue();
		result.Parent.Exists.Should().BeTrue();
		result.Parent.Parent.Exists.Should().BeTrue();
	}

#if NETFRAMEWORK
	[Theory]
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
		FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
	}
#endif

#if NETFRAMEWORK
	[Theory]
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
		FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
	}
#else
	[Theory]
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
		else if (string.Equals(suffix, "\\", StringComparison.Ordinal))
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
		FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
	}
#endif
}

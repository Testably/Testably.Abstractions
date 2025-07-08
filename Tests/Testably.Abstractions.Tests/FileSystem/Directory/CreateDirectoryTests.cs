using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class CreateDirectoryTests
{
	[Theory]
	[AutoData]
	public async Task CreateDirectory_AlreadyExisting_ShouldDoNothing(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		await That(exception).IsNull();
		await That(FileSystem.Directory.Exists(path)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task CreateDirectory_ReadOnlyParent_ShouldStillCreateDirectoryUnderWindows(string parent,
		string subdirectory)
	{
		string subdirectoryPath = FileSystem.Path.Combine(parent, subdirectory);
		FileSystem.Directory.CreateDirectory(parent);
		FileSystem.DirectoryInfo.New(parent).Attributes = FileAttributes.ReadOnly;

		void Act()
		{
			FileSystem.Directory.CreateDirectory(subdirectoryPath);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).DoesNotThrow();
			await That(FileSystem.Directory.Exists(subdirectoryPath)).IsTrue();
			await That(FileSystem.DirectoryInfo.New(parent).Attributes).HasFlag(FileAttributes.ReadOnly);
		}
		else
		{
			await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
			await That(FileSystem.Directory.Exists(subdirectoryPath)).IsFalse();
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
	public async Task CreateDirectory_FileWithSameNameAlreadyExists_ShouldThrowIOException(string name)
	{
		FileSystem.File.WriteAllText(name, "");

		void Act()
		{
			FileSystem.Directory.CreateDirectory(name);
		}

		await That(Act).Throws<IOException>()
			.WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
		await That(FileSystem.Directory.Exists(name)).IsFalse();
	}

	[Fact]
	public async Task CreateDirectory_Root_ShouldNotThrowException()
	{
		string path = FileTestHelper.RootDrive(Test);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		await That(exception).IsNull();
		await That(FileSystem.Directory.Exists(path)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task CreateDirectory_ShouldTrimTrailingSpaces_OnWindows(string path)
	{
		string pathWithSpaces = path + "  ";

		IDirectoryInfo result = FileSystem.Directory.CreateDirectory(pathWithSpaces);

		if (Test.RunsOnWindows)
		{
			await That(result.Name).IsEqualTo(path);
		}
		else
		{
			await That(result.Name).IsEqualTo(pathWithSpaces);
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateDirectory_ShouldAdjustTimes(string path, string subdirectoryName)
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
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public async Task CreateDirectory_ShouldAdjustTimesOnlyForDirectParentDirectory(
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

			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd);
			await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd);
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateDirectory_ShouldSetCreationTime(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.Directory.CreateDirectory(path);

		DateTime end = TimeSystem.DateTime.Now;
		DateTime result = FileSystem.Directory.GetCreationTime(path);
		await That(result).IsBetween(start).And(end);
		await That(result.Kind).IsEqualTo(DateTimeKind.Local);
	}

	[Theory]
	[AutoData]
	public async Task CreateDirectory_ShouldSetCreationTimeUtc(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.CreateDirectory(path);

		DateTime end = TimeSystem.DateTime.UtcNow;
		DateTime result = FileSystem.Directory.GetCreationTimeUtc(path);
		await That(result).IsBetween(start).And(end);
		await That(result.Kind).IsEqualTo(DateTimeKind.Utc);
	}

	[Fact]
	public async Task CreateDirectory_NullCharacter_ShouldThrowArgumentException()
	{
		string path = "foo\0bar";

		void Act() => FileSystem.Directory.CreateDirectory(path);

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Fact]
	public async Task CreateDirectory_ShouldCreateDirectoryInBasePath()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateDirectory("foo");

		await That(FileSystem.Directory.Exists("foo")).IsTrue();
		await That(result.FullName).StartsWith(BasePath);
	}

	[Fact]
	public async Task CreateDirectory_ShouldCreateParentDirectories()
	{
		string directoryLevel1 = "lvl1";
		string directoryLevel2 = "lvl2";
		string directoryLevel3 = "lvl3";
		string path =
			FileSystem.Path.Combine(directoryLevel1, directoryLevel2, directoryLevel3);

		IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

		await That(result.Name).IsEqualTo(directoryLevel3);
		await That(result.Parent!.Name).IsEqualTo(directoryLevel2);
		await That(result.Parent.Parent!.Name).IsEqualTo(directoryLevel1);
		await That(result.Exists).IsTrue();
		await That(result.Parent.Exists).IsTrue();
		await That(result.Parent.Parent.Exists).IsTrue();
	}

#if NETFRAMEWORK
	[Theory]
	[InlineData("/")]
	[InlineData("\\")]
	public async Task CreateDirectory_TrailingDirectorySeparator_ShouldNotBeTrimmed(
		string suffix)
	{
		string nameWithSuffix = "foobar" + suffix;
		string expectedName = nameWithSuffix;
		expectedName = expectedName.TrimEnd(' ');

		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(nameWithSuffix);

		await That(result.Name).IsEqualTo(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		await That(result.FullName).IsEqualTo(System.IO.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		await That(FileSystem.Directory.Exists(nameWithSuffix)).IsTrue();
	}
#endif

#if NETFRAMEWORK
	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public async Task CreateDirectory_EmptyOrWhitespace_ShouldReturnEmptyString(
		string suffix)
	{
		string nameWithSuffix = "foobar" + suffix;
		string expectedName = nameWithSuffix;
		expectedName = expectedName.TrimEnd(' ');

		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(nameWithSuffix);

		await That(result.Name).IsEqualTo(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		await That(result.FullName).IsEqualTo(System.IO.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		await That(FileSystem.Directory.Exists(nameWithSuffix)).IsTrue();
	}
#else
	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("/")]
	[InlineData("\\")]
	public async Task CreateDirectory_TrailingDirectorySeparator_ShouldNotBeTrimmed(
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

		await That(result.Name).IsEqualTo(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		await That(result.FullName).IsEqualTo($"{BasePath}{FileSystem.Path.DirectorySeparatorChar}{expectedName}"
				.Replace(FileSystem.Path.AltDirectorySeparatorChar,
					FileSystem.Path.DirectorySeparatorChar));
		await That(FileSystem.Directory.Exists(nameWithSuffix)).IsTrue();
	}
#endif
}

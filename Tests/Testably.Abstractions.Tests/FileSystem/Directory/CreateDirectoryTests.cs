using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.FileSystem;

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
		FileSystem.Directory.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ReadOnlyParent_ShouldStillCreateDirectory(string parent,
		string subdirectory)
	{
		string subdirectoryPath = FileSystem.Path.Combine(parent, subdirectory);
		FileSystem.Directory.CreateDirectory(parent);
		FileSystem.DirectoryInfo.New(parent).Attributes = FileAttributes.ReadOnly;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(subdirectoryPath);
		});

		exception.Should().BeNull();
		FileSystem.Directory.Exists(subdirectoryPath).Should().BeTrue();
		FileSystem.DirectoryInfo.New(parent).Attributes
			.Should().HaveFlag(FileAttributes.ReadOnly);
	}

	[SkippableFact]
	public void CreateDirectory_Root_ShouldNotThrowException()
	{
		string path = FileTestHelper.RootDrive();
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		exception.Should().BeNull();
		FileSystem.Directory.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ShouldTrimTrailingSpacesOnlyOnWindows(string path)
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
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

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
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

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
	public void CreateDirectory_Empty_ShouldThrowArgumentException()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(string.Empty);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
#else
		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
		exception.Should().BeOfType<ArgumentException>()
			.Which.ParamName.Should().Be("path");
#endif
	}

	[SkippableFact]
	public void CreateDirectory_IllegalCharacters_ShouldThrowArgumentException()
	{
		IEnumerable<char> invalidChars = FileSystem.Path
			.GetInvalidPathChars().Where(c => c != '\0')
			.Concat(new[]
			{
				'*', '?'
			});
		foreach (char invalidChar in invalidChars)
		{
			string path = $"{invalidChar}foo{invalidChar}bar";
			Exception? exception = Record.Exception(() =>
			{
				FileSystem.Directory.CreateDirectory(path);
			});

			if (Test.RunsOnWindows)
			{
#if NETFRAMEWORK
				exception.Should().BeOfType<ArgumentException>();
#else
				string expectedMessage = $"'{System.IO.Path.Combine(BasePath, path)}'";
				exception.Should()
					.BeOfType<IOException>(
						$"'{invalidChar}' is an invalid path character.")
					.Which.Message.Should().Contain(expectedMessage);
				exception.Should().BeOfType<IOException>()
					.Which.HResult.Should().Be(-2147024773);
#endif
			}
			else
			{
				exception.Should().BeNull();
			}
		}
	}

	[SkippableFact]
	public void CreateDirectory_Null_ShouldThrowArgumentNullException()
	{
		Exception? exception =
			Record.Exception(() => FileSystem.Directory.CreateDirectory(null!));

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.HResult.Should().Be(-2147467261);
		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("path");
	}

	[SkippableFact]
	public void CreateDirectory_NullCharacter_ShouldThrowArgumentException()
	{
		string path = "foo\0bar";
		Exception? exception =
			Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
	}

	[SkippableFact]
	public void CreateDirectory_ShouldCreateDirectoryInBasePath()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateDirectory("foo");
		bool exists = FileSystem.Directory.Exists("foo");

		exists.Should().BeTrue();
		result.FullName.Should().StartWith(BasePath);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_ShouldCreateParentDirectories(
		string directoryLevel1, string directoryLevel2, string directoryLevel3)
	{
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
		FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
	}

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
		FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
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
		result.FullName.Should().Be(System.IO.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
	}
#endif
}
using System.Collections.Generic;
using System.Linq;
#if !NETFRAMEWORK
using System.IO;
using System.Runtime.InteropServices;
#endif

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
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

		foreach (string path in new[] { rootPath, subdirectoryLevel1Path })
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
		   .Which.Message.Should()
		   .Be("Path cannot be the empty string or all whitespace.");
#else
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("path");
		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should()
		   .Be("Path cannot be the empty string or all whitespace. (Parameter 'path')");
#endif
	}

	[SkippableFact]
	public void CreateDirectory_IllegalCharacters_ShouldThrowArgumentException()
	{
		IEnumerable<char> invalidChars = FileSystem.Path
		   .GetInvalidPathChars().Where(c => c != '\0')
		   .Concat(new[] { '*', '?' });
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

		exception.Should().BeOfType<ArgumentNullException>().Which.ParamName
		   .Should().Be("path");
	}

	[SkippableFact]
	public void CreateDirectory_NullCharacter_ShouldThrowArgumentException()
	{
		string path = "foo\0bar";
		Exception? exception =
			Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

		exception.Should().BeOfType<ArgumentException>();
	}

	[SkippableFact]
	public void CreateDirectory_ShouldCreateDirectoryInBasePath()
	{
		IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory("foo");
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

		IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

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

		IFileSystem.IDirectoryInfo result =
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

		IFileSystem.IDirectoryInfo result =
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
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			expectedName = expectedName.TrimEnd(' ');
		}
		else if (suffix == "\\")
		{
			//This case is only supported on Windows
			return;
		}

		IFileSystem.IDirectoryInfo result =
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
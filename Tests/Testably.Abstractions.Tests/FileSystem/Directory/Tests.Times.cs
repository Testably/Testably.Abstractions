using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class Tests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void GetCreationTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.Directory.GetCreationTime(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetCreationTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.Directory.GetCreationTimeUtc(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetLastAccessTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.Directory.GetLastAccessTime(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetLastAccessTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.Directory.GetLastAccessTimeUtc(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetLastWriteTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.Directory.GetLastWriteTime(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetLastWriteTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.Directory.GetLastWriteTimeUtc(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void LastAccessTime_CreateSubDirectory_ShouldUpdateLastAccessAndLastWriteTime(
		string path, string subPath)
	{
		DateTime start = TimeSystem.DateTime.Now;
		IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);
		TimeSystem.Thread.Sleep(100);
		DateTime sleepTime = TimeSystem.DateTime.Now;
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subPath));

		result.CreationTime.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.CreationTime.Should().BeBefore(sleepTime);
		// Last Access Time is only updated on Windows
		if (Test.RunsOnWindows)
		{
			result.LastAccessTime.Should()
			   .BeOnOrAfter(sleepTime.ApplySystemClockTolerance());
			result.LastAccessTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
		}
		else
		{
			result.LastAccessTime.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
			result.LastAccessTime.Should().BeBefore(sleepTime);
		}

		result.LastWriteTime.Should().BeOnOrAfter(sleepTime.ApplySystemClockTolerance());
		result.LastWriteTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
	}

	[SkippableTheory]
	[AutoData]
	public void LastAccessTime_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetLastAccessTime(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
		result.Kind.Should().Be(DateTimeKind.Local);
	}

	[SkippableTheory]
	[AutoData]
	public void LastAccessTimeUtc_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetLastAccessTimeUtc(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
		result.Kind.Should().Be(DateTimeKind.Utc);
	}

	[SkippableTheory]
	[AutoData]
	public void LastWriteTime_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetLastWriteTime(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
		result.Kind.Should().Be(DateTimeKind.Local);
	}

	[SkippableTheory]
	[AutoData]
	public void LastWriteTimeUtc_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetLastWriteTimeUtc(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
		result.Kind.Should().Be(DateTimeKind.Utc);
	}

	[SkippableTheory]
	[AutoData]
	public void SetCreationTime_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime creationTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.SetCreationTime(path, creationTime);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.HResult.Should().Be(-2147024894);
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.HResult.Should().Be(-2147024893);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void SetCreationTime_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = creationTime.ToLocalTime();
		DateTime expectedTime = creationTime.ToUniversalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetCreationTime(path, creationTime);

		FileSystem.Directory.GetCreationTimeUtc(path)
		   .Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetCreationTime_Unspecified_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = DateTime.SpecifyKind(creationTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetCreationTime(path, creationTime);

		FileSystem.Directory.GetCreationTimeUtc(path)
		   .Should().Be(creationTime.ToUniversalTime());
		FileSystem.Directory.GetCreationTime(path)
		   .Should().Be(creationTime);
		FileSystem.Directory.GetCreationTime(path).Kind
		   .Should().NotBe(DateTimeKind.Unspecified);
	}

	[SkippableTheory]
	[AutoData]
	public void SetCreationTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime creationTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.SetCreationTimeUtc(path, creationTime);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.HResult.Should().Be(-2147024894);
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.HResult.Should().Be(-2147024893);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void SetCreationTimeUtc_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = creationTime.ToUniversalTime();
		DateTime expectedTime = creationTime.ToLocalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetCreationTimeUtc(path, creationTime);

		FileSystem.Directory.GetCreationTime(path)
		   .Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetCreationTimeUtc_Unspecified_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = DateTime.SpecifyKind(creationTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetCreationTimeUtc(path, creationTime);

		FileSystem.Directory.GetCreationTimeUtc(path)
		   .Should().Be(creationTime);
		FileSystem.Directory.GetCreationTime(path)
		   .Should().Be(creationTime.ToLocalTime());
		FileSystem.Directory.GetCreationTime(path).Kind
		   .Should().NotBe(DateTimeKind.Unspecified);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTime_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastAccessTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.SetLastAccessTime(path, lastAccessTime);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.HResult.Should().Be(-2147024894);
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.HResult.Should().Be(-2147024893);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTime_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToLocalTime();
		DateTime expectedTime = lastAccessTime.ToUniversalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastAccessTime(path, lastAccessTime);

		FileSystem.Directory.GetLastAccessTimeUtc(path)
		   .Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTime_Unspecified_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = DateTime.SpecifyKind(lastAccessTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastAccessTime(path, lastAccessTime);

		FileSystem.Directory.GetLastAccessTimeUtc(path)
		   .Should().Be(lastAccessTime.ToUniversalTime());
		FileSystem.Directory.GetLastAccessTime(path)
		   .Should().Be(lastAccessTime);
		FileSystem.Directory.GetLastAccessTime(path).Kind
		   .Should().NotBe(DateTimeKind.Unspecified);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastAccessTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.SetLastAccessTimeUtc(path, lastAccessTime);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.HResult.Should().Be(-2147024894);
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.HResult.Should().Be(-2147024893);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTimeUtc_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToUniversalTime();
		DateTime expectedTime = lastAccessTime.ToLocalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastAccessTimeUtc(path, lastAccessTime);

		FileSystem.Directory.GetLastAccessTime(path)
		   .Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTimeUtc_Unspecified_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = DateTime.SpecifyKind(lastAccessTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastAccessTimeUtc(path, lastAccessTime);

		FileSystem.Directory.GetLastAccessTimeUtc(path)
		   .Should().Be(lastAccessTime);
		FileSystem.Directory.GetLastAccessTime(path)
		   .Should().Be(lastAccessTime.ToLocalTime());
		FileSystem.Directory.GetLastAccessTime(path).Kind
		   .Should().NotBe(DateTimeKind.Unspecified);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTime_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastWriteTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.SetLastWriteTime(path, lastWriteTime);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.HResult.Should().Be(-2147024894);
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.HResult.Should().Be(-2147024893);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTime_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToLocalTime();
		DateTime expectedTime = lastWriteTime.ToUniversalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastWriteTime(path, lastWriteTime);

		FileSystem.Directory.GetLastWriteTimeUtc(path)
		   .Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTime_Unspecified_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = DateTime.SpecifyKind(lastWriteTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastWriteTime(path, lastWriteTime);

		FileSystem.Directory.GetLastWriteTimeUtc(path)
		   .Should().Be(lastWriteTime.ToUniversalTime());
		FileSystem.Directory.GetLastWriteTime(path)
		   .Should().Be(lastWriteTime);
		FileSystem.Directory.GetLastWriteTime(path).Kind
		   .Should().NotBe(DateTimeKind.Unspecified);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastWriteTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.SetLastWriteTimeUtc(path, lastWriteTime);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<FileNotFoundException>()
			   .Which.HResult.Should().Be(-2147024894);
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
			exception.Should().BeOfType<DirectoryNotFoundException>()
			   .Which.HResult.Should().Be(-2147024893);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTimeUtc_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToUniversalTime();
		DateTime expectedTime = lastWriteTime.ToLocalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastWriteTimeUtc(path, lastWriteTime);

		FileSystem.Directory.GetLastWriteTime(path)
		   .Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTimeUtc_Unspecified_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = DateTime.SpecifyKind(lastWriteTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastWriteTimeUtc(path, lastWriteTime);

		FileSystem.Directory.GetLastWriteTimeUtc(path)
		   .Should().Be(lastWriteTime);
		FileSystem.Directory.GetLastWriteTime(path)
		   .Should().Be(lastWriteTime.ToLocalTime());
		FileSystem.Directory.GetLastWriteTime(path).Kind
		   .Should().NotBe(DateTimeKind.Unspecified);
	}
}
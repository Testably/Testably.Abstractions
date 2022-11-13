using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void GetCreationTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.File.GetCreationTime(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetCreationTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.File.GetCreationTimeUtc(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetLastAccessTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.File.GetLastAccessTime(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetLastAccessTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.File.GetLastAccessTimeUtc(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetLastWriteTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.File.GetLastWriteTime(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void GetLastWriteTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.File.GetLastWriteTimeUtc(path);

		result.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void LastAccessTime_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.File.WriteAllText(path, null);

		DateTime result = FileSystem.File.GetLastAccessTime(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
		result.Kind.Should().Be(DateTimeKind.Local);
	}

	[SkippableTheory]
	[AutoData]
	public void LastAccessTimeUtc_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.File.WriteAllText(path, null);

		DateTime result = FileSystem.File.GetLastAccessTimeUtc(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
		result.Kind.Should().Be(DateTimeKind.Utc);
	}

	[SkippableTheory]
	[AutoData]
	public void LastWriteTime_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.File.WriteAllText(path, null);

		DateTime result = FileSystem.File.GetLastWriteTime(path);
		result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
		result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
		result.Kind.Should().Be(DateTimeKind.Local);
	}

	[SkippableTheory]
	[AutoData]
	public void LastWriteTimeUtc_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.File.WriteAllText(path, null);

		DateTime result = FileSystem.File.GetLastWriteTimeUtc(path);
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
			FileSystem.File.SetCreationTime(path, creationTime);
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
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
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetCreationTime(path, creationTime);

		FileSystem.File.GetCreationTimeUtc(path)
			.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetCreationTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime creationTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetCreationTimeUtc(path, creationTime);
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
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
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetCreationTimeUtc(path, creationTime);

		FileSystem.File.GetCreationTime(path)
			.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTime_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastAccessTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetLastAccessTime(path, lastAccessTime);
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTime_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToLocalTime();
		DateTime expectedTime = lastAccessTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetLastAccessTime(path, lastAccessTime);

		FileSystem.File.GetLastAccessTimeUtc(path)
			.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastAccessTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetLastAccessTimeUtc(path, lastAccessTime);
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastAccessTimeUtc_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToUniversalTime();
		DateTime expectedTime = lastAccessTime.ToLocalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetLastAccessTimeUtc(path, lastAccessTime);

		FileSystem.File.GetLastAccessTime(path)
			.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTime_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastWriteTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetLastWriteTime(path, lastWriteTime);
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTime_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToLocalTime();
		DateTime expectedTime = lastWriteTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetLastWriteTime(path, lastWriteTime);

		FileSystem.File.GetLastWriteTimeUtc(path)
			.Should().Be(expectedTime);
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastWriteTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetLastWriteTimeUtc(path, lastWriteTime);
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void SetLastWriteTimeUtc_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToUniversalTime();
		DateTime expectedTime = lastWriteTime.ToLocalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetLastWriteTimeUtc(path, lastWriteTime);

		FileSystem.File.GetLastWriteTime(path)
			.Should().Be(expectedTime);
	}
}
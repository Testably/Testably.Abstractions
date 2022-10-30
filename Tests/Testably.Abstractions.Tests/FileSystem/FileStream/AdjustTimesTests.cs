using System.IO;
using Testably.Abstractions.FileSystem;
#if FEATURE_SPAN
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class AdjustTimesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CopyTo_ShouldAdjustTimes(string path, byte[] contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		byte[] buffer = new byte[contents.Length];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);
		using MemoryStream destination = new(buffer);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		stream.CopyTo(destination);
		destination.Flush();

		DateTime lastAccessTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastAccessTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void Read_AsSpan_ShouldAdjustTimes(string path, byte[] contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		_ = stream.Read(buffer.AsSpan());

		DateTime lastAccessTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastAccessTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void Read_ShouldAdjustTimes(string path, byte[] contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		_ = stream.Read(buffer, 0, 2);

		DateTime lastAccessTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastAccessTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public async Task ReadAsync_AsMemory_ShouldAdjustTimes(string path, byte[] contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		await FileSystem.File.WriteAllBytesAsync(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

		_ = await stream.ReadAsync(buffer.AsMemory());

		DateTime lastAccessTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastAccessTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableTheory]
	[AutoData]
	public async Task ReadAsync_ShouldAdjustTimes(string path, byte[] contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		await FileSystem.File.WriteAllBytesAsync(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

#pragma warning disable CA1835
		_ = await stream.ReadAsync(buffer, 0, 2);
#pragma warning restore CA1835

		DateTime lastAccessTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastAccessTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void ReadByte_ShouldAdjustTimes(string path, byte[] contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		stream.ReadByte();

		DateTime lastAccessTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastAccessTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}

	[SkippableTheory]
	[AutoData]
	public void Seek_ShouldNotAdjustTimes(string path, byte[] contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		stream.Seek(2, SeekOrigin.Current);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void Write_AsSpan_ShouldAdjustTimes(string path, byte[] contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, Array.Empty<byte>());
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		stream.Write(contents.AsSpan());
		stream.Dispose();

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
			lastAccessTime.Should()
			   .BeOnOrAfter(updateTime);
		}
		else
		{
			lastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastWriteTime.Should()
		   .BeOnOrAfter(updateTime);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void Write_ShouldAdjustTimes(string path, byte[] contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, Array.Empty<byte>());
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		stream.Write(contents, 0, 2);
		stream.Dispose();

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
			lastAccessTime.Should()
			   .BeOnOrAfter(updateTime);
		}
		else
		{
			lastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastWriteTime.Should()
		   .BeOnOrAfter(updateTime);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public async Task WriteAsync_AsMemory_ShouldAdjustTimes(string path, byte[] contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		await FileSystem.File.WriteAllBytesAsync(path, Array.Empty<byte>());
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		await using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		await stream.WriteAsync(contents.AsMemory());
		await stream.DisposeAsync();

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
			lastAccessTime.Should()
			   .BeOnOrAfter(updateTime);
		}
		else
		{
			lastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastWriteTime.Should()
		   .BeOnOrAfter(updateTime);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableTheory]
	[AutoData]
	public async Task WriteAsync_ShouldAdjustTimes(string path, byte[] contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		await FileSystem.File.WriteAllBytesAsync(path, Array.Empty<byte>());
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		await using FileSystemStream stream = FileSystem.File.OpenWrite(path);

#pragma warning disable CA1835
		await stream.WriteAsync(contents, 0, 2);
#pragma warning restore CA1835
		await stream.DisposeAsync();

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
			lastAccessTime.Should()
			   .BeOnOrAfter(updateTime);
		}
		else
		{
			lastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastWriteTime.Should()
		   .BeOnOrAfter(updateTime);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void WriteByte_ShouldAdjustTimes(string path, byte[] contents, byte content)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		stream.WriteByte(content);
		stream.Dispose();

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
			lastAccessTime.Should()
			   .BeOnOrAfter(updateTime);
		}
		else
		{
			lastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastWriteTime.Should()
		   .BeOnOrAfter(updateTime);
	}

	private DateTime WaitToBeUpdatedToAfter(Func<DateTime> callback,
	                                        DateTime expectedAfter)
	{
		for (int i = 0; i < 100; i++)
		{
			DateTime time = callback();
			if (time >= expectedAfter)
			{
				return time;
			}

			TimeSystem.Thread.Sleep(100);
		}

		return callback();
	}
}
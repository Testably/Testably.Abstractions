using System.IO;
#if FEATURE_SPAN
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class AdjustTimesTests
{
	[Theory]
	[AutoData]
	public async Task CopyTo_ShouldAdjustTimes(string path, byte[] bytes)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		byte[] buffer = new byte[bytes.Length];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
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
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task Read_AsSpan_ShouldAdjustTimes(string path, byte[] bytes)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
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
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}
#endif

	[Theory]
	[AutoData]
	public async Task Read_ShouldAdjustTimes(string path, byte[] bytes)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
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
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task ReadAsync_AsMemory_ShouldAdjustTimes(string path, byte[] bytes)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(FileTestHelper.AdjustTimesDelay, TestContext.Current.CancellationToken);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

		_ = await stream.ReadAsync(buffer.AsMemory(), TestContext.Current.CancellationToken);

		DateTime lastAccessTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastAccessTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[AutoData]
	public async Task ReadAsync_ShouldAdjustTimes(string path, byte[] bytes)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(FileTestHelper.AdjustTimesDelay, TestContext.Current.CancellationToken);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

#pragma warning disable CA1835
		_ = await stream.ReadAsync(buffer, 0, 2, TestContext.Current.CancellationToken);
#pragma warning restore CA1835

		DateTime lastAccessTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastAccessTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}
#endif

	[Theory]
	[AutoData]
	public async Task ReadByte_ShouldAdjustTimes(string path, byte[] bytes)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
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
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

	[Theory]
	[AutoData]
	public async Task Seek_ShouldNotAdjustTimes(string path, byte[] bytes)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		stream.Seek(2, SeekOrigin.Current);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task Write_AsSpan_ShouldAdjustTimes(string path, byte[] bytes)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, Array.Empty<byte>());
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			stream.Write(bytes.AsSpan());
		}

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}
#endif

	[Theory]
	[AutoData]
	public async Task Write_ShouldAdjustTimes(string path, byte[] bytes)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, Array.Empty<byte>());
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			stream.Write(bytes, 0, 2);
		}

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task WriteAsync_AsMemory_ShouldAdjustTimes(string path, byte[] bytes)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		await FileSystem.File.WriteAllBytesAsync(path, Array.Empty<byte>(), TestContext.Current.CancellationToken);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(FileTestHelper.AdjustTimesDelay, TestContext.Current.CancellationToken);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		await using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			await stream.WriteAsync(bytes.AsMemory(), TestContext.Current.CancellationToken);
		}

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And( creationTimeEnd);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[AutoData]
	public async Task WriteAsync_ShouldAdjustTimes(string path, byte[] bytes)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		await FileSystem.File.WriteAllBytesAsync(path, Array.Empty<byte>(), TestContext.Current.CancellationToken);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(FileTestHelper.AdjustTimesDelay, TestContext.Current.CancellationToken);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		await using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
#pragma warning disable CA1835
			await stream.WriteAsync(bytes, 0, 2, TestContext.Current.CancellationToken);
#pragma warning restore CA1835
		}

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}
#endif

	[Theory]
	[AutoData]
	public async Task WriteByte_ShouldAdjustTimes(string path, byte[] bytes, byte singleByte)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			stream.WriteByte(singleByte);
		}

		DateTime lastWriteTime = WaitToBeUpdatedToAfter(
			() => FileSystem.File.GetLastWriteTimeUtc(path), updateTime);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	#region Helpers

	private DateTime WaitToBeUpdatedToAfter(Func<DateTime> callback,
		DateTime expectedAfter)
	{
		for (int i = 0; i < 20; i++)
		{
			DateTime time = callback();
			if (time >= expectedAfter.ApplySystemClockTolerance())
			{
				return time;
			}

			TimeSystem.Thread.Sleep(100);
		}

		return callback();
	}

	#endregion
}

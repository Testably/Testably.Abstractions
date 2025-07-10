using System.Threading;
#if FEATURE_SPAN
using System.Collections.Generic;
using System.IO;
#endif
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class ReadTests
{
	[Theory]
	[AutoData]
	public async Task BeginRead_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.FileInfo.New(path).OpenWrite();

		byte[] buffer = new byte[bytes.Length];

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.BeginRead(buffer, 0, buffer.Length, _ => { }, null);
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}

	[Theory]
	[AutoData]
	public async Task BeginRead_ShouldCopyContentsToBuffer(
		string path, byte[] bytes)
	{
		using ManualResetEventSlim ms = new();
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		byte[] buffer = new byte[bytes.Length];
		stream.BeginRead(buffer, 0, buffer.Length, ar =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				stream.EndRead(ar);
				// ReSharper disable once AccessToDisposedClosure
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}, null);

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(buffer).IsEqualTo(bytes).InAnyOrder();
	}

	[Theory]
	[AutoData]
	public async Task EndRead_Null_ShouldThrowArgumentNullException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndRead(null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithHResult(-2147467261);
	}

	[Theory]
	[AutoData]
	public async Task EndRead_ShouldNotAdjustTimes(string path, byte[] bytes)
	{
		SkipIfBrittleTestsShouldBeSkipped(Test.RunsOnMac);

		using ManualResetEventSlim ms = new();
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;

		using (FileSystemStream stream = FileSystem.File.OpenRead(path))
		{
			byte[] buffer = new byte[bytes.Length];
			stream.BeginRead(buffer, 0, buffer.Length, ar =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
					stream.EndRead(ar);
					// ReSharper disable once AccessToDisposedClosure
					ms.Set();
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, null);

			await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		}

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd)
			.Within(TimeComparison.Tolerance);
		await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd)
			.Within(TimeComparison.Tolerance);
		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd)
			.Within(TimeComparison.Tolerance);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task Read_AsSpan_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);

		void Act()
		{
			using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
			{
				_ = stream.Read(buffer.AsSpan());
			}
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task Read_AsSpan_ShouldFillBuffer(string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		int result = stream.Read(buffer.AsSpan());

		await That(result).IsEqualTo(bytes.Length);
		await That(buffer).IsEqualTo(bytes).InAnyOrder();
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task Read_AsSpan_ShouldUseSharedBuffer(string path)
	{
		List<int> results = [];
		using FileSystemStream fileStream1 = FileSystem.FileStream.New(
			path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		using FileSystemStream fileStream2 = FileSystem.FileStream.New(
			path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		byte[] buffer = new byte[4];

		for (int ix = 0; ix < 10; ix++)
		{
			fileStream1.Position = 0;
			fileStream1.Write(BitConverter.GetBytes(ix));
			fileStream1.Flush();

			fileStream2.Position = 0;
			fileStream2.Flush();
			_ = fileStream2.Read(buffer);
			results.Add(BitConverter.ToInt32(buffer));
		}

		await That(results).IsEqualTo([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]).InAnyOrder();
	}
#endif

	[Theory]
	[AutoData]
	public async Task Read_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);

		void Act()
		{
			using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
			{
				_ = stream.Read(buffer, 0, bytes.Length);
			}
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}

	[Theory]
	[AutoData]
	public async Task Read_ShouldFillBuffer(string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		int result = stream.Read(buffer, 0, bytes.Length);

		await That(result).IsEqualTo(bytes.Length);
		await That(buffer).IsEqualTo(bytes).InAnyOrder();
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[AutoData]
	public async Task ReadAsync_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(ExpectSuccess);
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);

		async Task Act()
		{
			await using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
			{
				// ReSharper disable once AccessToDisposedClosure
				#pragma warning disable CA1835
				_ = await stream.ReadAsync(buffer, 0, bytes.Length, cts.Token);
				#pragma warning restore CA1835
			}
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[AutoData]
	public async Task ReadAsync_Memory_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(ExpectSuccess);
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);

		async Task Act()
		{
			await using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
			{
				// ReSharper disable once AccessToDisposedClosure
				#pragma warning disable CA1835
				_ = await stream.ReadAsync(buffer.AsMemory(), cts.Token);
				#pragma warning restore CA1835
			}
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[AutoData]
	public async Task ReadAsync_ShouldFillBuffer(string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(ExpectSuccess);
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

#pragma warning disable CA1835
		int result = await stream.ReadAsync(buffer, 0, bytes.Length, cts.Token);
#pragma warning restore CA1835

		await That(result).IsEqualTo(bytes.Length);
		await That(buffer).IsEqualTo(bytes).InAnyOrder();
	}
#endif

	[Theory]
	[AutoData]
	public async Task ReadByte_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		void Act()
		{
			using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
			{
				_ = stream.ReadByte();
			}
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}

	[Theory]
	[AutoData]
	public async Task ReadByte_ShouldReadSingleByteAndAdvancePosition(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		int result1 = stream.ReadByte();
		int result2 = stream.ReadByte();

		await That(stream.Position).IsEqualTo(2);
		await That(result1).IsEqualTo(bytes[0]);
		await That(result2).IsEqualTo(bytes[1]);
	}

	[Theory]
	[AutoData]
	public async Task ReadTimeout_ShouldThrowInvalidOperationException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		void Act()
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			_ = stream.ReadTimeout;
		}

		await That(Act).Throws<InvalidOperationException>().WithHResult(-2146233079);
	}
}

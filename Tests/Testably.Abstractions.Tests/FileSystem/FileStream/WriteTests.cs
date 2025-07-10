using System.IO;
using System.Threading;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class WriteTests
{
	[Theory]
	[AutoData]
	public async Task BeginWrite_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.FileInfo.New(path).OpenRead();

		byte[] buffer = new byte[bytes.Length];
		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.BeginWrite(buffer, 0, buffer.Length, _ => { }, null);
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}

	[Theory]
	[AutoData]
	public async Task BeginWrite_ShouldCopyContentsToFile(
		string path, byte[] bytes)
	{
		using ManualResetEventSlim ms = new();
		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			stream.Flush();

			stream.BeginWrite(bytes, 0, bytes.Length, ar =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					stream.EndWrite(ar);
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

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task EndWrite_Null_ShouldThrowArgumentNullException(string path)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndWrite(null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithHResult(-2147467261);
	}

	[Theory]
	[AutoData]
	public async Task EndWrite_ShouldAdjustTimes(string path, byte[] bytes)
	{
		using ManualResetEventSlim ms = new();
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		DateTime updateTime = DateTime.MinValue;

		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			stream.BeginWrite(bytes, 0, bytes.Length, ar =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
					updateTime = TimeSystem.DateTime.UtcNow;
					stream.EndWrite(ar);
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
	public async Task Write_AsSpan_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);

		void Act()
		{
			using (FileSystemStream stream = FileSystem.File.OpenRead(path))
			{
				// ReSharper disable once AccessToDisposedClosure
				stream.Write(buffer.AsSpan());
			}
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task Write_AsSpan_ShouldFillBuffer(string path, byte[] bytes)
	{
		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			stream.Write(bytes.AsSpan());
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}
#endif

	[Theory]
	[AutoData]
	public async Task Write_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);

		void Act()
		{
			using (FileSystemStream stream = FileSystem.File.OpenRead(path))
			{
				// ReSharper disable once AccessToDisposedClosure
				stream.Write(buffer, 0, bytes.Length);
			}
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}

	[Theory]
	[AutoData]
	public async Task Write_ShouldFillBuffer(string path, byte[] bytes)
	{
		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			stream.Write(bytes, 0, bytes.Length);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[AutoData]
	public async Task WriteAsync_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(ExpectSuccess);
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);

		async Task Act()
		{
			await using (FileSystemStream stream = FileSystem.File.OpenRead(path))
			{
				// ReSharper disable once AccessToDisposedClosure
				#pragma warning disable CA1835
				await stream.WriteAsync(buffer, 0, bytes.Length, cts.Token);
				#pragma warning restore CA1835
			}
		}

		await That(Act).Throws<NotSupportedException>().WithHResult(-2146233067);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[AutoData]
	public async Task WriteAsync_ShouldFillBuffer(string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(ExpectSuccess);

		await using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			#pragma warning disable CA1835
			await stream.WriteAsync(bytes, 0, bytes.Length, cts.Token);
			#pragma warning restore CA1835
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}
#endif

	[Theory]
	[AutoData]
	public async Task WriteByte_HiddenFile_ShouldNotThrow(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);
		
		void Act()
		{
			using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
			{
				// ReSharper disable once AccessToDisposedClosure
				stream.WriteByte(0);
			}
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task WriteByte_ShouldWriteSingleByteAndAdvancePosition(
		string path, byte byte1, byte byte2)
	{
		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			stream.WriteByte(byte1);
			stream.WriteByte(byte2);

			await That(stream.Position).IsEqualTo(2);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo([byte1, byte2]);
	}

	[Theory]
	[AutoData]
	public async Task WriteTimeout_ShouldThrowInvalidOperationException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		void Act()
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			_ = stream.WriteTimeout;
		}

		await That(Act).Throws<InvalidOperationException>().WithHResult(-2146233079);
	}
}

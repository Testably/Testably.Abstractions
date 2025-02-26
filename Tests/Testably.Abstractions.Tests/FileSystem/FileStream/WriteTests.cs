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
	public void BeginWrite_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystemStream stream = FileSystem.FileInfo.New(path).OpenRead();

		byte[] buffer = new byte[bytes.Length];
		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.BeginWrite(buffer, 0, buffer.Length, _ => { }, null);
		});

		stream.Dispose();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[Theory]
	[AutoData]
	public void BeginWrite_ShouldCopyContentsToFile(
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

			ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken).Should().BeTrue();
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void EndWrite_Null_ShouldThrowArgumentNullException(string path)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndWrite(null!);
		});

		exception.Should().BeException<ArgumentNullException>(hResult: -2147467261);
	}

	[Theory]
	[AutoData]
	public void EndWrite_ShouldAdjustTimes(string path, byte[] bytes)
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

			ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken).Should().BeTrue();
		}

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

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

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void Write_AsSpan_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		Exception? exception;

		using (FileSystemStream stream = FileSystem.File.OpenRead(path))
		{
			exception = Record.Exception(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				stream.Write(buffer.AsSpan());
			});
		}

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void Write_AsSpan_ShouldFillBuffer(string path, byte[] bytes)
	{
		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			stream.Write(bytes.AsSpan());
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
	}
#endif

	[Theory]
	[AutoData]
	public void Write_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		Exception? exception;

		using (FileSystemStream stream = FileSystem.File.OpenRead(path))
		{
			exception = Record.Exception(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				stream.Write(buffer, 0, bytes.Length);
			});
		}

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[Theory]
	[AutoData]
	public void Write_ShouldFillBuffer(string path, byte[] bytes)
	{
		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			stream.Write(bytes, 0, bytes.Length);
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
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
		Exception? exception;

		await using (FileSystemStream stream = FileSystem.File.OpenRead(path))
		{
			async Task Act()
			{
				// ReSharper disable once AccessToDisposedClosure
				#pragma warning disable CA1835
				await stream.WriteAsync(buffer, 0, bytes.Length, cts.Token);
				#pragma warning restore CA1835
			}
			
			exception = await Record.ExceptionAsync(Act);
		}

		exception.Should().BeException<NotSupportedException>(
			hResult: -2146233067);
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

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
	}
#endif

	[Theory]
	[AutoData]
	public void WriteByte_HiddenFile_ShouldNotThrow(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);
		Exception? exception;

		using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			exception = Record.Exception(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				stream.WriteByte(0);
			});
		}

		exception.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void WriteByte_ShouldWriteSingleByteAndAdvancePosition(
		string path, byte byte1, byte byte2)
	{
		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			stream.WriteByte(byte1);
			stream.WriteByte(byte2);

			stream.Position.Should().Be(2);
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo([byte1, byte2]);
	}

	[Theory]
	[AutoData]
	public void WriteTimeout_ShouldThrowInvalidOperationException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			_ = stream.WriteTimeout;
		});

		exception.Should().BeException<InvalidOperationException>(
			hResult: -2146233079);
	}
}

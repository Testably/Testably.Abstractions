using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileStreamStatisticsTests
{
	[SkippableFact]
	public void BeginRead_ByteArray_Int_Int_AsyncCallback_Object_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		AsyncCallback? callback = null;
		object? state = null;

		fileStream.BeginRead(buffer, offset, count, callback, state);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.BeginRead),
			buffer, offset, count, callback, state);
	}

	[SkippableFact]
	public void BeginWrite_ByteArray_Int_Int_AsyncCallback_Object_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		AsyncCallback? callback = null;
		object? state = null;

		fileStream.BeginWrite(buffer, offset, count, callback, state);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.BeginWrite),
			buffer, offset, count, callback, state);
	}

	[SkippableFact]
	public void CopyTo_Stream_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Stream destination = new MemoryStream();
		int bufferSize = 42;

		fileStream.CopyTo(destination, bufferSize);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.CopyTo),
			destination, bufferSize);
	}

	[SkippableFact]
	public async Task CopyToAsync_Stream_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Stream destination = new MemoryStream();
		int bufferSize = 42;
		CancellationToken cancellationToken = CancellationToken.None;

		await fileStream.CopyToAsync(destination, bufferSize, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.CopyToAsync),
			destination, bufferSize, cancellationToken);
	}

	[SkippableFact]
	public void EndRead_IAsyncResult_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		IAsyncResult asyncResult = fileStream.BeginRead(new byte[10], 0, 10, null, null);

		fileStream.EndRead(asyncResult);

		sut.Statistics.FileStream["foo"].Methods.Count.Should().Be(2);
		sut.Statistics.FileStream["foo"].Methods.Values.Should()
			.ContainSingle(c => c.Name == nameof(FileSystemStream.EndRead) &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(asyncResult));
	}

	[SkippableFact]
	public void EndWrite_IAsyncResult_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		IAsyncResult asyncResult = fileStream.BeginWrite(new byte[10], 0, 10, null, null);

		fileStream.EndWrite(asyncResult);

		sut.Statistics.FileStream["foo"].Methods.Count.Should().Be(2);
		sut.Statistics.FileStream["foo"].Methods.Values.Should()
			.ContainSingle(c => c.Name == nameof(FileSystemStream.EndWrite) &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(asyncResult));
	}

	[SkippableFact]
	public void Flush_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		fileStream.Flush();

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Flush));
	}

	[SkippableFact]
	public void Flush_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		bool flushToDisk = true;

		fileStream.Flush(flushToDisk);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Flush),
			flushToDisk);
	}

	[SkippableFact]
	public async Task FlushAsync_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		CancellationToken cancellationToken = CancellationToken.None;

		await fileStream.FlushAsync(cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.FlushAsync),
			cancellationToken);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Read_SpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Span<byte> buffer = new();

		_ = fileStream.Read(buffer);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Read),
			buffer);
	}
#endif

	[SkippableFact]
	public void Read_ByteArray_Int_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;

		_ = fileStream.Read(buffer, offset, count);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Read),
			buffer, offset, count);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public async Task ReadAsync_MemoryByte_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		await using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Memory<byte> buffer = new();
		CancellationToken cancellationToken = CancellationToken.None;

		_ = await fileStream.ReadAsync(buffer, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.ReadAsync),
			buffer, cancellationToken);
	}
#endif

	[SkippableFact]
	public async Task ReadAsync_ByteArray_Int_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		CancellationToken cancellationToken = CancellationToken.None;

		_ = await fileStream.ReadAsync(buffer, offset, count, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.ReadAsync),
			buffer, offset, count, cancellationToken);
	}

	[SkippableFact]
	public void ReadByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		fileStream.ReadByte();

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.ReadByte));
	}

	[SkippableFact]
	public void Seek_Int64_SeekOrigin_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		long offset = new();
		SeekOrigin origin = new();

		fileStream.Seek(offset, origin);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Seek),
			offset, origin);
	}

	[SkippableFact]
	public void SetLength_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		long value = new();

		fileStream.SetLength(value);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.SetLength),
			value);
	}

	[SkippableFact]
	public void ToString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.ToString();

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.ToString));
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Write_ReadOnlySpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		ReadOnlySpan<byte> buffer = new();

		fileStream.Write(buffer);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Write),
			buffer);
	}
#endif

	[SkippableFact]
	public void Write_ByteArray_Int_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;

		fileStream.Write(buffer, offset, count);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Write),
			buffer, offset, count);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public async Task WriteAsync_ReadOnlyMemoryByte_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		await using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		ReadOnlyMemory<byte> buffer = new();
		CancellationToken cancellationToken = CancellationToken.None;

		await fileStream.WriteAsync(buffer, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.WriteAsync),
			buffer, cancellationToken);
	}
#endif

	[SkippableFact]
	public async Task WriteAsync_ByteArray_Int_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		CancellationToken cancellationToken = CancellationToken.None;

		await fileStream.WriteAsync(buffer, offset, count, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.WriteAsync),
			buffer, offset, count, cancellationToken);
	}

	[SkippableFact]
	public void WriteByte_Byte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte value = new();

		fileStream.WriteByte(value);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.WriteByte),
			value);
	}
}

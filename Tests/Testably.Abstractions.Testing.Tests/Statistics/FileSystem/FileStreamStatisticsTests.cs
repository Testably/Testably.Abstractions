using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileStreamStatisticsTests
{
	[SkippableFact]
	public void BeginRead_ByteArray_Int_Int_AsyncCallback_Object_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		AsyncCallback? callback = null;
		object? state = new();

		sut.FileStream.New("foo", FileMode.OpenOrCreate)
			.BeginRead(buffer, offset, count, callback, state);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.BeginRead),
			buffer, offset, count, callback, state);
	}

	[SkippableFact]
	public void BeginWrite_ByteArray_Int_Int_AsyncCallback_Object_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		AsyncCallback? callback = null;
		object? state = null;

		sut.FileStream.New("foo", FileMode.OpenOrCreate)
			.BeginWrite(buffer, offset, count, callback, state);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.BeginWrite),
			buffer, offset, count, callback, state);
	}

	[SkippableFact]
	public void CopyTo_Stream_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		Stream destination = new MemoryStream();
		int bufferSize = 42;

		sut.FileStream.New("foo", FileMode.OpenOrCreate).CopyTo(destination, bufferSize);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.CopyTo),
			destination, bufferSize);
	}

	[SkippableFact]
	public async Task CopyToAsync_Stream_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		Stream destination = new MemoryStream();
		int bufferSize = 42;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.FileStream.New("foo", FileMode.OpenOrCreate)
			.CopyToAsync(destination, bufferSize, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.CopyToAsync),
			destination, bufferSize, cancellationToken);
	}

	[SkippableFact]
	public void EndRead_IAsyncResult_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		FileSystemStream stream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		IAsyncResult asyncResult = stream.BeginRead(new byte[10], 0, 10, null, null);

		stream.EndRead(asyncResult);

		sut.Statistics.FileStream["foo"].Calls.Count.Should().Be(2);
		sut.Statistics.FileStream["foo"].Calls.Should()
			.ContainSingle(c => c.Name == nameof(FileSystemStream.EndRead) &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(asyncResult));
	}

	[SkippableFact]
	public void EndWrite_IAsyncResult_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		FileSystemStream stream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		IAsyncResult asyncResult = stream.BeginWrite(new byte[10], 0, 10, null, null);

		stream.EndWrite(asyncResult);

		sut.Statistics.FileStream["foo"].Calls.Count.Should().Be(2);
		sut.Statistics.FileStream["foo"].Calls.Should()
			.ContainSingle(c => c.Name == nameof(FileSystemStream.EndWrite) &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(asyncResult));
	}

	[SkippableFact]
	public void Flush_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileStream.New("foo", FileMode.OpenOrCreate).Flush();

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Flush));
	}

	[SkippableFact]
	public void Flush_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		bool flushToDisk = true;

		sut.FileStream.New("foo", FileMode.OpenOrCreate).Flush(flushToDisk);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Flush),
			flushToDisk);
	}

	[SkippableFact]
	public async Task FlushAsync_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.FileStream.New("foo", FileMode.OpenOrCreate).FlushAsync(cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.FlushAsync),
			cancellationToken);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Read_SpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		Span<byte> buffer = new();

		_ = sut.FileStream.New("foo", FileMode.OpenOrCreate).Read(buffer);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Read),
			buffer);
	}
#endif

	[SkippableFact]
	public void Read_ByteArray_Int_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;

		sut.FileStream.New("foo", FileMode.OpenOrCreate).Read(buffer, offset, count);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Read),
			buffer, offset, count);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void ReadAsync_MemoryByte_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		Memory<byte> buffer = new();
		CancellationToken cancellationToken = CancellationToken.None;

		sut.FileStream.New("foo", FileMode.OpenOrCreate).ReadAsync(buffer, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.ReadAsync),
			buffer, cancellationToken);
	}
#endif

	[SkippableFact]
	public async Task ReadAsync_ByteArray_Int_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.FileStream.New("foo", FileMode.OpenOrCreate)
			.ReadAsync(buffer, offset, count, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.ReadAsync),
			buffer, offset, count, cancellationToken);
	}

	[SkippableFact]
	public void ReadByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileStream.New("foo", FileMode.OpenOrCreate).ReadByte();

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.ReadByte));
	}

	[SkippableFact]
	public void Seek_Int64_SeekOrigin_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		long offset = new();
		SeekOrigin origin = new();

		sut.FileStream.New("foo", FileMode.OpenOrCreate).Seek(offset, origin);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Seek),
			offset, origin);
	}

	[SkippableFact]
	public void SetLength_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		long value = new();

		sut.FileStream.New("foo", FileMode.OpenOrCreate).SetLength(value);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.SetLength),
			value);
	}

	[SkippableFact]
	public void ToString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileStream.New("foo", FileMode.OpenOrCreate).ToString();

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.ToString));
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Write_ReadOnlySpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<byte> buffer = new();

		sut.FileStream.New("foo", FileMode.OpenOrCreate).Write(buffer);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Write),
			buffer);
	}
#endif

	[SkippableFact]
	public void Write_ByteArray_Int_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;

		sut.FileStream.New("foo", FileMode.OpenOrCreate).Write(buffer, offset, count);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.Write),
			buffer, offset, count);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void WriteAsync_ReadOnlyMemoryByte_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlyMemory<byte> buffer = new();
		CancellationToken cancellationToken = CancellationToken.None;

		sut.FileStream.New("foo", FileMode.OpenOrCreate).WriteAsync(buffer, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.WriteAsync),
			buffer, cancellationToken);
	}
#endif

	[SkippableFact]
	public async Task WriteAsync_ByteArray_Int_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.FileStream.New("foo", FileMode.OpenOrCreate)
			.WriteAsync(buffer, offset, count, cancellationToken);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.WriteAsync),
			buffer, offset, count, cancellationToken);
	}

	[SkippableFact]
	public void WriteByte_Byte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		byte value = new();

		sut.FileStream.New("foo", FileMode.OpenOrCreate).WriteByte(value);

		sut.Statistics.FileStream["foo"].ShouldOnlyContain(nameof(FileSystemStream.WriteByte),
			value);
	}
}

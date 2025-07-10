using System.IO;
using System.Text;
using System.Threading;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileStreamStatisticsTests
{
	[Fact]
	public async Task Method_BeginRead_ByteArray_Int_Int_AsyncCallback_Object_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		AsyncCallback? callback = null;
		object? state = null;

		fileStream.BeginRead(buffer, offset, count, callback, state);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.BeginRead),
				buffer, offset, count, callback, state);
	}

	[Fact]
	public async Task Method_BeginWrite_ByteArray_Int_Int_AsyncCallback_Object_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		AsyncCallback? callback = null;
		object? state = null;

		fileStream.BeginWrite(buffer, offset, count, callback, state);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.BeginWrite),
				buffer, offset, count, callback, state);
	}

	[Fact]
	public async Task Method_Close_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		fileStream.Close();

		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.Close));
	}

	[Fact]
	public async Task Method_CopyTo_Stream_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Stream destination = new MemoryStream();
		int bufferSize = 42;

		fileStream.CopyTo(destination, bufferSize);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.CopyTo),
				destination, bufferSize);
	}

	[Fact]
	public async Task Method_CopyToAsync_Stream_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Stream destination = new MemoryStream();
		int bufferSize = 42;
		CancellationToken cancellationToken = CancellationToken.None;

		await fileStream.CopyToAsync(destination, bufferSize, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.CopyToAsync),
				destination, bufferSize, cancellationToken);
	}

	[Fact]
	public async Task Method_EndRead_IAsyncResult_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		IAsyncResult asyncResult = fileStream.BeginRead(new byte[10], 0, 10, null, null);

		fileStream.EndRead(asyncResult);

		await That(sut.Statistics.TotalCount).IsEqualTo(3);
		await That(sut.Statistics.FileStream["foo"].Methods.Length).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"].Methods).HasSingle().Matching(c =>
			string.Equals(c.Name, nameof(FileSystemStream.EndRead), StringComparison.Ordinal) &&
			c.Parameters.Length == 1 &&
			c.Parameters[0].Is(asyncResult));
	}

	[Fact]
	public async Task Method_EndWrite_IAsyncResult_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		IAsyncResult asyncResult = fileStream.BeginWrite(new byte[10], 0, 10, null, null);

		fileStream.EndWrite(asyncResult);

		await That(sut.Statistics.TotalCount).IsEqualTo(3);
		await That(sut.Statistics.FileStream["foo"].Methods.Length).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"].Methods).HasSingle().Matching(c =>
			string.Equals(c.Name, nameof(FileSystemStream.EndWrite), StringComparison.Ordinal) &&
			c.Parameters.Length == 1 &&
			c.Parameters[0].Is(asyncResult));
	}

	[Fact]
	public async Task Method_Flush_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		bool flushToDisk = true;

		fileStream.Flush(flushToDisk);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.Flush),
				flushToDisk);
	}

	[Fact]
	public async Task Method_Flush_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		fileStream.Flush();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.Flush));
	}

	[Fact]
	public async Task Method_FlushAsync_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		CancellationToken cancellationToken = CancellationToken.None;

		await fileStream.FlushAsync(cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.FlushAsync),
				cancellationToken);
	}

	[Fact]
	public async Task Method_Read_ByteArray_Int_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;

		_ = fileStream.Read(buffer, offset, count);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.Read),
				buffer, offset, count);
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_Read_SpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Span<byte> buffer = new();

		_ = fileStream.Read(buffer);

		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.Read),
				buffer);
		await That(sut.Statistics.TotalCount).IsEqualTo(2);
	}
#endif

	[Fact]
	public async Task Method_ReadAsync_ByteArray_Int_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		CancellationToken cancellationToken = CancellationToken.None;

		#pragma warning disable CA1835 // Change the 'ReadAsync' method call to use the 'Stream.ReadAsync(Memory<byte>, CancellationToken)' overload
		_ = await fileStream.ReadAsync(buffer, offset, count, cancellationToken);
		#pragma warning restore CA1835

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.ReadAsync),
				buffer, offset, count, cancellationToken);
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_ReadAsync_MemoryByte_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		await using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Memory<byte> buffer = new();
		CancellationToken cancellationToken = CancellationToken.None;

		_ = await fileStream.ReadAsync(buffer, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.ReadAsync),
				buffer, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_ReadByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		fileStream.ReadByte();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.ReadByte));
	}

	[Fact]
	public async Task Method_Seek_Int64_SeekOrigin_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		long offset = new();
		SeekOrigin origin = new();

		fileStream.Seek(offset, origin);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.Seek),
				offset, origin);
	}

	[Fact]
	public async Task Method_SetLength_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		long value = new();

		fileStream.SetLength(value);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.SetLength),
				value);
	}

	[Fact]
	public async Task Method_ToString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.ToString();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.ToString));
	}

	[Fact]
	public async Task Method_Write_ByteArray_Int_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;

		fileStream.Write(buffer, offset, count);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.Write),
				buffer, offset, count);
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_Write_ReadOnlySpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		ReadOnlySpan<byte> buffer = new();

		fileStream.Write(buffer);

		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.Write),
				buffer);
		await That(sut.Statistics.TotalCount).IsEqualTo(2);
	}
#endif

	[Fact]
	public async Task Method_WriteAsync_ByteArray_Int_Int_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		CancellationToken cancellationToken = CancellationToken.None;

		#pragma warning disable CA1835 // Change the 'WriteAsync' method call to use the 'Stream.WriteAsync(ReadOnlyMemory<byte>, CancellationToken)' overload
		await fileStream.WriteAsync(buffer, offset, count, cancellationToken);
		#pragma warning restore CA1835

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.WriteAsync),
				buffer, offset, count, cancellationToken);
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_WriteAsync_ReadOnlyMemoryByte_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		await using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		ReadOnlyMemory<byte> buffer = new();
		CancellationToken cancellationToken = CancellationToken.None;

		await fileStream.WriteAsync(buffer, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.WriteAsync),
				buffer, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_WriteByte_Byte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte value = new();

		fileStream.WriteByte(value);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsMethodCall(nameof(FileSystemStream.WriteByte),
				value);
	}

	[Fact]
	public async Task Property_CanRead_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.CanRead;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.CanRead));
	}

	[Fact]
	public async Task Property_CanSeek_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.CanSeek;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.CanSeek));
	}

	[Fact]
	public async Task Property_CanTimeout_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.CanTimeout;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.CanTimeout));
	}

	[Fact]
	public async Task Property_CanWrite_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.CanWrite;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.CanWrite));
	}

	[Fact]
	public async Task Property_IsAsync_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.IsAsync;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.IsAsync));
	}

	[Fact]
	public async Task Property_Length_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.Length;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.Length));
	}

	[Fact]
	public async Task Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.Name;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.Name));
	}

	[Fact]
	public async Task Property_Position_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.Position;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.Position));
	}

	[Fact]
	public async Task Property_Position_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		long value = new();

		fileStream.Position = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertySetAccess(nameof(FileSystemStream.Position));
	}

	[Fact]
	public async Task Property_ReadTimeout_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		try
		{
			_ = fileStream.ReadTimeout;
		}
		catch (InvalidOperationException)
		{
			// Timeouts are not supported on this stream.
		}

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.ReadTimeout));
	}

	[Fact]
	public async Task Property_ReadTimeout_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		int value = 42;

		try
		{
			fileStream.ReadTimeout = value;
		}
		catch (InvalidOperationException)
		{
			// Timeouts are not supported on this stream.
		}

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertySetAccess(nameof(FileSystemStream.ReadTimeout));
	}

	[Fact]
	public async Task Property_WriteTimeout_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		try
		{
			_ = fileStream.WriteTimeout;
		}
		catch (InvalidOperationException)
		{
			// Timeouts are not supported on this stream.
		}

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertyGetAccess(nameof(FileSystemStream.WriteTimeout));
	}

	[Fact]
	public async Task Property_WriteTimeout_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		int value = 42;

		try
		{
			fileStream.WriteTimeout = value;
		}
		catch (InvalidOperationException)
		{
			// Timeouts are not supported on this stream.
		}

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileStream["foo"])
			.OnlyContainsPropertySetAccess(nameof(FileSystemStream.WriteTimeout));
	}

	[Fact]
	public async Task ToString_ShouldBeFileStreamWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.FileStream[@"\\some\path"];

		string? result = sut.ToString();

		await That(result).IsEqualTo(@"FileStream[\\some\path]");
	}
}

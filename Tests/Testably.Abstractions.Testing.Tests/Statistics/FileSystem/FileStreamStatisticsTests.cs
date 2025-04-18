﻿using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileStreamStatisticsTests
{
	[Fact]
	public void Method_BeginRead_ByteArray_Int_Int_AsyncCallback_Object_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		AsyncCallback? callback = null;
		object? state = null;

		fileStream.BeginRead(buffer, offset, count, callback, state);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.BeginRead),
				buffer, offset, count, callback, state);
	}

	[Fact]
	public void Method_BeginWrite_ByteArray_Int_Int_AsyncCallback_Object_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;
		AsyncCallback? callback = null;
		object? state = null;

		fileStream.BeginWrite(buffer, offset, count, callback, state);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.BeginWrite),
				buffer, offset, count, callback, state);
	}

	[Fact]
	public void Method_Close_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
			
		fileStream.Close();

		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.Close));
	}

	[Fact]
	public void Method_CopyTo_Stream_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Stream destination = new MemoryStream();
		int bufferSize = 42;

		fileStream.CopyTo(destination, bufferSize);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.CopyTo),
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.CopyToAsync),
				destination, bufferSize, cancellationToken);
	}

	[Fact]
	public void Method_EndRead_IAsyncResult_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		IAsyncResult asyncResult = fileStream.BeginRead(new byte[10], 0, 10, null, null);

		fileStream.EndRead(asyncResult);

		sut.Statistics.TotalCount.Should().Be(3);
		sut.Statistics.FileStream["foo"].Methods.Length.Should().Be(2);
		sut.Statistics.FileStream["foo"].Methods.Should()
			.ContainSingle(c => c.Name == nameof(FileSystemStream.EndRead) &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(asyncResult));
	}

	[Fact]
	public void Method_EndWrite_IAsyncResult_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		IAsyncResult asyncResult = fileStream.BeginWrite(new byte[10], 0, 10, null, null);

		fileStream.EndWrite(asyncResult);

		sut.Statistics.TotalCount.Should().Be(3);
		sut.Statistics.FileStream["foo"].Methods.Length.Should().Be(2);
		sut.Statistics.FileStream["foo"].Methods.Should()
			.ContainSingle(c => c.Name == nameof(FileSystemStream.EndWrite) &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(asyncResult));
	}

	[Fact]
	public void Method_Flush_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		bool flushToDisk = true;

		fileStream.Flush(flushToDisk);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.Flush),
				flushToDisk);
	}

	[Fact]
	public void Method_Flush_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		fileStream.Flush();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.Flush));
	}

	[Fact]
	public async Task Method_FlushAsync_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		CancellationToken cancellationToken = CancellationToken.None;

		await fileStream.FlushAsync(cancellationToken);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.FlushAsync),
				cancellationToken);
	}

	[Fact]
	public void Method_Read_ByteArray_Int_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;

		_ = fileStream.Read(buffer, offset, count);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.Read),
				buffer, offset, count);
	}

#if FEATURE_SPAN
	[Fact]
	public void Method_Read_SpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		Span<byte> buffer = new();

		_ = fileStream.Read(buffer);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.Read),
				buffer);
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.ReadAsync),
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.ReadAsync),
				buffer, cancellationToken);
	}
#endif

	[Fact]
	public void Method_ReadByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		fileStream.ReadByte();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.ReadByte));
	}

	[Fact]
	public void Method_Seek_Int64_SeekOrigin_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		long offset = new();
		SeekOrigin origin = new();

		fileStream.Seek(offset, origin);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.Seek),
				offset, origin);
	}

	[Fact]
	public void Method_SetLength_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		long value = new();

		fileStream.SetLength(value);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.SetLength),
				value);
	}

	[Fact]
	public void Method_ToString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.ToString();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.ToString));
	}

	[Fact]
	public void Method_Write_ByteArray_Int_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte[] buffer = Encoding.UTF8.GetBytes("foo");
		int offset = 0;
		int count = 2;

		fileStream.Write(buffer, offset, count);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.Write),
				buffer, offset, count);
	}

#if FEATURE_SPAN
	[Fact]
	public void Method_Write_ReadOnlySpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		ReadOnlySpan<byte> buffer = new();

		fileStream.Write(buffer);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.Write),
				buffer);
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.WriteAsync),
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.WriteAsync),
				buffer, cancellationToken);
	}
#endif

	[Fact]
	public void Method_WriteByte_Byte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		byte value = new();

		fileStream.WriteByte(value);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainMethodCall(nameof(FileSystemStream.WriteByte),
				value);
	}

	[Fact]
	public void Property_CanRead_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.CanRead;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.CanRead));
	}

	[Fact]
	public void Property_CanSeek_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.CanSeek;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.CanSeek));
	}

	[Fact]
	public void Property_CanTimeout_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.CanTimeout;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.CanTimeout));
	}

	[Fact]
	public void Property_CanWrite_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.CanWrite;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.CanWrite));
	}

	[Fact]
	public void Property_IsAsync_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.IsAsync;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.IsAsync));
	}

	[Fact]
	public void Property_Length_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.Length;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.Length));
	}

	[Fact]
	public void Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.Name;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.Name));
	}

	[Fact]
	public void Property_Position_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);

		_ = fileStream.Position;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.Position));
	}

	[Fact]
	public void Property_Position_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		using FileSystemStream fileStream = sut.FileStream.New("foo", FileMode.OpenOrCreate);
		long value = new();

		fileStream.Position = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(FileSystemStream.Position));
	}

	[Fact]
	public void Property_ReadTimeout_Get_ShouldRegisterPropertyAccess()
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.ReadTimeout));
	}

	[Fact]
	public void Property_ReadTimeout_Set_ShouldRegisterPropertyAccess()
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(FileSystemStream.ReadTimeout));
	}

	[Fact]
	public void Property_WriteTimeout_Get_ShouldRegisterPropertyAccess()
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(FileSystemStream.WriteTimeout));
	}

	[Fact]
	public void Property_WriteTimeout_Set_ShouldRegisterPropertyAccess()
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

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileStream["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(FileSystemStream.WriteTimeout));
	}

	[Fact]
	public void ToString_ShouldBeFileStreamWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.FileStream[@"\\some\path"];

		string? result = sut.ToString();

		result.Should().Be(@"FileStream[\\some\path]");
	}
}

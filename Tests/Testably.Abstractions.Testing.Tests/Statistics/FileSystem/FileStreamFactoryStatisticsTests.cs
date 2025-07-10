using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#if NET6_0_OR_GREATER
using Microsoft.Win32.SafeHandles;
using Testably.Abstractions.Testing.FileSystem;
#endif

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileStreamFactoryStatisticsTests
{
#if NET6_0_OR_GREATER
	[Fact]
	public async Task Method_New_SafeFileHandle_FileAccess_Int_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle handle = new();
		FileAccess access = FileAccess.ReadWrite;
		int bufferSize = 42;
		bool isAsync = true;

		using FileSystemStream result = sut.FileStream.New(handle, access, bufferSize, isAsync);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			handle, access, bufferSize, isAsync);
	}
#endif

#if NET6_0_OR_GREATER
	[Fact]
	public async Task Method_New_SafeFileHandle_FileAccess_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle handle = new();
		FileAccess access = FileAccess.ReadWrite;
		int bufferSize = 42;

		using FileSystemStream result = sut.FileStream.New(handle, access, bufferSize);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			handle, access, bufferSize);
	}
#endif
#if NET6_0_OR_GREATER
	[Fact]
	public async Task Method_New_SafeFileHandle_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle handle = new();
		FileAccess access = FileAccess.ReadWrite;

		using FileSystemStream result = sut.FileStream.New(handle, access);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			handle, access);
	}
#endif

	[Fact]
	public async Task Method_New_String_FileMode_FileAccess_FileShare_Int_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;
		int bufferSize = 42;
		bool useAsync = true;

		using FileSystemStream result =
			sut.FileStream.New(path, mode, access, share, bufferSize, useAsync);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize, useAsync);
	}

	[Fact]
	public async Task
		Method_New_String_FileMode_FileAccess_FileShare_Int_FileOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;
		int bufferSize = 42;
		FileOptions options = new();

		using FileSystemStream result =
			sut.FileStream.New(path, mode, access, share, bufferSize, options);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize, options);
	}

	[Fact]
	public async Task Method_New_String_FileMode_FileAccess_FileShare_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;
		int bufferSize = 42;

		using FileSystemStream result = sut.FileStream.New(path, mode, access, share, bufferSize);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize);
	}

	[Fact]
	public async Task Method_New_String_FileMode_FileAccess_FileShare_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;

		using FileSystemStream result = sut.FileStream.New(path, mode, access, share);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access, share);
	}

	[Fact]
	public async Task Method_New_String_FileMode_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;

		using FileSystemStream result = sut.FileStream.New(path, mode, access);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access);
	}

	[Fact]
	public async Task Method_New_String_FileMode_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;

		using FileSystemStream result = sut.FileStream.New(path, mode);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			path, mode);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[Fact]
	public async Task Method_New_String_FileStreamOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		FileStreamOptions options = new();

		using FileSystemStream result = sut.FileStream.New(path, options);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileStream).OnlyContainsMethodCall(nameof(IFileStreamFactory.New),
			path, options);
	}
#endif

	[Fact]
	public async Task Method_Wrap_FileStream_ShouldRegisterCall()
	{
		string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		MockFileSystem sut = new();
		try
		{
			using FileStream fileStream = new(path, FileMode.OpenOrCreate);

			try
			{
				using FileSystemStream result = sut.FileStream.Wrap(fileStream);
			}
			catch (NotSupportedException)
			{
				// Wrap is not possible on the MockFileSystem, but should still be registered!
			}

			await That(sut.Statistics.TotalCount).IsEqualTo(1);
			await That(sut.Statistics.FileStream).OnlyContainsMethodCall(
				nameof(IFileStreamFactory.Wrap),
				fileStream);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[Fact]
	public async Task ToString_ShouldBeFileStream()
	{
		IPathStatistics<IFileStreamFactory, FileSystemStream> sut
			= new MockFileSystem().Statistics.FileStream;

		string? result = sut.ToString();

		await That(result).IsEqualTo("FileStream");
	}
}

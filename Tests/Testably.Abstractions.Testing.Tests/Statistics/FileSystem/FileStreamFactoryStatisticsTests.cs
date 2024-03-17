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
	[SkippableFact]
	public void Method_New_SafeFileHandle_FileAccess_Int_Bool_ShouldRegisterCall()
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

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			handle, access, bufferSize, isAsync);
	}
#endif

#if NET6_0_OR_GREATER
	[SkippableFact]
	public void Method_New_SafeFileHandle_FileAccess_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle handle = new();
		FileAccess access = FileAccess.ReadWrite;
		int bufferSize = 42;

		using FileSystemStream result = sut.FileStream.New(handle, access, bufferSize);

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			handle, access, bufferSize);
	}
#endif
#if NET6_0_OR_GREATER
	[SkippableFact]
	public void Method_New_SafeFileHandle_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle handle = new();
		FileAccess access = FileAccess.ReadWrite;

		using FileSystemStream result = sut.FileStream.New(handle, access);

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			handle, access);
	}
#endif

	[SkippableFact]
	public void Method_New_String_FileMode_FileAccess_FileShare_Int_Bool_ShouldRegisterCall()
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

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize, useAsync);
	}

	[SkippableFact]
	public void Method_New_String_FileMode_FileAccess_FileShare_Int_FileOptions_ShouldRegisterCall()
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

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize, options);
	}

	[SkippableFact]
	public void Method_New_String_FileMode_FileAccess_FileShare_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;
		int bufferSize = 42;

		using FileSystemStream result = sut.FileStream.New(path, mode, access, share, bufferSize);

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize);
	}

	[SkippableFact]
	public void Method_New_String_FileMode_FileAccess_FileShare_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;

		using FileSystemStream result = sut.FileStream.New(path, mode, access, share);

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access, share);
	}

	[SkippableFact]
	public void Method_New_String_FileMode_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;

		using FileSystemStream result = sut.FileStream.New(path, mode, access);

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			path, mode, access);
	}

	[SkippableFact]
	public void Method_New_String_FileMode_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;

		using FileSystemStream result = sut.FileStream.New(path, mode);

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			path, mode);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[SkippableFact]
	public void Method_New_String_FileStreamOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		FileStreamOptions options = new();

		using FileSystemStream result = sut.FileStream.New(path, options);

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.New),
			path, options);
	}
#endif

	[SkippableFact]
	public void Method_Wrap_FileStream_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileStream fileStream = new("foo", FileMode.OpenOrCreate);

		try
		{
			using FileSystemStream result = sut.FileStream.Wrap(fileStream);
		}
		catch (NotSupportedException)
		{
			// Wrap is not possible on the MockFileSystem, but should still be registered!
		}

		sut.Statistics.FileStream.ShouldOnlyContainMethodCall(nameof(IFileStreamFactory.Wrap),
			fileStream);
	}

	[SkippableFact]
	public void ToString_ShouldBeFileStream()
	{
		IPathStatistics sut = new MockFileSystem().Statistics.FileStream;

		string? result = sut.ToString();

		result.Should().Be("FileStream");
	}
}

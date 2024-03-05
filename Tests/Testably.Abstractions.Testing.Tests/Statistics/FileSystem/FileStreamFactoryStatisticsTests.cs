using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#if NET6_0_OR_GREATER
using Microsoft.Win32.SafeHandles;
#endif

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileStreamFactoryStatisticsTests
{
#if NET6_0_OR_GREATER
	[SkippableFact]
	public void New_SafeFileHandle_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle handle = new();
		FileAccess access = FileAccess.ReadWrite;

		sut.FileStream.New(handle, access);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			handle, access);
	}
#endif

	[SkippableFact]
	public void New_String_FileMode_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;

		sut.FileStream.New(path, mode);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			path, mode);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[SkippableFact]
	public void New_String_FileStreamOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		FileStreamOptions options = new();

		sut.FileStream.New(path, options);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			path, options);
	}
#endif

#if NET6_0_OR_GREATER
	[SkippableFact]
	public void New_SafeFileHandle_FileAccess_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle handle = new();
		FileAccess access = FileAccess.ReadWrite;
		int bufferSize = 42;

		sut.FileStream.New(handle, access, bufferSize);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			handle, access, bufferSize);
	}
#endif

	[SkippableFact]
	public void New_String_FileMode_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;

		sut.FileStream.New(path, mode, access);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			path, mode, access);
	}

#if NET6_0_OR_GREATER
	[SkippableFact]
	public void New_SafeFileHandle_FileAccess_Int_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle handle = new();
		FileAccess access = FileAccess.ReadWrite;
		int bufferSize = 42;
		bool isAsync = true;

		sut.FileStream.New(handle, access, bufferSize, isAsync);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			handle, access, bufferSize, isAsync);
	}
#endif

	[SkippableFact]
	public void New_String_FileMode_FileAccess_FileShare_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;

		sut.FileStream.New(path, mode, access, share);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			path, mode, access, share);
	}

	[SkippableFact]
	public void New_String_FileMode_FileAccess_FileShare_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;
		int bufferSize = 42;

		sut.FileStream.New(path, mode, access, share, bufferSize);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize);
	}

	[SkippableFact]
	public void New_String_FileMode_FileAccess_FileShare_Int_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;
		int bufferSize = 42;
		bool useAsync = true;

		sut.FileStream.New(path, mode, access, share, bufferSize, useAsync);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize, useAsync);
	}

	[SkippableFact]
	public void New_String_FileMode_FileAccess_FileShare_Int_FileOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = FileMode.OpenOrCreate;
		FileAccess access = FileAccess.ReadWrite;
		FileShare share = FileShare.ReadWrite;
		int bufferSize = 42;
		FileOptions options = new();

		sut.FileStream.New(path, mode, access, share, bufferSize, options);

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.New),
			path, mode, access, share, bufferSize, options);
	}

	[SkippableFact]
	public void Wrap_FileStream_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileStream fileStream = new("foo", FileMode.OpenOrCreate);

		try
		{
			sut.FileStream.Wrap(fileStream);
		}
		catch (NotSupportedException)
		{
			// Wrap is not possible on the MockFileSystem, but should still be registered!
		}

		sut.Statistics.FileStream.ShouldOnlyContain(nameof(IFileStreamFactory.Wrap),
			fileStream);
	}
}

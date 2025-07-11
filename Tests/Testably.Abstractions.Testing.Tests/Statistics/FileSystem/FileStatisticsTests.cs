using System.Collections.Generic;
using System.IO;
using System.Text;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
using Testably.Abstractions.Testing.FileSystem;
using Microsoft.Win32.SafeHandles;
#endif
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif
// ReSharper disable PossibleMultipleEnumeration

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class FileStatisticsTests
{
#if FEATURE_FILE_SPAN
	[Fact]
	public async Task Method_AppendAllBytes_String_ByteArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		byte[] bytes = "foo"u8.ToArray();

		sut.File.AppendAllBytes(path, bytes);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllBytes),
			path, bytes);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task Method_AppendAllBytes_String_ReadOnlySpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlySpan<byte> bytes = new();

		sut.File.AppendAllBytes(path, bytes);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllBytes),
			path, bytes);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task
		Method_AppendAllBytesAsync_String_ByteArray_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		byte[] bytes = "foo"u8.ToArray();
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllBytesAsync(path, bytes, cancellationToken);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllBytesAsync),
			path, bytes, cancellationToken);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task
		Method_AppendAllBytesAsync_String_ReadOnlyMemoryByte_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlyMemory<byte> bytes = new();
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllBytesAsync(path, bytes, cancellationToken);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllBytesAsync),
			path, bytes, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_AppendAllLines_String_IEnumerableString_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;

		sut.File.AppendAllLines(path, contents, encoding);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllLines),
			path, contents, encoding);
	}

	[Fact]
	public async Task Method_AppendAllLines_String_IEnumerableString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];

		sut.File.AppendAllLines(path, contents);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllLines),
			path, contents);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task
		Method_AppendAllLinesAsync_String_IEnumerableString_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllLinesAsync(path, contents, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllLinesAsync),
			path, contents, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task
		Method_AppendAllLinesAsync_String_IEnumerableString_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllLinesAsync(path, contents, encoding, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllLinesAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task Method_AppendAllText_String_ReadOnlySpanChar_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlySpan<char> contents = new();
		Encoding encoding = Encoding.UTF8;

		sut.File.AppendAllText(path, contents, encoding);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllText),
			path, contents, encoding);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task Method_AppendAllText_String_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlySpan<char> contents = new();

		sut.File.AppendAllText(path, contents);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllText),
			path, contents);
	}
#endif

	[Fact]
	public async Task Method_AppendAllText_String_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.AppendAllText(path, contents, encoding);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllText),
			path, contents, encoding);
	}

	[Fact]
	public async Task Method_AppendAllText_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";

		sut.File.AppendAllText(path, contents);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllText),
			path, contents);
	}

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task
		Method_AppendAllTextAsync_String_ReadOnlyMemoryChar_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlyMemory<char> contents = new();
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllTextAsync(path, contents, cancellationToken);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllTextAsync),
			path, contents, cancellationToken);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task
		Method_AppendAllTextAsync_String_ReadOnlyMemoryChar_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlyMemory<char> contents = new();
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllTextAsync(path, contents, encoding, cancellationToken);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllTextAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task Method_AppendAllTextAsync_String_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllTextAsync(path, contents, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllTextAsync),
			path, contents, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task
		Method_AppendAllTextAsync_String_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllTextAsync(path, contents, encoding, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendAllTextAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_AppendText_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.AppendText(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.AppendText),
			path);
	}

	[Fact]
	public async Task Method_Copy_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string sourceFileName = "foo";
		string destFileName = "bar";
		bool overwrite = true;

		sut.File.Copy(sourceFileName, destFileName, overwrite);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Copy),
			sourceFileName, destFileName, overwrite);
	}

	[Fact]
	public async Task Method_Copy_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string sourceFileName = "foo";
		string destFileName = "bar";

		sut.File.Copy(sourceFileName, destFileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Copy),
			sourceFileName, destFileName);
	}

	[Fact]
	public async Task Method_Create_String_Int_FileOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		int bufferSize = 42;
		FileOptions options = new();

		sut.File.Create(path, bufferSize, options);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Create),
			path, bufferSize, options);
	}

	[Fact]
	public async Task Method_Create_String_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		int bufferSize = 42;

		sut.File.Create(path, bufferSize);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Create),
			path, bufferSize);
	}

	[Fact]
	public async Task Method_Create_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.Create(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Create),
			path);
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Method_CreateSymbolicLink_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string pathToTarget = "foo";

		sut.File.CreateSymbolicLink(path, pathToTarget);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.CreateSymbolicLink),
			path, pathToTarget);
	}
#endif

	[Fact]
	public async Task Method_CreateText_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.CreateText(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.CreateText),
			path);
	}

	[Fact]
	public async Task Method_Decrypt_String_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		#pragma warning disable CA1416
		sut.File.Decrypt(path);
		#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Decrypt),
			path);
	}

	[Fact]
	public async Task Method_Delete_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.Delete(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Delete),
			path);
	}

	[Fact]
	public async Task Method_Encrypt_String_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		#pragma warning disable CA1416
		sut.File.Encrypt(path);
		#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Encrypt),
			path);
	}

	[Fact]
	public async Task Method_Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.Exists(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Exists),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_GetAttributes_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetAttributes(fileHandle);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetAttributes),
			fileHandle);
	}
#endif

	[Fact]
	public async Task Method_GetAttributes_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetAttributes(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetAttributes),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_GetCreationTime_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetCreationTime(fileHandle);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetCreationTime),
			fileHandle);
	}
#endif

	[Fact]
	public async Task Method_GetCreationTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetCreationTime(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetCreationTime),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_GetCreationTimeUtc_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetCreationTimeUtc(fileHandle);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetCreationTimeUtc),
			fileHandle);
	}
#endif

	[Fact]
	public async Task Method_GetCreationTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetCreationTimeUtc(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetCreationTimeUtc),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_GetLastAccessTime_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetLastAccessTime(fileHandle);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetLastAccessTime),
			fileHandle);
	}
#endif

	[Fact]
	public async Task Method_GetLastAccessTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetLastAccessTime(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetLastAccessTime),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_GetLastAccessTimeUtc_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetLastAccessTimeUtc(fileHandle);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetLastAccessTimeUtc),
			fileHandle);
	}
#endif

	[Fact]
	public async Task Method_GetLastAccessTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetLastAccessTimeUtc(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetLastAccessTimeUtc),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_GetLastWriteTime_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetLastWriteTime(fileHandle);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetLastWriteTime),
			fileHandle);
	}
#endif

	[Fact]
	public async Task Method_GetLastWriteTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetLastWriteTime(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetLastWriteTime),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_GetLastWriteTimeUtc_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetLastWriteTimeUtc(fileHandle);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetLastWriteTimeUtc),
			fileHandle);
	}
#endif

	[Fact]
	public async Task Method_GetLastWriteTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetLastWriteTimeUtc(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetLastWriteTimeUtc),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_GetUnixFileMode_SafeFileHandle_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

#pragma warning disable CA1416
		sut.File.GetUnixFileMode(fileHandle);
#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetUnixFileMode),
			fileHandle);
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public async Task Method_GetUnixFileMode_String_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

#pragma warning disable CA1416
		sut.File.GetUnixFileMode(path);
#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.GetUnixFileMode),
			path);
	}
#endif

#if FEATURE_FILE_MOVETO_OVERWRITE
	[Fact]
	public async Task Method_Move_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string sourceFileName = "foo";
		string destFileName = "bar";
		bool overwrite = true;

		sut.File.Move(sourceFileName, destFileName, overwrite);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Move),
			sourceFileName, destFileName, overwrite);
	}
#endif

	[Fact]
	public async Task Method_Move_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string sourceFileName = "foo";
		string destFileName = "bar";

		sut.File.Move(sourceFileName, destFileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Move),
			sourceFileName, destFileName);
	}

	[Fact]
	public async Task Method_Open_String_FileMode_FileAccess_FileShare_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = new();
		FileAccess access = new();
		FileShare share = new();

		sut.File.Open(path, mode, access, share);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Open),
			path, mode, access, share);
	}

	[Fact]
	public async Task Method_Open_String_FileMode_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = new();
		FileAccess access = new();

		sut.File.Open(path, mode, access);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Open),
			path, mode, access);
	}

	[Fact]
	public async Task Method_Open_String_FileMode_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = new();

		sut.File.Open(path, mode);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Open),
			path, mode);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[Fact]
	public async Task Method_Open_String_FileStreamOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		FileStreamOptions options = new();

		sut.File.Open(path, options);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Open),
			path, options);
	}
#endif

	[Fact]
	public async Task Method_OpenRead_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.OpenRead(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.OpenRead),
			path);
	}

	[Fact]
	public async Task Method_OpenText_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.OpenText(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.OpenText),
			path);
	}

	[Fact]
	public async Task Method_OpenWrite_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.OpenWrite(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.OpenWrite),
			path);
	}

	[Fact]
	public async Task Method_ReadAllBytes_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.ReadAllBytes(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllBytes),
			path);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task Method_ReadAllBytesAsync_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllBytesAsync(path, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllBytesAsync),
			path, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_ReadAllLines_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.ReadAllLines(path, encoding);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllLines),
			path, encoding);
	}

	[Fact]
	public async Task Method_ReadAllLines_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.ReadAllLines(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllLines),
			path);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task Method_ReadAllLinesAsync_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllLinesAsync(path, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllLinesAsync),
			path, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task
		Method_ReadAllLinesAsync_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllLinesAsync(path, encoding, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllLinesAsync),
			path, encoding, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_ReadAllText_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.ReadAllText(path, encoding);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllText),
			path, encoding);
	}

	[Fact]
	public async Task Method_ReadAllText_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.ReadAllText(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllText),
			path);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task Method_ReadAllTextAsync_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllTextAsync(path, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllTextAsync),
			path, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task Method_ReadAllTextAsync_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllTextAsync(path, encoding, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadAllTextAsync),
			path, encoding, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_ReadLines_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.ReadLines(path, encoding);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadLines),
			path, encoding);
	}

	[Fact]
	public async Task Method_ReadLines_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.ReadLines(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadLines),
			path);
	}

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
	[Fact]
	public async Task Method_ReadLinesAsync_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		sut.File.ReadLinesAsync(path, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadLinesAsync),
			path, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
	[Fact]
	public async Task Method_ReadLinesAsync_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		sut.File.ReadLinesAsync(path, encoding, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ReadLinesAsync),
			path, encoding, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_Replace_String_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string sourceFileName = "foo";
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";
		bool ignoreMetadataErrors = true;

		sut.File.Replace(sourceFileName, destinationFileName, destinationBackupFileName,
			ignoreMetadataErrors);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Replace),
			sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
	}

	[Fact]
	public async Task Method_Replace_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string sourceFileName = "foo";
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";

		sut.File.Replace(sourceFileName, destinationFileName, destinationBackupFileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.Replace),
			sourceFileName, destinationFileName, destinationBackupFileName);
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Method_ResolveLinkTarget_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string linkPath = "foo";
		bool returnFinalTarget = true;

		sut.File.ResolveLinkTarget(linkPath, returnFinalTarget);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.ResolveLinkTarget),
			linkPath, returnFinalTarget);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_SetAttributes_SafeFileHandle_FileAttributes_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		FileAttributes fileAttributes = new();

		sut.File.SetAttributes(fileHandle, fileAttributes);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetAttributes),
			fileHandle, fileAttributes);
	}
#endif

	[Fact]
	public async Task Method_SetAttributes_String_FileAttributes_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		FileAttributes fileAttributes = new();

		sut.File.SetAttributes(path, fileAttributes);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetAttributes),
			path, fileAttributes);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_SetCreationTime_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime creationTime = new();

		sut.File.SetCreationTime(fileHandle, creationTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetCreationTime),
			fileHandle, creationTime);
	}
#endif

	[Fact]
	public async Task Method_SetCreationTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime creationTime = new();

		sut.File.SetCreationTime(path, creationTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetCreationTime),
			path, creationTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_SetCreationTimeUtc_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime creationTimeUtc = new();

		sut.File.SetCreationTimeUtc(fileHandle, creationTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetCreationTimeUtc),
			fileHandle, creationTimeUtc);
	}
#endif

	[Fact]
	public async Task Method_SetCreationTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime creationTimeUtc = new();

		sut.File.SetCreationTimeUtc(path, creationTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetCreationTimeUtc),
			path, creationTimeUtc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_SetLastAccessTime_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime lastAccessTime = new();

		sut.File.SetLastAccessTime(fileHandle, lastAccessTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetLastAccessTime),
			fileHandle, lastAccessTime);
	}
#endif

	[Fact]
	public async Task Method_SetLastAccessTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime lastAccessTime = new();

		sut.File.SetLastAccessTime(path, lastAccessTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetLastAccessTime),
			path, lastAccessTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_SetLastAccessTimeUtc_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime lastAccessTimeUtc = new();

		sut.File.SetLastAccessTimeUtc(fileHandle, lastAccessTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetLastAccessTimeUtc),
			fileHandle, lastAccessTimeUtc);
	}
#endif

	[Fact]
	public async Task Method_SetLastAccessTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime lastAccessTimeUtc = new();

		sut.File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetLastAccessTimeUtc),
			path, lastAccessTimeUtc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_SetLastWriteTime_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime lastWriteTime = new();

		sut.File.SetLastWriteTime(fileHandle, lastWriteTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetLastWriteTime),
			fileHandle, lastWriteTime);
	}
#endif

	[Fact]
	public async Task Method_SetLastWriteTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime lastWriteTime = new();

		sut.File.SetLastWriteTime(path, lastWriteTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetLastWriteTime),
			path, lastWriteTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_SetLastWriteTimeUtc_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime lastWriteTimeUtc = new();

		sut.File.SetLastWriteTimeUtc(fileHandle, lastWriteTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetLastWriteTimeUtc),
			fileHandle, lastWriteTimeUtc);
	}
#endif

	[Fact]
	public async Task Method_SetLastWriteTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime lastWriteTimeUtc = new();

		sut.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetLastWriteTimeUtc),
			path, lastWriteTimeUtc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Fact]
	public async Task Method_SetUnixFileMode_SafeFileHandle_UnixFileMode_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		UnixFileMode mode = new();

#pragma warning disable CA1416
		sut.File.SetUnixFileMode(fileHandle, mode);
#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetUnixFileMode),
			fileHandle, mode);
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public async Task Method_SetUnixFileMode_String_UnixFileMode_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		UnixFileMode mode = new();

#pragma warning disable CA1416
		sut.File.SetUnixFileMode(path, mode);
#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.SetUnixFileMode),
			path, mode);
	}
#endif

	[Fact]
	public async Task Method_WriteAllBytes_String_ByteArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		byte[] bytes = Encoding.UTF8.GetBytes("foo");

		sut.File.WriteAllBytes(path, bytes);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllBytes),
			path, bytes);
	}

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task Method_WriteAllBytes_String_ReadOnlySpanByte_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlySpan<byte> bytes = new();

		sut.File.WriteAllBytes(path, bytes);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllBytes),
			path, bytes);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task
		Method_WriteAllBytesAsync_String_ByteArray_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		byte[] bytes = "foo"u8.ToArray();
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllBytesAsync(path, bytes, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllBytesAsync),
			path, bytes, cancellationToken);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task
		Method_WriteAllBytesAsync_String_ReadOnlyMemoryByte_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlyMemory<byte> bytes = new();
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllBytesAsync(path, bytes, cancellationToken);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllBytesAsync),
			path, bytes, cancellationToken);
	}
#endif

	[Fact]
	public async Task Method_WriteAllLines_String_IEnumerableString_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;

		sut.File.WriteAllLines(path, contents, encoding);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllLines),
			path, contents, encoding);
	}

	[Fact]
	public async Task Method_WriteAllLines_String_IEnumerableString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];

		sut.File.WriteAllLines(path, contents);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllLines),
			path, contents);
	}

	[Fact]
	public async Task Method_WriteAllLines_String_StringArray_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string[] contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;

		sut.File.WriteAllLines(path, contents, encoding);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllLines),
			path, contents, encoding);
	}

	[Fact]
	public async Task Method_WriteAllLines_String_StringArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string[] contents = ["foo", "bar"];

		sut.File.WriteAllLines(path, contents);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllLines),
			path, contents);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task
		Method_WriteAllLinesAsync_String_IEnumerableString_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllLinesAsync(path, contents, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllLinesAsync),
			path, contents, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task
		Method_WriteAllLinesAsync_String_IEnumerableString_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllLinesAsync(path, contents, encoding, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllLinesAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task Method_WriteAllText_String_ReadOnlySpanChar_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlySpan<char> contents = new();
		Encoding encoding = Encoding.UTF8;

		sut.File.WriteAllText(path, contents, encoding);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllText),
			path, contents, encoding);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task Method_WriteAllText_String_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlySpan<char> contents = new();

		sut.File.WriteAllText(path, contents);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllText),
			path, contents);
	}
#endif

	[Fact]
	public async Task Method_WriteAllText_String_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.WriteAllText(path, contents, encoding);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllText),
			path, contents, encoding);
	}

	[Fact]
	public async Task Method_WriteAllText_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";

		sut.File.WriteAllText(path, contents);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllText),
			path, contents);
	}

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task
		Method_WriteAllTextAsync_String_ReadOnlyMemoryChar_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlyMemory<char> contents = new();
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllTextAsync(path, contents, cancellationToken);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllTextAsync),
			path, contents, cancellationToken);
	}
#endif

#if FEATURE_FILE_SPAN
	[Fact]
	public async Task
		Method_WriteAllTextAsync_String_ReadOnlyMemoryChar_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		ReadOnlyMemory<char> contents = new();
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllTextAsync(path, contents, encoding, cancellationToken);

		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllTextAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task Method_WriteAllTextAsync_String_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllTextAsync(path, contents, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllTextAsync),
			path, contents, cancellationToken);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task
		Method_WriteAllTextAsync_String_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllTextAsync(path, contents, encoding, cancellationToken);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.File).OnlyContainsMethodCall(nameof(IFile.WriteAllTextAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

	[Fact]
	public async Task ToString_ShouldBeFile()
	{
		IStatistics sut = new MockFileSystem().Statistics.File;

		string? result = sut.ToString();

		await That(result).IsEqualTo("File");
	}
}

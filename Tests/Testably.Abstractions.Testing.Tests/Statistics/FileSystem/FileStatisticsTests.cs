using System.Collections.Generic;
using System.IO;
using System.Text;
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
	[SkippableFact]
	public void AppendAllLines_String_IEnumerableString_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;

		sut.File.AppendAllLines(path, contents, encoding);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendAllLines),
			path, contents, encoding);
	}

	[SkippableFact]
	public void AppendAllLines_String_IEnumerableString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];

		sut.File.AppendAllLines(path, contents);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendAllLines),
			path, contents);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableFact]
	public async Task
		AppendAllLinesAsync_String_IEnumerableString_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllLinesAsync(path, contents, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendAllLinesAsync),
			path, contents, cancellationToken);
	}

	[SkippableFact]
	public async Task
		AppendAllLinesAsync_String_IEnumerableString_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllLinesAsync(path, contents, encoding, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendAllLinesAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

	[SkippableFact]
	public void AppendAllText_String_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.AppendAllText(path, contents, encoding);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendAllText),
			path, contents, encoding);
	}

	[SkippableFact]
	public void AppendAllText_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";

		sut.File.AppendAllText(path, contents);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendAllText),
			path, contents);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableFact]
	public async Task AppendAllTextAsync_String_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllTextAsync(path, contents, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendAllTextAsync),
			path, contents, cancellationToken);
	}

	[SkippableFact]
	public async Task
		AppendAllTextAsync_String_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.AppendAllTextAsync(path, contents, encoding, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendAllTextAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

	[SkippableFact]
	public void AppendText_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.AppendText(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.AppendText),
			path);
	}

	[SkippableFact]
	public void Copy_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string sourceFileName = "foo";
		string destFileName = "bar";
		bool overwrite = true;

		sut.File.Copy(sourceFileName, destFileName, overwrite);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Copy),
			sourceFileName, destFileName, overwrite);
	}

	[SkippableFact]
	public void Copy_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string sourceFileName = "foo";
		string destFileName = "bar";

		sut.File.Copy(sourceFileName, destFileName);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Copy),
			sourceFileName, destFileName);
	}

	[SkippableFact]
	public void Create_String_Int_FileOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		int bufferSize = 42;
		FileOptions options = new();

		sut.File.Create(path, bufferSize, options);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Create),
			path, bufferSize, options);
	}

	[SkippableFact]
	public void Create_String_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		int bufferSize = 42;

		sut.File.Create(path, bufferSize);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Create),
			path, bufferSize);
	}

	[SkippableFact]
	public void Create_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.Create(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Create),
			path);
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void CreateSymbolicLink_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string pathToTarget = "foo";

		sut.File.CreateSymbolicLink(path, pathToTarget);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.CreateSymbolicLink),
			path, pathToTarget);
	}
#endif

	[SkippableFact]
	public void CreateText_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.CreateText(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.CreateText),
			path);
	}

	[SkippableFact]
	public void Decrypt_String_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		#pragma warning disable CA1416
		sut.File.Decrypt(path);
		#pragma warning restore CA1416

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Decrypt),
			path);
	}

	[SkippableFact]
	public void Delete_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.Delete(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Delete),
			path);
	}

	[SkippableFact]
	public void Encrypt_String_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		#pragma warning disable CA1416
		sut.File.Encrypt(path);
		#pragma warning restore CA1416

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Encrypt),
			path);
	}

	[SkippableFact]
	public void Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.Exists(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Exists),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void GetAttributes_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetAttributes(fileHandle);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetAttributes),
			fileHandle);
	}
#endif

	[SkippableFact]
	public void GetAttributes_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetAttributes(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetAttributes),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void GetCreationTime_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetCreationTime(fileHandle);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetCreationTime),
			fileHandle);
	}
#endif

	[SkippableFact]
	public void GetCreationTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetCreationTime(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetCreationTime),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void GetCreationTimeUtc_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetCreationTimeUtc(fileHandle);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetCreationTimeUtc),
			fileHandle);
	}
#endif

	[SkippableFact]
	public void GetCreationTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetCreationTimeUtc(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetCreationTimeUtc),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void GetLastAccessTime_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetLastAccessTime(fileHandle);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetLastAccessTime),
			fileHandle);
	}
#endif

	[SkippableFact]
	public void GetLastAccessTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetLastAccessTime(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetLastAccessTime),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void GetLastAccessTimeUtc_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetLastAccessTimeUtc(fileHandle);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetLastAccessTimeUtc),
			fileHandle);
	}
#endif

	[SkippableFact]
	public void GetLastAccessTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetLastAccessTimeUtc(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetLastAccessTimeUtc),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void GetLastWriteTime_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetLastWriteTime(fileHandle);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetLastWriteTime),
			fileHandle);
	}
#endif

	[SkippableFact]
	public void GetLastWriteTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetLastWriteTime(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetLastWriteTime),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void GetLastWriteTimeUtc_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();

		sut.File.GetLastWriteTimeUtc(fileHandle);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetLastWriteTimeUtc),
			fileHandle);
	}
#endif

	[SkippableFact]
	public void GetLastWriteTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.GetLastWriteTimeUtc(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetLastWriteTimeUtc),
			path);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void GetUnixFileMode_SafeFileHandle_ShouldRegisterCall()
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

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetUnixFileMode),
			fileHandle);
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[SkippableFact]
	public void GetUnixFileMode_String_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		#pragma warning disable CA1416
		sut.File.GetUnixFileMode(path);
		#pragma warning restore CA1416

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.GetUnixFileMode),
			path);
	}
#endif

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableFact]
	public void Move_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string sourceFileName = "foo";
		string destFileName = "bar";
		bool overwrite = true;

		sut.File.Move(sourceFileName, destFileName, overwrite);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Move),
			sourceFileName, destFileName, overwrite);
	}
#endif

	[SkippableFact]
	public void Move_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string sourceFileName = "foo";
		string destFileName = "bar";

		sut.File.Move(sourceFileName, destFileName);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Move),
			sourceFileName, destFileName);
	}

	[SkippableFact]
	public void Open_String_FileMode_FileAccess_FileShare_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = new();
		FileAccess access = new();
		FileShare share = new();

		sut.File.Open(path, mode, access, share);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Open),
			path, mode, access, share);
	}

	[SkippableFact]
	public void Open_String_FileMode_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = new();
		FileAccess access = new();

		sut.File.Open(path, mode, access);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Open),
			path, mode, access);
	}

	[SkippableFact]
	public void Open_String_FileMode_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		FileMode mode = new();

		sut.File.Open(path, mode);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Open),
			path, mode);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[SkippableFact]
	public void Open_String_FileStreamOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		FileStreamOptions options = new();

		sut.File.Open(path, options);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Open),
			path, options);
	}
#endif

	[SkippableFact]
	public void OpenRead_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.OpenRead(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.OpenRead),
			path);
	}

	[SkippableFact]
	public void OpenText_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.OpenText(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.OpenText),
			path);
	}

	[SkippableFact]
	public void OpenWrite_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.File.OpenWrite(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.OpenWrite),
			path);
	}

	[SkippableFact]
	public void ReadAllBytes_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.ReadAllBytes(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllBytes),
			path);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableFact]
	public async Task ReadAllBytesAsync_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllBytesAsync(path, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllBytesAsync),
			path, cancellationToken);
	}
#endif

	[SkippableFact]
	public void ReadAllLines_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.ReadAllLines(path, encoding);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllLines),
			path, encoding);
	}

	[SkippableFact]
	public void ReadAllLines_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.ReadAllLines(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllLines),
			path);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableFact]
	public async Task ReadAllLinesAsync_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllLinesAsync(path, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllLinesAsync),
			path, cancellationToken);
	}

	[SkippableFact]
	public async Task ReadAllLinesAsync_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllLinesAsync(path, encoding, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllLinesAsync),
			path, encoding, cancellationToken);
	}
#endif

	[SkippableFact]
	public void ReadAllText_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.ReadAllText(path, encoding);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllText),
			path, encoding);
	}

	[SkippableFact]
	public void ReadAllText_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.ReadAllText(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllText),
			path);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableFact]
	public async Task ReadAllTextAsync_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllTextAsync(path, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllTextAsync),
			path, cancellationToken);
	}

	[SkippableFact]
	public async Task ReadAllTextAsync_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.ReadAllTextAsync(path, encoding, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadAllTextAsync),
			path, encoding, cancellationToken);
	}
#endif

	[SkippableFact]
	public void ReadLines_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.ReadLines(path, encoding);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadLines),
			path, encoding);
	}

	[SkippableFact]
	public void ReadLines_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";

		sut.File.ReadLines(path);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadLines),
			path);
	}

#if FEATURE_FILESYSTEM_NET7
	[SkippableFact]
	public void ReadLinesAsync_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		sut.File.ReadLinesAsync(path, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadLinesAsync),
			path, cancellationToken);
	}

	[SkippableFact]
	public void ReadLinesAsync_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		sut.File.ReadLinesAsync(path, encoding, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ReadLinesAsync),
			path, encoding, cancellationToken);
	}
#endif

	[SkippableFact]
	public void Replace_String_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string sourceFileName = "foo";
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";
		bool ignoreMetadataErrors = true;

		sut.File.Replace(sourceFileName, destinationFileName, destinationBackupFileName,
			ignoreMetadataErrors);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Replace),
			sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
	}

	[SkippableFact]
	public void Replace_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string sourceFileName = "foo";
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";

		sut.File.Replace(sourceFileName, destinationFileName, destinationBackupFileName);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.Replace),
			sourceFileName, destinationFileName, destinationBackupFileName);
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void ResolveLinkTarget_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string linkPath = "foo";
		bool returnFinalTarget = true;

		sut.File.ResolveLinkTarget(linkPath, returnFinalTarget);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.ResolveLinkTarget),
			linkPath, returnFinalTarget);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void SetAttributes_SafeFileHandle_FileAttributes_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		FileAttributes fileAttributes = new();

		sut.File.SetAttributes(fileHandle, fileAttributes);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetAttributes),
			fileHandle, fileAttributes);
	}
#endif

	[SkippableFact]
	public void SetAttributes_String_FileAttributes_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		FileAttributes fileAttributes = new();

		sut.File.SetAttributes(path, fileAttributes);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetAttributes),
			path, fileAttributes);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void SetCreationTime_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime creationTime = new();

		sut.File.SetCreationTime(fileHandle, creationTime);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetCreationTime),
			fileHandle, creationTime);
	}
#endif

	[SkippableFact]
	public void SetCreationTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime creationTime = new();

		sut.File.SetCreationTime(path, creationTime);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetCreationTime),
			path, creationTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void SetCreationTimeUtc_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime creationTimeUtc = new();

		sut.File.SetCreationTimeUtc(fileHandle, creationTimeUtc);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetCreationTimeUtc),
			fileHandle, creationTimeUtc);
	}
#endif

	[SkippableFact]
	public void SetCreationTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime creationTimeUtc = new();

		sut.File.SetCreationTimeUtc(path, creationTimeUtc);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetCreationTimeUtc),
			path, creationTimeUtc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void SetLastAccessTime_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime lastAccessTime = new();

		sut.File.SetLastAccessTime(fileHandle, lastAccessTime);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetLastAccessTime),
			fileHandle, lastAccessTime);
	}
#endif

	[SkippableFact]
	public void SetLastAccessTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime lastAccessTime = new();

		sut.File.SetLastAccessTime(path, lastAccessTime);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetLastAccessTime),
			path, lastAccessTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void SetLastAccessTimeUtc_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime lastAccessTimeUtc = new();

		sut.File.SetLastAccessTimeUtc(fileHandle, lastAccessTimeUtc);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetLastAccessTimeUtc),
			fileHandle, lastAccessTimeUtc);
	}
#endif

	[SkippableFact]
	public void SetLastAccessTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime lastAccessTimeUtc = new();

		sut.File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetLastAccessTimeUtc),
			path, lastAccessTimeUtc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void SetLastWriteTime_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime lastWriteTime = new();

		sut.File.SetLastWriteTime(fileHandle, lastWriteTime);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetLastWriteTime),
			fileHandle, lastWriteTime);
	}
#endif

	[SkippableFact]
	public void SetLastWriteTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime lastWriteTime = new();

		sut.File.SetLastWriteTime(path, lastWriteTime);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetLastWriteTime),
			path, lastWriteTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void SetLastWriteTimeUtc_SafeFileHandle_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("foo")))
			.Initialize().WithFile("foo");
		SafeFileHandle fileHandle = new();
		DateTime lastWriteTimeUtc = new();

		sut.File.SetLastWriteTimeUtc(fileHandle, lastWriteTimeUtc);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetLastWriteTimeUtc),
			fileHandle, lastWriteTimeUtc);
	}
#endif

	[SkippableFact]
	public void SetLastWriteTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		DateTime lastWriteTimeUtc = new();

		sut.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetLastWriteTimeUtc),
			path, lastWriteTimeUtc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableFact]
	public void SetUnixFileMode_SafeFileHandle_UnixFileMode_ShouldRegisterCall()
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

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetUnixFileMode),
			fileHandle, mode);
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[SkippableFact]
	public void SetUnixFileMode_String_UnixFileMode_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string path = "foo";
		UnixFileMode mode = new();

		#pragma warning disable CA1416
		sut.File.SetUnixFileMode(path, mode);
		#pragma warning restore CA1416

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.SetUnixFileMode),
			path, mode);
	}
#endif

	[SkippableFact]
	public void WriteAllBytes_String_ByteArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		byte[] bytes = Encoding.UTF8.GetBytes("foo");

		sut.File.WriteAllBytes(path, bytes);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllBytes),
			path, bytes);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableFact]
	public async Task WriteAllBytesAsync_String_ByteArray_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		byte[] bytes = "foo"u8.ToArray();
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllBytesAsync(path, bytes, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllBytesAsync),
			path, bytes, cancellationToken);
	}
#endif

	[SkippableFact]
	public void WriteAllLines_String_IEnumerableString_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;

		sut.File.WriteAllLines(path, contents, encoding);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllLines),
			path, contents, encoding);
	}

	[SkippableFact]
	public void WriteAllLines_String_IEnumerableString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];

		sut.File.WriteAllLines(path, contents);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllLines),
			path, contents);
	}

	[SkippableFact]
	public void WriteAllLines_String_StringArray_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string[] contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;

		sut.File.WriteAllLines(path, contents, encoding);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllLines),
			path, contents, encoding);
	}

	[SkippableFact]
	public void WriteAllLines_String_StringArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string[] contents = ["foo", "bar"];

		sut.File.WriteAllLines(path, contents);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllLines),
			path, contents);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableFact]
	public async Task
		WriteAllLinesAsync_String_IEnumerableString_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllLinesAsync(path, contents, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllLinesAsync),
			path, contents, cancellationToken);
	}

	[SkippableFact]
	public async Task
		WriteAllLinesAsync_String_IEnumerableString_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		IEnumerable<string> contents = ["foo", "bar"];
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllLinesAsync(path, contents, encoding, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllLinesAsync),
			path, contents, encoding, cancellationToken);
	}
#endif

	[SkippableFact]
	public void WriteAllText_String_String_Encoding_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		Encoding encoding = Encoding.UTF8;

		sut.File.WriteAllText(path, contents, encoding);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllText),
			path, contents, encoding);
	}

	[SkippableFact]
	public void WriteAllText_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";

		sut.File.WriteAllText(path, contents);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllText),
			path, contents);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableFact]
	public async Task WriteAllTextAsync_String_String_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllTextAsync(path, contents, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllTextAsync),
			path, contents, cancellationToken);
	}

	[SkippableFact]
	public async Task
		WriteAllTextAsync_String_String_Encoding_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string contents = "foo";
		Encoding encoding = Encoding.UTF8;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.File.WriteAllTextAsync(path, contents, encoding, cancellationToken);

		sut.Statistics.File.ShouldOnlyContain(nameof(IFile.WriteAllTextAsync),
			path, contents, encoding, cancellationToken);
	}
#endif
}

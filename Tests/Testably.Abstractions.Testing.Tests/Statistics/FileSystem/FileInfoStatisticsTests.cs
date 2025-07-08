using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileInfoStatisticsTests
{
	[Fact]
	public async Task Method_AppendText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").AppendText();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.AppendText));
	}

	[Fact]
	public async Task Method_CopyTo_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";
		bool overwrite = true;

		sut.FileInfo.New("foo").CopyTo(destFileName, overwrite);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.CopyTo),
				destFileName, overwrite);
	}

	[Fact]
	public async Task Method_CopyTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";

		sut.FileInfo.New("foo").CopyTo(destFileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.CopyTo),
				destFileName);
	}

	[Fact]
	public async Task Method_Create_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").Create();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Create));
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Method_CreateAsSymbolicLink_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string pathToTarget = "foo";

		sut.FileInfo.New("foo").CreateAsSymbolicLink(pathToTarget);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.CreateAsSymbolicLink),
				pathToTarget);
	}
#endif

	[Fact]
	public async Task Method_CreateText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").CreateText();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.CreateText));
	}

	[Fact]
	public async Task Method_Decrypt_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();

#pragma warning disable CA1416
		sut.FileInfo.New("foo").Decrypt();
#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Decrypt));
	}

	[Fact]
	public async Task Method_Delete_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").Delete();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Delete));
	}

	[Fact]
	public async Task Method_Encrypt_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();

#pragma warning disable CA1416
		sut.FileInfo.New("foo").Encrypt();
#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Encrypt));
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[Fact]
	public async Task Method_MoveTo_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";
		bool overwrite = true;

		sut.FileInfo.New("foo").MoveTo(destFileName, overwrite);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.MoveTo),
				destFileName, overwrite);
	}
#endif

	[Fact]
	public async Task Method_MoveTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";

		sut.FileInfo.New("foo").MoveTo(destFileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.MoveTo),
				destFileName);
	}

	[Fact]
	public async Task Method_Open_FileMode_FileAccess_FileShare_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();
		FileAccess access = new();
		FileShare share = new();

		sut.FileInfo.New("foo").Open(mode, access, share);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Open),
				mode, access, share);
	}

	[Fact]
	public async Task Method_Open_FileMode_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();
		FileAccess access = new();

		sut.FileInfo.New("foo").Open(mode, access);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Open),
				mode, access);
	}

	[Fact]
	public async Task Method_Open_FileMode_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();

		sut.FileInfo.New("foo").Open(mode);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Open),
				mode);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[Fact]
	public async Task Method_Open_FileStreamOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		FileStreamOptions options = new();

		sut.FileInfo.New("foo").Open(options);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Open),
				options);
	}
#endif

	[Fact]
	public async Task Method_OpenRead_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").OpenRead();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.OpenRead));
	}

	[Fact]
	public async Task Method_OpenText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").OpenText();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.OpenText));
	}

	[Fact]
	public async Task Method_OpenWrite_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").OpenWrite();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.OpenWrite));
	}

	[Fact]
	public async Task Method_Refresh_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").Refresh();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Refresh));
	}

	[Fact]
	public async Task Method_Replace_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";
		bool ignoreMetadataErrors = true;

		sut.FileInfo.New("foo").Replace(destinationFileName, destinationBackupFileName,
			ignoreMetadataErrors);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Replace),
				destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
	}

	[Fact]
	public async Task Method_Replace_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";

		sut.FileInfo.New("foo").Replace(destinationFileName, destinationBackupFileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(nameof(IFileInfo.Replace),
				destinationFileName, destinationBackupFileName);
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Method_ResolveLinkTarget_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		bool returnFinalTarget = true;

		sut.FileInfo.New("foo").ResolveLinkTarget(returnFinalTarget);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsMethodCall(
				nameof(IFileInfo.ResolveLinkTarget),
				returnFinalTarget);
	}
#endif

	[Fact]
	public async Task Property_Attributes_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Attributes;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.Attributes));
	}

	[Fact]
	public async Task Property_Attributes_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		FileAttributes value = new();

		sut.FileInfo.New("foo").Attributes = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.Attributes));
	}

	[Fact]
	public async Task Property_CreationTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").CreationTime;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.CreationTime));
	}

	[Fact]
	public async Task Property_CreationTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.Now;

		sut.FileInfo.New("foo").CreationTime = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.CreationTime));
	}

	[Fact]
	public async Task Property_CreationTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").CreationTimeUtc;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.CreationTimeUtc));
	}

	[Fact]
	public async Task Property_CreationTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.UtcNow;

		sut.FileInfo.New("foo").CreationTimeUtc = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.CreationTimeUtc));
	}

	[Fact]
	public async Task Property_Directory_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Directory;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.Directory));
	}

	[Fact]
	public async Task Property_DirectoryName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").DirectoryName;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.DirectoryName));
	}

	[Fact]
	public async Task Property_Exists_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Exists;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"]).OnlyContainsPropertyGetAccess(nameof(IFileInfo.Exists));
	}

	[Fact]
	public async Task Property_Extension_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Extension;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.Extension));
	}

	[Fact]
	public async Task Property_FullName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").FullName;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.FullName));
	}

	[Fact]
	public async Task Property_IsReadOnly_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").IsReadOnly;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.IsReadOnly));
	}

	[Fact]
	public async Task Property_IsReadOnly_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		bool value = true;

		sut.FileInfo.New("foo").IsReadOnly = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.IsReadOnly));
	}

	[Fact]
	public async Task Property_LastAccessTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LastAccessTime;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.LastAccessTime));
	}

	[Fact]
	public async Task Property_LastAccessTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.Now;

		sut.FileInfo.New("foo").LastAccessTime = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.LastAccessTime));
	}

	[Fact]
	public async Task Property_LastAccessTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LastAccessTimeUtc;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.LastAccessTimeUtc));
	}

	[Fact]
	public async Task Property_LastAccessTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.UtcNow;

		sut.FileInfo.New("foo").LastAccessTimeUtc = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.LastAccessTimeUtc));
	}

	[Fact]
	public async Task Property_LastWriteTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LastWriteTime;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.LastWriteTime));
	}

	[Fact]
	public async Task Property_LastWriteTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.Now;

		sut.FileInfo.New("foo").LastWriteTime = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.LastWriteTime));
	}

	[Fact]
	public async Task Property_LastWriteTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LastWriteTimeUtc;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.LastWriteTimeUtc));
	}

	[Fact]
	public async Task Property_LastWriteTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.UtcNow;

		sut.FileInfo.New("foo").LastWriteTimeUtc = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.LastWriteTimeUtc));
	}

	[Fact]
	public async Task Property_Length_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Length;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"]).OnlyContainsPropertyGetAccess(nameof(IFileInfo.Length));
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Property_LinkTarget_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LinkTarget;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.LinkTarget));
	}
#endif

	[Fact]
	public async Task Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Name;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"]).OnlyContainsPropertyGetAccess(nameof(IFileInfo.Name));
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public async Task Property_UnixFileMode_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").UnixFileMode;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileInfo.UnixFileMode));
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public async Task Property_UnixFileMode_Set_ShouldRegisterPropertyAccess()
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		UnixFileMode value = new();

#pragma warning disable CA1416
		sut.FileInfo.New("foo").UnixFileMode = value;
#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileInfo.UnixFileMode));
	}
#endif

	[Fact]
	public async Task ToString_ShouldBeFileInfoWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.FileInfo[@"\\some\path"];

		string? result = sut.ToString();

		await That(result).IsEqualTo(@"FileInfo[\\some\path]");
	}
}

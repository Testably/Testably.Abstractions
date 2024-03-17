using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileInfoStatisticsTests
{
	[SkippableFact]
	public void Method_AppendText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").AppendText();

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.AppendText));
	}

	[SkippableFact]
	public void Method_CopyTo_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";
		bool overwrite = true;

		sut.FileInfo.New("foo").CopyTo(destFileName, overwrite);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.CopyTo),
				destFileName, overwrite);
	}

	[SkippableFact]
	public void Method_CopyTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";

		sut.FileInfo.New("foo").CopyTo(destFileName);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.CopyTo),
				destFileName);
	}

	[SkippableFact]
	public void Method_Create_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").Create();

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Create));
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void Method_CreateAsSymbolicLink_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string pathToTarget = "foo";

		sut.FileInfo.New("foo").CreateAsSymbolicLink(pathToTarget);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.CreateAsSymbolicLink),
				pathToTarget);
	}
#endif

	[SkippableFact]
	public void Method_CreateText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").CreateText();

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.CreateText));
	}

	[SkippableFact]
	public void Method_Decrypt_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();

		#pragma warning disable CA1416
		sut.FileInfo.New("foo").Decrypt();
		#pragma warning restore CA1416

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Decrypt));
	}

	[SkippableFact]
	public void Method_Delete_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").Delete();

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Delete));
	}

	[SkippableFact]
	public void Method_Encrypt_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();

		#pragma warning disable CA1416
		sut.FileInfo.New("foo").Encrypt();
		#pragma warning restore CA1416

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Encrypt));
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableFact]
	public void Method_MoveTo_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";
		bool overwrite = true;

		sut.FileInfo.New("foo").MoveTo(destFileName, overwrite);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.MoveTo),
				destFileName, overwrite);
	}
#endif

	[SkippableFact]
	public void Method_MoveTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";

		sut.FileInfo.New("foo").MoveTo(destFileName);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.MoveTo),
				destFileName);
	}

	[SkippableFact]
	public void Method_Open_FileMode_FileAccess_FileShare_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();
		FileAccess access = new();
		FileShare share = new();

		sut.FileInfo.New("foo").Open(mode, access, share);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Open),
				mode, access, share);
	}

	[SkippableFact]
	public void Method_Open_FileMode_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();
		FileAccess access = new();

		sut.FileInfo.New("foo").Open(mode, access);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Open),
				mode, access);
	}

	[SkippableFact]
	public void Method_Open_FileMode_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();

		sut.FileInfo.New("foo").Open(mode);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Open),
				mode);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[SkippableFact]
	public void Method_Open_FileStreamOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		FileStreamOptions options = new();

		sut.FileInfo.New("foo").Open(options);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Open),
				options);
	}
#endif

	[SkippableFact]
	public void Method_OpenRead_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").OpenRead();

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.OpenRead));
	}

	[SkippableFact]
	public void Method_OpenText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").OpenText();

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.OpenText));
	}

	[SkippableFact]
	public void Method_OpenWrite_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").OpenWrite();

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.OpenWrite));
	}

	[SkippableFact]
	public void Method_Refresh_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").Refresh();

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Refresh));
	}

	[SkippableFact]
	public void Method_Replace_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";
		bool ignoreMetadataErrors = true;

		sut.FileInfo.New("foo").Replace(destinationFileName, destinationBackupFileName,
			ignoreMetadataErrors);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Replace),
				destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
	}

	[SkippableFact]
	public void Method_Replace_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";

		sut.FileInfo.New("foo").Replace(destinationFileName, destinationBackupFileName);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileInfo.Replace),
				destinationFileName, destinationBackupFileName);
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void Method_ResolveLinkTarget_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		bool returnFinalTarget = true;

		sut.FileInfo.New("foo").ResolveLinkTarget(returnFinalTarget);

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainMethodCall(
				nameof(IFileInfo.ResolveLinkTarget),
				returnFinalTarget);
	}
#endif

	[SkippableFact]
	public void Property_Attributes_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Attributes;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.Attributes));
	}

	[SkippableFact]
	public void Property_Attributes_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		FileAttributes value = new();

		sut.FileInfo.New("foo").Attributes = value;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.Attributes));
	}

	[SkippableFact]
	public void Property_CreationTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").CreationTime;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.CreationTime));
	}

	[SkippableFact]
	public void Property_CreationTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.Now;

		sut.FileInfo.New("foo").CreationTime = value;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.CreationTime));
	}

	[SkippableFact]
	public void Property_CreationTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").CreationTimeUtc;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.CreationTimeUtc));
	}

	[SkippableFact]
	public void Property_CreationTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.UtcNow;

		sut.FileInfo.New("foo").CreationTimeUtc = value;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.CreationTimeUtc));
	}

	[SkippableFact]
	public void Property_Directory_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Directory;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.Directory));
	}

	[SkippableFact]
	public void Property_DirectoryName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").DirectoryName;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.DirectoryName));
	}

	[SkippableFact]
	public void Property_Exists_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Exists;

		sut.Statistics.FileInfo["foo"].ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.Exists));
	}

	[SkippableFact]
	public void Property_Extension_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Extension;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.Extension));
	}

	[SkippableFact]
	public void Property_FullName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").FullName;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.FullName));
	}

	[SkippableFact]
	public void Property_IsReadOnly_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").IsReadOnly;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.IsReadOnly));
	}

	[SkippableFact]
	public void Property_IsReadOnly_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		bool value = true;

		sut.FileInfo.New("foo").IsReadOnly = value;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.IsReadOnly));
	}

	[SkippableFact]
	public void Property_LastAccessTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LastAccessTime;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.LastAccessTime));
	}

	[SkippableFact]
	public void Property_LastAccessTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.Now;

		sut.FileInfo.New("foo").LastAccessTime = value;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.LastAccessTime));
	}

	[SkippableFact]
	public void Property_LastAccessTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LastAccessTimeUtc;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.LastAccessTimeUtc));
	}

	[SkippableFact]
	public void Property_LastAccessTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.UtcNow;

		sut.FileInfo.New("foo").LastAccessTimeUtc = value;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.LastAccessTimeUtc));
	}

	[SkippableFact]
	public void Property_LastWriteTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LastWriteTime;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.LastWriteTime));
	}

	[SkippableFact]
	public void Property_LastWriteTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.Now;

		sut.FileInfo.New("foo").LastWriteTime = value;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.LastWriteTime));
	}

	[SkippableFact]
	public void Property_LastWriteTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LastWriteTimeUtc;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.LastWriteTimeUtc));
	}

	[SkippableFact]
	public void Property_LastWriteTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		DateTime value = DateTime.UtcNow;

		sut.FileInfo.New("foo").LastWriteTimeUtc = value;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.LastWriteTimeUtc));
	}

	[SkippableFact]
	public void Property_Length_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Length;

		sut.Statistics.FileInfo["foo"].ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.Length));
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void Property_LinkTarget_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").LinkTarget;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.LinkTarget));
	}
#endif

	[SkippableFact]
	public void Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").Name;

		sut.Statistics.FileInfo["foo"].ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.Name));
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[SkippableFact]
	public void Property_UnixFileMode_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileInfo.New("foo").UnixFileMode;

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileInfo.UnixFileMode));
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[SkippableFact]
	public void Property_UnixFileMode_Set_ShouldRegisterPropertyAccess()
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		UnixFileMode value = new();

		#pragma warning disable CA1416
		sut.FileInfo.New("foo").UnixFileMode = value;
		#pragma warning restore CA1416

		sut.Statistics.FileInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileInfo.UnixFileMode));
	}
#endif

	[SkippableFact]
	public void ToString_ShouldBeDirectoryInfoWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.FileInfo[@"\\some\path"];

		string? result = sut.ToString();

		result.Should().Be(@"FileInfo[\\some\path]");
	}
}

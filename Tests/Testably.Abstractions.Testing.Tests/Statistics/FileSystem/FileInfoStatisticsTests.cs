using System.IO;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileInfoStatisticsTests
{
	[SkippableFact]
	public void AppendText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").AppendText();

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.AppendText));
	}

	[SkippableFact]
	public void CopyTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";

		sut.FileInfo.New("foo").CopyTo(destFileName);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.CopyTo),
		destFileName);
	}

	[SkippableFact]
	public void CopyTo_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";
		bool overwrite = true;

		sut.FileInfo.New("foo").CopyTo(destFileName, overwrite);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.CopyTo),
		destFileName, overwrite);
	}

	[SkippableFact]
	public void Create_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").Create();

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Create));
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void CreateAsSymbolicLink_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string pathToTarget = "foo";

		sut.FileInfo.New("foo").CreateAsSymbolicLink(pathToTarget);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.CreateAsSymbolicLink),
		pathToTarget);
	}
#endif

	[SkippableFact]
	public void CreateText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").CreateText();

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.CreateText));
	}

	[SkippableFact]
	public void Decrypt_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();

		#pragma warning disable CA1416
		sut.FileInfo.New("foo").Decrypt();
		#pragma warning restore CA1416

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Decrypt));
	}

	[SkippableFact]
	public void Delete_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").Delete();

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Delete));
	}

	[SkippableFact]
	public void Encrypt_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();

		#pragma warning disable CA1416
		sut.FileInfo.New("foo").Encrypt();
		#pragma warning restore CA1416

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Encrypt));
	}

	[SkippableFact]
	public void MoveTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";

		sut.FileInfo.New("foo").MoveTo(destFileName);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.MoveTo),
		destFileName);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableFact]
	public void MoveTo_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		string destFileName = "bar";
		bool overwrite = true;

		sut.FileInfo.New("foo").MoveTo(destFileName, overwrite);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.MoveTo),
		destFileName, overwrite);
	}
#endif

	[SkippableFact]
	public void Open_FileMode_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();

		sut.FileInfo.New("foo").Open(mode);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Open),
		mode);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[SkippableFact]
	public void Open_FileStreamOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");
		FileStreamOptions options = new();

		sut.FileInfo.New("foo").Open(options);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Open),
		options);
	}
#endif

	[SkippableFact]
	public void Open_FileMode_FileAccess_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();
		FileAccess access = new();

		sut.FileInfo.New("foo").Open(mode, access);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Open),
		mode, access);
	}

	[SkippableFact]
	public void Open_FileMode_FileAccess_FileShare_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileMode mode = new();
		FileAccess access = new();
		FileShare share = new();

		sut.FileInfo.New("foo").Open(mode, access, share);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Open),
		mode, access, share);
	}

	[SkippableFact]
	public void OpenRead_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").OpenRead();

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.OpenRead));
	}

	[SkippableFact]
	public void OpenText_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		sut.FileInfo.New("foo").OpenText();

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.OpenText));
	}

	[SkippableFact]
	public void OpenWrite_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").OpenWrite();

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.OpenWrite));
	}

	[SkippableFact]
	public void Refresh_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileInfo.New("foo").Refresh();

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Refresh));
	}

	[SkippableFact]
	public void Replace_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";

		sut.FileInfo.New("foo").Replace(destinationFileName, destinationBackupFileName);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Replace),
		destinationFileName, destinationBackupFileName);
	}

	[SkippableFact]
	public void Replace_String_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo").WithFile("bar");
		string destinationFileName = "bar";
		string destinationBackupFileName = "xyz";
		bool ignoreMetadataErrors = true;

		sut.FileInfo.New("foo").Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.Replace),
		destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void ResolveLinkTarget_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		bool returnFinalTarget = true;

		sut.FileInfo.New("foo").ResolveLinkTarget(returnFinalTarget);

		sut.Statistics.FileInfo["foo"].ShouldOnlyContain(nameof(IFileInfo.ResolveLinkTarget),
		returnFinalTarget);
	}
#endif
}

using System.IO;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryInfoStatisticsTests
{
	[SkippableFact]
	public void Create_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").Create();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.Create));
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void CreateAsSymbolicLink_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string pathToTarget = "foo";

		sut.DirectoryInfo.New("foo").CreateAsSymbolicLink(pathToTarget);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.CreateAsSymbolicLink),
		pathToTarget);
	}
#endif

	[SkippableFact]
	public void CreateSubdirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.DirectoryInfo.New("foo").CreateSubdirectory(path);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.CreateSubdirectory),
		path);
	}

	[SkippableFact]
	public void Delete_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").Delete();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.Delete));
	}

	[SkippableFact]
	public void Delete_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool recursive = true;

		sut.DirectoryInfo.New("foo").Delete(recursive);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.Delete),
		recursive);
	}

	[SkippableFact]
	public void EnumerateDirectories_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateDirectories();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateDirectories));
	}

	[SkippableFact]
	public void EnumerateDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateDirectories),
		searchPattern);
	}

	[SkippableFact]
	public void EnumerateDirectories_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateDirectories),
		searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void EnumerateDirectories_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateDirectories),
		searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void EnumerateFiles_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateFiles();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateFiles));
	}

	[SkippableFact]
	public void EnumerateFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateFiles),
		searchPattern);
	}

	[SkippableFact]
	public void EnumerateFiles_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateFiles),
		searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void EnumerateFiles_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateFiles),
		searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void EnumerateFileSystemInfos_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateFileSystemInfos));
	}

	[SkippableFact]
	public void EnumerateFileSystemInfos_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
		searchPattern);
	}

	[SkippableFact]
	public void EnumerateFileSystemInfos_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
		searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void EnumerateFileSystemInfos_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
		searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void GetDirectories_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetDirectories();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetDirectories));
	}

	[SkippableFact]
	public void GetDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetDirectories),
		searchPattern);
	}

	[SkippableFact]
	public void GetDirectories_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetDirectories),
		searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void GetDirectories_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetDirectories),
		searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void GetFiles_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetFiles();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetFiles));
	}

	[SkippableFact]
	public void GetFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetFiles),
		searchPattern);
	}

	[SkippableFact]
	public void GetFiles_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetFiles),
		searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void GetFiles_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetFiles),
		searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void GetFileSystemInfos_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetFileSystemInfos();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetFileSystemInfos));
	}

	[SkippableFact]
	public void GetFileSystemInfos_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetFileSystemInfos),
		searchPattern);
	}

	[SkippableFact]
	public void GetFileSystemInfos_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetFileSystemInfos),
		searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void GetFileSystemInfos_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.GetFileSystemInfos),
		searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void MoveTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string destDirName = "bar";

		sut.DirectoryInfo.New("foo").MoveTo(destDirName);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.MoveTo),
		destDirName);
	}

	[SkippableFact]
	public void Refresh_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").Refresh();

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.Refresh));
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void ResolveLinkTarget_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		bool returnFinalTarget = true;

		sut.DirectoryInfo.New("foo").ResolveLinkTarget(returnFinalTarget);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContain(nameof(IDirectoryInfo.ResolveLinkTarget),
		returnFinalTarget);
	}
#endif
}

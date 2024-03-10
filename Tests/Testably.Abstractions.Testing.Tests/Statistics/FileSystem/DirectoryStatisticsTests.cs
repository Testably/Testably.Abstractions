using System.IO;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryStatisticsTests
{
	[Fact]
	public void CreateDirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.CreateDirectory(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.CreateDirectory),
			path);
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[SkippableFact]
	public void CreateDirectory_String_UnixFileMode_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		string path = "foo";
		UnixFileMode unixCreateMode = UnixFileMode.None;

		sut.Directory.CreateDirectory(path, unixCreateMode);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.CreateDirectory),
			path, unixCreateMode);
	}
#endif

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void CreateSymbolicLink_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string pathToTarget = "foo";

		sut.Directory.CreateSymbolicLink(path, pathToTarget);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.CreateSymbolicLink),
		path, pathToTarget);
	}
#endif

#if FEATURE_FILESYSTEM_NET7
	[SkippableFact]
	public void CreateTempSubdirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string prefix = "foo";

		sut.Directory.CreateTempSubdirectory(prefix);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.CreateTempSubdirectory),
		prefix);
	}
#endif

	[SkippableFact]
	public void Delete_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.Delete(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.Delete),
		path);
	}

	[SkippableFact]
	public void Delete_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		bool recursive = true;

		sut.Directory.Delete(path, recursive);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.Delete),
		path, recursive);
	}

	[SkippableFact]
	public void EnumerateDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateDirectories(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateDirectories),
		path);
	}

	[SkippableFact]
	public void EnumerateDirectories_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateDirectories(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateDirectories),
		path, searchPattern);
	}

	[SkippableFact]
	public void EnumerateDirectories_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateDirectories(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateDirectories),
		path, searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void EnumerateDirectories_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateDirectories(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateDirectories),
		path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void EnumerateFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateFiles(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateFiles),
		path);
	}

	[SkippableFact]
	public void EnumerateFiles_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateFiles(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateFiles),
		path, searchPattern);
	}

	[SkippableFact]
	public void EnumerateFiles_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateFiles(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateFiles),
		path, searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void EnumerateFiles_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateFiles(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateFiles),
		path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void EnumerateFileSystemEntries_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateFileSystemEntries(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateFileSystemEntries),
		path);
	}

	[SkippableFact]
	public void EnumerateFileSystemEntries_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateFileSystemEntries),
		path, searchPattern);
	}

	[SkippableFact]
	public void EnumerateFileSystemEntries_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateFileSystemEntries),
		path, searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void EnumerateFileSystemEntries_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.EnumerateFileSystemEntries),
		path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.Exists(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.Exists),
		path);
	}

	[SkippableFact]
	public void GetCreationTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetCreationTime(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetCreationTime),
		path);
	}

	[SkippableFact]
	public void GetCreationTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetCreationTimeUtc(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetCreationTimeUtc),
		path);
	}

	[SkippableFact]
	public void GetCurrentDirectory_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Directory.GetCurrentDirectory();

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetCurrentDirectory));
	}

	[SkippableFact]
	public void GetDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetDirectories(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetDirectories),
		path);
	}

	[SkippableFact]
	public void GetDirectories_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetDirectories(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetDirectories),
		path, searchPattern);
	}

	[SkippableFact]
	public void GetDirectories_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetDirectories(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetDirectories),
		path, searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void GetDirectories_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetDirectories(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetDirectories),
		path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void GetDirectoryRoot_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetDirectoryRoot(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetDirectoryRoot),
		path);
	}

	[SkippableFact]
	public void GetFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetFiles(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetFiles),
		path);
	}

	[SkippableFact]
	public void GetFiles_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetFiles(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetFiles),
		path, searchPattern);
	}

	[SkippableFact]
	public void GetFiles_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetFiles(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetFiles),
		path, searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void GetFiles_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetFiles(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetFiles),
		path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void GetFileSystemEntries_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetFileSystemEntries(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetFileSystemEntries),
		path);
	}

	[SkippableFact]
	public void GetFileSystemEntries_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetFileSystemEntries(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetFileSystemEntries),
		path, searchPattern);
	}

	[SkippableFact]
	public void GetFileSystemEntries_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetFileSystemEntries(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetFileSystemEntries),
		path, searchPattern, searchOption);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void GetFileSystemEntries_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetFileSystemEntries(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetFileSystemEntries),
		path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void GetLastAccessTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastAccessTime(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetLastAccessTime),
		path);
	}

	[SkippableFact]
	public void GetLastAccessTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastAccessTimeUtc(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetLastAccessTimeUtc),
		path);
	}

	[SkippableFact]
	public void GetLastWriteTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastWriteTime(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetLastWriteTime),
		path);
	}

	[SkippableFact]
	public void GetLastWriteTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastWriteTimeUtc(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetLastWriteTimeUtc),
		path);
	}

	[SkippableFact]
	public void GetLogicalDrives_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Directory.GetLogicalDrives();

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetLogicalDrives));
	}

	[SkippableFact]
	public void GetParent_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetParent(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.GetParent),
		path);
	}

	[SkippableFact]
	public void Move_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string sourceDirName = "foo";
		string destDirName = "bar";

		sut.Directory.Move(sourceDirName, destDirName);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.Move),
		sourceDirName, destDirName);
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void ResolveLinkTarget_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string linkPath = "foo";
		bool returnFinalTarget = true;

		sut.Directory.ResolveLinkTarget(linkPath, returnFinalTarget);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.ResolveLinkTarget),
		linkPath, returnFinalTarget);
	}
#endif

	[SkippableFact]
	public void SetCreationTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime creationTime = new();

		sut.Directory.SetCreationTime(path, creationTime);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.SetCreationTime),
		path, creationTime);
	}

	[SkippableFact]
	public void SetCreationTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime creationTimeUtc = new();

		sut.Directory.SetCreationTimeUtc(path, creationTimeUtc);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.SetCreationTimeUtc),
		path, creationTimeUtc);
	}

	[SkippableFact]
	public void SetCurrentDirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.SetCurrentDirectory(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.SetCurrentDirectory),
		path);
	}

	[SkippableFact]
	public void SetLastAccessTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastAccessTime = new();

		sut.Directory.SetLastAccessTime(path, lastAccessTime);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.SetLastAccessTime),
		path, lastAccessTime);
	}

	[SkippableFact]
	public void SetLastAccessTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastAccessTimeUtc = new();

		sut.Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.SetLastAccessTimeUtc),
		path, lastAccessTimeUtc);
	}

	[SkippableFact]
	public void SetLastWriteTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastWriteTime = new();

		sut.Directory.SetLastWriteTime(path, lastWriteTime);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.SetLastWriteTime),
		path, lastWriteTime);
	}

	[SkippableFact]
	public void SetLastWriteTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastWriteTimeUtc = new();

		sut.Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(IDirectory.SetLastWriteTimeUtc),
		path, lastWriteTimeUtc);
	}
}

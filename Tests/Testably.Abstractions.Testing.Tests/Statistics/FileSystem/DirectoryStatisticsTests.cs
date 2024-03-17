using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryStatisticsTests
{
	[Fact]
	public void Method_CreateDirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.CreateDirectory(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.CreateDirectory),
			path);
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[SkippableFact]
	public void Method_CreateDirectory_String_UnixFileMode_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		string path = "foo";
		UnixFileMode unixCreateMode = UnixFileMode.None;

		sut.Directory.CreateDirectory(path, unixCreateMode);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.CreateDirectory),
			path, unixCreateMode);
	}
#endif

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void Method_CreateSymbolicLink_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string pathToTarget = "foo";

		sut.Directory.CreateSymbolicLink(path, pathToTarget);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.CreateSymbolicLink),
			path, pathToTarget);
	}
#endif

#if FEATURE_FILESYSTEM_NET7
	[SkippableFact]
	public void Method_CreateTempSubdirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string prefix = "foo";

		sut.Directory.CreateTempSubdirectory(prefix);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.CreateTempSubdirectory),
			prefix);
	}
#endif

	[SkippableFact]
	public void Method_Delete_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		bool recursive = true;

		sut.Directory.Delete(path, recursive);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.Delete),
			path, recursive);
	}

	[SkippableFact]
	public void Method_Delete_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.Delete(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.Delete),
			path);
	}

	[SkippableFact]
	public void Method_EnumerateDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateDirectories(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_EnumerateDirectories_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateDirectories(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_EnumerateDirectories_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateDirectories(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_EnumerateDirectories_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateDirectories(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern);
	}

	[SkippableFact]
	public void Method_EnumerateFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateFiles(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.EnumerateFiles),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_EnumerateFiles_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateFiles(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.EnumerateFiles),
			path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_EnumerateFiles_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateFiles(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.EnumerateFiles),
			path, searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_EnumerateFiles_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateFiles(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.EnumerateFiles),
			path, searchPattern);
	}

	[SkippableFact]
	public void Method_EnumerateFileSystemEntries_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateFileSystemEntries(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void
		Method_EnumerateFileSystemEntries_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_EnumerateFileSystemEntries_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_EnumerateFileSystemEntries_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern);
	}

	[SkippableFact]
	public void Method_Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.Exists(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.Exists),
			path);
	}

	[SkippableFact]
	public void Method_GetCreationTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetCreationTime(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetCreationTime),
			path);
	}

	[SkippableFact]
	public void Method_GetCreationTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetCreationTimeUtc(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetCreationTimeUtc),
			path);
	}

	[SkippableFact]
	public void Method_GetCurrentDirectory_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Directory.GetCurrentDirectory();

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetCurrentDirectory));
	}

	[SkippableFact]
	public void Method_GetDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetDirectories(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectories),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_GetDirectories_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetDirectories(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectories),
			path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_GetDirectories_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetDirectories(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectories),
			path, searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_GetDirectories_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetDirectories(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectories),
			path, searchPattern);
	}

	[SkippableFact]
	public void Method_GetDirectoryRoot_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetDirectoryRoot(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectoryRoot),
			path);
	}

	[SkippableFact]
	public void Method_GetFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetFiles(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetFiles),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_GetFiles_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetFiles(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_GetFiles_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetFiles(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_GetFiles_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetFiles(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern);
	}

	[SkippableFact]
	public void Method_GetFileSystemEntries_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetFileSystemEntries(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_GetFileSystemEntries_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetFileSystemEntries(path, searchPattern, enumerationOptions);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_GetFileSystemEntries_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetFileSystemEntries(path, searchPattern, searchOption);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_GetFileSystemEntries_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetFileSystemEntries(path, searchPattern);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern);
	}

	[SkippableFact]
	public void Method_GetLastAccessTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastAccessTime(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetLastAccessTime),
			path);
	}

	[SkippableFact]
	public void Method_GetLastAccessTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastAccessTimeUtc(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetLastAccessTimeUtc),
			path);
	}

	[SkippableFact]
	public void Method_GetLastWriteTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastWriteTime(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetLastWriteTime),
			path);
	}

	[SkippableFact]
	public void Method_GetLastWriteTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastWriteTimeUtc(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetLastWriteTimeUtc),
			path);
	}

	[SkippableFact]
	public void Method_GetLogicalDrives_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Directory.GetLogicalDrives();

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetLogicalDrives));
	}

	[SkippableFact]
	public void Method_GetParent_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetParent(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetParent),
			path);
	}

	[SkippableFact]
	public void Method_Move_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string sourceDirName = "foo";
		string destDirName = "bar";

		sut.Directory.Move(sourceDirName, destDirName);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.Move),
			sourceDirName, destDirName);
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void Method_ResolveLinkTarget_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string linkPath = "foo";
		bool returnFinalTarget = true;

		sut.Directory.ResolveLinkTarget(linkPath, returnFinalTarget);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.ResolveLinkTarget),
			linkPath, returnFinalTarget);
	}
#endif

	[SkippableFact]
	public void Method_SetCreationTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime creationTime = new();

		sut.Directory.SetCreationTime(path, creationTime);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetCreationTime),
			path, creationTime);
	}

	[SkippableFact]
	public void Method_SetCreationTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime creationTimeUtc = new();

		sut.Directory.SetCreationTimeUtc(path, creationTimeUtc);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetCreationTimeUtc),
			path, creationTimeUtc);
	}

	[SkippableFact]
	public void Method_SetCurrentDirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.SetCurrentDirectory(path);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetCurrentDirectory),
			path);
	}

	[SkippableFact]
	public void Method_SetLastAccessTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastAccessTime = new();

		sut.Directory.SetLastAccessTime(path, lastAccessTime);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetLastAccessTime),
			path, lastAccessTime);
	}

	[SkippableFact]
	public void Method_SetLastAccessTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastAccessTimeUtc = new();

		sut.Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.SetLastAccessTimeUtc),
			path, lastAccessTimeUtc);
	}

	[SkippableFact]
	public void Method_SetLastWriteTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastWriteTime = new();

		sut.Directory.SetLastWriteTime(path, lastWriteTime);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetLastWriteTime),
			path, lastWriteTime);
	}

	[SkippableFact]
	public void Method_SetLastWriteTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastWriteTimeUtc = new();

		sut.Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetLastWriteTimeUtc),
			path, lastWriteTimeUtc);
	}

	[SkippableFact]
	public void ToString_ShouldBeDirectory()
	{
		IStatistics sut = new MockFileSystem().Statistics.Directory;

		string? result = sut.ToString();

		result.Should().Be("Directory");
	}
}

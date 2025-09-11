using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryStatisticsTests
{
	[Fact]
	public async Task Method_CreateDirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.CreateDirectory(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.CreateDirectory),
			path);
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public async Task Method_CreateDirectory_String_UnixFileMode_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		string path = "foo";
		UnixFileMode unixCreateMode = UnixFileMode.None;

		sut.Directory.CreateDirectory(path, unixCreateMode);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.CreateDirectory),
			path, unixCreateMode);
	}
#endif

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Method_CreateSymbolicLink_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string pathToTarget = "foo";

		sut.Directory.CreateSymbolicLink(path, pathToTarget);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.CreateSymbolicLink),
			path, pathToTarget);
	}
#endif

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
	[Fact]
	public async Task Method_CreateTempSubdirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string prefix = "foo";

		sut.Directory.CreateTempSubdirectory(prefix);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.CreateTempSubdirectory),
			prefix);
	}
#endif

	[Fact]
	public async Task Method_Delete_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		bool recursive = true;

		sut.Directory.Delete(path, recursive);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.Delete),
			path, recursive);
	}

	[Fact]
	public async Task Method_Delete_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.Delete(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.Delete),
			path);
	}

	[Fact]
	public async Task Method_EnumerateDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.EnumerateDirectories(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_EnumerateDirectories_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateDirectories(path, searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_EnumerateDirectories_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateDirectories(path, searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_EnumerateDirectories_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateDirectories(path, searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern);
	}

	[Fact]
	public async Task Method_EnumerateFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.EnumerateFiles(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateFiles),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_EnumerateFiles_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateFiles(path, searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.EnumerateFiles),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_EnumerateFiles_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateFiles(path, searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateFiles),
			path, searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_EnumerateFiles_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateFiles(path, searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateFiles),
			path, searchPattern);
	}

	[Fact]
	public async Task Method_EnumerateFileSystemEntries_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.EnumerateFileSystemEntries(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_EnumerateFileSystemEntries_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task
		Method_EnumerateFileSystemEntries_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_EnumerateFileSystemEntries_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern);
	}

	[Fact]
	public async Task Method_Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.Exists(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.Exists),
			path);
	}

	[Fact]
	public async Task Method_GetCreationTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetCreationTime(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetCreationTime),
			path);
	}

	[Fact]
	public async Task Method_GetCreationTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetCreationTimeUtc(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetCreationTimeUtc),
			path);
	}

	[Fact]
	public async Task Method_GetCurrentDirectory_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Directory.GetCurrentDirectory();

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetCurrentDirectory));
	}

	[Fact]
	public async Task Method_GetDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetDirectories(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetDirectories),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_GetDirectories_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetDirectories(path, searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.GetDirectories),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_GetDirectories_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetDirectories(path, searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetDirectories),
			path, searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_GetDirectories_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetDirectories(path, searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetDirectories),
			path, searchPattern);
	}

	[Fact]
	public async Task Method_GetDirectoryRoot_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetDirectoryRoot(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetDirectoryRoot),
			path);
	}

	[Fact]
	public async Task Method_GetFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetFiles(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.GetFiles),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_GetFiles_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetFiles(path, searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_GetFiles_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetFiles(path, searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_GetFiles_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetFiles(path, searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern);
	}

	[Fact]
	public async Task Method_GetFileSystemEntries_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetFileSystemEntries(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_GetFileSystemEntries_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetFileSystemEntries(path, searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_GetFileSystemEntries_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetFileSystemEntries(path, searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_GetFileSystemEntries_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetFileSystemEntries(path, searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern);
	}

	[Fact]
	public async Task Method_GetLastAccessTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastAccessTime(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetLastAccessTime),
			path);
	}

	[Fact]
	public async Task Method_GetLastAccessTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastAccessTimeUtc(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetLastAccessTimeUtc),
			path);
	}

	[Fact]
	public async Task Method_GetLastWriteTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastWriteTime(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetLastWriteTime),
			path);
	}

	[Fact]
	public async Task Method_GetLastWriteTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastWriteTimeUtc(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.GetLastWriteTimeUtc),
			path);
	}

	[Fact]
	public async Task Method_GetLogicalDrives_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Directory.GetLogicalDrives();

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory)
			.OnlyContainsMethodCall(nameof(IDirectory.GetLogicalDrives));
	}

	[Fact]
	public async Task Method_GetParent_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetParent(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.GetParent),
			path);
	}

	[Fact]
	public async Task Method_Move_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string sourceDirName = "foo";
		string destDirName = "bar";

		sut.Directory.Move(sourceDirName, destDirName);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.Move),
			sourceDirName, destDirName);
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Method_ResolveLinkTarget_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string linkPath = "foo";
		bool returnFinalTarget = true;

		sut.Directory.ResolveLinkTarget(linkPath, returnFinalTarget);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(nameof(IDirectory.ResolveLinkTarget),
			linkPath, returnFinalTarget);
	}
#endif

	[Fact]
	public async Task Method_SetCreationTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime creationTime = new();

		sut.Directory.SetCreationTime(path, creationTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.SetCreationTime),
			path, creationTime);
	}

	[Fact]
	public async Task Method_SetCreationTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime creationTimeUtc = new();

		sut.Directory.SetCreationTimeUtc(path, creationTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.SetCreationTimeUtc),
			path, creationTimeUtc);
	}

	[Fact]
	public async Task Method_SetCurrentDirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.SetCurrentDirectory(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.SetCurrentDirectory),
			path);
	}

	[Fact]
	public async Task Method_SetLastAccessTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastAccessTime = new();

		sut.Directory.SetLastAccessTime(path, lastAccessTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.SetLastAccessTime),
			path, lastAccessTime);
	}

	[Fact]
	public async Task Method_SetLastAccessTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastAccessTimeUtc = new();

		sut.Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.SetLastAccessTimeUtc),
			path, lastAccessTimeUtc);
	}

	[Fact]
	public async Task Method_SetLastWriteTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastWriteTime = new();

		sut.Directory.SetLastWriteTime(path, lastWriteTime);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.SetLastWriteTime),
			path, lastWriteTime);
	}

	[Fact]
	public async Task Method_SetLastWriteTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastWriteTimeUtc = new();

		sut.Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Directory).OnlyContainsMethodCall(
			nameof(IDirectory.SetLastWriteTimeUtc),
			path, lastWriteTimeUtc);
	}

	[Fact]
	public async Task ToString_ShouldBeDirectory()
	{
		IStatistics sut = new MockFileSystem().Statistics.Directory;

		string? result = sut.ToString();

		await That(result).IsEqualTo("Directory");
	}
}

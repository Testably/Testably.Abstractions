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

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.CreateDirectory),
			path);
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public void Method_CreateDirectory_String_UnixFileMode_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		string path = "foo";
		UnixFileMode unixCreateMode = UnixFileMode.None;

		sut.Directory.CreateDirectory(path, unixCreateMode);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.CreateDirectory),
			path, unixCreateMode);
	}
#endif

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public void Method_CreateSymbolicLink_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string pathToTarget = "foo";

		sut.Directory.CreateSymbolicLink(path, pathToTarget);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.CreateSymbolicLink),
			path, pathToTarget);
	}
#endif

#if FEATURE_FILESYSTEM_NET7
	[Fact]
	public void Method_CreateTempSubdirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string prefix = "foo";

		sut.Directory.CreateTempSubdirectory(prefix);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.CreateTempSubdirectory),
			prefix);
	}
#endif

	[Fact]
	public void Method_Delete_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		bool recursive = true;

		sut.Directory.Delete(path, recursive);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.Delete),
			path, recursive);
	}

	[Fact]
	public void Method_Delete_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.Delete(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.Delete),
			path);
	}

	[Fact]
	public void Method_EnumerateDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateDirectories(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_EnumerateDirectories_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateDirectories(path, searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_EnumerateDirectories_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateDirectories(path, searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern, searchOption);
	}

	[Fact]
	public void Method_EnumerateDirectories_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateDirectories(path, searchPattern);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateDirectories),
			path, searchPattern);
	}

	[Fact]
	public void Method_EnumerateFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateFiles(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.EnumerateFiles),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_EnumerateFiles_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateFiles(path, searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.EnumerateFiles),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_EnumerateFiles_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateFiles(path, searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.EnumerateFiles),
			path, searchPattern, searchOption);
	}

	[Fact]
	public void Method_EnumerateFiles_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateFiles(path, searchPattern);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.EnumerateFiles),
			path, searchPattern);
	}

	[Fact]
	public void Method_EnumerateFileSystemEntries_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.EnumerateFileSystemEntries(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void
		Method_EnumerateFileSystemEntries_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_EnumerateFileSystemEntries_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern, searchOption);
	}

	[Fact]
	public void Method_EnumerateFileSystemEntries_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.EnumerateFileSystemEntries(path, searchPattern);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.EnumerateFileSystemEntries),
			path, searchPattern);
	}

	[Fact]
	public void Method_Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.Exists(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.Exists),
			path);
	}

	[Fact]
	public void Method_GetCreationTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetCreationTime(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetCreationTime),
			path);
	}

	[Fact]
	public void Method_GetCreationTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetCreationTimeUtc(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetCreationTimeUtc),
			path);
	}

	[Fact]
	public void Method_GetCurrentDirectory_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Directory.GetCurrentDirectory();

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetCurrentDirectory));
	}

	[Fact]
	public void Method_GetDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetDirectories(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectories),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_GetDirectories_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetDirectories(path, searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectories),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_GetDirectories_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetDirectories(path, searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectories),
			path, searchPattern, searchOption);
	}

	[Fact]
	public void Method_GetDirectories_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetDirectories(path, searchPattern);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectories),
			path, searchPattern);
	}

	[Fact]
	public void Method_GetDirectoryRoot_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetDirectoryRoot(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetDirectoryRoot),
			path);
	}

	[Fact]
	public void Method_GetFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetFiles(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetFiles),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_GetFiles_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetFiles(path, searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_GetFiles_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetFiles(path, searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern, searchOption);
	}

	[Fact]
	public void Method_GetFiles_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetFiles(path, searchPattern);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetFiles),
			path, searchPattern);
	}

	[Fact]
	public void Method_GetFileSystemEntries_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.GetFileSystemEntries(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_GetFileSystemEntries_String_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.Directory.GetFileSystemEntries(path, searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_GetFileSystemEntries_String_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.Directory.GetFileSystemEntries(path, searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern, searchOption);
	}

	[Fact]
	public void Method_GetFileSystemEntries_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string searchPattern = "foo";

		sut.Directory.GetFileSystemEntries(path, searchPattern);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetFileSystemEntries),
			path, searchPattern);
	}

	[Fact]
	public void Method_GetLastAccessTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastAccessTime(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetLastAccessTime),
			path);
	}

	[Fact]
	public void Method_GetLastAccessTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastAccessTimeUtc(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.GetLastAccessTimeUtc),
			path);
	}

	[Fact]
	public void Method_GetLastWriteTime_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastWriteTime(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetLastWriteTime),
			path);
	}

	[Fact]
	public void Method_GetLastWriteTimeUtc_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetLastWriteTimeUtc(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetLastWriteTimeUtc),
			path);
	}

	[Fact]
	public void Method_GetLogicalDrives_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Directory.GetLogicalDrives();

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetLogicalDrives));
	}

	[Fact]
	public void Method_GetParent_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.GetParent(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.GetParent),
			path);
	}

	[Fact]
	public void Method_Move_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string sourceDirName = "foo";
		string destDirName = "bar";

		sut.Directory.Move(sourceDirName, destDirName);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.Move),
			sourceDirName, destDirName);
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public void Method_ResolveLinkTarget_String_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string linkPath = "foo";
		bool returnFinalTarget = true;

		sut.Directory.ResolveLinkTarget(linkPath, returnFinalTarget);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.ResolveLinkTarget),
			linkPath, returnFinalTarget);
	}
#endif

	[Fact]
	public void Method_SetCreationTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime creationTime = new();

		sut.Directory.SetCreationTime(path, creationTime);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetCreationTime),
			path, creationTime);
	}

	[Fact]
	public void Method_SetCreationTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime creationTimeUtc = new();

		sut.Directory.SetCreationTimeUtc(path, creationTimeUtc);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetCreationTimeUtc),
			path, creationTimeUtc);
	}

	[Fact]
	public void Method_SetCurrentDirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.Directory.SetCurrentDirectory(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetCurrentDirectory),
			path);
	}

	[Fact]
	public void Method_SetLastAccessTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastAccessTime = new();

		sut.Directory.SetLastAccessTime(path, lastAccessTime);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetLastAccessTime),
			path, lastAccessTime);
	}

	[Fact]
	public void Method_SetLastAccessTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastAccessTimeUtc = new();

		sut.Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(
			nameof(IDirectory.SetLastAccessTimeUtc),
			path, lastAccessTimeUtc);
	}

	[Fact]
	public void Method_SetLastWriteTime_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastWriteTime = new();

		sut.Directory.SetLastWriteTime(path, lastWriteTime);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetLastWriteTime),
			path, lastWriteTime);
	}

	[Fact]
	public void Method_SetLastWriteTimeUtc_String_DateTime_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		DateTime lastWriteTimeUtc = new();

		sut.Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Directory.ShouldOnlyContainMethodCall(nameof(IDirectory.SetLastWriteTimeUtc),
			path, lastWriteTimeUtc);
	}

	[Fact]
	public void ToString_ShouldBeDirectory()
	{
		IStatistics sut = new MockFileSystem().Statistics.Directory;

		string? result = sut.ToString();

		result.Should().Be("Directory");
	}
}

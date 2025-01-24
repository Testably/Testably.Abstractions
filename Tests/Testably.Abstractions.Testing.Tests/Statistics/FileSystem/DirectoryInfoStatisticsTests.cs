using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryInfoStatisticsTests
{
	[Fact]
	public void Method_Create_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").Create();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.Create));
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public void Method_CreateAsSymbolicLink_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string pathToTarget = "foo";

		sut.DirectoryInfo.New("foo").CreateAsSymbolicLink(pathToTarget);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.CreateAsSymbolicLink),
				pathToTarget);
	}
#endif

	[Fact]
	public void Method_CreateSubdirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.DirectoryInfo.New("foo").CreateSubdirectory(path);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.CreateSubdirectory),
				path);
	}

	[Fact]
	public void Method_Delete_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool recursive = true;

		sut.DirectoryInfo.New("foo").Delete(recursive);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.Delete),
				recursive);
	}

	[Fact]
	public void Method_Delete_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").Delete();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.Delete));
	}

	[Fact]
	public void Method_EnumerateDirectories_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateDirectories();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateDirectories));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_EnumerateDirectories_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateDirectories),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_EnumerateDirectories_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateDirectories),
				searchPattern, searchOption);
	}

	[Fact]
	public void Method_EnumerateDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateDirectories),
				searchPattern);
	}

	[Fact]
	public void Method_EnumerateFiles_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateFiles();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFiles));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_EnumerateFiles_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFiles),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_EnumerateFiles_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFiles),
				searchPattern, searchOption);
	}

	[Fact]
	public void Method_EnumerateFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFiles),
				searchPattern);
	}

	[Fact]
	public void Method_EnumerateFileSystemInfos_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_EnumerateFileSystemInfos_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_EnumerateFileSystemInfos_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
				searchPattern, searchOption);
	}

	[Fact]
	public void Method_EnumerateFileSystemInfos_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
				searchPattern);
	}

	[Fact]
	public void Method_GetDirectories_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetDirectories();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetDirectories));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_GetDirectories_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetDirectories),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_GetDirectories_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetDirectories),
				searchPattern, searchOption);
	}

	[Fact]
	public void Method_GetDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetDirectories),
				searchPattern);
	}

	[Fact]
	public void Method_GetFiles_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetFiles();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFiles));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_GetFiles_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFiles),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_GetFiles_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFiles),
				searchPattern, searchOption);
	}

	[Fact]
	public void Method_GetFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFiles),
				searchPattern);
	}

	[Fact]
	public void Method_GetFileSystemInfos_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetFileSystemInfos();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void Method_GetFileSystemInfos_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern, enumerationOptions);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public void Method_GetFileSystemInfos_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern, searchOption);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos),
				searchPattern, searchOption);
	}

	[Fact]
	public void Method_GetFileSystemInfos_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos),
				searchPattern);
	}

	[Fact]
	public void Method_MoveTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string destDirName = "bar";

		sut.DirectoryInfo.New("foo").MoveTo(destDirName);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.MoveTo),
				destDirName);
	}

	[Fact]
	public void Method_Refresh_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").Refresh();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.Refresh));
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public void Method_ResolveLinkTarget_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		bool returnFinalTarget = true;

		sut.DirectoryInfo.New("foo").ResolveLinkTarget(returnFinalTarget);

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.ResolveLinkTarget),
				returnFinalTarget);
	}
#endif
	[Fact]
	public void Property_Attributes_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Attributes;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Attributes));
	}

	[Fact]
	public void Property_Attributes_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		FileAttributes value = new();

		sut.DirectoryInfo.New("foo").Attributes = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.Attributes));
	}

	[Fact]
	public void Property_CreationTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").CreationTime;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.CreationTime));
	}

	[Fact]
	public void Property_CreationTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.Now;

		sut.DirectoryInfo.New("foo").CreationTime = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.CreationTime));
	}

	[Fact]
	public void Property_CreationTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").CreationTimeUtc;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.CreationTimeUtc));
	}

	[Fact]
	public void Property_CreationTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.UtcNow;

		sut.DirectoryInfo.New("foo").CreationTimeUtc = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.CreationTimeUtc));
	}

	[Fact]
	public void Property_Exists_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Exists;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Exists));
	}

	[Fact]
	public void Property_Extension_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Extension;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Extension));
	}

	[Fact]
	public void Property_FullName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").FullName;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.FullName));
	}

	[Fact]
	public void Property_LastAccessTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastAccessTime;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LastAccessTime));
	}

	[Fact]
	public void Property_LastAccessTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.Now;

		sut.DirectoryInfo.New("foo").LastAccessTime = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.LastAccessTime));
	}

	[Fact]
	public void Property_LastAccessTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastAccessTimeUtc;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LastAccessTimeUtc));
	}

	[Fact]
	public void Property_LastAccessTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.UtcNow;

		sut.DirectoryInfo.New("foo").LastAccessTimeUtc = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.LastAccessTimeUtc));
	}

	[Fact]
	public void Property_LastWriteTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastWriteTime;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LastWriteTime));
	}

	[Fact]
	public void Property_LastWriteTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.Now;

		sut.DirectoryInfo.New("foo").LastWriteTime = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.LastWriteTime));
	}

	[Fact]
	public void Property_LastWriteTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastWriteTimeUtc;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LastWriteTimeUtc));
	}

	[Fact]
	public void Property_LastWriteTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.UtcNow;

		sut.DirectoryInfo.New("foo").LastWriteTimeUtc = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.LastWriteTimeUtc));
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public void Property_LinkTarget_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LinkTarget;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LinkTarget));
	}
#endif

	[Fact]
	public void Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Name;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Name));
	}

	[Fact]
	public void Property_Parent_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Parent;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Parent));
	}

	[Fact]
	public void Property_Root_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Root;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Root));
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public void Property_UnixFileMode_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").UnixFileMode;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.UnixFileMode));
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public void Property_UnixFileMode_Set_ShouldRegisterPropertyAccess()
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		UnixFileMode value = new();

		#pragma warning disable CA1416
		sut.DirectoryInfo.New("foo").UnixFileMode = value;
		#pragma warning restore CA1416

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.UnixFileMode));
	}
#endif

	[Fact]
	public void ToString_ShouldBeDirectoryInfoWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.DirectoryInfo[@"\\some\path"];

		string? result = sut.ToString();

		result.Should().Be(@"DirectoryInfo[\\some\path]");
	}
}

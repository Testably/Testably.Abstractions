using System.IO;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryInfoStatisticsTests
{
	[SkippableFact]
	public void Method_Create_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").Create();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.Create));
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void Method_CreateAsSymbolicLink_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string pathToTarget = "foo";

		sut.DirectoryInfo.New("foo").CreateAsSymbolicLink(pathToTarget);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.CreateAsSymbolicLink),
			pathToTarget);
	}
#endif

	[SkippableFact]
	public void Method_CreateSubdirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.DirectoryInfo.New("foo").CreateSubdirectory(path);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.CreateSubdirectory),
			path);
	}

	[SkippableFact]
	public void Method_Delete_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool recursive = true;

		sut.DirectoryInfo.New("foo").Delete(recursive);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.Delete),
			recursive);
	}

	[SkippableFact]
	public void Method_Delete_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").Delete();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.Delete));
	}

	[SkippableFact]
	public void Method_EnumerateDirectories_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateDirectories();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateDirectories));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_EnumerateDirectories_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateDirectories),
			searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_EnumerateDirectories_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateDirectories),
			searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_EnumerateDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateDirectories),
			searchPattern);
	}

	[SkippableFact]
	public void Method_EnumerateFiles_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateFiles();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFiles));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_EnumerateFiles_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateFiles),
			searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_EnumerateFiles_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateFiles),
			searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_EnumerateFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateFiles),
			searchPattern);
	}

	[SkippableFact]
	public void Method_EnumerateFileSystemInfos_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_EnumerateFileSystemInfos_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateFileSystemInfos),
			searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_EnumerateFileSystemInfos_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateFileSystemInfos),
			searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_EnumerateFileSystemInfos_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.EnumerateFileSystemInfos),
			searchPattern);
	}

	[SkippableFact]
	public void Method_GetDirectories_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetDirectories();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetDirectories));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_GetDirectories_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetDirectories),
			searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_GetDirectories_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetDirectories),
			searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_GetDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetDirectories),
			searchPattern);
	}

	[SkippableFact]
	public void Method_GetFiles_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetFiles();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFiles));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_GetFiles_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetFiles),
			searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_GetFiles_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetFiles),
			searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_GetFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetFiles),
			searchPattern);
	}

	[SkippableFact]
	public void Method_GetFileSystemInfos_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetFileSystemInfos();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void Method_GetFileSystemInfos_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern, enumerationOptions);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetFileSystemInfos),
			searchPattern, enumerationOptions);
	}
#endif

	[SkippableFact]
	public void Method_GetFileSystemInfos_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern, searchOption);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetFileSystemInfos),
			searchPattern, searchOption);
	}

	[SkippableFact]
	public void Method_GetFileSystemInfos_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.GetFileSystemInfos),
			searchPattern);
	}

	[SkippableFact]
	public void Method_MoveTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string destDirName = "bar";

		sut.DirectoryInfo.New("foo").MoveTo(destDirName);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.MoveTo),
			destDirName);
	}

	[SkippableFact]
	public void Method_Refresh_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").Refresh();

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainMethodCall(nameof(IDirectoryInfo.Refresh));
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void Method_ResolveLinkTarget_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		bool returnFinalTarget = true;

		sut.DirectoryInfo.New("foo").ResolveLinkTarget(returnFinalTarget);

		sut.Statistics.DirectoryInfo["foo"].ShouldOnlyContainMethodCall(
			nameof(IDirectoryInfo.ResolveLinkTarget),
			returnFinalTarget);
	}
#endif
	[SkippableFact]
	public void Property_Attributes_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Attributes;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Attributes));
	}

	[SkippableFact]
	public void Property_Attributes_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		FileAttributes value = new();

		sut.DirectoryInfo.New("foo").Attributes = value;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.Attributes));
	}

	[SkippableFact]
	public void Property_CreationTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").CreationTime;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.CreationTime));
	}

	[SkippableFact]
	public void Property_CreationTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = new();

		sut.DirectoryInfo.New("foo").CreationTime = value;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.CreationTime));
	}

	[SkippableFact]
	public void Property_CreationTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").CreationTimeUtc;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.CreationTimeUtc));
	}

	[SkippableFact]
	public void Property_CreationTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = new();

		sut.DirectoryInfo.New("foo").CreationTimeUtc = value;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.CreationTimeUtc));
	}

	[SkippableFact]
	public void Property_Exists_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Exists;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Exists));
	}

	[SkippableFact]
	public void Property_Extension_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Extension;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Extension));
	}

	[SkippableFact]
	public void Property_FullName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").FullName;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.FullName));
	}

	[SkippableFact]
	public void Property_LastAccessTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastAccessTime;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LastAccessTime));
	}

	[SkippableFact]
	public void Property_LastAccessTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = new();

		sut.DirectoryInfo.New("foo").LastAccessTime = value;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.LastAccessTime));
	}

	[SkippableFact]
	public void Property_LastAccessTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastAccessTimeUtc;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LastAccessTimeUtc));
	}

	[SkippableFact]
	public void Property_LastAccessTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = new();

		sut.DirectoryInfo.New("foo").LastAccessTimeUtc = value;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.LastAccessTimeUtc));
	}

	[SkippableFact]
	public void Property_LastWriteTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastWriteTime;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LastWriteTime));
	}

	[SkippableFact]
	public void Property_LastWriteTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = new();

		sut.DirectoryInfo.New("foo").LastWriteTime = value;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.LastWriteTime));
	}

	[SkippableFact]
	public void Property_LastWriteTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastWriteTimeUtc;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LastWriteTimeUtc));
	}

	[SkippableFact]
	public void Property_LastWriteTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = new();

		sut.DirectoryInfo.New("foo").LastWriteTimeUtc = value;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.LastWriteTimeUtc));
	}

#if FEATURE_FILESYSTEM_LINK
	[SkippableFact]
	public void Property_LinkTarget_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LinkTarget;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.LinkTarget));
	}
#endif

	[SkippableFact]
	public void Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Name;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Name));
	}

	[SkippableFact]
	public void Property_Parent_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Parent;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Parent));
	}

	[SkippableFact]
	public void Property_Root_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Root;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.Root));
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[SkippableFact]
	public void Property_UnixFileMode_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").UnixFileMode;

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDirectoryInfo.UnixFileMode));
	}

	[SkippableFact]
	public void Property_UnixFileMode_Set_ShouldRegisterPropertyAccess()
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		UnixFileMode value = new();

		#pragma warning disable CA1416
		sut.DirectoryInfo.New("foo").UnixFileMode = value;
		#pragma warning restore CA1416

		sut.Statistics.DirectoryInfo["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDirectoryInfo.UnixFileMode));
	}
#endif
}

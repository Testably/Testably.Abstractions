using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryInfoStatisticsTests
{
	[Fact]
	public async Task Method_Create_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").Create();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.Create));
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Method_CreateAsSymbolicLink_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string pathToTarget = "foo";

		sut.DirectoryInfo.New("foo").CreateAsSymbolicLink(pathToTarget);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.CreateAsSymbolicLink),
				pathToTarget);
	}
#endif

	[Fact]
	public async Task Method_CreateSubdirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.DirectoryInfo.New("foo").CreateSubdirectory(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.CreateSubdirectory),
				path);
	}

	[Fact]
	public async Task Method_Delete_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool recursive = true;

		sut.DirectoryInfo.New("foo").Delete(recursive);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.Delete),
				recursive);
	}

	[Fact]
	public async Task Method_Delete_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").Delete();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.Delete));
	}

	[Fact]
	public async Task Method_EnumerateDirectories_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateDirectories();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateDirectories));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_EnumerateDirectories_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateDirectories),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_EnumerateDirectories_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateDirectories),
				searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_EnumerateDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateDirectories(searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateDirectories),
				searchPattern);
	}

	[Fact]
	public async Task Method_EnumerateFiles_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateFiles();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateFiles));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_EnumerateFiles_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateFiles),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_EnumerateFiles_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateFiles),
				searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_EnumerateFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateFiles(searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateFiles),
				searchPattern);
	}

	[Fact]
	public async Task Method_EnumerateFileSystemInfos_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_EnumerateFileSystemInfos_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_EnumerateFileSystemInfos_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
				searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_EnumerateFileSystemInfos_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").EnumerateFileSystemInfos(searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.EnumerateFileSystemInfos),
				searchPattern);
	}

	[Fact]
	public async Task Method_GetDirectories_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetDirectories();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetDirectories));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_GetDirectories_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetDirectories),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_GetDirectories_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetDirectories),
				searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_GetDirectories_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetDirectories(searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetDirectories),
				searchPattern);
	}

	[Fact]
	public async Task Method_GetFiles_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetFiles();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetFiles));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_GetFiles_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetFiles),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_GetFiles_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetFiles),
				searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_GetFiles_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetFiles(searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetFiles),
				searchPattern);
	}

	[Fact]
	public async Task Method_GetFileSystemInfos_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.DirectoryInfo.New("foo").GetFileSystemInfos();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task Method_GetFileSystemInfos_String_EnumerationOptions_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		EnumerationOptions enumerationOptions = new();

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern, enumerationOptions);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos),
				searchPattern, enumerationOptions);
	}
#endif

	[Fact]
	public async Task Method_GetFileSystemInfos_String_SearchOption_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";
		SearchOption searchOption = SearchOption.AllDirectories;

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern, searchOption);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos),
				searchPattern, searchOption);
	}

	[Fact]
	public async Task Method_GetFileSystemInfos_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string searchPattern = "foo";

		sut.DirectoryInfo.New("foo").GetFileSystemInfos(searchPattern);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.GetFileSystemInfos),
				searchPattern);
	}

	[Fact]
	public async Task Method_MoveTo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string destDirName = "bar";

		sut.DirectoryInfo.New("foo").MoveTo(destDirName);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.MoveTo),
				destDirName);
	}

	[Fact]
	public async Task Method_Refresh_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DirectoryInfo.New("foo").Refresh();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.Refresh));
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Method_ResolveLinkTarget_Bool_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		bool returnFinalTarget = true;

		sut.DirectoryInfo.New("foo").ResolveLinkTarget(returnFinalTarget);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsMethodCall(nameof(IDirectoryInfo.ResolveLinkTarget),
				returnFinalTarget);
	}
#endif
	[Fact]
	public async Task Property_Attributes_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Attributes;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.Attributes));
	}

	[Fact]
	public async Task Property_Attributes_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		FileAttributes value = new();

		sut.DirectoryInfo.New("foo").Attributes = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IDirectoryInfo.Attributes));
	}

	[Fact]
	public async Task Property_CreationTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").CreationTime;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.CreationTime));
	}

	[Fact]
	public async Task Property_CreationTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.Now;

		sut.DirectoryInfo.New("foo").CreationTime = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IDirectoryInfo.CreationTime));
	}

	[Fact]
	public async Task Property_CreationTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").CreationTimeUtc;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.CreationTimeUtc));
	}

	[Fact]
	public async Task Property_CreationTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.UtcNow;

		sut.DirectoryInfo.New("foo").CreationTimeUtc = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IDirectoryInfo.CreationTimeUtc));
	}

	[Fact]
	public async Task Property_Exists_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Exists;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.Exists));
	}

	[Fact]
	public async Task Property_Extension_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Extension;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.Extension));
	}

	[Fact]
	public async Task Property_FullName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").FullName;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.FullName));
	}

	[Fact]
	public async Task Property_LastAccessTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastAccessTime;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.LastAccessTime));
	}

	[Fact]
	public async Task Property_LastAccessTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.Now;

		sut.DirectoryInfo.New("foo").LastAccessTime = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IDirectoryInfo.LastAccessTime));
	}

	[Fact]
	public async Task Property_LastAccessTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastAccessTimeUtc;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.LastAccessTimeUtc));
	}

	[Fact]
	public async Task Property_LastAccessTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.UtcNow;

		sut.DirectoryInfo.New("foo").LastAccessTimeUtc = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IDirectoryInfo.LastAccessTimeUtc));
	}

	[Fact]
	public async Task Property_LastWriteTime_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastWriteTime;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.LastWriteTime));
	}

	[Fact]
	public async Task Property_LastWriteTime_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.Now;

		sut.DirectoryInfo.New("foo").LastWriteTime = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IDirectoryInfo.LastWriteTime));
	}

	[Fact]
	public async Task Property_LastWriteTimeUtc_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LastWriteTimeUtc;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.LastWriteTimeUtc));
	}

	[Fact]
	public async Task Property_LastWriteTimeUtc_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		DateTime value = DateTime.UtcNow;

		sut.DirectoryInfo.New("foo").LastWriteTimeUtc = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IDirectoryInfo.LastWriteTimeUtc));
	}

#if FEATURE_FILESYSTEM_LINK
	[Fact]
	public async Task Property_LinkTarget_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").LinkTarget;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.LinkTarget));
	}
#endif

	[Fact]
	public async Task Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Name;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.Name));
	}

	[Fact]
	public async Task Property_Parent_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Parent;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.Parent));
	}

	[Fact]
	public async Task Property_Root_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").Root;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.Root));
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public async Task Property_UnixFileMode_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.DirectoryInfo.New("foo").UnixFileMode;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IDirectoryInfo.UnixFileMode));
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public async Task Property_UnixFileMode_Set_ShouldRegisterPropertyAccess()
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		UnixFileMode value = new();

#pragma warning disable CA1416
		sut.DirectoryInfo.New("foo").UnixFileMode = value;
#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DirectoryInfo["foo"])
			.OnlyContainsPropertySetAccess(nameof(IDirectoryInfo.UnixFileMode));
	}
#endif

	[Fact]
	public async Task ToString_ShouldBeDirectoryInfoWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.DirectoryInfo[@"\\some\path"];

		string? result = sut.ToString();

		await That(result).IsEqualTo(@"DirectoryInfo[\\some\path]");
	}
}

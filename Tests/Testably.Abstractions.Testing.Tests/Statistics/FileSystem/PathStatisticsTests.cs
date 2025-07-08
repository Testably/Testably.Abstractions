using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#if FEATURE_PATH_RELATIVE
using System.IO;
#endif

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class PathStatisticsTests
{
	[Fact]
	public async Task Method_ChangeExtension_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string extension = "foo";

		sut.Path.ChangeExtension(path, extension);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.ChangeExtension),
			path, extension);
	}
#if FEATURE_PATH_SPAN
	[Fact]
	public async Task Method_Combine_ReadOnlySpanString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<string> paths = new();

		sut.Path.Combine(paths);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Combine),
			paths);
	}
#endif

	[Fact]
	public async Task Method_Combine_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";

		sut.Path.Combine(path1, path2);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Combine),
			path1, path2);
	}

	[Fact]
	public async Task Method_Combine_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";

		sut.Path.Combine(path1, path2, path3);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Combine),
			path1, path2, path3);
	}

	[Fact]
	public async Task Method_Combine_String_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";
		string path4 = "foo";

		sut.Path.Combine(path1, path2, path3, path4);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Combine),
			path1, path2, path3, path4);
	}

	[Fact]
	public async Task Method_Combine_StringArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string[] paths = ["foo", "bar"];

		sut.Path.Combine(paths);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Combine),
			paths);
	}

#if FEATURE_PATH_ADVANCED
	[Fact]
	public async Task Method_EndsInDirectorySeparator_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.EndsInDirectorySeparator(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.EndsInDirectorySeparator),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

#if FEATURE_PATH_ADVANCED
	[Fact]
	public async Task Method_EndsInDirectorySeparator_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.EndsInDirectorySeparator(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.EndsInDirectorySeparator),
			path);
	}
#endif

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
	[Fact]
	public async Task Method_Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.Exists(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Exists),
			path);
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public async Task Method_GetDirectoryName_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetDirectoryName(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetDirectoryName),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

	[Fact]
	public async Task Method_GetDirectoryName_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetDirectoryName(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetDirectoryName),
			path);
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_GetExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetExtension(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetExtension),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

	[Fact]
	public async Task Method_GetExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetExtension(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetExtension),
			path);
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_GetFileName_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetFileName(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetFileName),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

	[Fact]
	public async Task Method_GetFileName_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFileName(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetFileName),
			path);
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_GetFileNameWithoutExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetFileNameWithoutExtension(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetFileNameWithoutExtension),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

	[Fact]
	public async Task Method_GetFileNameWithoutExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFileNameWithoutExtension(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetFileNameWithoutExtension),
			path);
	}

	[Fact]
	public async Task Method_GetFullPath_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFullPath(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetFullPath),
			path);
	}

#if FEATURE_PATH_RELATIVE
	[Fact]
	public async Task Method_GetFullPath_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string basePath = Path.GetFullPath("bar");

		sut.Path.GetFullPath(path, basePath);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetFullPath),
			path, basePath);
	}
#endif

	[Fact]
	public async Task Method_GetInvalidFileNameChars_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetInvalidFileNameChars();

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetInvalidFileNameChars));
	}

	[Fact]
	public async Task Method_GetInvalidPathChars_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetInvalidPathChars();

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetInvalidPathChars));
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_GetPathRoot_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetPathRoot(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetPathRoot),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

	[Fact]
	public async Task Method_GetPathRoot_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetPathRoot(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetPathRoot),
			path);
	}

	[Fact]
	public async Task Method_GetRandomFileName_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetRandomFileName();

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetRandomFileName));
	}

#if FEATURE_PATH_RELATIVE
	[Fact]
	public async Task Method_GetRelativePath_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string relativeTo = "foo";
		string path = "foo";

		sut.Path.GetRelativePath(relativeTo, path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetRelativePath),
			relativeTo, path);
	}
#endif

	[Fact]
	public async Task Method_GetTempFileName_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

#pragma warning disable CS0618
		sut.Path.GetTempFileName();
#pragma warning restore CS0618

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetTempFileName));
	}

	[Fact]
	public async Task Method_GetTempPath_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetTempPath();

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.GetTempPath));
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_HasExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.HasExtension(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.HasExtension),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

	[Fact]
	public async Task Method_HasExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.HasExtension(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.HasExtension),
			path);
	}

#if FEATURE_SPAN
	[Fact]
	public async Task Method_IsPathFullyQualified_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.IsPathFullyQualified(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.IsPathFullyQualified),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

#if FEATURE_PATH_RELATIVE
	[Fact]
	public async Task Method_IsPathFullyQualified_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.IsPathFullyQualified(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.IsPathFullyQualified),
			path);
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public async Task Method_IsPathRooted_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.IsPathRooted(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.IsPathRooted),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

	[Fact]
	public async Task Method_IsPathRooted_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.IsPathRooted(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.IsPathRooted),
			path);
	}

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_Join_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();
		ReadOnlySpan<char> path4 = new();

		sut.Path.Join(path1, path2, path3, path4);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Join),
			path1, path2, path3, path4);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_Join_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();

		sut.Path.Join(path1, path2, path3);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Join),
			path1, path2, path3);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_Join_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();

		sut.Path.Join(path1, path2);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Join),
			path1, path2);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

#if FEATURE_PATH_SPAN
	[Fact]
	public async Task Method_Join_ReadOnlySpanString_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<string?> paths = new();

		sut.Path.Join(paths);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Join),
			paths);
	}
#endif

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_Join_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";

		sut.Path.Join(path1, path2);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Join),
			path1, path2);
	}
#endif

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_Join_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";

		sut.Path.Join(path1, path2, path3);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Join),
			path1, path2, path3);
	}
#endif

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_Join_String_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";
		string path4 = "foo";

		sut.Path.Join(path1, path2, path3, path4);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Join),
			path1, path2, path3, path4);
	}
#endif

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_Join_StringArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string[] paths = ["foo", "bar"];

		sut.Path.Join(paths);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.Join),
			paths);
	}
#endif

#if FEATURE_PATH_ADVANCED
	[Fact]
	public async Task Method_TrimEndingDirectorySeparator_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.TrimEndingDirectorySeparator(path);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.TrimEndingDirectorySeparator),
			path);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

#if FEATURE_PATH_ADVANCED
	[Fact]
	public async Task Method_TrimEndingDirectorySeparator_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.TrimEndingDirectorySeparator(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.TrimEndingDirectorySeparator),
			path);
	}
#endif

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_TryJoin_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_SpanChar_OutInt_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();
		Span<char> destination = new();

		sut.Path.TryJoin(path1, path2, path3, destination, out int charsWritten);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.TryJoin),
			path1, path2, path3, destination, charsWritten);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

#if FEATURE_PATH_JOIN
	[Fact]
	public async Task Method_TryJoin_ReadOnlySpanChar_ReadOnlySpanChar_SpanChar_OutInt_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		Span<char> destination = new();

		sut.Path.TryJoin(path1, path2, destination, out int charsWritten);

		await That(sut.Statistics.Path).OnlyContainsMethodCall(nameof(IPath.TryJoin),
			path1, path2, destination, charsWritten);
		await That(sut.Statistics.TotalCount).IsEqualTo(1);
	}
#endif

	[Fact]
	public async Task Property_AltDirectorySeparatorChar_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.Path.AltDirectorySeparatorChar;

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsPropertyGetAccess(
			nameof(IPath.AltDirectorySeparatorChar));
	}

	[Fact]
	public async Task Property_DirectorySeparatorChar_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.Path.DirectorySeparatorChar;

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsPropertyGetAccess(
			nameof(IPath.DirectorySeparatorChar));
	}

	[Fact]
	public async Task Property_PathSeparator_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.Path.PathSeparator;

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsPropertyGetAccess(nameof(IPath.PathSeparator));
	}

	[Fact]
	public async Task Property_VolumeSeparatorChar_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.Path.VolumeSeparatorChar;

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.Path).OnlyContainsPropertyGetAccess(nameof(IPath.VolumeSeparatorChar));
	}

	[Fact]
	public async Task ToString_ShouldBePath()
	{
		IStatistics sut = new MockFileSystem().Statistics.Path;

		string? result = sut.ToString();

		await That(result).IsEqualTo("Path");
	}
}

using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class PathStatisticsTests
{
	[SkippableFact]
	public void ChangeExtension_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string extension = "foo";

		sut.Path.ChangeExtension(path, extension);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.ChangeExtension),
			path, extension);
	}

	[SkippableFact]
	public void Combine_StringArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string[] paths = ["foo", "bar"];

		sut.Path.Combine(paths);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Combine),
			paths);
	}

	[SkippableFact]
	public void Combine_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";

		sut.Path.Combine(path1, path2);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Combine),
			path1, path2);
	}

	[SkippableFact]
	public void Combine_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";

		sut.Path.Combine(path1, path2, path3);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Combine),
			path1, path2, path3);
	}

	[SkippableFact]
	public void Combine_String_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";
		string path4 = "foo";

		sut.Path.Combine(path1, path2, path3, path4);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Combine),
			path1, path2, path3, path4);
	}

#if FEATURE_PATH_ADVANCED
	[SkippableFact]
	public void EndsInDirectorySeparator_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.EndsInDirectorySeparator(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.EndsInDirectorySeparator),
			path);
	}

	[SkippableFact]
	public void EndsInDirectorySeparator_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.EndsInDirectorySeparator(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.EndsInDirectorySeparator),
			path);
	}
#endif

#if FEATURE_FILESYSTEM_NET7
	[SkippableFact]
	public void Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.Exists(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Exists),
		path);
	}
#endif

#if FEATURE_SPAN
	[SkippableFact]
	public void GetDirectoryName_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetDirectoryName(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetDirectoryName),
			path);
	}
#endif

	[SkippableFact]
	public void GetDirectoryName_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetDirectoryName(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetDirectoryName),
			path);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void GetExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetExtension(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetExtension),
			path);
	}
#endif

	[SkippableFact]
	public void GetExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetExtension(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetExtension),
			path);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void GetFileName_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetFileName(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetFileName),
			path);
	}
#endif

	[SkippableFact]
	public void GetFileName_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFileName(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetFileName),
			path);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void GetFileNameWithoutExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetFileNameWithoutExtension(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetFileNameWithoutExtension),
			path);
	}
#endif

	[SkippableFact]
	public void GetFileNameWithoutExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFileNameWithoutExtension(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetFileNameWithoutExtension),
			path);
	}

	[SkippableFact]
	public void GetFullPath_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFullPath(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetFullPath),
			path);
	}

#if FEATURE_PATH_RELATIVE
	[SkippableFact]
	public void GetFullPath_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string basePath = System.IO.Path.GetFullPath("bar");

		sut.Path.GetFullPath(path, basePath);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetFullPath),
			path, basePath);
	}
#endif

	[SkippableFact]
	public void GetInvalidFileNameChars_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetInvalidFileNameChars();

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetInvalidFileNameChars));
	}

	[SkippableFact]
	public void GetInvalidPathChars_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetInvalidPathChars();

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetInvalidPathChars));
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void GetPathRoot_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetPathRoot(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetPathRoot),
			path);
	}
#endif

	[SkippableFact]
	public void GetPathRoot_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetPathRoot(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetPathRoot),
			path);
	}

	[SkippableFact]
	public void GetRandomFileName_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetRandomFileName();

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetRandomFileName));
	}

#if FEATURE_PATH_RELATIVE
	[SkippableFact]
	public void GetRelativePath_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string relativeTo = "foo";
		string path = "foo";

		sut.Path.GetRelativePath(relativeTo, path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetRelativePath),
			relativeTo, path);
	}
#endif

	[SkippableFact]
	public void GetTempFileName_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetTempFileName();

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetTempFileName));
	}

	[SkippableFact]
	public void GetTempPath_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetTempPath();

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.GetTempPath));
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void HasExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.HasExtension(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.HasExtension),
			path);
	}
#endif

	[SkippableFact]
	public void HasExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.HasExtension(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.HasExtension),
			path);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void IsPathFullyQualified_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.IsPathFullyQualified(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.IsPathFullyQualified),
			path);
	}
#endif

#if FEATURE_PATH_RELATIVE
	[SkippableFact]
	public void IsPathFullyQualified_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.IsPathFullyQualified(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.IsPathFullyQualified),
			path);
	}
#endif

#if FEATURE_SPAN
	[SkippableFact]
	public void IsPathRooted_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.IsPathRooted(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.IsPathRooted),
			path);
	}
#endif

	[SkippableFact]
	public void IsPathRooted_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.IsPathRooted(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.IsPathRooted),
			path);
	}

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void Join_StringArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string[] paths = ["foo", "bar"];

		sut.Path.Join(paths);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Join),
			paths);
	}

	[SkippableFact]
	public void Join_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();

		sut.Path.Join(path1, path2);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Join),
			path1, path2);
	}

	[SkippableFact]
	public void Join_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";

		sut.Path.Join(path1, path2);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Join),
			path1, path2);
	}

	[SkippableFact]
	public void Join_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();

		sut.Path.Join(path1, path2, path3);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Join),
			path1, path2, path3);
	}

	[SkippableFact]
	public void Join_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";

		sut.Path.Join(path1, path2, path3);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Join),
			path1, path2, path3);
	}

	[SkippableFact]
	public void
		Join_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();
		ReadOnlySpan<char> path4 = new();

		sut.Path.Join(path1, path2, path3, path4);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Join),
			path1, path2, path3, path4);
	}

	[SkippableFact]
	public void Join_String_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";
		string path4 = "foo";

		sut.Path.Join(path1, path2, path3, path4);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.Join),
			path1, path2, path3, path4);
	}
#endif

#if FEATURE_PATH_ADVANCED
	[SkippableFact]
	public void TrimEndingDirectorySeparator_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.TrimEndingDirectorySeparator(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.TrimEndingDirectorySeparator),
			path);
	}

	[SkippableFact]
	public void TrimEndingDirectorySeparator_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.TrimEndingDirectorySeparator(path);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.TrimEndingDirectorySeparator),
			path);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void TryJoin_ReadOnlySpanChar_ReadOnlySpanChar_SpanChar_OutInt_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		Span<char> destination = new();

		sut.Path.TryJoin(path1, path2, destination, out int charsWritten);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.TryJoin),
			path1, path2, destination, charsWritten);
	}

	[SkippableFact]
	public void TryJoin_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_SpanChar_OutInt_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();
		Span<char> destination = new();

		sut.Path.TryJoin(path1, path2, path3, destination, out int charsWritten);

		sut.Statistics.Path.ShouldOnlyContain(nameof(IPath.TryJoin),
			path1, path2, path3, destination, charsWritten);
	}
#endif
}

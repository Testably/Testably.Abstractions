﻿using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#if FEATURE_PATH_RELATIVE
using System.IO;
#endif

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class PathStatisticsTests
{
	[SkippableFact]
	public void Method_ChangeExtension_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string extension = "foo";

		sut.Path.ChangeExtension(path, extension);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.ChangeExtension),
			path, extension);
	}

	[SkippableFact]
	public void Method_Combine_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";

		sut.Path.Combine(path1, path2);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Combine),
			path1, path2);
	}

	[SkippableFact]
	public void Method_Combine_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";

		sut.Path.Combine(path1, path2, path3);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Combine),
			path1, path2, path3);
	}

	[SkippableFact]
	public void Method_Combine_String_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";
		string path4 = "foo";

		sut.Path.Combine(path1, path2, path3, path4);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Combine),
			path1, path2, path3, path4);
	}

	[SkippableFact]
	public void Method_Combine_StringArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string[] paths = ["foo", "bar"];

		sut.Path.Combine(paths);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Combine),
			paths);
	}

#if FEATURE_PATH_ADVANCED
	[SkippableFact]
	public void Method_EndsInDirectorySeparator_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.EndsInDirectorySeparator(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.EndsInDirectorySeparator),
			path);
	}
#endif

#if FEATURE_PATH_ADVANCED
	[SkippableFact]
	public void Method_EndsInDirectorySeparator_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.EndsInDirectorySeparator(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.EndsInDirectorySeparator),
			path);
	}
#endif

#if FEATURE_FILESYSTEM_NET7
	[SkippableFact]
	public void Method_Exists_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.Exists(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Exists),
			path);
	}
#endif

#if FEATURE_SPAN
	[SkippableFact]
	public void Method_GetDirectoryName_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetDirectoryName(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetDirectoryName),
			path);
	}
#endif

	[SkippableFact]
	public void Method_GetDirectoryName_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetDirectoryName(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetDirectoryName),
			path);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Method_GetExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetExtension(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetExtension),
			path);
	}
#endif

	[SkippableFact]
	public void Method_GetExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetExtension(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetExtension),
			path);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Method_GetFileName_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetFileName(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetFileName),
			path);
	}
#endif

	[SkippableFact]
	public void Method_GetFileName_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFileName(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetFileName),
			path);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Method_GetFileNameWithoutExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetFileNameWithoutExtension(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetFileNameWithoutExtension),
			path);
	}
#endif

	[SkippableFact]
	public void Method_GetFileNameWithoutExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFileNameWithoutExtension(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetFileNameWithoutExtension),
			path);
	}

	[SkippableFact]
	public void Method_GetFullPath_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetFullPath(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetFullPath),
			path);
	}

#if FEATURE_PATH_RELATIVE
	[SkippableFact]
	public void Method_GetFullPath_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";
		string basePath = Path.GetFullPath("bar");

		sut.Path.GetFullPath(path, basePath);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetFullPath),
			path, basePath);
	}
#endif

	[SkippableFact]
	public void Method_GetInvalidFileNameChars_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetInvalidFileNameChars();

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetInvalidFileNameChars));
	}

	[SkippableFact]
	public void Method_GetInvalidPathChars_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetInvalidPathChars();

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetInvalidPathChars));
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Method_GetPathRoot_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.GetPathRoot(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetPathRoot),
			path);
	}
#endif

	[SkippableFact]
	public void Method_GetPathRoot_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.GetPathRoot(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetPathRoot),
			path);
	}

	[SkippableFact]
	public void Method_GetRandomFileName_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetRandomFileName();

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetRandomFileName));
	}

#if FEATURE_PATH_RELATIVE
	[SkippableFact]
	public void Method_GetRelativePath_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string relativeTo = "foo";
		string path = "foo";

		sut.Path.GetRelativePath(relativeTo, path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetRelativePath),
			relativeTo, path);
	}
#endif

	[SkippableFact]
	public void Method_GetTempFileName_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetTempFileName();

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetTempFileName));
	}

	[SkippableFact]
	public void Method_GetTempPath_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.Path.GetTempPath();

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.GetTempPath));
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Method_HasExtension_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.HasExtension(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.HasExtension),
			path);
	}
#endif

	[SkippableFact]
	public void Method_HasExtension_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.HasExtension(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.HasExtension),
			path);
	}

#if FEATURE_SPAN
	[SkippableFact]
	public void Method_IsPathFullyQualified_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.IsPathFullyQualified(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.IsPathFullyQualified),
			path);
	}
#endif

#if FEATURE_PATH_RELATIVE
	[SkippableFact]
	public void Method_IsPathFullyQualified_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.IsPathFullyQualified(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.IsPathFullyQualified),
			path);
	}
#endif

#if FEATURE_SPAN
	[SkippableFact]
	public void Method_IsPathRooted_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.IsPathRooted(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.IsPathRooted),
			path);
	}
#endif

	[SkippableFact]
	public void Method_IsPathRooted_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.IsPathRooted(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.IsPathRooted),
			path);
	}

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void
		Method_Join_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();
		ReadOnlySpan<char> path4 = new();

		sut.Path.Join(path1, path2, path3, path4);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Join),
			path1, path2, path3, path4);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void Method_Join_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();

		sut.Path.Join(path1, path2, path3);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Join),
			path1, path2, path3);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void Method_Join_ReadOnlySpanChar_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();

		sut.Path.Join(path1, path2);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Join),
			path1, path2);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void Method_Join_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";

		sut.Path.Join(path1, path2);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Join),
			path1, path2);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void Method_Join_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";

		sut.Path.Join(path1, path2, path3);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Join),
			path1, path2, path3);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void Method_Join_String_String_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path1 = "foo";
		string path2 = "foo";
		string path3 = "foo";
		string path4 = "foo";

		sut.Path.Join(path1, path2, path3, path4);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Join),
			path1, path2, path3, path4);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void Method_Join_StringArray_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string[] paths = ["foo", "bar"];

		sut.Path.Join(paths);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.Join),
			paths);
	}
#endif

#if FEATURE_PATH_ADVANCED
	[SkippableFact]
	public void Method_TrimEndingDirectorySeparator_ReadOnlySpanChar_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path = new();

		sut.Path.TrimEndingDirectorySeparator(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.TrimEndingDirectorySeparator),
			path);
	}
#endif

#if FEATURE_PATH_ADVANCED
	[SkippableFact]
	public void Method_TrimEndingDirectorySeparator_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Path.TrimEndingDirectorySeparator(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.TrimEndingDirectorySeparator),
			path);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void
		Method_TryJoin_ReadOnlySpanChar_ReadOnlySpanChar_ReadOnlySpanChar_SpanChar_OutInt_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		ReadOnlySpan<char> path3 = new();
		Span<char> destination = new();

		sut.Path.TryJoin(path1, path2, path3, destination, out int charsWritten);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.TryJoin),
			path1, path2, path3, destination, charsWritten);
	}
#endif

#if FEATURE_PATH_JOIN
	[SkippableFact]
	public void
		Method_TryJoin_ReadOnlySpanChar_ReadOnlySpanChar_SpanChar_OutInt_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		ReadOnlySpan<char> path1 = new();
		ReadOnlySpan<char> path2 = new();
		Span<char> destination = new();

		sut.Path.TryJoin(path1, path2, destination, out int charsWritten);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainMethodCall(nameof(IPath.TryJoin),
			path1, path2, destination, charsWritten);
	}
#endif

	[SkippableFact]
	public void Property_AltDirectorySeparatorChar_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.Path.AltDirectorySeparatorChar;

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainPropertyGetAccess(
			nameof(IPath.AltDirectorySeparatorChar));
	}

	[SkippableFact]
	public void Property_DirectorySeparatorChar_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.Path.DirectorySeparatorChar;

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainPropertyGetAccess(
			nameof(IPath.DirectorySeparatorChar));
	}

	[SkippableFact]
	public void Property_PathSeparator_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.Path.PathSeparator;

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainPropertyGetAccess(nameof(IPath.PathSeparator));
	}

	[SkippableFact]
	public void Property_VolumeSeparatorChar_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.Path.VolumeSeparatorChar;

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.Path.ShouldOnlyContainPropertyGetAccess(nameof(IPath.VolumeSeparatorChar));
	}

	[SkippableFact]
	public void ToString_ShouldBePath()
	{
		IStatistics sut = new MockFileSystem().Statistics.Path;

		string? result = sut.ToString();

		result.Should().Be("Path");
	}
}

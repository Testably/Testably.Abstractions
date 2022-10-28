using System.Diagnostics.CodeAnalysis;
using System.IO;
#if FEATURE_SPAN
using System;
#endif

namespace Testably.Abstractions.FileSystem;

/// <summary>
///     Abstractions for <see cref="Path" />.
/// </summary>
public interface IPath : IFileSystemExtensionPoint
{
	/// <inheritdoc cref="Path.AltDirectorySeparatorChar" />
	char AltDirectorySeparatorChar { get; }

	/// <inheritdoc cref="Path.DirectorySeparatorChar" />
	char DirectorySeparatorChar { get; }

	/// <inheritdoc cref="Path.PathSeparator" />
	char PathSeparator { get; }

	/// <inheritdoc cref="Path.VolumeSeparatorChar" />
	char VolumeSeparatorChar { get; }

	/// <inheritdoc cref="Path.ChangeExtension(string, string)" />
	[return: NotNullIfNotNull("path")]
	string? ChangeExtension(string? path, string? extension);

	/// <inheritdoc cref="Path.Combine(string, string)" />
	string Combine(string path1, string path2);

	/// <inheritdoc cref="Path.Combine(string, string, string)" />
	string Combine(string path1, string path2, string path3);

	/// <inheritdoc cref="Path.Combine(string, string, string, string)" />
	string Combine(string path1, string path2, string path3, string path4);

	/// <inheritdoc cref="Path.Combine(string[])" />
	string Combine(params string[] paths);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetDirectoryName(ReadOnlySpan{char})" />
	ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path);
#endif

	/// <inheritdoc cref="Path.GetDirectoryName(string)" />
	string? GetDirectoryName(string? path);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetExtension(ReadOnlySpan{char})" />
	ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path);
#endif

	/// <inheritdoc cref="Path.GetExtension(string)" />
	[return: NotNullIfNotNull("path")]
	string? GetExtension(string? path);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetFileName(ReadOnlySpan{char})" />
	ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path);
#endif

	/// <inheritdoc cref="Path.GetFileName(string)" />
	[return: NotNullIfNotNull("path")]
	string? GetFileName(string? path);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
	ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path);
#endif

	/// <inheritdoc cref="Path.GetFileNameWithoutExtension(string)" />
	[return: NotNullIfNotNull("path")]
	string? GetFileNameWithoutExtension(string? path);

	/// <inheritdoc cref="Path.GetFullPath(string)" />
	string GetFullPath(string path);

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="Path.GetFullPath(string, string)" />
	string GetFullPath(string path, string basePath);
#endif

	/// <inheritdoc cref="Path.GetInvalidFileNameChars()" />
	char[] GetInvalidFileNameChars();

	/// <inheritdoc cref="Path.GetInvalidPathChars()" />
	char[] GetInvalidPathChars();

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetPathRoot(ReadOnlySpan{char})" />
	ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path);
#endif

	/// <inheritdoc cref="Path.GetPathRoot(string?)" />
	string? GetPathRoot(string? path);

	/// <inheritdoc cref="Path.GetRandomFileName()" />
	string GetRandomFileName();

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="Path.GetRelativePath(string, string)" />
	string GetRelativePath(string relativeTo, string path);
#endif

	/// <inheritdoc cref="Path.GetTempFileName()" />
#if !NETSTANDARD2_0
	[Obsolete(
		"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
	string GetTempFileName();

	/// <inheritdoc cref="Path.GetTempPath()" />
	string GetTempPath();

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.HasExtension(ReadOnlySpan{char})" />
	bool HasExtension(ReadOnlySpan<char> path);
#endif

	/// <inheritdoc cref="Path.HasExtension(string)" />
	bool HasExtension([NotNullWhen(true)] string? path);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.IsPathFullyQualified(ReadOnlySpan{char})" />
	bool IsPathFullyQualified(ReadOnlySpan<char> path);
#endif

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="Path.IsPathFullyQualified(string)" />
	bool IsPathFullyQualified(string path);
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.IsPathRooted(ReadOnlySpan{char})" />
	bool IsPathRooted(ReadOnlySpan<char> path);
#endif

	/// <inheritdoc cref="Path.IsPathRooted(string?)" />
	bool IsPathRooted([NotNullWhen(true)] string? path);

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="Path.EndsInDirectorySeparator(ReadOnlySpan{char})" />
	bool EndsInDirectorySeparator(ReadOnlySpan<char> path);

	/// <inheritdoc cref="Path.EndsInDirectorySeparator(string)" />
	bool EndsInDirectorySeparator(string path);
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
	string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2);

	/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	string Join(ReadOnlySpan<char> path1,
				ReadOnlySpan<char> path2,
				ReadOnlySpan<char> path3);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	string Join(ReadOnlySpan<char> path1,
				ReadOnlySpan<char> path2,
				ReadOnlySpan<char> path3,
				ReadOnlySpan<char> path4);

	/// <inheritdoc cref="Path.Join(string, string)" />
	string Join(string? path1, string? path2);

	/// <inheritdoc cref="Path.Join(string, string, string)" />
	string Join(string? path1, string? path2, string? path3);

	/// <inheritdoc cref="Path.Join(string, string, string, string)" />
	string Join(string? path1, string? path2, string? path3, string? path4);

	/// <inheritdoc cref="Path.Join(string[])" />
	string Join(params string?[] paths);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="Path.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
	ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path);

	/// <inheritdoc cref="Path.TrimEndingDirectorySeparator(string)" />
	string TrimEndingDirectorySeparator(string path);
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	bool TryJoin(ReadOnlySpan<char> path1,
				 ReadOnlySpan<char> path2,
				 Span<char> destination,
				 out int charsWritten);

	/// <inheritdoc cref="Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	bool TryJoin(ReadOnlySpan<char> path1,
				 ReadOnlySpan<char> path2,
				 ReadOnlySpan<char> path3,
				 Span<char> destination,
				 out int charsWritten);
#endif
}
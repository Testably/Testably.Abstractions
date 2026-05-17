using System;
using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions
{
	/// <inheritdoc cref="System.IO.Path" />
	public interface IPath : System.IO.Abstractions.IFileSystemEntity
	{
		/// <inheritdoc cref="System.IO.Path.AltDirectorySeparatorChar" />
		char AltDirectorySeparatorChar { get; }

		/// <inheritdoc cref="System.IO.Path.DirectorySeparatorChar" />
		char DirectorySeparatorChar { get; }

		/// <inheritdoc cref="System.IO.Path.PathSeparator" />
		char PathSeparator { get; }

		/// <inheritdoc cref="System.IO.Path.VolumeSeparatorChar" />
		char VolumeSeparatorChar { get; }

		/// <inheritdoc cref="System.IO.Path.ChangeExtension(string, string)" />
		[return: NotNullIfNotNull("path")]
		string? ChangeExtension(string? path, string? extension);

		/// <inheritdoc cref="System.IO.Path.Combine(string, string)" />
		string Combine(string path1, string path2);

		/// <inheritdoc cref="System.IO.Path.Combine(string, string, string)" />
		string Combine(string path1, string path2, string path3);

		/// <inheritdoc cref="System.IO.Path.Combine(string, string, string, string)" />
		string Combine(string path1, string path2, string path3, string path4);

		/// <inheritdoc cref="System.IO.Path.Combine(string[])" />
		string Combine(params string[] paths);

#if FEATURE_PATH_SPAN
		/// <inheritdoc cref="System.IO.Path.Combine(ReadOnlySpan{string})" />
		string Combine(params ReadOnlySpan<string> paths);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.EndsInDirectorySeparator(ReadOnlySpan{char})" />
		bool EndsInDirectorySeparator(ReadOnlySpan<char> path);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.EndsInDirectorySeparator(string)" />
		bool EndsInDirectorySeparator(string path);
#endif

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
		/// <inheritdoc cref="System.IO.Path.Exists(string)" />
		bool Exists([NotNullWhen(true)] string? path);
#endif

#if FEATURE_SPAN
		/// <inheritdoc cref="System.IO.Path.GetDirectoryName(ReadOnlySpan{char})" />
		ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path);
#endif

		/// <inheritdoc cref="System.IO.Path.GetDirectoryName(string)" />
		string? GetDirectoryName(string? path);

#if FEATURE_SPAN
		/// <inheritdoc cref="System.IO.Path.GetExtension(ReadOnlySpan{char})" />
		ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path);
#endif

		/// <inheritdoc cref="System.IO.Path.GetExtension(string)" />
		[return: NotNullIfNotNull("path")]
		string? GetExtension(string? path);

#if FEATURE_SPAN
		/// <inheritdoc cref="System.IO.Path.GetFileName(ReadOnlySpan{char})" />
		ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path);
#endif

		/// <inheritdoc cref="System.IO.Path.GetFileName(string)" />
		[return: NotNullIfNotNull("path")]
		string? GetFileName(string? path);

#if FEATURE_SPAN
		/// <inheritdoc cref="System.IO.Path.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
		ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path);
#endif

		/// <inheritdoc cref="System.IO.Path.GetFileNameWithoutExtension(string)" />
		[return: NotNullIfNotNull("path")]
		string? GetFileNameWithoutExtension(string? path);

		/// <inheritdoc cref="System.IO.Path.GetFullPath(string)" />
		string GetFullPath(string path);

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="System.IO.Path.GetFullPath(string, string)" />
		string GetFullPath(string path, string basePath);
#endif

		/// <inheritdoc cref="System.IO.Path.GetInvalidFileNameChars()" />
		char[] GetInvalidFileNameChars();

		/// <inheritdoc cref="System.IO.Path.GetInvalidPathChars()" />
		char[] GetInvalidPathChars();

#if FEATURE_SPAN
		/// <inheritdoc cref="System.IO.Path.GetPathRoot(ReadOnlySpan{char})" />
		ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path);
#endif

		/// <inheritdoc cref="System.IO.Path.GetPathRoot(string?)" />
		string? GetPathRoot(string? path);

		/// <inheritdoc cref="System.IO.Path.GetRandomFileName()" />
		string GetRandomFileName();

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="System.IO.Path.GetRelativePath(string, string)" />
		string GetRelativePath(string relativeTo, string path);
#endif

		/// <inheritdoc cref="System.IO.Path.GetTempFileName()" />
#if !NETSTANDARD2_0
		[Obsolete(
			"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
		string GetTempFileName();

		/// <inheritdoc cref="System.IO.Path.GetTempPath()" />
		string GetTempPath();

#if FEATURE_SPAN
		/// <inheritdoc cref="System.IO.Path.HasExtension(ReadOnlySpan{char})" />
		bool HasExtension(ReadOnlySpan<char> path);
#endif

		/// <inheritdoc cref="System.IO.Path.HasExtension(string)" />
		bool HasExtension([NotNullWhen(true)] string? path);

#if FEATURE_SPAN
		/// <inheritdoc cref="System.IO.Path.IsPathFullyQualified(ReadOnlySpan{char})" />
		bool IsPathFullyQualified(ReadOnlySpan<char> path);
#endif

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="System.IO.Path.IsPathFullyQualified(string)" />
		bool IsPathFullyQualified(string path);
#endif

#if FEATURE_SPAN
		/// <inheritdoc cref="System.IO.Path.IsPathRooted(ReadOnlySpan{char})" />
		bool IsPathRooted(ReadOnlySpan<char> path);
#endif

		/// <inheritdoc cref="System.IO.Path.IsPathRooted(string)" />
		bool IsPathRooted(string? path);

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="System.IO.Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
		string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2);
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="System.IO.Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
		string Join(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
		string Join(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3,
			ReadOnlySpan<char> path4);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.Join(string, string)" />
		string Join(string? path1, string? path2);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.Join(string, string, string)" />
		string Join(string? path1, string? path2, string? path3);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.Join(string, string, string, string)" />
		string Join(string? path1, string? path2, string? path3, string? path4);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.Join(string[])" />
		string Join(params string?[] paths);
#endif

#if FEATURE_PATH_SPAN
		/// <inheritdoc cref="System.IO.Path.Join(ReadOnlySpan{string})" />
		string Join(params ReadOnlySpan<string?> paths);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
		ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="System.IO.Path.TrimEndingDirectorySeparator(string)" />
		string TrimEndingDirectorySeparator(string path);
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="System.IO.Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
		bool TryJoin(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			Span<char> destination,
			out int charsWritten);
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="System.IO.Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
		bool TryJoin(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3,
			Span<char> destination,
			out int charsWritten);
#endif
	}
}

namespace System.IO.Abstractions
{
	/// <summary>
	///     Backwards-compatibility alias for <see cref="Testably.Abstractions.IPath" />.
	///     <para />
	///     Prefer <see cref="Testably.Abstractions.IPath" /> in new code; this alias will be
	///     marked obsolete in a future major version and removed in the one after.
	/// </summary>
	public interface IPath : Testably.Abstractions.IPath
	{
	}
}

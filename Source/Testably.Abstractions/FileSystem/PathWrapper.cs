using System.Diagnostics.CodeAnalysis;
using System.IO;
#if !NETSTANDARD2_0
using System;
#endif

namespace Testably.Abstractions.FileSystem;

internal sealed class PathWrapper : IPath
{
	private readonly RealFileSystem _fileSystem;

	public PathWrapper(RealFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IPath Members

	/// <inheritdoc cref="IPath.AltDirectorySeparatorChar" />
	public char AltDirectorySeparatorChar
		=> Path.AltDirectorySeparatorChar;

	/// <inheritdoc cref="IPath.DirectorySeparatorChar" />
	public char DirectorySeparatorChar
		=> Path.DirectorySeparatorChar;

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem => _fileSystem;

	/// <inheritdoc cref="IPath.PathSeparator" />
	public char PathSeparator
		=> Path.PathSeparator;

	/// <inheritdoc cref="IPath.VolumeSeparatorChar" />
	public char VolumeSeparatorChar
		=> Path.VolumeSeparatorChar;

	/// <inheritdoc cref="IPath.ChangeExtension(string, string)" />
	[return: NotNullIfNotNull("path")]
	public string? ChangeExtension(string? path, string? extension)
		=> Path.ChangeExtension(path, extension);

	/// <inheritdoc cref="IPath.Combine(string, string)" />
	public string Combine(string path1, string path2)
		=> Path.Combine(path1, path2);

	/// <inheritdoc cref="IPath.Combine(string, string, string)" />
	public string Combine(string path1, string path2, string path3)
		=> Path.Combine(path1, path2, path3);

	/// <inheritdoc cref="IPath.Combine(string, string, string, string)" />
	public string Combine(string path1, string path2, string path3, string path4)
		=> Path.Combine(path1, path2, path3, path4);

	/// <inheritdoc cref="IPath.Combine(string[])" />
	public string Combine(params string[] paths)
		=> Path.Combine(paths);

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.EndsInDirectorySeparator(ReadOnlySpan{char})" />
	public bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
		=> Path.EndsInDirectorySeparator(path);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.EndsInDirectorySeparator(string)" />
	public bool EndsInDirectorySeparator(string path)
		=> Path.EndsInDirectorySeparator(path);
#endif

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IPath.Exists(string)" />
	public bool Exists([NotNullWhen(true)] string? path)
		=> Path.Exists(path);
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetDirectoryName(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
		=> Path.GetDirectoryName(path);
#endif

	/// <inheritdoc cref="IPath.GetDirectoryName(string)" />
	public string? GetDirectoryName(string? path)
		=> Path.GetDirectoryName(path);

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetExtension(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
		=> Path.GetExtension(path);
#endif

	/// <inheritdoc cref="IPath.GetExtension(string)" />
	[return: NotNullIfNotNull("path")]
	public string? GetExtension(string? path)
		=> Path.GetExtension(path);

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetFileName(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
		=> Path.GetFileName(path);
#endif

	/// <inheritdoc cref="IPath.GetFileName(string)" />
	[return: NotNullIfNotNull("path")]
	public string? GetFileName(string? path)
		=> Path.GetFileName(path);

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
		=> Path.GetFileNameWithoutExtension(path);
#endif

	/// <inheritdoc cref="IPath.GetFileNameWithoutExtension(string)" />
	[return: NotNullIfNotNull("path")]
	public string? GetFileNameWithoutExtension(string? path)
		=> Path.GetFileNameWithoutExtension(path);

	/// <inheritdoc cref="IPath.GetFullPath(string)" />
	public string GetFullPath(string path)
		=> Path.GetFullPath(path);

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.GetFullPath(string, string)" />
	public string GetFullPath(string path, string basePath)
		=> Path.GetFullPath(path, basePath);
#endif

	/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
	public char[] GetInvalidFileNameChars()
		=> Path.GetInvalidFileNameChars();

	/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
	public char[] GetInvalidPathChars()
		=> Path.GetInvalidPathChars();

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetPathRoot(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
		=> Path.GetPathRoot(path);
#endif

	/// <inheritdoc cref="IPath.GetPathRoot(string?)" />
	public string? GetPathRoot(string? path)
		=> Path.GetPathRoot(path);

	/// <inheritdoc cref="IPath.GetRandomFileName()" />
	public string GetRandomFileName()
		=> Path.GetRandomFileName();

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.GetRelativePath(string, string)" />
	public string GetRelativePath(string relativeTo, string path)
		=> Path.GetRelativePath(relativeTo, path);
#endif

	/// <inheritdoc cref="IPath.GetTempFileName()" />
#if !NETSTANDARD2_0
	[Obsolete(
		"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
	[ExcludeFromCodeCoverage]
#endif
	public string GetTempFileName()
		=> Path.GetTempFileName();

	/// <inheritdoc cref="IPath.GetTempPath()" />
	public string GetTempPath()
		=> Path.GetTempPath();

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.HasExtension(ReadOnlySpan{char})" />
	public bool HasExtension(ReadOnlySpan<char> path)
		=> Path.HasExtension(path);
#endif

	/// <inheritdoc cref="IPath.HasExtension(string)" />
	public bool HasExtension([NotNullWhen(true)] string? path)
		=> Path.HasExtension(path);

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.IsPathFullyQualified(ReadOnlySpan{char})" />
	public bool IsPathFullyQualified(ReadOnlySpan<char> path)
		=> Path.IsPathFullyQualified(path);
#endif

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.IsPathFullyQualified(string)" />
	public bool IsPathFullyQualified(string path)
		=> Path.IsPathFullyQualified(path);
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.IsPathRooted(ReadOnlySpan{char})" />
	public bool IsPathRooted(ReadOnlySpan<char> path)
		=> Path.IsPathRooted(path);
#endif

	/// <inheritdoc cref="IPath.IsPathRooted(string)" />
	public bool IsPathRooted(string? path)
		=> Path.IsPathRooted(path);

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
		=> Path.Join(path1, path2);
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public string Join(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3)
		=> Path.Join(path1, path2, path3);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public string Join(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3,
		ReadOnlySpan<char> path4)
		=> Path.Join(path1, path2, path3, path4);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(string, string)" />
	public string Join(string? path1, string? path2)
		=> Path.Join(path1, path2);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(string, string, string)" />
	public string Join(string? path1, string? path2, string? path3)
		=> Path.Join(path1, path2, path3);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(string, string, string, string)" />
	public string Join(string? path1, string? path2, string? path3, string? path4)
		=> Path.Join(path1, path2, path3, path4);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(string[])" />
	public string Join(params string?[] paths)
		=> Path.Join(paths);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
		=> Path.TrimEndingDirectorySeparator(path);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(string)" />
	public string TrimEndingDirectorySeparator(string path)
		=> Path.TrimEndingDirectorySeparator(path);
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="IPath.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	public bool TryJoin(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		Span<char> destination,
		out int charsWritten)
		=> Path.TryJoin(path1, path2, destination, out charsWritten);
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="IPath.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	public bool TryJoin(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3,
		Span<char> destination,
		out int charsWritten)
		=> Path.TryJoin(path1, path2, path3, destination, out charsWritten);
#endif

	#endregion
}

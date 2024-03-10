using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
#if !NETSTANDARD2_0
using System;
#endif

namespace Testably.Abstractions.Helpers;

/// <summary>
///     Default implementation for <see cref="System.IO.Path" /> abstractions.
///     <para />
///     Implements <seealso cref="IPath" />
/// </summary>
public abstract class PathSystemBase : IPath
{
	/// <summary>
	///     Initializes a new instance of <see cref="PathSystemBase" /> for the given <paramref name="fileSystem" />.
	/// </summary>
	protected PathSystemBase(IFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IPath Members

	/// <inheritdoc cref="Path.AltDirectorySeparatorChar" />
	public virtual char AltDirectorySeparatorChar
		=> Path.AltDirectorySeparatorChar;

	/// <inheritdoc cref="Path.DirectorySeparatorChar" />
	public virtual char DirectorySeparatorChar
		=> Path.DirectorySeparatorChar;

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="Path.PathSeparator" />
	public virtual char PathSeparator
		=> Path.PathSeparator;

	/// <inheritdoc cref="Path.VolumeSeparatorChar" />
	public virtual char VolumeSeparatorChar
		=> Path.VolumeSeparatorChar;

	/// <inheritdoc cref="Path.ChangeExtension(string, string)" />
	[return: NotNullIfNotNull("path")]
	public virtual string? ChangeExtension(string? path, string? extension)
		=> Path.ChangeExtension(path, extension);

	/// <inheritdoc cref="Path.Combine(string, string)" />
	public virtual string Combine(string path1, string path2)
		=> Path.Combine(path1, path2);

	/// <inheritdoc cref="Path.Combine(string, string, string)" />
	public virtual string Combine(string path1, string path2, string path3)
		=> Path.Combine(path1, path2, path3);

	/// <inheritdoc cref="Path.Combine(string, string, string, string)" />
	public virtual string Combine(string path1, string path2, string path3, string path4)
		=> Path.Combine(path1, path2, path3, path4);

	/// <inheritdoc cref="Path.Combine(string[])" />
	public virtual string Combine(params string[] paths)
		=> Path.Combine(paths);

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="Path.Exists(string)" />
	public abstract bool Exists([NotNullWhen(true)] string? path);
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetDirectoryName(ReadOnlySpan{char})" />
	public virtual ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
		=> Path.GetDirectoryName(path);
#endif

	/// <inheritdoc cref="Path.GetDirectoryName(string)" />
	public virtual string? GetDirectoryName(string? path)
		=> Path.GetDirectoryName(path);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetExtension(ReadOnlySpan{char})" />
	public virtual ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
		=> Path.GetExtension(path);
#endif

	/// <inheritdoc cref="Path.GetExtension(string)" />
	[return: NotNullIfNotNull("path")]
	public virtual string? GetExtension(string? path)
		=> Path.GetExtension(path);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetFileName(ReadOnlySpan{char})" />
	public virtual ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
		=> Path.GetFileName(path);
#endif

	/// <inheritdoc cref="Path.GetFileName(string)" />
	[return: NotNullIfNotNull("path")]
	public virtual string? GetFileName(string? path)
		=> Path.GetFileName(path);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
	public virtual ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
		=> Path.GetFileNameWithoutExtension(path);
#endif

	/// <inheritdoc cref="Path.GetFileNameWithoutExtension(string)" />
	[return: NotNullIfNotNull("path")]
	public virtual string? GetFileNameWithoutExtension(string? path)
		=> Path.GetFileNameWithoutExtension(path);

	/// <inheritdoc cref="Path.GetFullPath(string)" />
	public virtual string GetFullPath(string path)
		=> Path.GetFullPath(path);

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="Path.GetFullPath(string, string)" />
	public virtual string GetFullPath(string path, string basePath)
		=> Path.GetFullPath(path, basePath);
#endif

	/// <inheritdoc cref="Path.GetInvalidFileNameChars()" />
	public virtual char[] GetInvalidFileNameChars()
		=> Path.GetInvalidFileNameChars();

	/// <inheritdoc cref="Path.GetInvalidPathChars()" />
	public virtual char[] GetInvalidPathChars()
		=> Path.GetInvalidPathChars();

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.GetPathRoot(ReadOnlySpan{char})" />
	public virtual ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
		=> Path.GetPathRoot(path);
#endif

	/// <inheritdoc cref="Path.GetPathRoot(string?)" />
	public virtual string? GetPathRoot(string? path)
		=> Path.GetPathRoot(path);

	/// <inheritdoc cref="Path.GetRandomFileName()" />
	public virtual string GetRandomFileName()
		=> Path.GetRandomFileName();

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="Path.GetRelativePath(string, string)" />
	public virtual string GetRelativePath(string relativeTo, string path)
		=> Path.GetRelativePath(relativeTo, path);
#endif

	/// <inheritdoc cref="Path.GetTempFileName()" />
#if !NETSTANDARD2_0
	[Obsolete(
		"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
	public virtual string GetTempFileName()
		=> Path.GetTempFileName();

	/// <inheritdoc cref="Path.GetTempPath()" />
	public virtual string GetTempPath()
		=> Path.GetTempPath();

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.HasExtension(ReadOnlySpan{char})" />
	public virtual bool HasExtension(ReadOnlySpan<char> path)
		=> Path.HasExtension(path);
#endif

	/// <inheritdoc cref="Path.HasExtension(string)" />
	public virtual bool HasExtension([NotNullWhen(true)] string? path)
		=> Path.HasExtension(path);

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.IsPathFullyQualified(ReadOnlySpan{char})" />
	public virtual bool IsPathFullyQualified(ReadOnlySpan<char> path)
		=> Path.IsPathFullyQualified(path);
#endif

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="Path.IsPathFullyQualified(string)" />
	public virtual bool IsPathFullyQualified(string path)
		=> Path.IsPathFullyQualified(path);
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="Path.IsPathRooted(ReadOnlySpan{char})" />
	public virtual bool IsPathRooted(ReadOnlySpan<char> path)
		=> Path.IsPathRooted(path);
#endif

	/// <inheritdoc cref="Path.IsPathRooted(string)" />
	public virtual bool IsPathRooted(string? path)
		=> Path.IsPathRooted(path);

	#endregion

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="Path.EndsInDirectorySeparator(ReadOnlySpan{char})" />
	public virtual bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
		=> Path.EndsInDirectorySeparator(path);

	/// <inheritdoc cref="Path.EndsInDirectorySeparator(string)" />
	public virtual bool EndsInDirectorySeparator(string path)
		=> Path.EndsInDirectorySeparator(path);
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public virtual string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
		=> Path.Join(path1, path2);

	/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public virtual string Join(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3)
		=> Path.Join(path1, path2, path3);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public virtual string Join(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3,
		ReadOnlySpan<char> path4)
		=> Path.Join(path1, path2, path3, path4);

	/// <inheritdoc cref="Path.Join(string, string)" />
	public virtual string Join(string? path1, string? path2)
		=> Path.Join(path1, path2);

	/// <inheritdoc cref="Path.Join(string, string, string)" />
	public virtual string Join(string? path1, string? path2, string? path3)
		=> Path.Join(path1, path2, path3);

	/// <inheritdoc cref="Path.Join(string, string, string, string)" />
	public virtual string Join(string? path1, string? path2, string? path3, string? path4)
		=> Path.Join(path1, path2, path3, path4);

	/// <inheritdoc cref="Path.Join(string[])" />
	public virtual string Join(params string?[] paths)
		=> Path.Join(paths);
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="Path.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
	public virtual ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
		=> Path.TrimEndingDirectorySeparator(path);

	/// <inheritdoc cref="Path.TrimEndingDirectorySeparator(string)" />
	public virtual string TrimEndingDirectorySeparator(string path)
		=> Path.TrimEndingDirectorySeparator(path);
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	public virtual bool TryJoin(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		Span<char> destination,
		out int charsWritten)
		=> Path.TryJoin(path1, path2, destination, out charsWritten);

	/// <inheritdoc cref="Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	public virtual bool TryJoin(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3,
		Span<char> destination,
		out int charsWritten)
		=> Path.TryJoin(path1, path2, path3, destination, out charsWritten);
#endif
}

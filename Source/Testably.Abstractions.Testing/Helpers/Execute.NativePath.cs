using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private sealed class NativePath : IPath
	{
		private readonly MockFileSystem _fileSystem;

		public NativePath(MockFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		#region IPath Members

		/// <inheritdoc cref="Path.AltDirectorySeparatorChar" />
		public char AltDirectorySeparatorChar
			=> System.IO.Path.AltDirectorySeparatorChar;

		/// <inheritdoc cref="Path.DirectorySeparatorChar" />
		public char DirectorySeparatorChar
			=> System.IO.Path.DirectorySeparatorChar;

		/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
		public IFileSystem FileSystem => _fileSystem;

		/// <inheritdoc cref="Path.PathSeparator" />
		public char PathSeparator
			=> System.IO.Path.PathSeparator;

		/// <inheritdoc cref="Path.VolumeSeparatorChar" />
		public char VolumeSeparatorChar
			=> System.IO.Path.VolumeSeparatorChar;

		/// <inheritdoc cref="Path.ChangeExtension(string, string)" />
		[return: NotNullIfNotNull("path")]
		public string? ChangeExtension(string? path, string? extension)
			=> System.IO.Path.ChangeExtension(path, extension);

		/// <inheritdoc cref="Path.Combine(string, string)" />
		public string Combine(string path1, string path2)
			=> System.IO.Path.Combine(path1, path2);

		/// <inheritdoc cref="Path.Combine(string, string, string)" />
		public string Combine(string path1, string path2, string path3)
			=> System.IO.Path.Combine(path1, path2, path3);

		/// <inheritdoc cref="Path.Combine(string, string, string, string)" />
		public string Combine(string path1, string path2, string path3, string path4)
			=> System.IO.Path.Combine(path1, path2, path3, path4);

		/// <inheritdoc cref="Path.Combine(string[])" />
		public string Combine(params string[] paths)
			=> System.IO.Path.Combine(paths);

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.EndsInDirectorySeparator(ReadOnlySpan{char})" />
		public bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
			=> System.IO.Path.EndsInDirectorySeparator(path);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.EndsInDirectorySeparator(string)" />
		public bool EndsInDirectorySeparator(string path)
			=> System.IO.Path.EndsInDirectorySeparator(path);
#endif

#if FEATURE_FILESYSTEM_NET7
		/// <inheritdoc cref="Path.Exists(string)" />
		public bool Exists([NotNullWhen(true)] string? path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}

			return _fileSystem.Storage.GetContainer(_fileSystem.Storage.GetLocation(path))
				is not NullContainer;
		}
#endif

#if FEATURE_SPAN
		/// <inheritdoc cref="Path.GetDirectoryName(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
			=> System.IO.Path.GetDirectoryName(path);
#endif

		/// <inheritdoc cref="Path.GetDirectoryName(string)" />
		public string? GetDirectoryName(string? path)
			=> System.IO.Path.GetDirectoryName(path);

#if FEATURE_SPAN
		/// <inheritdoc cref="Path.GetExtension(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
			=> System.IO.Path.GetExtension(path);
#endif

		/// <inheritdoc cref="Path.GetExtension(string)" />
		[return: NotNullIfNotNull("path")]
		public string? GetExtension(string? path)
			=> System.IO.Path.GetExtension(path);

#if FEATURE_SPAN
		/// <inheritdoc cref="Path.GetFileName(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
			=> System.IO.Path.GetFileName(path);
#endif

		/// <inheritdoc cref="Path.GetFileName(string)" />
		[return: NotNullIfNotNull("path")]
		public string? GetFileName(string? path)
			=> System.IO.Path.GetFileName(path);

#if FEATURE_SPAN
		/// <inheritdoc cref="Path.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
			=> System.IO.Path.GetFileNameWithoutExtension(path);
#endif

		/// <inheritdoc cref="Path.GetFileNameWithoutExtension(string)" />
		[return: NotNullIfNotNull("path")]
		public string? GetFileNameWithoutExtension(string? path)
			=> System.IO.Path.GetFileNameWithoutExtension(path);

		/// <inheritdoc cref="Path.GetFullPath(string)" />
		public string GetFullPath(string path)
		{
			path.EnsureValidArgument(_fileSystem, nameof(path));

			string? pathRoot = System.IO.Path.GetPathRoot(path);
			string? directoryRoot =
				System.IO.Path.GetPathRoot(_fileSystem.Storage.CurrentDirectory);
			if (!string.IsNullOrEmpty(pathRoot) && !string.IsNullOrEmpty(directoryRoot))
			{
				if (char.ToUpperInvariant(pathRoot[0]) != char.ToUpperInvariant(directoryRoot[0]))
				{
					return System.IO.Path.GetFullPath(path);
				}

				if (pathRoot.Length < directoryRoot.Length)
				{
					path = path.Substring(pathRoot.Length);
				}
			}

			return System.IO.Path.GetFullPath(System.IO.Path.Combine(
				_fileSystem.Storage.CurrentDirectory,
				path));
		}

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="Path.GetFullPath(string, string)" />
		public string GetFullPath(string path, string basePath)
			=> System.IO.Path.GetFullPath(path, basePath);
#endif

		/// <inheritdoc cref="Path.GetInvalidFileNameChars()" />
		public char[] GetInvalidFileNameChars()
			=> System.IO.Path.GetInvalidFileNameChars();

		/// <inheritdoc cref="Path.GetInvalidPathChars()" />
		public char[] GetInvalidPathChars()
			=> System.IO.Path.GetInvalidPathChars();

#if FEATURE_SPAN
		/// <inheritdoc cref="Path.GetPathRoot(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
			=> System.IO.Path.GetPathRoot(path);
#endif

		/// <inheritdoc cref="Path.GetPathRoot(string?)" />
		public string? GetPathRoot(string? path)
			=> System.IO.Path.GetPathRoot(path);

		/// <inheritdoc cref="Path.GetRandomFileName()" />
		public string GetRandomFileName()
			=> System.IO.Path.GetRandomFileName();

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="Path.GetRelativePath(string, string)" />
		public string GetRelativePath(string relativeTo, string path)
		{
			relativeTo.EnsureValidArgument(_fileSystem, nameof(relativeTo));
			path.EnsureValidArgument(_fileSystem, nameof(path));

			relativeTo = _fileSystem.Execute.Path.GetFullPath(relativeTo);
			path = _fileSystem.Execute.Path.GetFullPath(path);

			return System.IO.Path.GetRelativePath(relativeTo, path);
		}
#endif

		/// <inheritdoc cref="Path.GetTempFileName()" />
#if !NETSTANDARD2_0
		[Obsolete(
			"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
		public string GetTempFileName()
			=> System.IO.Path.GetTempFileName();

		/// <inheritdoc cref="Path.GetTempPath()" />
		public string GetTempPath()
			=> System.IO.Path.GetTempPath();

#if FEATURE_SPAN
		/// <inheritdoc cref="Path.HasExtension(ReadOnlySpan{char})" />
		public bool HasExtension(ReadOnlySpan<char> path)
			=> System.IO.Path.HasExtension(path);
#endif

		/// <inheritdoc cref="Path.HasExtension(string)" />
		public bool HasExtension([NotNullWhen(true)] string? path)
			=> System.IO.Path.HasExtension(path);

#if FEATURE_SPAN
		/// <inheritdoc cref="Path.IsPathFullyQualified(ReadOnlySpan{char})" />
		public bool IsPathFullyQualified(ReadOnlySpan<char> path)
			=> System.IO.Path.IsPathFullyQualified(path);
#endif

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="Path.IsPathFullyQualified(string)" />
		public bool IsPathFullyQualified(string path)
			=> System.IO.Path.IsPathFullyQualified(path);
#endif

#if FEATURE_SPAN
		/// <inheritdoc cref="Path.IsPathRooted(ReadOnlySpan{char})" />
		public bool IsPathRooted(ReadOnlySpan<char> path)
			=> System.IO.Path.IsPathRooted(path);
#endif

		/// <inheritdoc cref="Path.IsPathRooted(string)" />
		public bool IsPathRooted(string? path)
			=> System.IO.Path.IsPathRooted(path);

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
		public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
			=> System.IO.Path.Join(path1, path2);
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
		public string Join(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3)
			=> System.IO.Path.Join(path1, path2, path3);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
		public string Join(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3,
			ReadOnlySpan<char> path4)
			=> System.IO.Path.Join(path1, path2, path3, path4);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.Join(string, string)" />
		public string Join(string? path1, string? path2)
			=> System.IO.Path.Join(path1, path2);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.Join(string, string, string)" />
		public string Join(string? path1, string? path2, string? path3)
			=> System.IO.Path.Join(path1, path2, path3);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.Join(string, string, string, string)" />
		public string Join(string? path1, string? path2, string? path3, string? path4)
			=> System.IO.Path.Join(path1, path2, path3, path4);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.Join(string[])" />
		public string Join(params string?[] paths)
			=> System.IO.Path.Join(paths);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
			=> System.IO.Path.TrimEndingDirectorySeparator(path);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="Path.TrimEndingDirectorySeparator(string)" />
		public string TrimEndingDirectorySeparator(string path)
			=> System.IO.Path.TrimEndingDirectorySeparator(path);
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
		public bool TryJoin(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			Span<char> destination,
			out int charsWritten)
			=> System.IO.Path.TryJoin(path1, path2, destination, out charsWritten);
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
		public bool TryJoin(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3,
			Span<char> destination,
			out int charsWritten)
			=> System.IO.Path.TryJoin(path1, path2, path3, destination, out charsWritten);
#endif

		#endregion
	}
}

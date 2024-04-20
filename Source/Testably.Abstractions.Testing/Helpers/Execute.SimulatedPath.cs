using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
#if FEATURE_FILESYSTEM_NET7
using Testably.Abstractions.Testing.Storage;
#endif

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private abstract class SimulatedPath(MockFileSystem fileSystem) : IPath
	{
		#region IPath Members

		/// <inheritdoc cref="IPath.AltDirectorySeparatorChar" />
		public abstract char AltDirectorySeparatorChar { get; }

		/// <inheritdoc cref="IPath.DirectorySeparatorChar" />
		public abstract char DirectorySeparatorChar { get; }

		/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
		public IFileSystem FileSystem => fileSystem;

		/// <inheritdoc cref="IPath.PathSeparator" />
		public abstract char PathSeparator { get; }

		/// <inheritdoc cref="IPath.VolumeSeparatorChar" />
		public abstract char VolumeSeparatorChar { get; }

		/// <inheritdoc cref="IPath.ChangeExtension(string, string)" />
		[return: NotNullIfNotNull("path")]
		public string? ChangeExtension(string? path, string? extension)
		{
			if (path == null)
			{
				return null;
			}

			if (path == string.Empty)
			{
				return string.Empty;
			}

			if (extension == null)
			{
				extension = "";
			}
			else if (!extension.StartsWith('.'))
			{
				extension = "." + extension;
			}

			if (!TryGetExtensionIndex(path, out int? dotIndex))
			{
				return path + extension;
			}

			return path.Substring(0, dotIndex.Value) + extension;
		}

		/// <inheritdoc cref="IPath.Combine(string, string)" />
		public string Combine(string path1, string path2)
		{
			if (path1 == null)
			{
				throw new ArgumentNullException(nameof(path1));
			}

			if (path2 == null)
			{
				throw new ArgumentNullException(nameof(path2));
			}

			return CombineInternal([path1, path2]);
		}

		/// <inheritdoc cref="IPath.Combine(string, string, string)" />
		public string Combine(string path1, string path2, string path3)
		{
			if (path1 == null)
			{
				throw new ArgumentNullException(nameof(path1));
			}

			if (path2 == null)
			{
				throw new ArgumentNullException(nameof(path2));
			}

			if (path3 == null)
			{
				throw new ArgumentNullException(nameof(path3));
			}

			return CombineInternal([path1, path2, path3]);
		}

		/// <inheritdoc cref="IPath.Combine(string, string, string, string)" />
		public string Combine(string path1, string path2, string path3, string path4)
		{
			if (path1 == null)
			{
				throw new ArgumentNullException(nameof(path1));
			}

			if (path2 == null)
			{
				throw new ArgumentNullException(nameof(path2));
			}

			if (path3 == null)
			{
				throw new ArgumentNullException(nameof(path3));
			}

			if (path4 == null)
			{
				throw new ArgumentNullException(nameof(path4));
			}

			return CombineInternal([path1, path2, path3, path4]);
		}

		/// <inheritdoc cref="IPath.Combine(string[])" />
		public string Combine(params string[] paths)
			=> CombineInternal(paths);

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.EndsInDirectorySeparator(ReadOnlySpan{char})" />
		public bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
			=> EndsInDirectorySeparator(path.ToString());
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.EndsInDirectorySeparator(string)" />
		public bool EndsInDirectorySeparator(string path)
			=> !string.IsNullOrEmpty(path) && IsDirectorySeparator(path[path.Length - 1]);
#endif

#if FEATURE_FILESYSTEM_NET7
		/// <inheritdoc cref="IPath.Exists(string)" />
		public bool Exists([NotNullWhen(true)] string? path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}

			return fileSystem.Storage.GetContainer(fileSystem.Storage.GetLocation(path))
				is not NullContainer;
		}
#endif

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.GetDirectoryName(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
			=> GetDirectoryName(path.ToString());
#endif

		/// <inheritdoc cref="IPath.GetDirectoryName(string)" />
		public string? GetDirectoryName(string? path)
		{
			if (path == null || IsEffectivelyEmpty(path))
			{
				return null;
			}

			int rootLength = GetRootLength(path);
			if (path.Length <= rootLength)
			{
				return null;
			}

			int end = path.Length;
			while (end > rootLength && !IsDirectorySeparator(path[end - 1]))
			{
				end--;
			}

			while (end > rootLength && IsDirectorySeparator(path[end - 1]))
			{
				end--;
			}

			return NormalizeDirectorySeparators(path.Substring(0, end));
		}

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.GetExtension(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
			=> GetExtension(path.ToString());
#endif

		/// <inheritdoc cref="IPath.GetExtension(string)" />
		[return: NotNullIfNotNull("path")]
		public string? GetExtension(string? path)
		{
			if (path == null)
			{
				return null;
			}

			if (TryGetExtensionIndex(path, out int? dotIndex))
			{
				return dotIndex != path.Length - 1
					? path.Substring(dotIndex.Value)
					: string.Empty;
			}

			return string.Empty;
		}

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.GetFileName(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
			=> GetFileName(path.ToString());
#endif

		/// <inheritdoc cref="IPath.GetFileName(string)" />
		[return: NotNullIfNotNull("path")]
		public string? GetFileName(string? path)
		{
			if (path == null)
			{
				return null;
			}

			for (int i = path.Length - 1; i >= 0; i--)
			{
				if (IsDirectorySeparator(path[i]))
				{
					return path.Substring(i + 1);
				}
			}

			return path;
		}

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
			=> GetFileNameWithoutExtension(path.ToString());
#endif

		/// <inheritdoc cref="IPath.GetFileNameWithoutExtension(string)" />
		[return: NotNullIfNotNull("path")]
		public string? GetFileNameWithoutExtension(string? path)
		{
			if (path == null)
			{
				return null;
			}

			string fileName = GetFileName(path);
			int lastPeriod = fileName.LastIndexOf('.');
			return lastPeriod < 0 ? fileName : fileName.Substring(0, lastPeriod);
		}

		/// <inheritdoc cref="IPath.GetFullPath(string)" />
		public abstract string GetFullPath(string path);

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="IPath.GetFullPath(string, string)" />
		public abstract string GetFullPath(string path, string basePath);
#endif

		/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
		public abstract char[] GetInvalidFileNameChars();

		/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
		public abstract char[] GetInvalidPathChars();

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.GetPathRoot(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
			=> GetPathRoot(path.ToString());
#endif

		/// <inheritdoc cref="IPath.GetPathRoot(string?)" />
		public abstract string? GetPathRoot(string? path);

		/// <inheritdoc cref="IPath.GetRandomFileName()" />
		public string GetRandomFileName()
			=> $"{RandomString(8)}.{RandomString(3)}";

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="IPath.GetRelativePath(string, string)" />
		public string GetRelativePath(string relativeTo, string path)
		{
			relativeTo.EnsureValidArgument(fileSystem, nameof(relativeTo));
			path.EnsureValidArgument(fileSystem, nameof(path));

			relativeTo = fileSystem.Execute.Path.GetFullPath(relativeTo);
			path = fileSystem.Execute.Path.GetFullPath(path);

			return System.IO.Path.GetRelativePath(relativeTo, path);
		}
#endif

		/// <inheritdoc cref="IPath.GetTempFileName()" />
#if !NETSTANDARD2_0
		[Obsolete(
			"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
		public string GetTempFileName()
			=> System.IO.Path.GetTempFileName();

		/// <inheritdoc cref="IPath.GetTempPath()" />
		public abstract string GetTempPath();

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.HasExtension(ReadOnlySpan{char})" />
		public bool HasExtension(ReadOnlySpan<char> path)
			=> HasExtension(path.ToString());
#endif

		/// <inheritdoc cref="IPath.HasExtension(string)" />
		public bool HasExtension([NotNullWhen(true)] string? path)
			=> System.IO.Path.HasExtension(path);

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.IsPathFullyQualified(ReadOnlySpan{char})" />
		public bool IsPathFullyQualified(ReadOnlySpan<char> path)
			=> IsPathFullyQualified(path.ToString());
#endif

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="IPath.IsPathFullyQualified(string)" />
		public bool IsPathFullyQualified(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			return !IsPartiallyQualified(path);
		}
#endif

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.IsPathRooted(ReadOnlySpan{char})" />
		public bool IsPathRooted(ReadOnlySpan<char> path)
			=> IsPathRooted(path.ToString());
#endif

		/// <inheritdoc cref="IPath.IsPathRooted(string)" />
		public abstract bool IsPathRooted(string? path);

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
		public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
			=> JoinInternal([path1.ToString(), path2.ToString()]);
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
		public string Join(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3)
			=> JoinInternal([path1.ToString(), path2.ToString(), path3.ToString()]);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
		public string Join(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3,
			ReadOnlySpan<char> path4)
			=> JoinInternal(
				[path1.ToString(), path2.ToString(), path3.ToString(), path4.ToString()]);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(string, string)" />
		public string Join(string? path1, string? path2)
			=> JoinInternal([path1, path2]);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(string, string, string)" />
		public string Join(string? path1, string? path2, string? path3)
			=> JoinInternal([path1, path2, path3]);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(string, string, string, string)" />
		public string Join(string? path1, string? path2, string? path3, string? path4)
			=> JoinInternal([path1, path2, path3, path4]);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(string[])" />
		public string Join(params string?[] paths)
			=> JoinInternal(paths);
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
			=> TrimEndingDirectorySeparator(path.ToString());
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(string)" />
		public string TrimEndingDirectorySeparator(string path)
			=> System.IO.Path.TrimEndingDirectorySeparator(path);
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="IPath.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
		public bool TryJoin(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			Span<char> destination,
			out int charsWritten)
		{
			string result = Join(path1, path2);
			if (destination.Length < result.Length)
			{
				charsWritten = 0;
				return false;
			}

			result.AsSpan().CopyTo(destination);
			charsWritten = result.Length;
			return true;
		}
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="IPath.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
		public bool TryJoin(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3,
			Span<char> destination,
			out int charsWritten)
		{
			string result = Join(path1, path2, path3);
			if (destination.Length < result.Length)
			{
				charsWritten = 0;
				return false;
			}

			result.AsSpan().CopyTo(destination);
			charsWritten = result.Length;
			return true;
		}
#endif

		#endregion

		private string CombineInternal(string[] paths)
		{
			string NormalizePath(string path, bool ignoreStartingSeparator)
			{
				if (!ignoreStartingSeparator && (
					path[0] == DirectorySeparatorChar ||
					path[0] == AltDirectorySeparatorChar))
				{
					path = path.Substring(1);
				}

				if (path[path.Length - 1] == DirectorySeparatorChar ||
				    path[path.Length - 1] == AltDirectorySeparatorChar)
				{
					path = path.Substring(0, path.Length - 1);
				}

				return NormalizeDirectorySeparators(path);
			}

			if (paths == null)
			{
				throw new ArgumentNullException(nameof(paths));
			}

			StringBuilder sb = new();

			bool isFirst = true;
			bool endsWithDirectorySeparator = false;
			foreach (string path in paths)
			{
				if (path == null)
				{
					throw new ArgumentNullException(nameof(paths));
				}

				if (string.IsNullOrEmpty(path))
				{
					continue;
				}

				if (IsPathRooted(path))
				{
					sb.Clear();
					isFirst = true;
				}

				sb.Append(NormalizePath(path, isFirst));
				sb.Append(DirectorySeparatorChar);
				endsWithDirectorySeparator = path.EndsWith(DirectorySeparatorChar) ||
				                             path.EndsWith(AltDirectorySeparatorChar);
			}

			if (!endsWithDirectorySeparator)
			{
				return sb.ToString(0, sb.Length - 1);
			}

			return sb.ToString();
		}

		protected abstract int GetRootLength(string path);
		protected abstract bool IsDirectorySeparator(char c);
		protected abstract bool IsEffectivelyEmpty(string path);

		protected abstract bool IsPartiallyQualified(string path);

#if FEATURE_PATH_JOIN || FEATURE_PATH_ADVANCED
		private string JoinInternal(string?[] paths)
		{
			if (paths == null)
			{
				throw new ArgumentNullException(nameof(paths));
			}

			if (paths.Length == 0)
			{
				return string.Empty;
			}

			StringBuilder sb = new();
			foreach (string? path in paths)
			{
				if (string.IsNullOrEmpty(path))
				{
					continue;
				}

				if (sb.Length == 0)
				{
					sb.Append(path);
				}
				else
				{
					if (!IsDirectorySeparator(sb[sb.Length - 1]) && !IsDirectorySeparator(path[0]))
					{
						sb.Append(DirectorySeparatorChar);
					}

					sb.Append(path);
				}
			}

			return sb.ToString();
		}
#endif

		protected abstract string NormalizeDirectorySeparators(string path);

		protected string RandomString(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[fileSystem.RandomSystem.Random.Shared.Next(s.Length)]).ToArray());
		}

		/// <summary>
		///     Remove relative segments from the given path (without combining with a root).
		/// </summary>
		protected string RemoveRelativeSegments(string path, int rootLength)
		{
			Debug.Assert(rootLength > 0);
			bool flippedSeparator = false;

			StringBuilder sb = new();

			int skip = rootLength;
			// We treat "\.." , "\." and "\\" as a relative segment. We want to collapse the first separator past the root presuming
			// the root actually ends in a separator. Otherwise the first segment for RemoveRelativeSegments
			// in cases like "\\?\C:\.\" and "\\?\C:\..\", the first segment after the root will be ".\" and "..\" which is not considered as a relative segment and hence not be removed.
			if (IsDirectorySeparator(path[skip - 1]))
			{
				skip--;
			}

			// Remove "//", "/./", and "/../" from the path by copying each character to the output,
			// except the ones we're removing, such that the builder contains the normalized path
			// at the end.
			if (skip > 0)
			{
				sb.Append(path.Substring(0, skip));
			}

			for (int i = skip; i < path.Length; i++)
			{
				char c = path[i];

				if (IsDirectorySeparator(c) && i + 1 < path.Length)
				{
					// Skip this character if it's a directory separator and if the next character is, too,
					// e.g. "parent//child" => "parent/child"
					if (IsDirectorySeparator(path[i + 1]))
					{
						continue;
					}

					// Skip this character and the next if it's referring to the current directory,
					// e.g. "parent/./child" => "parent/child"
					if ((i + 2 == path.Length || IsDirectorySeparator(path[i + 2])) &&
					    path[i + 1] == '.')
					{
						i++;
						continue;
					}

					// Skip this character and the next two if it's referring to the parent directory,
					// e.g. "parent/child/../grandchild" => "parent/grandchild"
					if (i + 2 < path.Length &&
					    (i + 3 == path.Length || IsDirectorySeparator(path[i + 3])) &&
					    path[i + 1] == '.' && path[i + 2] == '.')
					{
						// Unwind back to the last slash (and if there isn't one, clear out everything).
						int s;
						for (s = sb.Length - 1; s >= skip; s--)
						{
							if (IsDirectorySeparator(sb[s]))
							{
								sb.Length =
									i + 3 >= path.Length && s == skip
										? s + 1
										: s; // to avoid removing the complete "\tmp\" segment in cases like \\?\C:\tmp\..\, C:\tmp\..
								break;
							}
						}

						if (s < skip)
						{
							sb.Length = skip;
						}

						i += 2;
						continue;
					}
				}

				// Normalize the directory separator if needed
				if (c != DirectorySeparatorChar && c == AltDirectorySeparatorChar)
				{
					c = DirectorySeparatorChar;
					flippedSeparator = true;
				}

				sb.Append(c);
			}

			// If we haven't changed the source path, return the original
			if (!flippedSeparator && sb.Length == path.Length)
			{
				return path;
			}

			// We may have eaten the trailing separator from the root when we started and not replaced it
			if (skip != rootLength && sb.Length < rootLength)
			{
				sb.Append(path[rootLength - 1]);
			}

			return sb.ToString();
		}

		private bool TryGetExtensionIndex(string path, [NotNullWhen(true)] out int? dotIndex)
		{
			for (int i = path.Length - 1; i >= 0; i--)
			{
				char ch = path[i];

				if (ch == '.')
				{
					dotIndex = i;
					return true;
				}

				if (IsDirectorySeparator(ch))
				{
					break;
				}
			}

			dotIndex = null;
			return false;
		}
	}
}

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
using Testably.Abstractions.Testing.Storage;
#endif

namespace Testably.Abstractions.Testing.Helpers;

internal sealed partial class Execute
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

			#pragma warning disable CA1845 // Use span-based 'string.Concat' and 'AsSpan' instead of 'Substring
			return path.Substring(0, dotIndex.Value) + extension;
			#pragma warning restore CA1845
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
	
#if FEATURE_PATH_SPAN
		/// <inheritdoc cref="IPath.Combine(ReadOnlySpan{string})" />
		public string Combine(params ReadOnlySpan<string> paths)
			=> CombineInternal(paths.ToArray());
#endif

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

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
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
			=> $"{RandomString(fileSystem, 8)}.{RandomString(fileSystem, 3)}";

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="IPath.GetRelativePath(string, string)" />
		public string GetRelativePath(string relativeTo, string path)
		{
			relativeTo.EnsureValidArgument(fileSystem, nameof(relativeTo));

			relativeTo = fileSystem.Execute.Path.GetFullPath(relativeTo);
			path = fileSystem.Execute.Path.GetFullPath(path);

			Func<char, char, bool> charComparer = (c1, c2) => c1 == c2;
			if (fileSystem.Execute.StringComparisonMode == StringComparison.OrdinalIgnoreCase)
			{
				charComparer = (c1, c2) => char.ToUpperInvariant(c1) == char.ToUpperInvariant(c2);
			}

			int commonLength = GetCommonPathLength(relativeTo, path, charComparer);

			// If there is nothing in common they can't share the same root, return the "to" path as is.
			if (commonLength == 0)
			{
				return path;
			}

			// Trailing separators aren't significant for comparison
			int relativeToLength = relativeTo.Length;
			if (IsDirectorySeparator(relativeTo[relativeToLength - 1]))
			{
				relativeToLength--;
			}

			int pathLength = path.Length;
			bool pathEndsInSeparator = IsDirectorySeparator(path[pathLength - 1]);
			if (pathEndsInSeparator)
			{
				pathLength--;
			}

			// If we have effectively the same path, return "."
			if (relativeToLength == pathLength && commonLength >= relativeToLength)
			{
				return ".";
			}

			return CreateRelativePath(relativeTo, path,
				commonLength, relativeToLength, pathLength,
				pathEndsInSeparator);
		}
#endif

		/// <inheritdoc cref="IPath.GetTempFileName()" />
#if !NETSTANDARD2_0
		[Obsolete(
			"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
		public string GetTempFileName()
			=> CreateTempFileName(fileSystem);

		/// <inheritdoc cref="IPath.GetTempPath()" />
		public abstract string GetTempPath();

#if FEATURE_SPAN
		/// <inheritdoc cref="IPath.HasExtension(ReadOnlySpan{char})" />
		public bool HasExtension(ReadOnlySpan<char> path)
			=> HasExtension(path.ToString());
#endif

		/// <inheritdoc cref="IPath.HasExtension(string)" />
		public bool HasExtension([NotNullWhen(true)] string? path)
		{
			if (path == null)
			{
				return false;
			}

			return TryGetExtensionIndex(path, out int? dotIndex)
			       && dotIndex < path.Length - 1;
		}

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
	
#if FEATURE_PATH_SPAN
		/// <inheritdoc cref="IPath.Join(ReadOnlySpan{string})" />
		public string Join(params ReadOnlySpan<string?> paths)
			=> JoinInternal(paths.ToArray());
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
			=> TrimEndingDirectorySeparator(path.ToString());
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(string)" />
		public string TrimEndingDirectorySeparator(string path)
		{
			return path.Length != GetRootLength(path) && EndsInDirectorySeparator(path)
				? path.Substring(0, path.Length - 1)
				: path;
		}
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
			string NormalizePath(string path)
			{
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

			bool endsWithDirectorySeparator = true;
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
				}

				sb.Append(NormalizePath(path));
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

#if FEATURE_PATH_RELATIVE
		/// <summary>
		///     We have the same root, we need to calculate the difference now using the
		///     common Length and Segment count past the length.
		/// </summary>
		/// <remarks>
		///     Some examples:
		///     <para />
		///     C:\Foo C:\Bar L3, S1 -> ..\Bar<br />
		///     C:\Foo C:\Foo\Bar L6, S0 -> Bar<br />
		///     C:\Foo\Bar C:\Bar\Bar L3, S2 -> ..\..\Bar\Bar<br />
		///     C:\Foo\Foo C:\Foo\Bar L7, S1 -> ..\Bar<br />
		/// </remarks>
		private string CreateRelativePath(string relativeTo, string path, int commonLength,
			int relativeToLength, int pathLength, bool pathEndsInSeparator)
		{
			StringBuilder sb = new();

			// Add parent segments for segments past the common on the "from" path
			if (commonLength < relativeToLength)
			{
				sb.Append("..");

				for (int i = commonLength + 1; i < relativeToLength; i++)
				{
					if (IsDirectorySeparator(relativeTo[i]))
					{
						sb.Append(DirectorySeparatorChar);
						sb.Append("..");
					}
				}
			}
			else if (IsDirectorySeparator(path[commonLength]))
			{
				// No parent segments, and we need to eat the initial separator
				commonLength++;
			}

			// Now add the rest of the "to" path, adding back the trailing separator
			int differenceLength = pathLength - commonLength;
			if (pathEndsInSeparator)
			{
				differenceLength++;
			}

			if (differenceLength > 0)
			{
				if (sb.Length > 0)
				{
					sb.Append(DirectorySeparatorChar);
				}

				sb.Append(path, commonLength, differenceLength);
			}

			return sb.ToString();
		}
#endif

#if FEATURE_PATH_RELATIVE
		/// <summary>
		///     Get the common path length from the start of the string.
		/// </summary>
		private int GetCommonPathLength(string first, string second,
			Func<char, char, bool> charComparer)
		{
			int commonChars = 0;
			for (; commonChars < first.Length; commonChars++)
			{
				if (second.Length <= commonChars)
				{
					break;
				}

				if (!charComparer(first[commonChars], second[commonChars]))
				{
					break;
				}
			}

			// If nothing matches
			if (commonChars == 0)
			{
				return commonChars;
			}

			// Or we're a full string and equal length or match to a separator
			if (commonChars == first.Length && commonChars == second.Length)
			{
				return commonChars;
			}

			if ((second.Length > commonChars && IsDirectorySeparator(second[commonChars])) ||
			    IsDirectorySeparator(first[commonChars - 1]))
			{
				return commonChars;
			}

			// It's possible we matched somewhere in the middle of a segment e.g. C:\Foodie and C:\Foobar.
			while (commonChars > 0 && !IsDirectorySeparator(first[commonChars - 1]))
			{
				commonChars--;
			}

			return commonChars;
		}
#endif

		protected abstract int GetRootLength(string path);
		protected abstract bool IsDirectorySeparator(char c);
		protected abstract bool IsEffectivelyEmpty(string path);

#if FEATURE_PATH_RELATIVE
		protected abstract bool IsPartiallyQualified(string path);
#endif

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

			sb.Append(path, 0, skip);

			// Remove "//", "/./", and "/../" from the path by copying each character to the output,
			// except the ones we're removing, such that the builder contains the normalized path
			// at the end.
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
						for (int s = sb.Length - 1; s >= skip; s--)
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

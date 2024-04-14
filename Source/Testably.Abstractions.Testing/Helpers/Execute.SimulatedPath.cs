using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
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

			int subLength = path.Length;
			if (subLength == 0)
			{
				return string.Empty;
			}

			for (int i = path.Length - 1; i >= 0; i--)
			{
				char ch = path[i];

				if (ch == '.')
				{
					subLength = i;
					break;
				}

				if (ch == DirectorySeparatorChar || ch == AltDirectorySeparatorChar)
				{
					break;
				}
			}

			if (extension == null)
			{
				return path.Substring(0, subLength);
			}

			string subpath = path.Substring(0, subLength);
			return extension.StartsWith('.')
				? string.Concat(subpath, extension)
				: string.Concat(subpath, ".", extension);
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

			return Combine([path1, path2]);
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

			return Combine([path1, path2, path3]);
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

			return Combine([path1, path2, path3, path4]);
		}

		/// <inheritdoc cref="IPath.Combine(string[])" />
		public string Combine(params string[] paths)
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

				return path;
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
			int GetDirectoryNameOffset(string p)
			{
				int rootLength = GetRootLength(p);
				int end = p.Length;
				if (end <= rootLength)
				{
					return -1;
				}

				while (end > rootLength && !IsDirectorySeparator(p[--end]))
				{
					// Do nothing
				}

				// Trim off any remaining separators (to deal with C:\foo\\bar)
				while (end > rootLength && IsDirectorySeparator(p[end - 1]))
				{
					end--;
				}

				return end;
			}

			if (path == null || IsEffectivelyEmpty(path))
			{
				return null;
			}

			int end = GetDirectoryNameOffset(path);
			if (end >= 0)
			{
				return NormalizeDirectorySeparators(path.Substring(0, end));
			}

			return null;
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

			int length = path.Length;

			for (int i = length - 1; i >= 0; i--)
			{
				char ch = path[i];
				if (ch == '.')
				{
					return i != length - 1
						? path.Substring(i, length - i)
						: string.Empty;
				}

				if (IsDirectorySeparator(ch))
				{
					break;
				}
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

			int length = path.Length;

			for (int i = length - 1; i >= 0; i--)
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
			return lastPeriod < 0
				? fileName
				: // No extension was found
				fileName.Substring(0, lastPeriod);
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

			// Need to check if the roots are different- if they are we need to return the "to" path.
			if (!AreRootsEqual(relativeTo, path, fileSystem.Execute.StringComparisonMode))
				return path;

			int commonLength = GetCommonPathLength(relativeTo, path, ignoreCase: fileSystem.Execute.StringComparisonMode == StringComparison.OrdinalIgnoreCase);

			// If there is nothing in common they can't share the same root, return the "to" path as is.
			if (commonLength == 0)
				return path;

			// Trailing separators aren't significant for comparison
			int relativeToLength = relativeTo.Length;
			if (IsDirectorySeparator(relativeTo[relativeToLength-1]))
				relativeToLength--;

			int pathLength = path.Length;
			bool pathEndsInSeparator = IsDirectorySeparator(path[pathLength - 1]);
			if (pathEndsInSeparator)
				pathLength--;

			// If we have effectively the same path, return "."
			if (relativeToLength == pathLength && commonLength >= relativeToLength) return ".";

			// We have the same root, we need to calculate the difference now using the
			// common Length and Segment count past the length.
			//
			// Some examples:
			//
			//  C:\Foo C:\Bar L3, S1 -> ..\Bar
			//  C:\Foo C:\Foo\Bar L6, S0 -> Bar
			//  C:\Foo\Bar C:\Bar\Bar L3, S2 -> ..\..\Bar\Bar
			//  C:\Foo\Foo C:\Foo\Bar L7, S1 -> ..\Bar

			var sb = new StringBuilder();
			sb.EnsureCapacity(Math.Max(relativeTo.Length, path.Length));

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
				// No parent segments and we need to eat the initial separator
				//  (C:\Foo C:\Foo\Bar case)
				commonLength++;
			}

			// Now add the rest of the "to" path, adding back the trailing separator
			int differenceLength = pathLength - commonLength;
			if (pathEndsInSeparator)
				differenceLength++;

			if (differenceLength > 0)
			{
				if (sb.Length > 0)
				{
					sb.Append(DirectorySeparatorChar);
				}

				sb.Append(path.AsSpan(commonLength, differenceLength));
			}

			return sb.ToString();
		}
#endif

		/// <inheritdoc cref="IPath.GetTempFileName()" />
#if !NETSTANDARD2_0
		[Obsolete(
			"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
		public string GetTempFileName()
		{
			int i = 0;
			while (true)
			{
				string fileName = $"{RandomString(8)}.tmp";
				string path = string.Concat(GetTempPath(), fileName);
				try
				{
					File.Open(path, FileMode.CreateNew, FileAccess.Write).Dispose();
					return path;
				}
				catch (IOException) when (i < 100)
				{
					i++; // Don't let unforeseen circumstances cause us to loop forever
				}
			}
		}

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

			for (int i = path.Length - 1; i >= 0; i--)
			{
				char ch = path[i];
				if (ch == '.')
				{
					return i != path.Length - 1;
				}

				if (IsDirectorySeparator(ch))
				{
					break;
				}
			}

			return false;
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
			=> Join(path1.ToString(), path2.ToString());
#endif

#if FEATURE_PATH_JOIN
		/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
		public string Join(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3)
			=> Join(path1.ToString(), path2.ToString(), path3.ToString());
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
		public string Join(ReadOnlySpan<char> path1,
			ReadOnlySpan<char> path2,
			ReadOnlySpan<char> path3,
			ReadOnlySpan<char> path4)
			=> Join(path1.ToString(), path2.ToString(), path3.ToString(), path4.ToString());
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(string, string)" />
		public string Join(string? path1, string? path2)
		{
			if (string.IsNullOrEmpty(path1))
			{
				return path2 ?? string.Empty;
			}

			if (string.IsNullOrEmpty(path2))
			{
				return path1;
			}

			return Join([path1, path2]);
		}
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(string, string, string)" />
		public string Join(string? path1, string? path2, string? path3)
		{
			if (string.IsNullOrEmpty(path1))
			{
				return Join(path2, path3);
			}

			if (string.IsNullOrEmpty(path2))
			{
				return Join(path1, path3);
			}

			if (string.IsNullOrEmpty(path3))
			{
				return Join(path1, path2);
			}

			return Join([path1, path2, path3]);
		}
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(string, string, string, string)" />
		public string Join(string? path1, string? path2, string? path3, string? path4)
		{
			if (string.IsNullOrEmpty(path1))
			{
				return Join(path2, path3, path4);
			}

			if (string.IsNullOrEmpty(path2))
			{
				return Join(path1, path3, path4);
			}

			if (string.IsNullOrEmpty(path3))
			{
				return Join(path1, path2, path4);
			}

			if (string.IsNullOrEmpty(path4))
			{
				return Join(path1, path2, path3);
			}

			return Join([path1, path2, path3, path4]);
		}
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.Join(string[])" />
		public string Join(params string?[] paths)
		{
			if (paths == null)
			{
				throw new ArgumentNullException(nameof(paths));
			}

			if (paths.Length == 0)
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder();
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

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
		public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
			=> TrimEndingDirectorySeparator(path.ToString());
#endif

#if FEATURE_PATH_ADVANCED
		/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(string)" />
		public string TrimEndingDirectorySeparator(string path)
		{
			return EndsInDirectorySeparator(path) && path.Length != GetRootLength(path)
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
			charsWritten = 0;
			if (path1.Length == 0 && path2.Length == 0)
			{
				return true;
			}

			if (path1.Length == 0 || path2.Length == 0)
			{
				ref ReadOnlySpan<char> pathToUse = ref path1.Length == 0 ? ref path2 : ref path1;
				if (destination.Length < pathToUse.Length)
				{
					return false;
				}

				pathToUse.CopyTo(destination);
				charsWritten = pathToUse.Length;
				return true;
			}

			bool needsSeparator =
				!(path1.Length > 0 && IsDirectorySeparator(path1[path1.Length - 1]) || path2.Length > 0 && IsDirectorySeparator(path2[0]));
			int charsNeeded = path1.Length + path2.Length + (needsSeparator ? 1 : 0);
			if (destination.Length < charsNeeded)
			{
				return false;
			}

			path1.CopyTo(destination);
			if (needsSeparator)
			{
				destination[path1.Length] = DirectorySeparatorChar;
			}

			path2.CopyTo(destination.Slice(path1.Length + (needsSeparator ? 1 : 0)));

			charsWritten = charsNeeded;
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
			charsWritten = 0;
			if (path1.Length == 0 && path2.Length == 0 && path3.Length == 0)
			{
				return true;
			}

			if (path1.Length == 0)
			{
				return TryJoin(path2, path3, destination, out charsWritten);
			}

			if (path2.Length == 0)
			{
				return TryJoin(path1, path3, destination, out charsWritten);
			}

			if (path3.Length == 0)
			{
				return TryJoin(path1, path2, destination, out charsWritten);
			}
			int neededSeparators =
				path1.Length > 0 && IsDirectorySeparator(path1[path1.Length - 1]) || path2.Length > 0 && IsDirectorySeparator(path2[0]) ? 0 : 1;
			bool needsSecondSeparator =
				!(path2.Length > 0 && IsDirectorySeparator(path2[path2.Length - 1]) || path3.Length > 0 && IsDirectorySeparator(path3[0]));
			if (needsSecondSeparator)
			{
				neededSeparators++;
			}

			int charsNeeded = path1.Length + path2.Length + path3.Length + neededSeparators;
			if (destination.Length < charsNeeded)
			{
				return false;
			}

			bool result = TryJoin(path1, path2, destination, out charsWritten);
			Debug.Assert(result, "should never fail joining first two paths");

			if (needsSecondSeparator)
			{
				destination[charsWritten++] = DirectorySeparatorChar;
			}

			path3.CopyTo(destination.Slice(charsWritten));
			charsWritten += path3.Length;

			return true;
		}
#endif

		#endregion

		protected abstract int GetRootLength(string path);

		protected abstract bool IsDirectorySeparator(char c);

		protected abstract bool IsEffectivelyEmpty(string path);

		protected abstract bool IsPartiallyQualified(string path);

		[return: NotNullIfNotNull(nameof(path))]
		protected abstract string? NormalizeDirectorySeparators(string? path);


		/// <summary>
		/// Returns true if the two paths have the same root
		/// </summary>
		private bool AreRootsEqual(string first, string second, StringComparison comparisonType)
		{
			int firstRootLength = GetRootLength(first);
			int secondRootLength = GetRootLength(second);

			return firstRootLength == secondRootLength
			       && string.Compare(
				       strA: first,
				       indexA: 0,
				       strB: second,
				       indexB: 0,
				       length: firstRootLength,
				       comparisonType: comparisonType) == 0;
		}
		private string RandomString(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[fileSystem.RandomSystem.Random.Shared.Next(s.Length)]).ToArray());
		}
		/// <summary>
		/// Get the common path length from the start of the string.
		/// </summary>
		private int GetCommonPathLength(string first, string second, bool ignoreCase)
		{

			int commonChars = 0;
			for (; commonChars < first.Length; commonChars++)
			{
				if (second.Length < commonChars)
				{
					break;
				}
				if (first[commonChars] != second[commonChars] &&
				    (!ignoreCase || char.ToUpperInvariant(first[commonChars]) != char.ToUpperInvariant(second[commonChars])))
				{
					break;
				}
			}

			// If nothing matches
			if (commonChars == 0)
				return commonChars;

			// Or we're a full string and equal length or match to a separator
			if (commonChars == first.Length
			    && (commonChars == second.Length || IsDirectorySeparator(second[commonChars])))
				return commonChars;

			if (commonChars == second.Length && IsDirectorySeparator(first[commonChars]))
				return commonChars;

			// It's possible we matched somewhere in the middle of a segment e.g. C:\Foodie and C:\Foobar.
			while (commonChars > 0 && !IsDirectorySeparator(first[commonChars - 1]))
				commonChars--;

			return commonChars;
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
	}
}

using System;
using System.IO;
using System.Text;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

#if FEATURE_FILESYSTEM_NET7
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class PathMock : PathSystemBase
{
	private readonly MockFileSystem _fileSystem;

	internal PathMock(MockFileSystem fileSystem)
		: base(fileSystem)
	{
		_fileSystem = fileSystem;
	}

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IPath.Exists(string)" />
	public override bool Exists([NotNullWhen(true)] string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		return _fileSystem.Storage.GetContainer(_fileSystem.Storage.GetLocation(path))
			is not NullContainer;
	}
#endif

	/// <inheritdoc cref="IPath.GetFullPath(string)" />
	public override string GetFullPath(string path)
	{
		path.EnsureValidArgument(FileSystem, nameof(path));

		return Path.GetFullPath(Path.Combine(
			_fileSystem.Storage.CurrentDirectory,
			path));
	}

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.GetRelativePath(string, string)" />
	public override string GetRelativePath(string relativeTo, string path)
	{
		relativeTo.EnsureValidArgument(FileSystem, nameof(relativeTo));
		path.EnsureValidArgument(FileSystem, nameof(path));

		relativeTo = FileSystem.Path.GetFullPath(relativeTo);
		path = FileSystem.Path.GetFullPath(path);

		if (GetPathRoot(path) != GetPathRoot(relativeTo))
		{
			return path;
		}

		int commonLength = GetCommonPathLength(relativeTo, path);

		// If there is nothing in common they can't share the same root, return the "to" path as is.
		if (commonLength == 0)
		{
			return path;
		}

		// Trailing separators aren't significant for comparison
		int relativeToLength = relativeTo.Length;
		if (EndsInDirectorySeparator(relativeTo.AsSpan()))
			relativeToLength--;

		bool pathEndsInSeparator = EndsInDirectorySeparator(path.AsSpan());
		int pathLength = path.Length;
		if (pathEndsInSeparator)
			pathLength--;

		// If we have effectively the same path, return "."
		if (relativeToLength == pathLength && commonLength >= relativeToLength)
		{
			return ".";
		}


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

        // Add parent segments for segments past the common on the "from" path
        if (commonLength < relativeToLength)
        {
            sb.Append("..");

            for (int i = commonLength + 1; i < relativeToLength; i++)
            {
                if (EndsInDirectorySeparator(relativeTo.AsSpan(0, i)))
                {
                    sb.Append(DirectorySeparatorChar);
                    sb.Append("..");
                }
            }
        }
        else if (path[commonLength] == DirectorySeparatorChar)
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

            sb.Append(path.Substring(commonLength, differenceLength));
        }

        return sb.ToString();
	}

#if !FEATURE_PATH_ADVANCED
    private bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
    {
	    return path[path.Length - 1] == DirectorySeparatorChar;

    }
#endif

    /// <summary>
    ///     Get the common path length from the start of the string.
    /// </summary>
    private int GetCommonPathLength(string first, string second)
	{
		int commonChars = 0;
		var charsToCheck = first.IndexOf(DirectorySeparatorChar);
		if (charsToCheck <= 0)
		{
			return 0;
		}
		while (charsToCheck <= first.Length)
		{
			if (!first.Substring(0, charsToCheck).Equals(second.Substring(0, charsToCheck), InMemoryLocation.StringComparisonMode))
			{
				return commonChars;
			}

			commonChars = charsToCheck;
			int nextSeparator = first.Substring(commonChars).IndexOf(DirectorySeparatorChar);
			if (nextSeparator < 0)
			{
				if (charsToCheck >= first.Length)
				{
					return charsToCheck;
				}
				charsToCheck = first.Length;
			}
			else
			{
				charsToCheck += nextSeparator + 1;
            }
		}

		return 0;
	}
#endif
    }

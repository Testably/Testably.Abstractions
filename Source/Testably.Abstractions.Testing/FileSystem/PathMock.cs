using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;
#if FEATURE_FILESYSTEM_NET7
using Testably.Abstractions.Testing.Storage;
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

	/// <inheritdoc cref="IPath.ChangeExtension(string, string)" />
	[return: NotNullIfNotNull("path")]
	public override string? ChangeExtension(string? path, string? extension)
	{
		using IDisposable register = Register(nameof(ChangeExtension),
			path, extension);

		return base.ChangeExtension(path, extension);
	}

	/// <inheritdoc cref="IPath.Combine(string, string)" />
	public override string Combine(string path1, string path2)
	{
		using IDisposable register = Register(nameof(Combine),
			path1, path2);

		return base.Combine(path1, path2);
	}

	/// <inheritdoc cref="IPath.Combine(string, string, string)" />
	public override string Combine(string path1, string path2, string path3)
	{
		using IDisposable register = Register(nameof(Combine),
			path1, path2, path3);

		return base.Combine(path1, path2, path3);
	}

	/// <inheritdoc cref="IPath.Combine(string, string, string, string)" />
	public override string Combine(string path1, string path2, string path3, string path4)
	{
		using IDisposable register = Register(nameof(Combine),
			path1, path2, path3, path4);

		return base.Combine(path1, path2, path3, path4);
	}

	/// <inheritdoc cref="IPath.Combine(string[])" />
	public override string Combine(params string[] paths)
	{
		using IDisposable register = Register(nameof(Combine),
			paths);

		return base.Combine(paths);
	}

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.EndsInDirectorySeparator(ReadOnlySpan{char})" />
	public override bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(EndsInDirectorySeparator),
			path);

		return base.EndsInDirectorySeparator(path);
	}

	/// <inheritdoc cref="IPath.EndsInDirectorySeparator(string)" />
	public override bool EndsInDirectorySeparator(string path)
	{
		using IDisposable register = Register(nameof(EndsInDirectorySeparator),
			path);

		return base.EndsInDirectorySeparator(path);
	}
#endif

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IPath.Exists(string)" />
	public override bool Exists([NotNullWhen(true)] string? path)
	{
		using IDisposable register = Register(nameof(Exists),
			path);

		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		return _fileSystem.Storage.GetContainer(_fileSystem.Storage.GetLocation(path))
			is not NullContainer;
	}
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetDirectoryName(ReadOnlySpan{char})" />
	public override ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(GetDirectoryName),
			path);

		return base.GetDirectoryName(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetDirectoryName(string)" />
	public override string? GetDirectoryName(string? path)
	{
		using IDisposable register = Register(nameof(GetDirectoryName),
			path);

		return base.GetDirectoryName(path);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetExtension(ReadOnlySpan{char})" />
	public override ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(GetExtension),
			path);

		return base.GetExtension(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetExtension(string)" />
	[return: NotNullIfNotNull("path")]
	public override string? GetExtension(string? path)
	{
		using IDisposable register = Register(nameof(GetExtension),
			path);

		return base.GetExtension(path);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetFileName(ReadOnlySpan{char})" />
	public override ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(GetFileName),
			path);

		return base.GetFileName(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetFileName(string)" />
	[return: NotNullIfNotNull("path")]
	public override string? GetFileName(string? path)
	{
		using IDisposable register = Register(nameof(GetFileName),
			path);

		return base.GetFileName(path);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
	public override ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(GetFileNameWithoutExtension),
			path);

		return base.GetFileNameWithoutExtension(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetFileNameWithoutExtension(string)" />
	[return: NotNullIfNotNull("path")]
	public override string? GetFileNameWithoutExtension(string? path)
	{
		using IDisposable register = Register(nameof(GetFileNameWithoutExtension),
			path);

		return base.GetFileNameWithoutExtension(path);
	}

	/// <inheritdoc cref="IPath.GetFullPath(string)" />
	public override string GetFullPath(string path)
	{
		using IDisposable register = Register(nameof(GetFullPath),
			path);

		path.EnsureValidArgument(_fileSystem, nameof(path));

		string? pathRoot = Path.GetPathRoot(path);
		string? directoryRoot = Path.GetPathRoot(_fileSystem.Storage.CurrentDirectory);
		if (!string.IsNullOrEmpty(pathRoot) && !string.IsNullOrEmpty(directoryRoot))
		{
			if (char.ToUpperInvariant(pathRoot[0]) != char.ToUpperInvariant(directoryRoot[0]))
			{
				return Path.GetFullPath(path);
			}

			if (pathRoot.Length < directoryRoot.Length)
			{
				path = path.Substring(pathRoot.Length);
			}
		}

		return Path.GetFullPath(Path.Combine(
			_fileSystem.Storage.CurrentDirectory,
			path));
	}

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.GetFullPath(string, string)" />
	public override string GetFullPath(string path, string basePath)
	{
		using IDisposable register = Register(nameof(GetFullPath),
			path, basePath);

		return base.GetFullPath(path, basePath);
	}
#endif

	/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
	public override char[] GetInvalidFileNameChars()
	{
		using IDisposable register = Register(nameof(GetInvalidFileNameChars));

		return base.GetInvalidFileNameChars();
	}

	/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
	public override char[] GetInvalidPathChars()
	{
		using IDisposable register = Register(nameof(GetInvalidPathChars));

		return base.GetInvalidPathChars();
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetPathRoot(ReadOnlySpan{char})" />
	public override ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(GetPathRoot),
			path);

		return base.GetPathRoot(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetPathRoot(string?)" />
	public override string? GetPathRoot(string? path)
	{
		using IDisposable register = Register(nameof(GetPathRoot),
			path);

		return base.GetPathRoot(path);
	}

	/// <inheritdoc cref="IPath.GetRandomFileName()" />
	public override string GetRandomFileName()
	{
		using IDisposable register = Register(nameof(GetRandomFileName));

		return base.GetRandomFileName();
	}

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.GetRelativePath(string, string)" />
	public override string GetRelativePath(string relativeTo, string path)
	{
		using IDisposable register = Register(nameof(GetRelativePath),
			relativeTo, path);

		relativeTo.EnsureValidArgument(_fileSystem, nameof(relativeTo));
		path.EnsureValidArgument(_fileSystem, nameof(path));

		relativeTo = _fileSystem.Path.GetFullPath(relativeTo);
		path = _fileSystem.Path.GetFullPath(path);

		return Path.GetRelativePath(relativeTo, path);
	}
#endif

	/// <inheritdoc cref="IPath.GetTempFileName()" />
#if !NETSTANDARD2_0
	[Obsolete(
		"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
	public override string GetTempFileName()
	{
		using IDisposable register = Register(nameof(GetTempFileName));

		return base.GetTempFileName();
	}

	/// <inheritdoc cref="IPath.GetTempPath()" />
	public override string GetTempPath()
	{
		using IDisposable register = Register(nameof(GetTempPath));

		return base.GetTempPath();
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.HasExtension(ReadOnlySpan{char})" />
	public override bool HasExtension(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(HasExtension),
			path);

		return base.HasExtension(path);
	}
#endif

	/// <inheritdoc cref="IPath.HasExtension(string)" />
	public override bool HasExtension([NotNullWhen(true)] string? path)
	{
		using IDisposable register = Register(nameof(HasExtension),
			path);

		return base.HasExtension(path);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.IsPathFullyQualified(ReadOnlySpan{char})" />
	public override bool IsPathFullyQualified(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(IsPathFullyQualified),
			path);

		return base.IsPathFullyQualified(path);
	}
#endif

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.IsPathFullyQualified(string)" />
	public override bool IsPathFullyQualified(string path)
	{
		using IDisposable register = Register(nameof(IsPathFullyQualified),
			path);

		return base.IsPathFullyQualified(path);
	}
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.IsPathRooted(ReadOnlySpan{char})" />
	public override bool IsPathRooted(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(IsPathRooted),
			path);

		return base.IsPathRooted(path);
	}
#endif

	/// <inheritdoc cref="IPath.IsPathRooted(string)" />
	public override bool IsPathRooted(string? path)
	{
		using IDisposable register = Register(nameof(IsPathRooted),
			path);

		return base.IsPathRooted(path);
	}

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public override string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
	{
		using IDisposable register = Register(nameof(Join),
			path1, path2);

		return base.Join(path1, path2);
	}

	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public override string Join(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3)
	{
		using IDisposable register = Register(nameof(Join),
			path1,
			path2,
			path3);

		return base.Join(path1, path2, path3);
	}

	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public override string Join(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3,
		ReadOnlySpan<char> path4)
	{
		using IDisposable register = Register(nameof(Join),
			path1,
			path2,
			path3,
			path4);

		return base.Join(path1, path2, path3, path4);
	}

	/// <inheritdoc cref="IPath.Join(string, string)" />
	public override string Join(string? path1, string? path2)
	{
		using IDisposable register = Register(nameof(Join),
			path1, path2);

		return base.Join(path1, path2);
	}

	/// <inheritdoc cref="IPath.Join(string, string, string)" />
	public override string Join(string? path1, string? path2, string? path3)
	{
		using IDisposable register = Register(nameof(Join),
			path1, path2, path3);

		return base.Join(path1, path2, path3);
	}

	/// <inheritdoc cref="IPath.Join(string, string, string, string)" />
	public override string Join(string? path1, string? path2, string? path3, string? path4)
	{
		using IDisposable register = Register(nameof(Join),
			path1, path2, path3, path4);

		return base.Join(path1, path2, path3, path4);
	}

	/// <inheritdoc cref="IPath.Join(string[])" />
	public override string Join(params string?[] paths)
	{
		using IDisposable register = Register(nameof(Join),
			paths);

		return base.Join(paths);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
	public override ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
	{
		using IDisposable register = Register(nameof(TrimEndingDirectorySeparator),
			path);

		return base.TrimEndingDirectorySeparator(path);
	}

	/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(string)" />
	public override string TrimEndingDirectorySeparator(string path)
	{
		using IDisposable register = Register(nameof(TrimEndingDirectorySeparator),
			path);

		return base.TrimEndingDirectorySeparator(path);
	}
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="IPath.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	public override bool TryJoin(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		Span<char> destination,
		out int charsWritten)
	{
		int registerCharsWritten = 0;
		try
		{
			bool result = base.TryJoin(path1, path2, destination, out charsWritten);
			registerCharsWritten = charsWritten;
			return result;
		}
		finally
		{
			_fileSystem.StatisticsRegistration.Path.Register(nameof(TryJoin),
				ParameterDescription.FromParameter(path1),
				ParameterDescription.FromParameter(path2),
				ParameterDescription.FromParameter(destination),
				ParameterDescription.FromOutParameter(registerCharsWritten));
		}
	}

	/// <inheritdoc cref="IPath.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	public override bool TryJoin(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3,
		Span<char> destination,
		out int charsWritten)
	{
		int registerCharsWritten = 0;
		try
		{
			bool result = base.TryJoin(path1, path2, path3, destination, out charsWritten);
			registerCharsWritten = charsWritten;
			return result;
		}
		finally
		{
			_fileSystem.StatisticsRegistration.Path.Register(nameof(TryJoin),
				ParameterDescription.FromParameter(path1),
				ParameterDescription.FromParameter(path2),
				ParameterDescription.FromParameter(path3),
				ParameterDescription.FromParameter(destination),
				ParameterDescription.FromOutParameter(registerCharsWritten));
		}
	}
#endif

	private IDisposable Register(string name)
		=> _fileSystem.StatisticsRegistration.Path.Register(name);

	private IDisposable Register<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.Path.Register(name,
			ParameterDescription.FromParameter(parameter1));

#if FEATURE_SPAN
	private IDisposable Register<T1>(string name, ReadOnlySpan<T1> parameter1)
		=> _fileSystem.StatisticsRegistration.Path.Register(name,
			ParameterDescription.FromParameter(parameter1));
#endif

#if FEATURE_PATH_ADVANCED
	private IDisposable Register<T1, T2>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2)
		=> _fileSystem.StatisticsRegistration.Path.Register(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2));

	private IDisposable Register<T1, T2, T3>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3)
		=> _fileSystem.StatisticsRegistration.Path.Register(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3));

	private IDisposable Register<T1, T2, T3, T4>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3, ReadOnlySpan<T4> parameter4)
		=> _fileSystem.StatisticsRegistration.Path.Register(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3),
			ParameterDescription.FromParameter(parameter4));
#endif

	private IDisposable Register<T1, T2>(string name, T1 parameter1, T2 parameter2)
		=> _fileSystem.StatisticsRegistration.Path.Register(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2));

	private IDisposable Register<T1, T2, T3>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3)
		=> _fileSystem.StatisticsRegistration.Path.Register(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3));

	private IDisposable Register<T1, T2, T3, T4>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3, T4 parameter4)
		=> _fileSystem.StatisticsRegistration.Path.Register(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3),
			ParameterDescription.FromParameter(parameter4));
}

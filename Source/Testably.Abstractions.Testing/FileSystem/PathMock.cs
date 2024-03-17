using System;
using System.Diagnostics.CodeAnalysis;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class PathMock : IPath
{
	private readonly MockFileSystem _fileSystem;

	internal PathMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IPath Members

	/// <inheritdoc cref="IPath.AltDirectorySeparatorChar" />
	public char AltDirectorySeparatorChar
	{
		get
		{
			using IDisposable register = RegisterProperty(nameof(AltDirectorySeparatorChar));

			return _fileSystem.Execute.Path.AltDirectorySeparatorChar;
		}
	}

	/// <inheritdoc cref="IPath.DirectorySeparatorChar" />
	public char DirectorySeparatorChar
	{
		get
		{
			using IDisposable register = RegisterProperty(nameof(DirectorySeparatorChar));

			return _fileSystem.Execute.Path.DirectorySeparatorChar;
		}
	}

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IPath.PathSeparator" />
	public char PathSeparator
	{
		get
		{
			using IDisposable register = RegisterProperty(nameof(PathSeparator));

			return _fileSystem.Execute.Path.PathSeparator;
		}
	}

	/// <inheritdoc cref="IPath.VolumeSeparatorChar" />
	public char VolumeSeparatorChar
	{
		get
		{
			using IDisposable register = RegisterProperty(nameof(VolumeSeparatorChar));

			return _fileSystem.Execute.Path.VolumeSeparatorChar;
		}
	}

	/// <inheritdoc cref="IPath.ChangeExtension(string, string)" />
	[return: NotNullIfNotNull("path")]
	public string? ChangeExtension(string? path, string? extension)
	{
		using IDisposable register = RegisterMethod(nameof(ChangeExtension),
			path, extension);

		return _fileSystem.Execute.Path.ChangeExtension(path, extension);
	}

	/// <inheritdoc cref="IPath.Combine(string, string)" />
	public string Combine(string path1, string path2)
	{
		using IDisposable register = RegisterMethod(nameof(Combine),
			path1, path2);

		return _fileSystem.Execute.Path.Combine(path1, path2);
	}

	/// <inheritdoc cref="IPath.Combine(string, string, string)" />
	public string Combine(string path1, string path2, string path3)
	{
		using IDisposable register = RegisterMethod(nameof(Combine),
			path1, path2, path3);

		return _fileSystem.Execute.Path.Combine(path1, path2, path3);
	}

	/// <inheritdoc cref="IPath.Combine(string, string, string, string)" />
	public string Combine(string path1, string path2, string path3, string path4)
	{
		using IDisposable register = RegisterMethod(nameof(Combine),
			path1, path2, path3, path4);

		return _fileSystem.Execute.Path.Combine(path1, path2, path3, path4);
	}

	/// <inheritdoc cref="IPath.Combine(string[])" />
	public string Combine(params string[] paths)
	{
		using IDisposable register = RegisterMethod(nameof(Combine),
			paths);

		return _fileSystem.Execute.Path.Combine(paths);
	}

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.EndsInDirectorySeparator(ReadOnlySpan{char})" />
	public bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(EndsInDirectorySeparator),
			path);

		return _fileSystem.Execute.Path.EndsInDirectorySeparator(path);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.EndsInDirectorySeparator(string)" />
	public bool EndsInDirectorySeparator(string path)
	{
		using IDisposable register = RegisterMethod(nameof(EndsInDirectorySeparator),
			path);

		return _fileSystem.Execute.Path.EndsInDirectorySeparator(path);
	}
#endif

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IPath.Exists(string)" />
	public bool Exists([NotNullWhen(true)] string? path)
	{
		using IDisposable register = RegisterMethod(nameof(Exists),
			path);

		return _fileSystem.Execute.Path.Exists(path);
	}
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetDirectoryName(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(GetDirectoryName),
			path);

		return _fileSystem.Execute.Path.GetDirectoryName(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetDirectoryName(string)" />
	public string? GetDirectoryName(string? path)
	{
		using IDisposable register = RegisterMethod(nameof(GetDirectoryName),
			path);

		return _fileSystem.Execute.Path.GetDirectoryName(path);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetExtension(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(GetExtension),
			path);

		return _fileSystem.Execute.Path.GetExtension(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetExtension(string)" />
	[return: NotNullIfNotNull("path")]
	public string? GetExtension(string? path)
	{
		using IDisposable register = RegisterMethod(nameof(GetExtension),
			path);

		return _fileSystem.Execute.Path.GetExtension(path);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetFileName(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(GetFileName),
			path);

		return _fileSystem.Execute.Path.GetFileName(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetFileName(string)" />
	[return: NotNullIfNotNull("path")]
	public string? GetFileName(string? path)
	{
		using IDisposable register = RegisterMethod(nameof(GetFileName),
			path);

		return _fileSystem.Execute.Path.GetFileName(path);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(GetFileNameWithoutExtension),
			path);

		return _fileSystem.Execute.Path.GetFileNameWithoutExtension(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetFileNameWithoutExtension(string)" />
	[return: NotNullIfNotNull("path")]
	public string? GetFileNameWithoutExtension(string? path)
	{
		using IDisposable register = RegisterMethod(nameof(GetFileNameWithoutExtension),
			path);

		return _fileSystem.Execute.Path.GetFileNameWithoutExtension(path);
	}

	/// <inheritdoc cref="IPath.GetFullPath(string)" />
	public string GetFullPath(string path)
	{
		using IDisposable register = RegisterMethod(nameof(GetFullPath),
			path);

		return _fileSystem.Execute.Path.GetFullPath(path);
	}

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.GetFullPath(string, string)" />
	public string GetFullPath(string path, string basePath)
	{
		using IDisposable register = RegisterMethod(nameof(GetFullPath),
			path, basePath);

		return _fileSystem.Execute.Path.GetFullPath(path, basePath);
	}
#endif

	/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
	public char[] GetInvalidFileNameChars()
	{
		using IDisposable register = RegisterMethod(nameof(GetInvalidFileNameChars));

		return _fileSystem.Execute.Path.GetInvalidFileNameChars();
	}

	/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
	public char[] GetInvalidPathChars()
	{
		using IDisposable register = RegisterMethod(nameof(GetInvalidPathChars));

		return _fileSystem.Execute.Path.GetInvalidPathChars();
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.GetPathRoot(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(GetPathRoot),
			path);

		return _fileSystem.Execute.Path.GetPathRoot(path);
	}
#endif

	/// <inheritdoc cref="IPath.GetPathRoot(string?)" />
	public string? GetPathRoot(string? path)
	{
		using IDisposable register = RegisterMethod(nameof(GetPathRoot),
			path);

		return _fileSystem.Execute.Path.GetPathRoot(path);
	}

	/// <inheritdoc cref="IPath.GetRandomFileName()" />
	public string GetRandomFileName()
	{
		using IDisposable register = RegisterMethod(nameof(GetRandomFileName));

		return _fileSystem.Execute.Path.GetRandomFileName();
	}

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.GetRelativePath(string, string)" />
	public string GetRelativePath(string relativeTo, string path)
	{
		using IDisposable register = RegisterMethod(nameof(GetRelativePath),
			relativeTo, path);

		return _fileSystem.Execute.Path.GetRelativePath(relativeTo, path);
	}
#endif

	/// <inheritdoc cref="IPath.GetTempFileName()" />
#if !NETSTANDARD2_0
	[Obsolete(
		"Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
	public string GetTempFileName()
	{
		using IDisposable register = RegisterMethod(nameof(GetTempFileName));

		return _fileSystem.Execute.Path.GetTempFileName();
	}

	/// <inheritdoc cref="IPath.GetTempPath()" />
	public string GetTempPath()
	{
		using IDisposable register = RegisterMethod(nameof(GetTempPath));

		return _fileSystem.Execute.Path.GetTempPath();
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.HasExtension(ReadOnlySpan{char})" />
	public bool HasExtension(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(HasExtension),
			path);

		return _fileSystem.Execute.Path.HasExtension(path);
	}
#endif

	/// <inheritdoc cref="IPath.HasExtension(string)" />
	public bool HasExtension([NotNullWhen(true)] string? path)
	{
		using IDisposable register = RegisterMethod(nameof(HasExtension),
			path);

		return _fileSystem.Execute.Path.HasExtension(path);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.IsPathFullyQualified(ReadOnlySpan{char})" />
	public bool IsPathFullyQualified(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(IsPathFullyQualified),
			path);

		return _fileSystem.Execute.Path.IsPathFullyQualified(path);
	}
#endif

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.IsPathFullyQualified(string)" />
	public bool IsPathFullyQualified(string path)
	{
		using IDisposable register = RegisterMethod(nameof(IsPathFullyQualified),
			path);

		return _fileSystem.Execute.Path.IsPathFullyQualified(path);
	}
#endif

#if FEATURE_SPAN
	/// <inheritdoc cref="IPath.IsPathRooted(ReadOnlySpan{char})" />
	public bool IsPathRooted(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(IsPathRooted),
			path);

		return _fileSystem.Execute.Path.IsPathRooted(path);
	}
#endif

	/// <inheritdoc cref="IPath.IsPathRooted(string)" />
	public bool IsPathRooted(string? path)
	{
		using IDisposable register = RegisterMethod(nameof(IsPathRooted),
			path);

		return _fileSystem.Execute.Path.IsPathRooted(path);
	}

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
	{
		using IDisposable register = RegisterMethod(nameof(Join),
			path1, path2);

		return _fileSystem.Execute.Path.Join(path1, path2);
	}
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public string Join(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3)
	{
		using IDisposable register = RegisterMethod(nameof(Join),
			path1,
			path2,
			path3);

		return _fileSystem.Execute.Path.Join(path1, path2, path3);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public string Join(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		ReadOnlySpan<char> path3,
		ReadOnlySpan<char> path4)
	{
		using IDisposable register = RegisterMethod(nameof(Join),
			path1,
			path2,
			path3,
			path4);

		return _fileSystem.Execute.Path.Join(path1, path2, path3, path4);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(string, string)" />
	public string Join(string? path1, string? path2)
	{
		using IDisposable register = RegisterMethod(nameof(Join),
			path1, path2);

		return _fileSystem.Execute.Path.Join(path1, path2);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(string, string, string)" />
	public string Join(string? path1, string? path2, string? path3)
	{
		using IDisposable register = RegisterMethod(nameof(Join),
			path1, path2, path3);

		return _fileSystem.Execute.Path.Join(path1, path2, path3);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(string, string, string, string)" />
	public string Join(string? path1, string? path2, string? path3, string? path4)
	{
		using IDisposable register = RegisterMethod(nameof(Join),
			path1, path2, path3, path4);

		return _fileSystem.Execute.Path.Join(path1, path2, path3, path4);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.Join(string[])" />
	public string Join(params string?[] paths)
	{
		using IDisposable register = RegisterMethod(nameof(Join),
			paths);

		return _fileSystem.Execute.Path.Join(paths);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
	public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
	{
		using IDisposable register = RegisterMethod(nameof(TrimEndingDirectorySeparator),
			path);

		return _fileSystem.Execute.Path.TrimEndingDirectorySeparator(path);
	}
#endif

#if FEATURE_PATH_ADVANCED
	/// <inheritdoc cref="IPath.TrimEndingDirectorySeparator(string)" />
	public string TrimEndingDirectorySeparator(string path)
	{
		using IDisposable register = RegisterMethod(nameof(TrimEndingDirectorySeparator),
			path);

		return _fileSystem.Execute.Path.TrimEndingDirectorySeparator(path);
	}
#endif

#if FEATURE_PATH_JOIN
	/// <inheritdoc cref="IPath.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
	public bool TryJoin(ReadOnlySpan<char> path1,
		ReadOnlySpan<char> path2,
		Span<char> destination,
		out int charsWritten)
	{
		int registerCharsWritten = 0;
		try
		{
			bool result =
				_fileSystem.Execute.Path.TryJoin(path1, path2, destination, out charsWritten);
			registerCharsWritten = charsWritten;
			return result;
		}
		finally
		{
			_fileSystem.StatisticsRegistration.Path.RegisterMethod(nameof(TryJoin),
				ParameterDescription.FromParameter(path1),
				ParameterDescription.FromParameter(path2),
				ParameterDescription.FromParameter(destination),
				ParameterDescription.FromOutParameter(registerCharsWritten));
		}
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
		int registerCharsWritten = 0;
		try
		{
			bool result =
				_fileSystem.Execute.Path.TryJoin(path1, path2, path3, destination,
					out charsWritten);
			registerCharsWritten = charsWritten;
			return result;
		}
		finally
		{
			_fileSystem.StatisticsRegistration.Path.RegisterMethod(nameof(TryJoin),
				ParameterDescription.FromParameter(path1),
				ParameterDescription.FromParameter(path2),
				ParameterDescription.FromParameter(path3),
				ParameterDescription.FromParameter(destination),
				ParameterDescription.FromOutParameter(registerCharsWritten));
		}
	}
#endif

	#endregion

	private IDisposable RegisterMethod(string name)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name);

	private IDisposable RegisterMethod<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1));

#if FEATURE_SPAN
	private IDisposable RegisterMethod<T1>(string name, ReadOnlySpan<T1> parameter1)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1));
#endif

#if FEATURE_PATH_JOIN
	private IDisposable RegisterMethod<T1, T2>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2));
#endif

#if FEATURE_PATH_JOIN
	private IDisposable RegisterMethod<T1, T2, T3>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3));
#endif

#if FEATURE_PATH_ADVANCED
	private IDisposable RegisterMethod<T1, T2, T3, T4>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3, ReadOnlySpan<T4> parameter4)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3),
			ParameterDescription.FromParameter(parameter4));
#endif

	private IDisposable RegisterMethod<T1, T2>(string name, T1 parameter1, T2 parameter2)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2));

	private IDisposable RegisterMethod<T1, T2, T3>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3));

	private IDisposable RegisterMethod<T1, T2, T3, T4>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3, T4 parameter4)
		=> _fileSystem.StatisticsRegistration.Path.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3),
			ParameterDescription.FromParameter(parameter4));

	private IDisposable RegisterProperty(string name, PropertyAccess access = PropertyAccess.Get)
		=> _fileSystem.StatisticsRegistration.Path.RegisterProperty(name, access);
}

using System;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class InMemoryLocation : IStorageLocation
{
	private readonly MockFileSystem _fileSystem;

	private readonly string _key;

	private InMemoryLocation(
		MockFileSystem fileSystem,
		IStorageDrive? drive,
		string fullPath,
		string friendlyName)
	{
		_fileSystem = fileSystem;
		FullPath = fullPath
			.NormalizePath(_fileSystem)
			.TrimOnWindows(_fileSystem);
		_key = NormalizeKey(_fileSystem, FullPath);
		_fileSystem.Execute.OnNetFramework(()
			=> friendlyName = friendlyName.TrimOnWindows(_fileSystem));

		IsRooted = string.Equals(drive?.Name, fullPath, StringComparison.OrdinalIgnoreCase) ||
		           string.Equals(drive?.Name.Substring(1), fullPath,
			           StringComparison.OrdinalIgnoreCase);
		FriendlyName = friendlyName;
		Drive = drive;
	}

	#region IStorageLocation Members

	/// <inheritdoc cref="IStorageLocation.Drive" />
	public IStorageDrive? Drive { get; }

	/// <inheritdoc cref="IStorageLocation.FriendlyName" />
	public string FriendlyName { get; }

	/// <inheritdoc cref="IStorageLocation.FullPath" />
	public string FullPath { get; }

	/// <inheritdoc cref="IStorageLocation.IsRooted" />
	public bool IsRooted { get; }

	/// <inheritdoc cref="IEquatable{IStorageLocation}.Equals(IStorageLocation)" />
	public bool Equals(IStorageLocation? other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		if (other is InMemoryLocation location)
		{
			return _key.Equals(location._key, _fileSystem.Execute.StringComparisonMode);
		}

		return NormalizeKey(_fileSystem, FullPath)
			.Equals(NormalizeKey(_fileSystem, other.FullPath),
				_fileSystem.Execute.StringComparisonMode);
	}

	/// <inheritdoc cref="object.Equals(object?)" />
	public override bool Equals(object? obj)
		=> ReferenceEquals(this, obj) ||
		   (obj is IStorageLocation other && Equals(other));

#if NETSTANDARD2_0
	/// <inheritdoc cref="object.GetHashCode()" />
	public override int GetHashCode()
		#pragma warning disable MA0021 // Use StringComparer.GetHashCode
		=> _key.ToLowerInvariant().GetHashCode();
	#pragma warning restore MA0021 // Use StringComparer.GetHashCode
#else
	/// <inheritdoc cref="object.GetHashCode()" />
	public override int GetHashCode()
		=> _key.GetHashCode(_fileSystem.Execute.StringComparisonMode);
#endif

	/// <inheritdoc cref="IStorageLocation.GetParent()" />
	public IStorageLocation? GetParent()
	{
		string? parentPath = _fileSystem.Execute.Path.GetDirectoryName(FullPath);
		if (string.Equals(
			    _fileSystem.Execute.Path.GetPathRoot(FullPath),
			    FullPath,
			    _fileSystem.Execute.StringComparisonMode)
		    || parentPath == null)
		{
			return null;
		}

		return New(
			_fileSystem,
			Drive,
			parentPath,
			GetFriendlyNameParent(parentPath));
	}

	private string GetFriendlyNameParent(string parentPath)
		=> _fileSystem.Execute.OnNetFramework(
			() => _fileSystem.Execute.Path.GetFileName(parentPath),
			() => parentPath);

	#endregion

	/// <summary>
	///     Creates a new <see cref="IStorageLocation" /> on the specified <paramref name="drive" /> with the given
	///     <paramref name="path" />
	/// </summary>
	/// <param name="fileSystem">The file system.</param>
	/// <param name="drive">The drive on which the path is located.</param>
	/// <param name="path">The full path on the <paramref name="drive" />.</param>
	/// <param name="friendlyName">The friendly name is the provided name or the full path.</param>
	internal static IStorageLocation New(
		MockFileSystem fileSystem,
		IStorageDrive? drive,
		string path,
		string? friendlyName = null)
	{
		if (path == string.Empty)
		{
			throw ExceptionFactory.PathIsEmpty(nameof(path));
		}

		friendlyName ??= path;
		return new InMemoryLocation(fileSystem, drive, path, friendlyName);
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> FullPath;

	private static string NormalizeKey(MockFileSystem fileSystem, string fullPath)
	{
#if FEATURE_PATH_ADVANCED
		return fileSystem.Execute.Path.TrimEndingDirectorySeparator(fullPath);
#else
		return FileFeatureExtensionMethods.TrimEndingDirectorySeparator(
			fileSystem,
			fullPath,
			fileSystem.Execute.Path.DirectorySeparatorChar,
			fileSystem.Execute.Path.AltDirectorySeparatorChar);
#endif
	}
}

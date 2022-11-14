using System;
using System.Diagnostics;
using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class InMemoryLocation : IStorageLocation
{
	internal static readonly StringComparison StringComparisonMode =
		Execute.IsLinux
			? StringComparison.Ordinal
			: StringComparison.OrdinalIgnoreCase;

	private readonly string _key;

	private InMemoryLocation(IStorageDrive? drive,
		string fullPath,
		string friendlyName)
	{
		FullPath = fullPath
			.NormalizePath()
			.TrimOnWindows();
		_key = NormalizeKey(FullPath);
		Execute.OnNetFramework(()
			=> friendlyName = friendlyName.TrimOnWindows());

		IsRooted = drive?.Name == fullPath || drive?.Name.Substring(1) == fullPath;
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
			return _key.Equals(location._key, StringComparisonMode);
		}

		return NormalizeKey(FullPath)
			.Equals(NormalizeKey(other.FullPath), StringComparisonMode);
	}

	/// <inheritdoc cref="object.Equals(object?)" />
	public override bool Equals(object? obj)
		=> ReferenceEquals(this, obj) ||
		   (obj is IStorageLocation other && Equals(other));

#if NETSTANDARD2_0
	/// <inheritdoc cref="object.GetHashCode()" />
	public override int GetHashCode()
		=> _key.ToLowerInvariant().GetHashCode();
#else
	/// <inheritdoc cref="object.GetHashCode()" />
	public override int GetHashCode()
		=> _key.GetHashCode(StringComparisonMode);
#endif

	/// <inheritdoc cref="IStorageLocation.GetParent()" />
	public IStorageLocation? GetParent()
	{
		string? parentPath = Path.GetDirectoryName(FullPath);
		if (Path.GetPathRoot(FullPath) == FullPath)
		{
			return null;
		}

		Debug.Assert(parentPath != null,
			"When parentPath is null, FullPath must be null or a root path!");

		return New(Drive,
			parentPath,
			GetFriendlyNameParent(parentPath));
	}

	private static string GetFriendlyNameParent(string parentPath)
		=> Execute.OnNetFramework(
			() => Path.GetFileName(parentPath),
			() => parentPath);

	#endregion

	/// <summary>
	///     Creates a new <see cref="IStorageLocation" /> on the specified <paramref name="drive" /> with the given
	///     <paramref name="path" />
	/// </summary>
	/// <param name="drive">The drive on which the path is located.</param>
	/// <param name="path">The full path on the <paramref name="drive" />.</param>
	/// <param name="friendlyName">The friendly name is the provided name or the full path.</param>
	internal static IStorageLocation New(IStorageDrive? drive,
		string path,
		string? friendlyName = null)
	{
		if (path == string.Empty)
		{
			throw ExceptionFactory.PathIsEmpty(nameof(path));
		}

		friendlyName ??= path;
		return new InMemoryLocation(drive, path, friendlyName);
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> FullPath;

	private static string NormalizeKey(string fullPath)
	{
#if FEATURE_PATH_ADVANCED
		return Path.TrimEndingDirectorySeparator(fullPath);
#else
		return FileFeatureExtensionMethods.TrimEndingDirectorySeparator(
			fullPath,
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar);
#endif
	}
}

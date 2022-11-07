using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Describes the change in the <see cref="MockFileSystem" />.
/// </summary>
public class ChangeDescription
{
	/// <summary>
	///     The name of the file or directory that changed.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	///     The old name of the file or directory that changed on a <see cref="WatcherChangeTypes.Renamed" /> change.
	/// </summary>
	public string? OldName { get; }

	/// <summary>
	///     The property changes affected by the change.
	/// </summary>
	public NotifyFilters NotifyFilters { get; }

	/// <summary>
	///     The path of the file or directory that changed.
	/// </summary>
	public string Path { get; }

	/// <summary>
	///     The old path of the file or directory that changed on a <see cref="WatcherChangeTypes.Renamed" /> change.
	/// </summary>
	public string? OldPath { get; }

	/// <summary>
	///     Changes that might occur to a file or directory.
	/// </summary>
	public WatcherChangeTypes ChangeType { get; }

	/// <summary>
	///     The type of the file system entry where the change originated.
	/// </summary>
	public FileSystemTypes FileSystemType { get; }

	internal ChangeDescription(WatcherChangeTypes changeType,
	                           FileSystemTypes fileSystemType,
	                           NotifyFilters notifyFilters,
	                           IStorageLocation location,
	                           IStorageLocation? oldLocation)
	{
		Path = location.FullPath;
		Name = location.FriendlyName;
		OldPath = oldLocation?.FullPath;
		OldName = oldLocation?.FriendlyName;
		ChangeType = changeType;
		FileSystemType = fileSystemType;
		NotifyFilters = notifyFilters;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{ChangeType} ({FileSystemType}) {Path} [{NotifyFilters}]";
}
namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Describes a change in the <see cref="MockFileSystem" /> that was emitted by a specific
///     <see cref="IFileSystemWatcher" />. The <see cref="FileSystemWatcher" /> property allows
///     consumers to filter watcher-triggered notifications by the emitting watcher instance.
/// </summary>
public sealed class WatcherChangeDescription : ChangeDescription
{
	/// <summary>
	///     The <see cref="IFileSystemWatcher" /> that emitted this change notification.
	/// </summary>
	public IFileSystemWatcher FileSystemWatcher { get; }

	internal WatcherChangeDescription(ChangeDescription source, IFileSystemWatcher fileSystemWatcher)
		: base(source)
	{
		FileSystemWatcher = fileSystemWatcher;
	}
}

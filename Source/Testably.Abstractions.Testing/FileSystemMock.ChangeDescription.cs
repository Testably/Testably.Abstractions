using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	/// <summary>
	///     Describes the change in the <see cref="FileSystemMock" />.
	/// </summary>
	public class ChangeDescription
	{
		/// <summary>
		///     The name of the file or directory that changed.
		/// </summary>
		public string? Name { get; }

		/// <summary>
		///     The property changes affected by the change.
		/// </summary>
		public NotifyFilters NotifyFilters { get; }

		/// <summary>
		///     The path of the file or directory that changed.
		/// </summary>
		public string Path { get; }

		/// <summary>
		///     Changes that might occur to a file or directory.
		/// </summary>
		public WatcherChangeTypes ChangeType { get; }

		/// <summary>
		///     The type of the file system entry where the change originated.
		/// </summary>
		public FileSystemTypes FileSystemType { get; }

		internal ChangeDescription(IStorageLocation location,
		                           WatcherChangeTypes changeType,
		                           FileSystemTypes fileSystemType,
		                           NotifyFilters notifyFilters)
		{
			Path = location.FullPath;
			Name = location.FriendlyName;
			ChangeType = changeType;
			FileSystemType = fileSystemType;
			NotifyFilters = notifyFilters;
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{ChangeType} ({FileSystemType}) {Path} [{NotifyFilters}]";
	}
}
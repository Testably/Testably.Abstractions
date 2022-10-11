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
		///     The type of the change.
		/// </summary>
		public ChangeTypes Type { get; }

		internal ChangeDescription(IStorageLocation location,
		                           ChangeTypes type,
		                           NotifyFilters notifyFilters)
		{
			Path = location.FullPath;
			Name = location.FriendlyName;
			Type = type;
			NotifyFilters = notifyFilters;
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			return $"{Type} {Path} [{NotifyFilters}]";
		}
	}
}
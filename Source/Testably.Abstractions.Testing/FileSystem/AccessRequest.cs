using System.IO;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Describes the access to a file or directory that is checked by an <see cref="AccessControlStrategy" />.
/// </summary>
public sealed class AccessRequest
{
	/// <summary>
	///     The requested <see cref="FileAccess" /> of the file or directory.
	/// </summary>
	/// <remarks>
	///     Enumerating a directory requests <see cref="FileAccess.Read" />. As <see cref="FileAccess" /> is a flags enum,
	///     check write access with <c>Access.HasFlag(FileAccess.Write)</c> to also cover
	///     <see cref="FileAccess.ReadWrite" />.
	/// </remarks>
	public FileAccess Access { get; }

	/// <summary>
	///     Flag, indicating if the file or directory is deleted.
	/// </summary>
	/// <remarks>
	///     A delete also requests <see cref="FileAccess.Write" /> in <see cref="Access" />.
	/// </remarks>
	public bool IsDelete { get; }

	/// <summary>
	///     Initializes a new instance of <see cref="AccessRequest" />.
	/// </summary>
	public AccessRequest(FileAccess access, bool isDelete = false)
	{
		Access = access;
		IsDelete = isDelete;
	}
}

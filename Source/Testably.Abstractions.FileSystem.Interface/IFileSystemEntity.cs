using System;
using System.IO;
namespace Testably.Abstractions
{
/// <summary>
///     Interface to support implementing extension methods on top of nested <see cref="IFileSystem" /> interfaces.
/// </summary>
public interface IFileSystemEntity
{
	/// <summary>
	///     Exposes the underlying file system implementation.
	///     <para />
	///     This is useful for implementing extension methods.
	/// </summary>
	IFileSystem FileSystem { get; }
}
}

namespace System.IO.Abstractions
{
	/// <summary>
	///     Backwards-compatibility alias for <see cref="Testably.Abstractions.IFileSystemEntity" />.
	/// </summary>
	public interface IFileSystemEntity : Testably.Abstractions.IFileSystemEntity
	{
	}
}

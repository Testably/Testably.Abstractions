using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions
{
/// <summary>
///     A factory for the creation of wrappers for <see cref="DirectoryInfo" /> in a <see cref="IFileSystem" />.
/// </summary>
public interface IDirectoryInfoFactory : IFileSystemEntity
{
	/// <summary>
	///     Initializes a new instance of a wrapper for <see cref="DirectoryInfo" /> which implements
	///     <see cref="IDirectoryInfo" />.
	/// </summary>
	/// <param name="path">A string specifying the path on which to create the <see cref="IDirectoryInfo" />.</param>
	IDirectoryInfo New(string path);

	/// <summary>
	///     Wraps the <paramref name="directoryInfo" /> in a wrapper for <see cref="DirectoryInfo" /> which implements
	///     <see cref="IDirectoryInfo" />.
	/// </summary>
	[return: NotNullIfNotNull("directoryInfo")]
	IDirectoryInfo? Wrap(DirectoryInfo? directoryInfo);
}
}

namespace System.IO.Abstractions
{
	/// <summary>
	///     Backwards-compatibility alias for <see cref="Testably.Abstractions.IDirectoryInfoFactory" />.
	/// </summary>
	public interface IDirectoryInfoFactory : Testably.Abstractions.IDirectoryInfoFactory
	{
	}
}
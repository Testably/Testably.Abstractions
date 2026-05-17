using System;
using System.IO;
using System.Diagnostics;

namespace Testably.Abstractions
{
/// <summary>
///     A factory for the creation of wrappers for <see cref="FileVersionInfo" /> in a <see cref="IFileSystem" />.
/// </summary>
public interface IFileVersionInfoFactory : IFileSystemEntity
{
	/// <inheritdoc cref="FileVersionInfo.GetVersionInfo(string)" />
	IFileVersionInfo GetVersionInfo(string fileName);
}
}

namespace System.IO.Abstractions
{
	/// <summary>
	///     Backwards-compatibility alias for <see cref="Testably.Abstractions.IFileVersionInfoFactory" />.
	/// </summary>
	public interface IFileVersionInfoFactory : Testably.Abstractions.IFileVersionInfoFactory
	{
	}
}
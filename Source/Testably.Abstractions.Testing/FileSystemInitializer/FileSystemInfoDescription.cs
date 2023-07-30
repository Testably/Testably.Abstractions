using System.IO;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

/// <summary>
///     Abstract class for defining directories or files.
///     <para />
///     See <see cref="DirectoryDescription" /> or <see cref="FileDescription" /> for implementations.
/// </summary>
public abstract class FileSystemInfoDescription
{
	/// <summary>
	///     Gives access to the child at the given <paramref name="path" />.
	/// </summary>
	public abstract FileSystemInfoDescription this[string path]
	{
		get;
	}

	/// <summary>
	///     The name of the file or directory.
	/// </summary>
	public string Name { get; }

	/// <summary>
	///     Initializes a new instance of <see cref="FileSystemInfoDescription" /> with the given <paramref name="name" />.
	/// </summary>
	protected FileSystemInfoDescription(string name)
	{
		Name = name.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}
}

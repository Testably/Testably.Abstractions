using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

/// <summary>
///     The description of a directory for the <see cref="IFileSystem" />.
/// </summary>
public class DirectoryDescription : FileSystemInfoDescription
{
	/// <summary>
	///     The defined children of this directory.
	/// </summary>
	public FileSystemInfoDescription[] Children => GetChildren();

	/// <inheritdoc cref="FileSystemInfoDescription.this[string]" />
	public override FileSystemInfoDescription this[string path]
	{
		get
		{
			if (_children.TryGetValue(path, out FileSystemInfoDescription? value))
			{
				return value!;
			}

			throw new TestingException($"The child named '{path}' does not exist.!");
		}
	}

	private readonly Dictionary<string, FileSystemInfoDescription> _children;

	/// <summary>
	///     Describes an empty directory with the given <paramref name="name" />.
	/// </summary>
	public DirectoryDescription(string name)
		: base(name)
	{
		_children = new Dictionary<string, FileSystemInfoDescription>();
	}

	/// <summary>
	///     Describes a directory with the given <paramref name="name" /> and the <paramref name="children" />.
	/// </summary>
	public DirectoryDescription(string name, params FileSystemInfoDescription[] children)
		: base(name)
	{
		_children = children.ToDictionary(c => c.Name);
	}

	private FileSystemInfoDescription[] GetChildren()
		=> _children
			.OrderBy(x => x.Key)
			.Select(x => x.Value)
			.ToArray();
}

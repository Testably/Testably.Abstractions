using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

public class DirectoryDescription : FileSystemInfoDescription
{
	public FileSystemInfoDescription[] Children => GetChildren();

	/// <inheritdoc />
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

	public DirectoryDescription(string name)
		: base(name)
	{
		_children = new Dictionary<string, FileSystemInfoDescription>();
	}

	public DirectoryDescription(string name, params FileSystemInfoDescription[] children)
		: base(name)
	{
		_children = children.ToDictionary(c => c.Name);
	}

	private FileSystemInfoDescription[] GetChildren()
	{
		return _children
			.OrderBy(x => x.Key)
			.Select(x => x.Value)
			.ToArray();
	}
}

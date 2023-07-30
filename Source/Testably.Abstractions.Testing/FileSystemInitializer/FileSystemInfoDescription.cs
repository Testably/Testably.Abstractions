namespace Testably.Abstractions.Testing.FileSystemInitializer;

public abstract class FileSystemInfoDescription
{
	public abstract FileSystemInfoDescription this[string path]
	{
		get;
	}

	public string Name { get; }

	protected FileSystemInfoDescription(string name)
	{
		Name = name;
	}
}

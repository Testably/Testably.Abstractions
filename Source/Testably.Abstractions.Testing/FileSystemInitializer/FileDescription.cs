using System;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

public class FileDescription : FileSystemInfoDescription
{
	public byte[]? Bytes { get; }
	public string? Content { get; }
	public bool IsReadOnly { get; set; }

	/// <inheritdoc />
	public override FileSystemInfoDescription this[string path]
		=> throw new TestingException("Files cannot have children.");

	public FileDescription(string name, string? content = null)
		: base(name)
	{
		Content = content;
	}

	public FileDescription(string name, byte[] bytes)
		: base(name)
	{
		Bytes = bytes;
	}
}

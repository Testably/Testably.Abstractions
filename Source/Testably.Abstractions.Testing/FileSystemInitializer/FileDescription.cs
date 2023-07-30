namespace Testably.Abstractions.Testing.FileSystemInitializer;

/// <summary>
///     The description of a file for the <see cref="IFileSystem" />.
/// </summary>
public class FileDescription : FileSystemInfoDescription
{
	/// <summary>
	///     The bytes content.
	/// </summary>
	public byte[]? Bytes { get; }

	/// <summary>
	///     The string content.
	/// </summary>
	public string? Content { get; }

	/// <summary>
	///     Flag indicating, if the file is read-only.
	/// </summary>
	/// <remarks>This sets <seealso cref="IFileInfo.IsReadOnly" />.</remarks>
	public bool IsReadOnly { get; set; }

	/// <inheritdoc cref="FileSystemInfoDescription.this[string]" />
	public override FileSystemInfoDescription this[string path]
		=> throw new TestingException("Files cannot have children.");

	/// <summary>
	///     Describes a text file with the given <paramref name="name" /> and the string content set to
	///     <paramref name="content" />.
	/// </summary>
	public FileDescription(string name, string? content = null)
		: base(name)
	{
		Content = content;
	}

	/// <summary>
	///     Describes a binary file with the given <paramref name="name" /> and the content set to <paramref name="bytes" />.
	/// </summary>
	public FileDescription(string name, byte[] bytes)
		: base(name)
	{
		Bytes = bytes;
	}
}

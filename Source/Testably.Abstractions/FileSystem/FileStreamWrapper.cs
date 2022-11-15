using System.IO;

namespace Testably.Abstractions.FileSystem;

internal sealed class FileStreamWrapper : FileSystemStream
{
	public FileStreamWrapper(FileStream fileStream)
		: base(fileStream, fileStream.Name, fileStream.IsAsync)

	{
		Extensibility = new FileSystemExtensibility(fileStream);
	}

	/// <inheritdoc cref="FileSystemStream.Extensibility" />
	public override IFileSystemExtensibility Extensibility
	{
		get;
	}
}

using System.IO;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class FileStreamWrapper : FileSystemStream
	{
		public FileStreamWrapper(FileStream fileStream)
			: base(fileStream, fileStream.Name, fileStream.IsAsync)

		{
			ExtensionContainer = new FileSystemExtensionContainer(fileStream);
		}

		/// <inheritdoc cref="FileSystemStream.ExtensionContainer" />
		public override IFileSystem.IFileSystemExtensionContainer ExtensionContainer
		{
			get;
		}
	}
}
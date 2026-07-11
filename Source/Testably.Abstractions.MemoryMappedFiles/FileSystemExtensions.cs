namespace Testably.Abstractions;

/// <summary>
///     Extension property to support abstractions for
///     <see cref="System.IO.MemoryMappedFiles.MemoryMappedFile" />.
/// </summary>
public static class FileSystemExtensions
{
	#pragma warning disable CA1822 // False positive: an extension property cannot be static.
	/// <inheritdoc cref="FileSystemExtensions" />
	extension(IFileSystem fileSystem)
	{
		/// <summary>
		///     Factory for abstracting creation of
		///     <see cref="System.IO.MemoryMappedFiles.MemoryMappedFile" />.
		/// </summary>
		public IMemoryMappedFileFactory MemoryMappedFile
			=> new MemoryMappedFileFactory(fileSystem);
	}
	#pragma warning restore CA1822
}

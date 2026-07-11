namespace Testably.Abstractions;

/// <summary>
///     Extension property to support abstractions for
///     <see cref="System.IO.MemoryMappedFiles.MemoryMappedFile" />.
/// </summary>
public static class FileSystemExtensions
{
	extension(IFileSystem fileSystem)
	{
		/// <summary>
		///     Factory for abstracting creation of
		///     <see cref="System.IO.MemoryMappedFiles.MemoryMappedFile" />.
		/// </summary>
#pragma warning disable S2325 // False positive: an extension property cannot be static.
		public IMemoryMappedFileFactory MemoryMappedFile
			=> new MemoryMappedFileFactory(fileSystem);
#pragma warning restore S2325
	}
}

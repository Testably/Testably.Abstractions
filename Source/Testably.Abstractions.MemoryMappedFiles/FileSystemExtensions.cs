namespace Testably.Abstractions;

/// <summary>
///     Extension property to support abstractions for
///     <see cref="System.IO.MemoryMappedFiles.MemoryMappedFile" />.
/// </summary>
public static class FileSystemExtensions
{
	/// <inheritdoc cref="FileSystemExtensions" />
	extension(IFileSystem fileSystem)
	{
		/// <summary>
		///     Factory for abstracting creation of
		///     <see cref="System.IO.MemoryMappedFiles.MemoryMappedFile" />.
		/// </summary>
		#pragma warning disable CA1822, S2325, MA0041 // False positive: an extension property cannot be static.
		public IMemoryMappedFileFactory MemoryMappedFile
			=> new MemoryMappedFileFactory(fileSystem);
		#pragma warning restore CA1822, S2325, MA0041
	}
}

namespace Testably.Abstractions;

/// <summary>
///     Extension method to support abstractions for <see cref="System.IO.Compression.ZipFile" />
///     and <see cref="System.IO.Compression.ZipArchive" />.
/// </summary>
public static class FileSystemExtensions
{
	#pragma warning disable CA1822 // False positive: an extension property cannot be static
	/// <inheritdoc cref="FileSystemExtensions" />
	extension(IFileSystem fileSystem)
	{
		/// <summary>
		///     Factory for abstracting creation of <see cref="System.IO.Compression.ZipArchive" />.
		/// </summary>
		public IZipArchiveFactory ZipArchive
			=> new ZipArchiveFactory(fileSystem);

		/// <summary>
		///     Abstractions for <see cref="System.IO.Compression.ZipFile" />.
		/// </summary>
		public IZipFile ZipFile
			=> new ZipFileWrapper(fileSystem);
	}
	#pragma warning restore CA1822
}

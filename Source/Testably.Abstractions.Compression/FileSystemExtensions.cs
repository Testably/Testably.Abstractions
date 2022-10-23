namespace Testably.Abstractions;

/// <summary>
///     Extension method to support abstractions for <see cref="System.IO.Compression.ZipFile" />
///     and <see cref="System.IO.Compression.ZipArchive" />.
/// </summary>
public static class FileSystemExtensions
{
	/// <summary>
	///     Factory for abstracting creation of <see cref="System.IO.Compression.ZipArchive" />.
	/// </summary>
	public static IZipArchiveFactory ZipArchive(this IFileSystem fileSystem)
		=> new ZipArchiveFactory(fileSystem);

	/// <summary>
	///     Abstractions for <see cref="System.IO.Compression.ZipFile" />.
	/// </summary>
	public static IZipFile ZipFile(this IFileSystem fileSystem)
		=> new ZipFileWrapper(fileSystem);
}
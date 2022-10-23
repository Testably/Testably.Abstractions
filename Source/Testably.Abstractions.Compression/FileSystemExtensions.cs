namespace Testably.Abstractions;

/// <summary>
///     Extension method to support abstractions for <see cref="System.IO.Compression.ZipFile" />.
/// </summary>
public static class FileSystemExtensions
{
	/// <summary>
	///     Abstractions for <see cref="System.IO.Compression.ZipFile" />.
	/// </summary>
	public static IZipFile ZipFile(this IFileSystem fileSystem)
	{
		return new ZipFileWrapper(fileSystem);
	}
}
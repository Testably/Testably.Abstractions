using Testably.Abstractions.Internal;

namespace Testably.Abstractions;

public static class FileSystemExtensions
{
	public static IZipFile ZipFile(this IFileSystem fileSystem)
	{
		return new ZipFileWrapper(fileSystem);
	}
}
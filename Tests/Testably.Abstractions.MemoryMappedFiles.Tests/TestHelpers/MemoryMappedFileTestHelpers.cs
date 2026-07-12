namespace Testably.Abstractions.MemoryMappedFiles.Tests.TestHelpers;

public static class MemoryMappedFileTestHelpers
{
	/// <summary>
	///     Creates a file with <paramref name="size" /> zero-bytes at <paramref name="path" /> and
	///     returns a memory-mapped file over it.
	/// </summary>
	public static IMemoryMappedFile CreateMappedFile(this IFileSystem fileSystem,
		int size = 100, string path = "data.bin")
	{
		fileSystem.File.WriteAllBytes(path, new byte[size]);
		return fileSystem.MemoryMappedFile.CreateFromFile(path);
	}
}

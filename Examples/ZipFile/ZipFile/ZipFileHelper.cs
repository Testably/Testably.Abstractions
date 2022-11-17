using System.IO;
using System.IO.Compression;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Examples.ZipFile;

public sealed class ZipFileHelper
{
	private readonly IFileSystem _fileSystem;

	public ZipFileHelper(IFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	/// <summary>
	///     Create a zip archive from all files and subdirectories in <paramref name="directory" />.
	/// </summary>
	/// <param name="directory">The directory which should be packed in a zip archive.</param>
	/// <returns>A stream containing the zip archive.</returns>
	public Stream CreateZipFromDirectory(string directory)
	{
		MemoryStream memoryStream = new();
		using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
		{
			AddDirectoryToArchive("", directory, archive);
		}

		memoryStream.Flush();
		memoryStream.Seek(0, SeekOrigin.Begin);
		return memoryStream;
	}

	/// <summary>
	///     Extracts the zip archive from <paramref name="stream" /> to the <paramref name="directory" />.
	/// </summary>
	/// <param name="stream">
	///     The stream containing the zip archive.<br />
	///     E.g. with <see cref="IFile.OpenRead(string)" /> from the file system.
	/// </param>
	/// <param name="directory">
	///     The destination directory in which to extract the <see cref="ZipArchiveEntry" />s from the zip
	///     archive.
	/// </param>
	public void ExtractZipToDirectory(Stream stream, string directory)
	{
		using ZipArchive archive = new(stream, ZipArchiveMode.Read);
		foreach (ZipArchiveEntry entry in archive.Entries)
		{
			string filePath = _fileSystem.Path.Combine(directory, entry.FullName);
			string? directoryPath = _fileSystem.Path.GetDirectoryName(filePath);
			if (directoryPath != null &&
			    !_fileSystem.Directory.Exists(directoryPath))
			{
				_fileSystem.Directory.CreateDirectory(directoryPath);
			}

			using MemoryStream ms = new();
			entry.Open().CopyTo(ms);
			_fileSystem.File.WriteAllBytes(filePath, ms.ToArray());
		}
	}

	/// <summary>
	///     Recursively adds all files and subdirectories from the <paramref name="directory" /> into the
	///     <paramref name="archive" /> using <paramref name="directoryBase" /> as directory prefix in the zip archive.
	/// </summary>
	private void AddDirectoryToArchive(
		string directoryBase,
		string directory,
		ZipArchive archive)
	{
		foreach (string file in _fileSystem.Directory.GetFiles(directory))
		{
			ZipArchiveEntry entry =
				archive.CreateEntry(directoryBase + Path.GetFileName(file));
			using Stream stream = entry.Open();
			byte[] bytes = _fileSystem.File.ReadAllBytes(file);
			stream.Write(bytes, 0, bytes.Length);
		}

		foreach (string subDirectory in _fileSystem.Directory.GetDirectories(directory))
		{
			string subDirectoryName = _fileSystem.Path.GetFileName(subDirectory);
			if (subDirectoryName == "")
			{
				continue;
			}

			archive.CreateEntry(directoryBase + subDirectoryName + "/");
			AddDirectoryToArchive(directoryBase + subDirectoryName + "/",
				subDirectory,
				archive);
		}
	}
}

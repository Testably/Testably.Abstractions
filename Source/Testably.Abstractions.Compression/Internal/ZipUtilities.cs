using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Internal;

internal static class ZipUtilities
{
	/// <summary>
	///     Create a <c>ZipArchive</c> from the files and directories in <paramref name="sourceDirectoryName" />.
	/// </summary>
	/// <remarks>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.10/src/libraries/System.IO.Compression.ZipFile/src/System/IO/Compression/ZipFile.Create.cs#L354" />
	/// </remarks>
	internal static void CreateFromDirectory(IFileSystem fileSystem,
	                                         string sourceDirectoryName,
	                                         string destinationArchiveFileName,
	                                         CompressionLevel? compressionLevel,
	                                         bool includeBaseDirectory,
	                                         Encoding? entryNameEncoding)

	{
		sourceDirectoryName = fileSystem.Path.GetFullPath(sourceDirectoryName);
		destinationArchiveFileName =
			fileSystem.Path.GetFullPath(destinationArchiveFileName);

		using (ZipArchive archive = Open(fileSystem, destinationArchiveFileName,
			ZipArchiveMode.Create, entryNameEncoding))
		{
			bool directoryIsEmpty = true;

			IFileSystem.IDirectoryInfo di =
				fileSystem.DirectoryInfo.New(sourceDirectoryName);

			string basePath = di.FullName;

			if (includeBaseDirectory && di.Parent != null)
			{
				basePath = di.Parent.FullName;
			}

			foreach (IFileSystem.IFileSystemInfo file in di
			   .EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
			{
				directoryIsEmpty = false;

				int entryNameLength = file.FullName.Length - basePath.Length;
				Debug.Assert(entryNameLength > 0);

				if (file is IFileSystem.IFileInfo fileInfo)
				{
					string entryName = file.FullName
					   .Substring(basePath.Length + 1)
					   .Replace("\\", "/");
					ZipArchiveEntry entry = compressionLevel.HasValue
						? archive.CreateEntry(entryName, compressionLevel.Value)
						: archive.CreateEntry(entryName);
					using Stream stream = entry.Open();
					fileInfo.OpenRead().CopyTo(stream);
				}
				else if (file is IFileSystem.IDirectoryInfo directoryInfo &&
				         directoryInfo.GetFileSystemInfos().Length == 0)
				{
					string entryName = file.FullName.Substring(basePath.Length);
					archive.CreateEntry(entryName);
				}
			}

			if (includeBaseDirectory && directoryIsEmpty)
			{
				string entryName = di.Name;
				archive.CreateEntry(entryName);
			}
		}
	}

	/// <summary>
	///     Extract the archive at <paramref name="sourceArchiveFileName" /> to the
	///     <paramref name="destinationDirectoryName" />.
	/// </summary>
	internal static void ExtractToDirectory(IFileSystem fileSystem,
	                                        string sourceArchiveFileName,
	                                        string destinationDirectoryName,
	                                        Encoding? entryNameEncoding,
	                                        bool overwriteFiles)
	{
		if (sourceArchiveFileName == null)
		{
			throw new ArgumentNullException(nameof(sourceArchiveFileName));
		}

		string destinationPath =
			fileSystem.Path.GetFullPath(destinationDirectoryName);
		using (ZipArchive archive = Open(fileSystem, sourceArchiveFileName,
			ZipArchiveMode.Read,
			entryNameEncoding))
		{
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				string filePath =
					fileSystem.Path.Combine(destinationPath,
						entry.FullName.TrimStart(
							fileSystem.Path.DirectorySeparatorChar,
							fileSystem.Path.AltDirectorySeparatorChar));
				string? directoryPath = fileSystem.Path.GetDirectoryName(filePath);
				if (directoryPath != null &&
				    !fileSystem.Directory.Exists(directoryPath))
				{
					fileSystem.Directory.CreateDirectory(directoryPath);
				}

				if (fileSystem.File.Exists(filePath) && !overwriteFiles)
				{
					throw new IOException($"The file '{filePath}' already exists.");
				}

				using MemoryStream ms = new();
				entry.Open().CopyTo(ms);
				fileSystem.File.WriteAllBytes(filePath, ms.ToArray());
			}
		}
	}

	/// <summary>
	///     Opens a <c>ZipArchive</c> on the specified <paramref name="archiveFileName" /> in the specified
	///     <paramref name="mode" />.
	/// </summary>
	/// <remarks>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.10/src/libraries/System.IO.Compression.ZipFile/src/System/IO/Compression/ZipFile.Create.cs#L146" />
	/// </remarks>
	internal static ZipArchive Open(IFileSystem fileSystem,
	                                string archiveFileName,
	                                ZipArchiveMode mode,
	                                Encoding? entryNameEncoding)
	{
		FileMode fileMode;
		FileAccess access;
		FileShare fileShare;

		switch (mode)
		{
			case ZipArchiveMode.Read:
				fileMode = FileMode.Open;
				access = FileAccess.Read;
				fileShare = FileShare.Read;
				break;

			case ZipArchiveMode.Create:
				fileMode = FileMode.CreateNew;
				access = FileAccess.Write;
				fileShare = FileShare.None;
				break;

			case ZipArchiveMode.Update:
				fileMode = FileMode.OpenOrCreate;
				access = FileAccess.ReadWrite;
				fileShare = FileShare.None;
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(mode));
		}

		FileSystemStream fs = fileSystem.FileStream.New(archiveFileName, fileMode,
			access, fileShare, bufferSize: 0x1000, useAsync: false);

		try
		{
			return new ZipArchive(fs, mode, leaveOpen: false,
				entryNameEncoding: entryNameEncoding);
		}
		catch
		{
			fs.Dispose();
			throw;
		}
	}
}
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Internal;

internal static class ZipUtilities
{
	private const string SearchPattern = "*";
	private static readonly DateTime FallbackTime = new(1980, 1, 1, 0, 0, 0);

	internal static IZipArchiveEntry CreateEntryFromFile(
		IZipArchive destination,
		string sourceFileName,
		string entryName,
		CompressionLevel? compressionLevel = null
#if FEATURE_COMPRESSION_ENCRYPTION
		, ReadOnlyMemory<char> password = default,
		ZipEncryptionMethod encryption = ZipEncryptionMethod.None
#endif
	)
	{
		if (sourceFileName == null)
		{
			throw new ArgumentNullException(nameof(sourceFileName));
		}

		using (FileSystemStream fs = destination.FileSystem.FileStream.New(sourceFileName,
			FileMode.Open, FileAccess.Read, FileShare.Read))
		{
#if FEATURE_COMPRESSION_ENCRYPTION
			if (encryption != ZipEncryptionMethod.None && password.IsEmpty)
			{
				throw EmptyPassword(nameof(password));
			}

			IZipArchiveEntry entry;
			if (encryption != ZipEncryptionMethod.None)
			{
				entry = compressionLevel.HasValue
					? destination.CreateEntry(entryName, compressionLevel.Value, password.Span,
						encryption)
					: destination.CreateEntry(entryName, password.Span, encryption);
			}
			else
			{
				entry = compressionLevel.HasValue
					? destination.CreateEntry(entryName, compressionLevel.Value)
					: destination.CreateEntry(entryName);
			}
#else
			IZipArchiveEntry entry = compressionLevel.HasValue
				? destination.CreateEntry(entryName, compressionLevel.Value)
				: destination.CreateEntry(entryName);
#endif

			DateTime lastWrite =
				destination.FileSystem.File.GetLastWriteTime(sourceFileName);

			if (lastWrite.Year is < 1980 or > 2107)
			{
				lastWrite = FallbackTime;
			}

			entry.LastWriteTime = new DateTimeOffset(lastWrite);

			using (Stream es = entry.Open())
			{
				fs.CopyTo(es);
			}

			return entry;
		}
	}

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
		CompressionLevel? compressionLevel = null,
		bool includeBaseDirectory = false,
		Encoding? entryNameEncoding = null)
	{
		sourceDirectoryName = fileSystem.Path.GetFullPath(sourceDirectoryName);
		destinationArchiveFileName =
			fileSystem.Path.GetFullPath(destinationArchiveFileName);

		using ZipArchive archive = Open(fileSystem, destinationArchiveFileName,
			ZipArchiveMode.Create, entryNameEncoding);
		AddDirectoryEntries(fileSystem, archive, sourceDirectoryName, compressionLevel,
			includeBaseDirectory);
	}

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="CreateFromDirectory(IFileSystem, string, string, CompressionLevel?, bool, Encoding?)" />
	internal static void CreateFromDirectory(IFileSystem fileSystem,
		string sourceDirectoryName,
		string destinationArchiveFileName,
		ZipFileCreationOptions options)
	{
		sourceDirectoryName = fileSystem.Path.GetFullPath(sourceDirectoryName);
		destinationArchiveFileName =
			fileSystem.Path.GetFullPath(destinationArchiveFileName);

		using ZipArchive archive = Open(fileSystem, destinationArchiveFileName,
			ZipArchiveMode.Create, options.EntryNameEncoding);
		AddDirectoryEntries(fileSystem, archive, sourceDirectoryName, options.CompressionLevel,
			options.IncludeBaseDirectory, options.Password, options.EncryptionMethod);
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <summary>
	///     Create a <c>ZipArchive</c> from the files and directories in <paramref name="sourceDirectoryName" />.
	/// </summary>
	/// <remarks>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.10/src/libraries/System.IO.Compression.ZipFile/src/System/IO/Compression/ZipFile.Create.cs#L354" />
	/// </remarks>
	internal static void CreateFromDirectory(
		IFileSystem fileSystem,
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel? compressionLevel = null,
		bool includeBaseDirectory = false,
		Encoding? entryNameEncoding = null)
	{
		ThrowIfUnwritable(destination);
		sourceDirectoryName = fileSystem.Path.GetFullPath(sourceDirectoryName);

		using ZipArchive archive = new(destination, ZipArchiveMode.Create,
			leaveOpen: true,
			entryNameEncoding: entryNameEncoding);
		AddDirectoryEntries(fileSystem, archive, sourceDirectoryName, compressionLevel,
			includeBaseDirectory);
	}
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="CreateFromDirectory(IFileSystem, string, Stream, CompressionLevel?, bool, Encoding?)" />
	internal static void CreateFromDirectory(
		IFileSystem fileSystem,
		string sourceDirectoryName,
		Stream destination,
		ZipFileCreationOptions options)
	{
		ThrowIfUnwritable(destination);
		sourceDirectoryName = fileSystem.Path.GetFullPath(sourceDirectoryName);

		using ZipArchive archive = new(destination, ZipArchiveMode.Create,
			leaveOpen: true,
			entryNameEncoding: options.EntryNameEncoding);
		AddDirectoryEntries(fileSystem, archive, sourceDirectoryName, options.CompressionLevel,
			options.IncludeBaseDirectory, options.Password, options.EncryptionMethod);
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	private static void ThrowIfUnwritable(Stream destination)
	{
		ArgumentNullException.ThrowIfNull(destination);
		if (!destination.CanWrite)
		{
			throw new ArgumentException("The stream is unwritable.", nameof(destination));
		}
	}
#endif

	private static void AddDirectoryEntries(IFileSystem fileSystem,
		ZipArchive archive,
		string sourceDirectoryName,
		CompressionLevel? compressionLevel,
		bool includeBaseDirectory
#if FEATURE_COMPRESSION_ENCRYPTION
		, ReadOnlyMemory<char> password = default,
		ZipEncryptionMethod encryption = ZipEncryptionMethod.None
#endif
	)
	{
		bool directoryIsEmpty = true;

		IDirectoryInfo di =
			fileSystem.DirectoryInfo.New(sourceDirectoryName);

		string basePath = di.FullName;

		if (includeBaseDirectory && di.Parent != null)
		{
			basePath = di.Parent.FullName;
		}

		foreach (IFileSystemInfo file in di
			.EnumerateFileSystemInfos(SearchPattern, SearchOption.AllDirectories))
		{
			directoryIsEmpty = false;

			if (file is IFileInfo fileInfo)
			{
				#pragma warning disable MA0074
				string entryName = file.FullName
					.Substring(basePath.Length + 1)
					.Replace('\\', '/');
				#pragma warning restore MA0074
				ZipArchiveEntry entry = CreateFileEntry(archive, entryName, compressionLevel
#if FEATURE_COMPRESSION_ENCRYPTION
					, password, encryption
#endif
				);
				using Stream stream = entry.Open();
				using FileSystemStream fileStream = fileInfo.OpenRead();
				fileStream.CopyTo(stream);
			}
			else if (file is IDirectoryInfo directoryInfo &&
			         directoryInfo.GetFileSystemInfos().Length == 0)
			{
				#pragma warning disable CA1845
				string entryName = file.FullName.Substring(basePath.Length + 1) + "/";
				#pragma warning restore CA1845
				archive.CreateEntry(entryName);
			}
		}

		if (includeBaseDirectory && directoryIsEmpty)
		{
			string entryName = di.Name + "/";
			archive.CreateEntry(entryName);
		}
	}

	/// <summary>
	///     Creates the entry for a file in <c>CreateFromDirectory</c>, which, unlike the entries for directories, is
	///     encrypted when an encryption method is given.
	/// </summary>
	private static ZipArchiveEntry CreateFileEntry(ZipArchive archive,
		string entryName,
		CompressionLevel? compressionLevel
#if FEATURE_COMPRESSION_ENCRYPTION
		, ReadOnlyMemory<char> password,
		ZipEncryptionMethod encryption
#endif
	)
	{
#if FEATURE_COMPRESSION_ENCRYPTION
		if (encryption != ZipEncryptionMethod.None)
		{
			return compressionLevel.HasValue
				? archive.CreateEntry(entryName, compressionLevel.Value, password.Span, encryption)
				: archive.CreateEntry(entryName, password.Span, encryption);
		}
#endif

		return compressionLevel.HasValue
			? archive.CreateEntry(entryName, compressionLevel.Value)
			: archive.CreateEntry(entryName);
	}

#if FEATURE_COMPRESSION_ENCRYPTION
	internal static ArgumentException EmptyPassword(string paramName)
		=> new("The password cannot be empty.", paramName);
#endif

	internal static void ExtractRelativeToDirectory(this IZipArchiveEntry source,
		string destinationDirectoryName,
		bool overwrite
#if FEATURE_COMPRESSION_ENCRYPTION
		, ReadOnlyMemory<char> password = default
#endif
	)
	{
		string fileDestinationPath =
			source.FileSystem.Path.Combine(destinationDirectoryName,
				source.FullName.TrimStart(
					source.FileSystem.Path.DirectorySeparatorChar,
					source.FileSystem.Path.AltDirectorySeparatorChar));

		if (source.FullName.EndsWith('/', StringComparison.Ordinal))
		{
			if (source.Length != 0)
			{
				throw new IOException(
					"Zip entry name ends in directory separator character but contains data.");
			}

			source.FileSystem.Directory.CreateDirectory(fileDestinationPath);
		}
		else
		{
			source.FileSystem.Directory.CreateDirectory(
				source.FileSystem.Path.GetDirectoryName(fileDestinationPath) ?? ".");
#if FEATURE_COMPRESSION_ENCRYPTION
			ExtractToFile(source, fileDestinationPath, overwrite,
				password.IsEmpty ? (ReadOnlyMemory<char>?)null : password);
#else
			ExtractToFile(source, fileDestinationPath, overwrite);
#endif
		}
	}

	/// <summary>
	///     Extract the archive at <paramref name="sourceArchiveFileName" /> to the
	///     <paramref name="destinationDirectoryName" />.
	/// </summary>
	internal static void ExtractToDirectory(IFileSystem fileSystem,
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding = null,
		bool overwriteFiles = false
#if FEATURE_COMPRESSION_ENCRYPTION
		, ReadOnlyMemory<char> password = default
#endif
	)
	{
		if (sourceArchiveFileName == null)
		{
			throw new ArgumentNullException(nameof(sourceArchiveFileName));
		}

		using (ZipArchive archive = Open(fileSystem, sourceArchiveFileName,
			ZipArchiveMode.Read,
			entryNameEncoding))
		{
			ZipArchiveWrapper wrappedArchive = new(fileSystem, archive);
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				IZipArchiveEntry wrappedEntry = ZipArchiveEntryWrapper.New(
					fileSystem, wrappedArchive, entry);
				ExtractRelativeToDirectory(wrappedEntry, destinationDirectoryName,
					overwriteFiles
#if FEATURE_COMPRESSION_ENCRYPTION
					, password
#endif
				);
			}
		}
	}

#if FEATURE_COMPRESSION_STREAM
	/// <summary>
	///     Extract the archive at <paramref name="source" /> to the
	///     <paramref name="destinationDirectoryName" />.
	/// </summary>
	internal static void ExtractToDirectory(IFileSystem fileSystem,
		Stream source,
		string destinationDirectoryName,
		Encoding? entryNameEncoding = null,
		bool overwriteFiles = false
#if FEATURE_COMPRESSION_ENCRYPTION
		, ReadOnlyMemory<char> password = default
#endif
	)
	{
		ArgumentNullException.ThrowIfNull(source);
		if (!source.CanRead)
		{
			throw new ArgumentException("The stream is unreadable.", nameof(source));
		}

		using (ZipArchive archive = new(source, ZipArchiveMode.Read,
			leaveOpen: true, entryNameEncoding))
		{
			ZipArchiveWrapper wrappedArchive = new(fileSystem, archive);
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				IZipArchiveEntry wrappedEntry = ZipArchiveEntryWrapper.New(
					fileSystem, wrappedArchive, entry);
				ExtractRelativeToDirectory(wrappedEntry, destinationDirectoryName,
					overwriteFiles
#if FEATURE_COMPRESSION_ENCRYPTION
					, password
#endif
				);
			}
		}
	}
#endif

	/// <summary>
	///     Extracts the <paramref name="source" /> entry to the <paramref name="destinationFileName" />.
	/// </summary>
	/// <remarks>
	///     When a <c>password</c> is given, even an empty one, the entry is opened with it, like the overloads with
	///     <c>ZipExtractionOptions</c> do.
	/// </remarks>
	internal static void ExtractToFile(IZipArchiveEntry source,
		string destinationFileName, bool overwrite
#if FEATURE_COMPRESSION_ENCRYPTION
		, ReadOnlyMemory<char>? password = null
#endif
	)
	{
		FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;

		using (FileSystemStream fileStream = source.FileSystem.FileStream
			.New(destinationFileName, mode, FileAccess.Write, FileShare.None))
		{
#if FEATURE_COMPRESSION_ENCRYPTION
			using (Stream entryStream = password.HasValue
				? source.Open(password.Value.Span)
				: source.Open())
#else
			using (Stream entryStream = source.Open())
#endif
			{
				entryStream.CopyTo(fileStream);
			}
		}

		try
		{
			source.FileSystem.File.SetLastWriteTime(destinationFileName,
				source.LastWriteTime.DateTime);
		}
		catch
		{
			// some OSes might not support setting the last write time
			// the extraction should not fail because of that
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
		Encoding? entryNameEncoding = null)
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
			access, fileShare, bufferSize: 0x1000);

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

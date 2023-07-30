namespace Testably.Abstractions.Testing.FileSystemVerifier;

internal class FileSystemVerifier : IFileSystemVerifier
{
	private readonly IFileSystem _fileSystem;
	private readonly string _path;

	/// <summary>
	///     Initializes a new <see cref="FileSystemVerifier" /> for the <paramref name="fileSystem" /> at the given
	///     <paramref name="path" />.
	/// </summary>
	public FileSystemVerifier(IFileSystem fileSystem, string path)
	{
		_fileSystem = fileSystem;
		_path = path;
		IFileInfo? fileInfo = _fileSystem.FileInfo.New(path);
		IDirectoryInfo? directoryInfo = _fileSystem.DirectoryInfo.New(path);
		if (fileInfo.Exists)
		{
			Type = FileSystemTypes.File;
		}
		else if (directoryInfo.Exists)
		{
			Type = FileSystemTypes.Directory;
		}
		else
		{
			Type = FileSystemTypes.DirectoryOrFile;
		}
	}

	/// <inheritdoc cref="IFileSystemVerifier.Exists" />
	public bool Exists =>
		Type == FileSystemTypes.File
			? _fileSystem.File.Exists(_path)
			: _fileSystem.Directory.Exists(_path);

	/// <inheritdoc cref="IFileSystemVerifier.Type" />
	public FileSystemTypes Type { get; }
}

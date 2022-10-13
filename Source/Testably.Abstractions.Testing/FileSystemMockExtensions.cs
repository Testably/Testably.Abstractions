using System;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods for the <see cref="FileSystemMock" />
/// </summary>
public static class FileSystemMockExtensions
{
	/// <summary>
	///     Changes the parameters of the default drive ('C:\' on Windows, '/' on Linux)
	/// </summary>
	public static FileSystemMock WithDrive(
		this FileSystemMock fileSystemMock,
		Action<IStorageDrive> driveCallback)
		=> fileSystemMock.WithDrive(null, driveCallback);

	/// <summary>
	///     Creates a new UNC (Universal Naming Convention) drive to the given <paramref name="server" />.
	/// </summary>
	public static FileSystemMock WithUncDrive(this FileSystemMock fileSystemMock,
	                                          string server,
	                                          Action<IStorageDrive>? driveCallback = null)
	{
		string uncPrefix = new(fileSystemMock.Path.DirectorySeparatorChar, 2);
		server = server.TrimStart(
			fileSystemMock.Path.DirectorySeparatorChar,
			fileSystemMock.Path.AltDirectorySeparatorChar);
		string drive = $"{uncPrefix}{server}";
		return fileSystemMock.WithDrive(drive, driveCallback);
	}
}
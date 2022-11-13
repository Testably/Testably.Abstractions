using System;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods for the <see cref="MockFileSystem" />
/// </summary>
public static class MockFileSystemExtensions
{
	/// <summary>
	///     Changes the parameters of the default drive ('C:\' on Windows, '/' on Linux)
	/// </summary>
	public static MockFileSystem WithDrive(
		this MockFileSystem mockFileSystem,
		Action<IStorageDrive> driveCallback)
		=> mockFileSystem.WithDrive(null, driveCallback);

	/// <summary>
	///     Creates a new UNC (Universal Naming Convention) drive to the given <paramref name="server" />.
	/// </summary>
	public static MockFileSystem WithUncDrive(this MockFileSystem mockFileSystem,
		string server,
		Action<IStorageDrive>? driveCallback = null)
	{
		string uncPrefix = new(mockFileSystem.Path.DirectorySeparatorChar, 2);
		server = server.TrimStart(
			mockFileSystem.Path.DirectorySeparatorChar,
			mockFileSystem.Path.AltDirectorySeparatorChar);
		string drive = $"{uncPrefix}{server}";
		return mockFileSystem.WithDrive(drive, driveCallback);
	}
}
namespace Testably.Abstractions.Testing;

/// <summary>
///     Options for the file system initializer.
/// </summary>
public class FileSystemInitializerOptions
{
	/// <summary>
	///     If set to <see langword="true" /> create the directory at <see cref="System.IO.Path.GetTempPath()" />.
	/// </summary>
	public bool InitializeTempDirectory { get; set; } = true;
}

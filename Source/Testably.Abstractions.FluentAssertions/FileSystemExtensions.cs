namespace Testably.Abstractions.FluentAssertions;

/// <summary>
///     Assertion extensions on <see cref="IFileSystem" />.
/// </summary>
public static class FileSystemExtensions
{
	/// <summary>
	///     Returns a <see cref="FileSystemAssertions" /> object that can be used to
	///     assert the current <see cref="IFileSystem" />.
	/// </summary>
	public static FileSystemAssertions Should(this IFileSystem instance)
	{
		return new FileSystemAssertions(instance);
	}
}

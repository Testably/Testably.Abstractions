using System;

namespace Testably.Abstractions.Internal;

internal static class Execute
{
	/// <summary>
	///     Executes <paramref name="onRealFileSystem" /> when
	///     the <paramref name="fileSystem" /> is a real file system,
	///     otherwise executes <paramref name="onMockFileSystem" />.
	/// </summary>
	public static void WhenRealFileSystem(IFileSystem fileSystem, Action onRealFileSystem,
		Action onMockFileSystem)
	{
		if (IsRealFileSystem(fileSystem))
		{
			onRealFileSystem();
			return;
		}

		onMockFileSystem();
	}

	/// <summary>
	///     Returns the value from <paramref name="onRealFileSystem" /> when
	///     the <paramref name="fileSystem" /> is a real file system,
	///     otherwise returns the value from  <paramref name="onMockFileSystem" />.
	/// </summary>
	public static T WhenRealFileSystem<T>(IFileSystem fileSystem,
		Func<T> onRealFileSystem,
		Func<T> onMockFileSystem)
		=> IsRealFileSystem(fileSystem)
			? onRealFileSystem()
			: onMockFileSystem();

	private static bool IsRealFileSystem(IFileSystem fileSystem)
		=> string.Equals(fileSystem.GetType().Name, "RealFileSystem",
			StringComparison.Ordinal);
}

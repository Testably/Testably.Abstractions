using System;
using System.Threading.Tasks;

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

	/// <summary>
	///     Returns the value from <paramref name="onRealFileSystem" /> when
	///     the <paramref name="fileSystem" /> is a real file system,
	///     otherwise returns the value from  <paramref name="onMockFileSystem" />.
	/// </summary>
	public static async Task WhenRealFileSystemAsync(IFileSystem fileSystem,
		Func<Task> onRealFileSystem,
		Action onMockFileSystem)
	{
		if (IsRealFileSystem(fileSystem))
		{
			await onRealFileSystem();
		}
		else
		{
			onMockFileSystem();
		}
	}

	/// <summary>
	///     Returns the value from <paramref name="onRealFileSystem" /> when
	///     the <paramref name="fileSystem" /> is a real file system,
	///     otherwise returns the value from  <paramref name="onMockFileSystem" />.
	/// </summary>
	public static async Task<T> WhenRealFileSystemAsync<T>(IFileSystem fileSystem,
		Func<Task<T>> onRealFileSystem,
		Func<T> onMockFileSystem)
		=> IsRealFileSystem(fileSystem)
			? await onRealFileSystem()
			: onMockFileSystem();

	/// <summary>
	///     Returns <see langword="true" /> when the <paramref name="fileSystem" /> is the real file
	///     system (and therefore has an underlying operating-system file system to delegate to).
	/// </summary>
	/// <remarks>
	///     Uses the same side-effect-free type-name check as the other companion packages; probing
	///     via a factory would register a phantom call in the statistics of the mocked file system.
	/// </remarks>
	public static bool IsRealFileSystem(this IFileSystem fileSystem)
		=> string.Equals(fileSystem.GetType().Name, "RealFileSystem",
			StringComparison.Ordinal);
}

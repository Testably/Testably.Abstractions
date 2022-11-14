using System;

namespace Testably.Abstractions.Testing.Storage;

internal static class StorageContainerExtensions
{
	/// <summary>
	///     Adjust the times in the <see cref="IStorageContainer" />.
	/// </summary>
	/// <param name="container">The container on which to adjust the times.</param>
	/// <param name="timeAdjustments">Flag indicating which times to adjust.</param>
	public static void AdjustTimes(this IStorageContainer container,
		TimeAdjustments timeAdjustments)
	{
		DateTime now = container.TimeSystem.DateTime.UtcNow;
		if (timeAdjustments.HasFlag(TimeAdjustments.CreationTime))
		{
			container.CreationTime.Set(now, DateTimeKind.Utc);
		}

		if (timeAdjustments.HasFlag(TimeAdjustments.LastAccessTime))
		{
			container.LastAccessTime.Set(now, DateTimeKind.Utc);
		}

		if (timeAdjustments.HasFlag(TimeAdjustments.LastWriteTime))
		{
			container.LastWriteTime.Set(now, DateTimeKind.Utc);
		}
	}
}

using System.IO;

namespace Testably.Abstractions.Testing.Helpers;

internal static class FileModeHelper
{
	internal static void ThrowIfInvalidModeAccess(FileMode mode, FileAccess access)
	{
		if (mode == FileMode.Append)
		{
			if (access == FileAccess.Read)
			{
				throw ExceptionFactory.InvalidAccessCombination(mode, access);
			}

			if (access != FileAccess.Write)
			{
				throw ExceptionFactory.AppendAccessOnlyInWriteOnlyMode();
			}
		}

		if (!access.HasFlag(FileAccess.Write) &&
		    (mode == FileMode.Truncate || mode == FileMode.CreateNew ||
		     mode == FileMode.Create || mode == FileMode.Append))
		{
			throw ExceptionFactory.InvalidAccessCombination(mode, access);
		}
	}
}

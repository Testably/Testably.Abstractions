#if EXECUTE_SAFEFILEHANDLE_TESTS
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers;

/// <summary>
///     <see
///         href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.safehandles.safefilehandle?view=net-6.0#examples" />
/// </summary>
public static class UnmanagedFileLoader
{
	private const uint CreateAlways = 2;
	private const uint CreateNew = 1;
	private const uint GenericRead = 0x80000000;
	private const uint GenericWrite = 0x40000000;
	private const uint OpenExisting = 3;

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern SafeFileHandle CreateFile(string lpFileName,
		uint dwDesiredAccess,
		uint dwShareMode,
		IntPtr lpSecurityAttributes,
		uint dwCreationDisposition,
		uint dwFlagsAndAttributes,
		IntPtr hTemplateFile);

	public static SafeFileHandle CreateSafeFileHandle(string? path,
		FileMode mode = FileMode.Open,
		FileAccess access = FileAccess.Write)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentNullException(nameof(path));
		}

		uint desiredAccess = 0;
		if (access.HasFlag(FileAccess.Read))
		{
			desiredAccess |= GenericRead;
		}

		if (access.HasFlag(FileAccess.Write))
		{
			desiredAccess |= GenericWrite;
		}

		uint creationDisposition;
		switch (mode)
		{
			case FileMode.Open:
				creationDisposition = OpenExisting;
				break;
			case FileMode.Create:
				creationDisposition = CreateAlways;
				break;
			case FileMode.CreateNew:
				creationDisposition = CreateNew;
				break;
			default:
				throw new NotSupportedException($"File mode '{mode}' is not supported!");
		}

		return CreateFile(path, desiredAccess, 0, IntPtr.Zero, creationDisposition, 0,
			IntPtr.Zero);
	}
}
#endif

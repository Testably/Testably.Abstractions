using System;
using System.IO;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.Helpers;

internal static class ExceptionFactory
{
	public static NotSupportedException NotSupportedFileStreamWrapping()
		=> new("You cannot wrap an existing FileStream in the MockFileSystem instance!");

	internal static UnauthorizedAccessException AccessToPathDenied(string path = "")
		=> new(string.IsNullOrEmpty(path)
			? "Access to the path is denied."
			: $"Access to the path '{path}' is denied.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024891
#endif
		};

	internal static IOException AclAccessToPathDenied(string path)
		=> new($"Access to the path '{path}' is denied.");

	internal static ArgumentException AppendAccessOnlyInWriteOnlyMode(
		string paramName = "access")
		=> new($"{FileMode.Append} access can be requested only in write-only mode.",
			paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809
#endif
		};

	internal static IOException CannotCreateFileAsAlreadyExists(string path)
		=> new(
			$"Cannot create '{path}' because a file or directory with the same name already exists.",
			Execute.IsWindows ? -2147024713 : 17);

	internal static IOException CannotCreateFileWhenAlreadyExists(int hResult)
		=> new("Cannot create a file when that file already exists.", hResult);

	internal static ArgumentException DirectoryNameDoesNotExist(string path)
		=> new($"The directory name '{path}' does not exist.", nameof(path))
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809
#endif
		};

	internal static IOException DirectoryNotEmpty(string path)
		=> new($"Directory not empty : '{path}'", Execute.IsWindows ? -2147024751 : 39);

	internal static DirectoryNotFoundException DirectoryNotFound(string? path = null)
		=> new(path == null
			? "Could not find a part of the path."
			: $"Could not find a part of the path '{path}'.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024893
#endif
		};

	internal static IOException FileAlreadyExists(string path, int hResult)
		=> new($"The file '{path}' already exists.", hResult);

	internal static IOException FileNameCannotBeResolved(string path, int hResult = -2147022975)
		=> new($"The name of the file cannot be resolved by the system. : '{path}'",
			hResult);

	internal static FileNotFoundException FileNotFound(string path)
		=> new($"Could not find file '{path}'.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024894
#endif
		};

	internal static InternalBufferOverflowException InternalBufferOverflowException(
		int internalBufferSize, int messages)
		=> new(
			$"The internal buffer is greater than the {internalBufferSize} allowed bytes (~ {messages} messages).");

	internal static ArgumentException InvalidAccessCombination(
		FileMode mode, FileAccess access)
		=> new($"Combining FileMode: {mode} with FileAccess: {access} is invalid.",
			nameof(access))
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809
#endif
		};

	internal static ArgumentException InvalidDriveName(string paramName = "driveName")
		=> new(
			"Drive name must be a root directory (i.e. 'C:\\') or a drive letter ('C').",
			paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809
#endif
		};

	internal static IOException NetworkPathNotFound(string path)
		=> new($"The network path was not found. : '{path}'");

	internal static IOException NotEnoughDiskSpace(string name)
		=> new($"There is not enough space on the disk: '{name}'");

	internal static PlatformNotSupportedException OperationNotSupportedOnThisPlatform()
		=> new("Operation is not supported on this platform.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233031
#endif
		};

	internal static ArgumentException PathCannotBeEmpty(string paramName = "path")
		=> Execute.OnNetFramework(
			() => new ArgumentException(
				"Path cannot be the empty string or all whitespace.")
			{
#if FEATURE_EXCEPTION_HRESULT
				HResult = -2147024809
#endif
			},
			() => new ArgumentException(
				"Path cannot be the empty string or all whitespace.", paramName)
			{
#if FEATURE_EXCEPTION_HRESULT
				HResult = -2147024809
#endif
			});

	internal static ArgumentException PathHasIllegalCharacters(
		string path, string paramName = "path")
		=> new($"Illegal characters in path '{path}'", paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809
#endif
		};

	internal static IOException PathHasIncorrectSyntax(string path)
		=> new(
			$"The filename, directory name, or volume label syntax is incorrect. : '{path}'",
			-2147024773);

	internal static ArgumentException PathIsEmpty(string paramName)
		=> new("The path is empty.", paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809
#endif
		};

	internal static IOException ProcessCannotAccessTheFile(string path, int hResult)
		=> new($"The process cannot access the file '{path}' because it is being used by another process.",
				hResult);

	internal static NotSupportedException StreamDoesNotSupportWriting()
		=> new("Stream does not support writing.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233067
#endif
		};

	internal static ArgumentOutOfRangeException TaskDelayOutOfRange(string paramName)
		=> new(paramName,
			"The value needs to be either -1 (signifying an infinite timeout), 0 or a positive integer.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233086
#endif
		};

	internal static TaskCanceledException TaskWasCanceled()
		=> new("A task was canceled.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233029
#endif
		};

	internal static ArgumentOutOfRangeException ThreadSleepOutOfRange(string paramName)
		=> new(paramName,
			"Number must be either non-negative and less than or equal to Int32.MaxValue or -1.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233086
#endif
		};

	internal static TimeoutException TimeoutExpired(int timeoutMilliseconds)
		=> new(
			$"The timeout of {timeoutMilliseconds}ms expired in the awaitable callback.");
}
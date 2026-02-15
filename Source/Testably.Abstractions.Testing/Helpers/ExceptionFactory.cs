using System;
using System.IO;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.Helpers;

internal static class ExceptionFactory
{
	internal static UnauthorizedAccessException AccessToPathDenied(string path = "")
		=> new(string.IsNullOrEmpty(path)
			? "Access to the path is denied."
			: $"Access to the path '{path}' is denied.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024891,
#endif
		};

	internal static ArgumentException AppendAccessOnlyInWriteOnlyMode(
		string paramName = "access")
		=> new($"{nameof(FileMode.Append)} access can be requested only in write-only mode.",
			paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809,
#endif
		};

	internal static ArgumentException BasePathNotFullyQualified(string paramName)
		=> new("Basepath argument is not fully qualified.", paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809,
#endif
		};

	internal static IOException CannotCreateFileAsAlreadyExists(Execute execute, string path)
		=> new(
			$"Cannot create '{path}' because a file or directory with the same name already exists.",
			execute.IsWindows ? -2147024713 : 17);

	internal static IOException CannotCreateFileWhenAlreadyExists(int hResult)
		=> new("Cannot create a file when that file already exists.", hResult);

	internal static ArgumentException DirectoryNameDoesNotExist(
		string path, string paramName = "path")
		=> new($"The directory name '{path}' does not exist.", paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809,
#endif
		};

	internal static IOException DirectoryNotEmpty(Execute execute, string path)
		=> new($"Directory not empty : '{path}'",
			execute.IsWindows
				? -2147024751
				: execute.IsMac
					? 66
					: 39);

	internal static DirectoryNotFoundException DirectoryNotFound(string? path = null)
		=> new(path == null
			? "Could not find a part of the path."
			: $"Could not find a part of the path '{path}'.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024893,
#endif
		};

	internal static IOException FileAlreadyExists(string path, int hResult)
		=> new($"The file '{path}' already exists.", hResult);

	internal static IOException FileNameCannotBeResolved(
		string path, int hResult = -2147022975)
		=> new($"The name of the file cannot be resolved by the system. : '{path}'",
			hResult);

	internal static FileNotFoundException FileNotFound(string path)
		=> new($"Could not find file '{path}'.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024894,
#endif
		};

	internal static IOException FileSharingViolation()
		=> new("The process cannot access the file because it is being used by another process.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024864,
#endif
		};

	internal static IOException FileSharingViolation(string path)
		=> new($"The process cannot access the file '{path}' because it is being used by another process.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024864,
#endif
		};

	internal static ArgumentException HandleIsInvalid(string? paramName = "handle")
		=> new("Invalid handle.", paramName);

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
			HResult = -2147024809,
#endif
		};

	internal static IOException InvalidDirectoryName(string path)
		=> new($"The directory name is invalid: '{path}'")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024629,
#endif
		};

	internal static ArgumentException InvalidDriveName(string paramName = "driveName")
		=> new(
			"Drive name must be a root directory (i.e. 'C:\\') or a drive letter ('C').",
			paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809,
#endif
		};

	internal static IOException MoveSourceMustBeDifferentThanDestination()
		=> new("Source and destination path must be different.", -2146232800);

	internal static IOException NetworkPathNotFound(string path)
		=> new($"The network path was not found. : '{path}'");

	internal static IOException NotEnoughDiskSpace(string name)
		=> new($"There is not enough space on the disk: '{name}'");

	internal static NotSupportedException NotSupportedFileStreamWrapping()
		=> new("You cannot wrap an existing FileStream in the MockFileSystem instance!");

	internal static NotSupportedException NotSupportedSafeFileHandle()
		=> new(
			"You cannot mock a safe file handle in the mocked file system without registering a strategy explicitly. Use `MockFileSystem.WithSafeFileHandleStrategy`!");

	internal static NotSupportedException NotSupportedTimerWrapping()
		=> new("You cannot wrap an existing Timer in the MockTimeSystem instance!");

	internal static ArgumentException NullCharacterInPath(string paramName)
#if NET8_0_OR_GREATER
		=> new("Null character in path.", paramName);
#else
		=> new("Illegal characters in path.", paramName);
#endif

	internal static PlatformNotSupportedException OperationNotSupportedOnThisPlatform()
		=> new("Operation is not supported on this platform.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233031,
#endif
		};

	internal static ArgumentException PathCannotBeEmpty(Execute execute, string paramName = "path")
	{
		if (execute.IsNetFramework)
		{
			#pragma warning disable MA0015 // Specify the parameter name
			return new ArgumentException(
				"Path cannot be the empty string or all whitespace.")
			{
#if FEATURE_EXCEPTION_HRESULT
				HResult = -2147024809,
#endif
			};
			#pragma warning restore MA0015 // Specify the parameter name
		}

		return new ArgumentException(
			"Path cannot be the empty string or all whitespace.", paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809,
#endif
		};
	}

	internal static ArgumentException PathHasIllegalCharacters(
		string path, string paramName = "path", int? hResult = -2147024809)
		=> new($"Illegal characters in path '{path}'", paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = hResult ?? -2147024809,
#endif
		};

	internal static IOException PathHasIncorrectSyntax(
		string path, int? hResult = -2147024773)
		=> new(
			$"The filename, directory name, or volume label syntax is incorrect. : '{path}'",
			hResult ?? -2147024773);

	internal static ArgumentException PathIsEmpty(string paramName,
		int hResult = -2147024809)
		=> new("The path is empty.", paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = hResult,
#endif
		};

	internal static IOException ProcessCannotAccessTheFile(string path, int hResult)
		=> new(
			$"The process cannot access the file '{path}' because it is being used by another process.",
			hResult);

	internal static IOException ReplaceSourceMustBeDifferentThanDestination(
		string sourcePath, string destinationPath)
		=> new($"The source '{sourcePath}' and destination '{destinationPath}' are the same file.", -2146232800);

	#pragma warning disable MA0015 // Specify the parameter name
	internal static ArgumentException SearchPatternCannotContainTwoDots()
		=> new(
			"Search pattern cannot contain \"..\" to move up directories and can be contained only internally in file/directory names, as in \"a..b\".");
	#pragma warning restore MA0015 // Specify the parameter name

	internal static IOException SeekBackwardNotPossibleInAppendMode()
		=> new(
			"Unable seek backward to overwrite data that previously existed in a file opened in Append mode.",
			-2146232800);

	internal static ArgumentException SpanMayNotBeEmpty(string paramName)
		=> new("Span may not be empty.", paramName)
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024809,
#endif
		};

	internal static NotSupportedException StreamDoesNotSupportReading()
		=> new("Stream does not support reading.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233067,
#endif
		};

	internal static NotSupportedException StreamDoesNotSupportWriting()
		=> new("Stream does not support writing.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233067,
#endif
		};

	internal static ArgumentOutOfRangeException TaskDelayOutOfRange(string paramName)
		=> new(paramName,
			"The value needs to be either -1 (signifying an infinite timeout), 0 or a positive integer.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233086,
#endif
		};

	internal static TaskCanceledException TaskWasCanceled()
		=> new("A task was canceled.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233029,
#endif
		};

	internal static ArgumentOutOfRangeException ThreadSleepOutOfRange(string paramName)
		=> new(paramName,
			"Number must be either non-negative and less than or equal to Int32.MaxValue or -1.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233086,
#endif
		};

	internal static TimeoutException TimeoutExpired(int timeoutMilliseconds)
		=> new(
			$"The timeout of {timeoutMilliseconds}ms expired in the awaitable callback.");

	internal static TimeoutException TimeoutExpired(TimeSpan timeout)
		=> new($"The timeout of {timeout} expired in the awaitable callback.");

	internal static ArgumentOutOfRangeException TimerArgumentOutOfRange(string propertyName)
		=> new(propertyName,
			"Number must be either non-negative and less than or equal to Int32.MaxValue or -1")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233086,
#endif
		};

	internal static TimeoutException TimerWaitTimeoutException(int executionCount, int timeout)
		=> new($"The execution count {executionCount} was not reached in {timeout}ms.");

	internal static UnauthorizedAccessException AccessDenied(string path)
		=> new($"Access to the path '{path}' is denied.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024891,
#endif
		};

	internal static IOException IOAccessDenied(string path)
		=> new($"Access to the path '{path}' is denied.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2147024891,
#endif
		};

	internal static PlatformNotSupportedException UnixFileModeNotSupportedOnThisPlatform()
		=> new("Unix file modes are not supported on this platform.")
		{
#if FEATURE_EXCEPTION_HRESULT
			HResult = -2146233031,
#endif
		};
	
	internal static ArgumentException InvalidUnixCreateMode(string paramName)
		=> new("UnixCreateMode can only be used with file modes that can create a new file.", paramName);
}

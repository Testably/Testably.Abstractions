using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.Internal;

[ExcludeFromCodeCoverage]
internal static class ExceptionFactory
{
    internal static UnauthorizedAccessException AccessToPathDenied(string path)
        => new($"Access to the path '{path}' is denied.");

    internal static ArgumentException AppendAccessOnlyInWriteOnlyMode(
        string paramName = "access")
        => new($"{FileMode.Append} access can be requested only in write-only mode.",
            paramName);

    internal static IOException CannotCreateFileAsAlreadyExists(string path)
        => new(
            $"Cannot create '{path}' because a file or directory with the same name already exists.");

    internal static IOException DirectoryNotEmpty(string path)
        => new($"Directory not empty : '{path}'");

    internal static DirectoryNotFoundException DirectoryNotFound(string path)
        => new($"Could not find a part of the path '{path}'.");

    internal static IOException FileAlreadyExists(string path)
        => new($"The file '{path}' already exists.");

    public static IOException FileNameCannotBeResolved(string path)
        => new($"The name of the file cannot be resolved by the system. : '{path}'");

    internal static FileNotFoundException FileNotFound(string path)
        => new($"Could not find file '{path}'.");

    internal static ArgumentException InvalidAccessCombination(
        FileMode mode, FileAccess access)
        => new($"Combining FileMode: {mode} with FileAccess: {access} is invalid.",
            nameof(access));

    internal static ArgumentException InvalidDriveName(string paramName = "driveName")
        => new(
            "Drive name must be a root directory (i.e. 'C:\\') or a drive letter ('C').",
            paramName);

    public static IOException NetworkPathNotFound(string path)
        => new($"The network path was not found. : '{path}'");

    public static IOException NotEnoughDiskSpace(string name)
        => new($"There is not enough space on the disk: '{name}'");

    internal static ArgumentException PathCannotBeEmpty(string paramName = "path")
    {
        if (Framework.IsNetFramework)
        {
            return new ArgumentException(
                "Path cannot be the empty string or all whitespace.");
        }

        return new ArgumentException(
            "Path cannot be the empty string or all whitespace.", paramName);
    }

    internal static ArgumentException PathHasIllegalCharacters(
        string path, string paramName = "path")
        => new($"Illegal characters in path '{path}'", paramName);

    internal static IOException PathHasIncorrectSyntax(string path)
        => new(
            $"The filename, directory name, or volume label syntax is incorrect. : '{path}'");

    internal static ArgumentException PathHasNoLegalForm()
        => new("The path is not of a legal form.");

    internal static ArgumentException PathIsEmpty(string paramName)
        => new("The path is empty.", paramName);

    internal static IOException ProcessCannotAccessTheFile(string path)
        => new(
            $"The process cannot access the file '{path}' because it is being used by another process.");

    internal static ArgumentOutOfRangeException TaskDelayOutOfRange(string paramName)
        => new(paramName,
            "The value needs to be either -1 (signifying an infinite timeout), 0 or a positive integer.");

    internal static TaskCanceledException TaskWasCanceled()
        => new("A task was canceled.");

    internal static ArgumentOutOfRangeException ThreadSleepOutOfRange(string paramName)
        => new(paramName,
            "Number must be either non-negative and less than or equal to Int32.MaxValue or -1.");
}
namespace Testably.Abstractions.Testing.Internal;

internal static class FileSystemExtensions
{
    /// <summary>
    ///     Returns the relative subdirectory path from <paramref name="fullFilePath" /> to the <paramref name="givenPath" />.
    /// </summary>
    internal static string GetSubdirectoryPath(this IFileSystem fileSystem,
                                               string fullFilePath, string givenPath)
    {
        if (fileSystem.Path.IsPathRooted(givenPath))
        {
            return fullFilePath;
        }

        string currentDirectory = fileSystem.Directory.GetCurrentDirectory();
        if (currentDirectory == string.Empty.PrefixRoot())
        {
            return fullFilePath.Substring(currentDirectory.Length);
        }

        return fullFilePath.Substring(currentDirectory.Length + 1);
    }
}
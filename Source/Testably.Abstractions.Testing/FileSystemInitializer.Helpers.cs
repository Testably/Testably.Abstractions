namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
    /// <summary>
    ///     Generates a random directory name in the format <c>{directoryName}-{suffix}</c>.
    /// </summary>
    internal static string GenerateRandomDirectoryName(string? directoryName = null,
                                                       int suffixMaxValue = 100000,
                                                       IRandomSystem? randomSystem = null)
    {
        string[] directoryNames = { "~WRL001", "foo", "bar", "_", "With whitespace" };
        randomSystem ??= new RandomSystem();
        directoryName ??=
            directoryNames[randomSystem.Random.Shared.Next(directoryNames.Length - 1)];
        int suffix = randomSystem.Random.Shared.Next(suffixMaxValue);
        return $"{directoryName}-{suffix}";
    }

    /// <summary>
    ///     Generates a random file extension without a leading dot (<c>.</c>).
    ///     <para />
    ///     If the <paramref name="fileExtension" /> is specified, it is used directly,
    ///     but a leading dot is removed.
    /// </summary>
    internal static string GenerateRandomFileExtension(string? fileExtension = null,
                                                       IRandomSystem? randomSystem = null)
    {
        string[] commonFileExtensions =
        {
            ".avi", ".bat", ".bin", ".bmp", ".csv", ".docx", ".exe", ".gif", ".html",
            ".ini", ".iso", ".jpeg", ".midi", ".mov", ".mpeg", ".png", ".rar", ".tmp",
            ".txt", ".xlsx", ".zip"
        };
        randomSystem ??= new RandomSystem();
        fileExtension ??=
            commonFileExtensions[
                randomSystem.Random.Shared.Next(commonFileExtensions.Length - 1)];
        return fileExtension.TrimStart('.');
    }

    /// <summary>
    ///     Generates a random file name in the format <c>{fileName}-{suffix}.{extension}</c>.
    /// </summary>
    internal static string GenerateRandomFileName(string? fileExtension = null,
                                                  string? fileName = null,
                                                  int suffixMaxValue = 100000,
                                                  IRandomSystem? randomSystem = null)
    {
        string[] fileNames = { "~WRL001", "foo", "bar", "_", "With whitespace" };
        randomSystem ??= new RandomSystem();
        fileExtension = GenerateRandomFileExtension(fileExtension);
        fileName ??= fileNames[randomSystem.Random.Shared.Next(fileNames.Length - 1)];
        int suffix = randomSystem.Random.Shared.Next(suffixMaxValue);
        return $"{fileName}-{suffix}.{fileExtension}";
    }
}
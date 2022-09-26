namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
    private static readonly string[] CommonFileExtensions =
    {
        ".avi", ".bat", ".bin", ".bmp", ".csv", ".docx", ".exe", ".gif", ".html",
        ".ini", ".iso", ".jpeg", ".midi", ".mov", ".mpeg", ".png", ".rar", ".tmp",
        ".txt", ".xlsx", ".zip"
    };

    private static readonly string[] FileNames =
    {
        "~WRL001", "foo", "bar", "_", "With whitespace"
    };

    /// <summary>
    ///     Generates a random directory name in the format <c>{directoryName}-{suffix}</c>.
    /// </summary>
    internal static string GenerateRandomDirectoryName(string? directoryName = null,
                                                       int suffixMaxValue = 100000,
                                                       IRandomSystem? randomSystem = null)
    {
        randomSystem ??= new RandomSystem();
        directoryName ??=
            FileNames[randomSystem.Random.Shared.Next(FileNames.Length - 1)];
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
        randomSystem ??= new RandomSystem();
        fileExtension ??=
            CommonFileExtensions[
                randomSystem.Random.Shared.Next(CommonFileExtensions.Length - 1)];
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
        randomSystem ??= new RandomSystem();
        fileExtension = GenerateRandomFileExtension(fileExtension);
        fileName ??= FileNames[randomSystem.Random.Shared.Next(FileNames.Length - 1)];
        int suffix = randomSystem.Random.Shared.Next(suffixMaxValue);
        return $"{fileName}-{suffix}.{fileExtension}";
    }
}
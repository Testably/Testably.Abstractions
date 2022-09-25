namespace Testably.Abstractions.Testing;

/// <summary>
///     Helper extension methods for testing purposes.
/// </summary>
public static class FileSystemTestingHelperExtensions
{
    private static readonly string[] CommonFileExtensions =
    {
        ".avi", ".bat", ".bin", ".bmp", ".csv", ".docx", ".exe", ".gif", ".html",
        ".ini", ".iso", ".jpeg", ".midi", ".mov", ".mpeg", ".png", ".rar", ".tmp",
        ".txt", ".xlsx", ".zip"
    };

    private static readonly string[] FileNames = { "~WRL001", "foo", "bar", "_" };

    /// <summary>
    ///     Generates a new random file in <paramref name="basePath" />.<br />
    ///     Creates the directory, if it does not yet exist.
    /// </summary>
    public static IFileSystem.IFileInfo GenerateFile(
        this IFileSystem fileSystem,
        string fileNameWithExtension,
        string basePath = ".")
    {
        fileSystem.Directory.CreateDirectory(basePath);
        IFileSystem.IFileInfo fileInfo =
            fileSystem.FileInfo.New(
                fileSystem.Path.Combine(basePath, fileNameWithExtension));
        if (fileInfo.Exists)
        {
            throw new TestingException($"The file '{fileInfo.FullName}' already exists!");
        }

        fileSystem.File.WriteAllText(fileInfo.FullName, null);
        return fileInfo;
    }

    /// <summary>
    ///     Generates a random file name in the format <c>{directoryName}-{suffix}</c>.
    /// </summary>
    public static string GenerateRandomDirectoryName(this IFileSystem fileSystem,
                                                     string? directoryName = null,
                                                     int suffixMaxValue = 100000,
                                                     IRandomSystem? randomSystem = null)
    {
        randomSystem ??= new RandomSystem();
        directoryName ??= FileNames[randomSystem.Random.Shared.Next(FileNames.Length - 1)];
        int suffix = randomSystem.Random.Shared.Next(suffixMaxValue);
        return $"{directoryName}-{suffix}";
    }

    /// <summary>
    ///     Generates a new random file in <paramref name="basePath" />.<br />
    ///     Creates the directory, if it does not yet exist.
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <param name="basePath"></param>
    /// <param name="fileExtension"></param>
    /// <param name="fileName"></param>
    public static IFileSystem.IFileInfo GenerateRandomFile(
        this IFileSystem fileSystem,
        string basePath = ".",
        string? fileExtension = null,
        string? fileName = null)
    {
        fileSystem.Directory.CreateDirectory(basePath);
        string filePath;
        do
        {
            filePath = fileSystem.Path.Combine(basePath,
                fileSystem.GenerateRandomFileName(fileExtension, fileName));
        } while (fileSystem.File.Exists(filePath));

        fileSystem.File.WriteAllText(filePath, null);
        return fileSystem.FileInfo.New(filePath);
    }

    /// <summary>
    ///     Generates a random file extension without a leading dot (<c>.</c>).
    ///     <para />
    ///     If the <paramref name="fileExtension" /> is specified, it is used directly,
    ///     but a leading dot is removed.
    /// </summary>
    public static string GenerateRandomFileExtension(this IFileSystem fileSystem,
                                                     string? fileExtension = null,
                                                     IRandomSystem? randomSystem = null)
    {
        randomSystem ??= new RandomSystem();
        fileExtension ??=
            CommonFileExtensions[randomSystem.Random.Shared.Next(CommonFileExtensions.Length - 1)];
        return fileExtension.TrimStart('.');
    }

    /// <summary>
    ///     Generates a new random file in <paramref name="basePath" />.<br />
    ///     Creates the directory, if it does not yet exist.
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <param name="basePath"></param>
    /// <param name="fileExtension"></param>
    /// <param name="fileName"></param>
    public static IFileSystem.IFileInfo GenerateRandomFileInRandomSubdirectoryOf(
        this IFileSystem fileSystem,
        string basePath = ".",
        string? fileExtension = null,
        string? fileName = null)
    {
        IFileSystem.IDirectoryInfo subdirectory =
            fileSystem.GenerateRandomSubdirectory(basePath);
        return GenerateRandomFile(fileSystem,
            fileSystem.Path.Combine(basePath, subdirectory.Name),
            fileExtension,
            fileName);
    }

    /// <summary>
    ///     Generates a random file name in the format <c>{fileName}-{suffix}.{extension}</c>.
    /// </summary>
    public static string GenerateRandomFileName(this IFileSystem fileSystem,
                                                string? fileExtension = null,
                                                string? fileName = null,
                                                int suffixMaxValue = 100000,
                                                IRandomSystem? randomSystem = null)
    {
        randomSystem ??= new RandomSystem();
        fileExtension = fileSystem.GenerateRandomFileExtension(fileExtension);
        fileName ??= FileNames[randomSystem.Random.Shared.Next(FileNames.Length - 1)];
        int suffix = randomSystem.Random.Shared.Next(suffixMaxValue);
        return $"{fileName}-{suffix}.{fileExtension}";
    }

    /// <summary>
    ///     Generates a new random file in <paramref name="basePath" />.<br />
    ///     Creates the directory, if it does not yet exist.
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <param name="basePath"></param>
    /// <param name="directoryName"></param>
    public static IFileSystem.IDirectoryInfo GenerateRandomSubdirectory(
        this IFileSystem fileSystem,
        string basePath = ".",
        string? directoryName = null)
    {
        do
        {
            IFileSystem.IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New(
                fileSystem.Path.Combine(basePath,
                    fileSystem.GenerateRandomDirectoryName(directoryName)));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
                return directoryInfo;
            }
        } while (true);
    }
}
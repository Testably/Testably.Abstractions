namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods for <see cref="IRandomSystem" /> to generate test data.
/// </summary>
public static class RandomSystemExtensions
{
    private static readonly string[] FileExtensions =
    {
        ".avi", ".bat", ".bin", ".bmp", ".csv", ".docx", ".exe", ".gif", ".html",
        ".ini", ".iso", ".jpeg", ".midi", ".mov", ".mpeg", ".png", ".rar", ".tmp",
        ".txt", ".xlsx", ".zip"
    };

    private static readonly string[] FileNames =
    {
        "~WRL001", "foo", "bar", "_", "With whitespace", ".hidden"
    };

    /// <summary>
    ///     Generates a random file extension without a leading dot (<c>.</c>).
    ///     <para />
    ///     If the <paramref name="fileExtension" /> is specified, it is used directly,
    ///     but a leading dot is removed.
    /// </summary>
    public static string GenerateFileExtension(this IRandomSystem randomSystem,
                                               string? fileExtension = null)
    {
        fileExtension ??=
            FileExtensions[
                randomSystem.Random.Shared.Next(FileExtensions.Length)];
        return fileExtension.TrimStart('.');
    }

    /// <summary>
    ///     Generates a random file name.
    ///     <para />
    ///     If the <paramref name="fileName" /> is specified, it is used directly.
    /// </summary>
    public static string GenerateFileName(this IRandomSystem randomSystem,
                                          string? fileName = null)
        => FileNames[randomSystem.Random.Shared.Next(FileNames.Length)];
}
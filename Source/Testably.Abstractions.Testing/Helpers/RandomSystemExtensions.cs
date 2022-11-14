using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.Helpers;

/// <summary>
///     Extension methods for <see cref="IRandomSystem" /> to generate test data.
/// </summary>
internal static class RandomSystemExtensions
{
	private static readonly string[] FileExtensions =
	{
		".avi",
		".bat",
		".bin",
		".bmp",
		".csv",
		".docx",
		".exe",
		".gif",
		".html",
		".ini",
		".iso",
		".jpeg",
		".midi",
		".mov",
		".mpeg",
		".png",
		".rar",
		".tmp",
		".txt",
		".xlsx",
		".zip"
	};

	private static readonly string[] FileNames =
	{
		"~WRL001",
		"foo",
		"bar",
		"_",
		"With whitespace",
		".hidden"
	};

	/// <summary>
	///     Generates a random file extension without a leading dot (<c>.</c>).
	///     <para />
	///     If the <paramref name="fileExtension" /> is specified, it is used directly,
	///     but a leading dot is removed.
	/// </summary>
	public static string GenerateFileExtension(this IRandom random,
		string? fileExtension = null)
	{
		fileExtension ??=
			FileExtensions[
				random.Next(FileExtensions.Length)];
		return fileExtension.TrimStart('.');
	}

	/// <summary>
	///     Generates a random file name.
	///     <para />
	///     If the <paramref name="fileName" /> is specified, it is used directly.
	/// </summary>
	public static string GenerateFileName(this IRandom random,
		string? fileName = null)
		=> FileNames[random.Next(FileNames.Length)];
}

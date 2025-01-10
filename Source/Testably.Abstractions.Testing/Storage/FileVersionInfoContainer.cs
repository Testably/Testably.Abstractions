using System;
using System.Globalization;
using System.Linq;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class FileVersionInfoContainer
{
	/// <inheritdoc cref="IFileVersionInfo.Comments" />
	public string? Comments { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.CompanyName" />
	public string? CompanyName { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.FileBuildPart" />
	public int FileBuildPart { get; private set; }

	/// <inheritdoc cref="IFileVersionInfo.FileDescription" />
	public string? FileDescription { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.FileMajorPart" />
	public int FileMajorPart { get; private set; }

	/// <inheritdoc cref="IFileVersionInfo.FileMinorPart" />
	public int FileMinorPart { get; private set; }

	/// <inheritdoc cref="IFileVersionInfo.FilePrivatePart" />
	public int FilePrivatePart { get; private set; }

	/// <inheritdoc cref="IFileVersionInfo.FileVersion" />
	public string? FileVersion
	{
		get => _fileVersion;
		set
		{
			_fileVersion = value;
			if (value != null)
			{
				string cleanedFileVersion = CleanVersion(value);
				if (Version.TryParse(cleanedFileVersion, out Version? version))
				{
					FileMajorPart = Math.Max(0, version.Major);
					FileMinorPart = Math.Max(0, version.Minor);
					FileBuildPart = Math.Max(0, version.Build);
					FilePrivatePart = Math.Max(0, version.Revision);
				}
				else if (int.TryParse(cleanedFileVersion, NumberStyles.Integer,
					CultureInfo.InvariantCulture, out int majorPart))
				{
					FileMajorPart = Math.Max(0, majorPart);
					FileMinorPart = 0;
					FileBuildPart = 0;
					FilePrivatePart = 0;
				}
				else
				{
					FileMajorPart = 0;
					FileMinorPart = 0;
					FileBuildPart = 0;
					FilePrivatePart = 0;
				}
			}
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.InternalName" />
	public string? InternalName { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.IsDebug" />
	public bool IsDebug { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.IsPatched" />
	public bool IsPatched { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.IsPreRelease" />
	public bool IsPreRelease { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.IsPrivateBuild" />
	public bool IsPrivateBuild { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.IsSpecialBuild" />
	public bool IsSpecialBuild { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.Language" />
	public string? Language { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.LegalCopyright" />
	public string? LegalCopyright { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.LegalTrademarks" />
	public string? LegalTrademarks { get; set; }

	public static FileVersionInfoContainer None => new();

	/// <inheritdoc cref="IFileVersionInfo.OriginalFilename" />
	public string? OriginalFilename { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.PrivateBuild" />
	public string? PrivateBuild { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.ProductBuildPart" />
	public int ProductBuildPart { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.ProductMajorPart" />
	public int ProductMajorPart { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.ProductMinorPart" />
	public int ProductMinorPart { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.ProductName" />
	public string? ProductName { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.ProductPrivatePart" />
	public int ProductPrivatePart { get; set; }

	/// <inheritdoc cref="IFileVersionInfo.ProductVersion" />
	public string? ProductVersion
	{
		get => _productVersion;
		set
		{
			_productVersion = value;
			if (value != null)
			{
				string cleanedFileVersion = CleanVersion(value);
				if (Version.TryParse(cleanedFileVersion, out Version? version))
				{
					ProductMajorPart = Math.Max(0, version.Major);
					ProductMinorPart = Math.Max(0, version.Minor);
					ProductBuildPart = Math.Max(0, version.Build);
					ProductPrivatePart = Math.Max(0, version.Revision);
				}
				else if (int.TryParse(cleanedFileVersion, NumberStyles.Integer,
					CultureInfo.InvariantCulture, out int majorPart))
				{
					ProductMajorPart = Math.Max(0, majorPart);
					ProductMinorPart = 0;
					ProductBuildPart = 0;
					ProductPrivatePart = 0;
				}
				else
				{
					ProductMajorPart = 0;
					ProductMinorPart = 0;
					ProductBuildPart = 0;
					ProductPrivatePart = 0;
				}
			}
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.SpecialBuild" />
	public string? SpecialBuild { get; set; }

	private string? _fileVersion;
	private string? _productVersion;

	private static string CleanVersion(string version)
	{
		char[] validVersionChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.'];
		for (int i = 0; i < version.Length; i++)
		{
			if (!validVersionChars.Contains(version[i]))
			{
				return version.Substring(0, i);
			}
		}

		return version;
	}
}

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions.FileSystem;

internal sealed class FileVersionInfoWrapper : IFileVersionInfo
{
	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	private readonly FileVersionInfo _instance;

	private FileVersionInfoWrapper(FileVersionInfo fileSystemWatcher,
		IFileSystem fileSystem)
	{
		_instance = fileSystemWatcher;
		FileSystem = fileSystem;
	}

	#region IFileVersionInfo Members

	/// <inheritdoc cref="IFileVersionInfo.Comments" />
	public string? Comments
		=> _instance.Comments;

	/// <inheritdoc cref="IFileVersionInfo.CompanyName" />
	public string? CompanyName
		=> _instance.CompanyName;

	/// <inheritdoc cref="IFileVersionInfo.FileBuildPart" />
	public int FileBuildPart
		=> _instance.FileBuildPart;

	/// <inheritdoc cref="IFileVersionInfo.FileDescription" />
	public string? FileDescription
		=> _instance.FileDescription;

	/// <inheritdoc cref="IFileVersionInfo.FileMajorPart" />
	public int FileMajorPart
		=> _instance.FileMajorPart;

	/// <inheritdoc cref="IFileVersionInfo.FileMinorPart" />
	public int FileMinorPart
		=> _instance.FileMinorPart;

	/// <inheritdoc cref="IFileVersionInfo.FileName" />
	public string FileName
		=> _instance.FileName;

	/// <inheritdoc cref="IFileVersionInfo.FilePrivatePart" />
	public int FilePrivatePart
		=> _instance.FilePrivatePart;

	/// <inheritdoc cref="IFileVersionInfo.FileVersion" />
	public string? FileVersion
		=> _instance.FileVersion;

	/// <inheritdoc cref="IFileVersionInfo.InternalName" />
	public string? InternalName
		=> _instance.InternalName;

	/// <inheritdoc cref="IFileVersionInfo.IsDebug" />
	public bool IsDebug
		=> _instance.IsDebug;

	/// <inheritdoc cref="IFileVersionInfo.IsPatched" />
	public bool IsPatched
		=> _instance.IsPatched;

	/// <inheritdoc cref="IFileVersionInfo.IsPreRelease" />
	public bool IsPreRelease
		=> _instance.IsPreRelease;

	/// <inheritdoc cref="IFileVersionInfo.IsPrivateBuild" />
	public bool IsPrivateBuild
		=> _instance.IsPrivateBuild;

	/// <inheritdoc cref="IFileVersionInfo.IsSpecialBuild" />
	public bool IsSpecialBuild
		=> _instance.IsSpecialBuild;

	/// <inheritdoc cref="IFileVersionInfo.Language" />
	public string? Language
		=> _instance.Language;

	/// <inheritdoc cref="IFileVersionInfo.LegalCopyright" />
	public string? LegalCopyright
		=> _instance.LegalCopyright;

	/// <inheritdoc cref="IFileVersionInfo.LegalTrademarks" />
	public string? LegalTrademarks
		=> _instance.LegalTrademarks;

	/// <inheritdoc cref="IFileVersionInfo.OriginalFilename" />
	public string? OriginalFilename
		=> _instance.OriginalFilename;

	/// <inheritdoc cref="IFileVersionInfo.PrivateBuild" />
	public string? PrivateBuild
		=> _instance.PrivateBuild;

	/// <inheritdoc cref="IFileVersionInfo.ProductBuildPart" />
	public int ProductBuildPart
		=> _instance.ProductBuildPart;

	/// <inheritdoc cref="IFileVersionInfo.ProductMajorPart" />
	public int ProductMajorPart
		=> _instance.ProductMajorPart;

	/// <inheritdoc cref="IFileVersionInfo.ProductMinorPart" />
	public int ProductMinorPart
		=> _instance.ProductMinorPart;

	/// <inheritdoc cref="IFileVersionInfo.ProductName" />
	public string? ProductName
		=> _instance.ProductName;

	/// <inheritdoc cref="IFileVersionInfo.ProductPrivatePart" />
	public int ProductPrivatePart
		=> _instance.ProductPrivatePart;

	/// <inheritdoc cref="IFileVersionInfo.ProductVersion" />
	public string? ProductVersion
		=> _instance.ProductVersion;

	/// <inheritdoc cref="IFileVersionInfo.SpecialBuild" />
	public string? SpecialBuild
		=> _instance.SpecialBuild;

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _instance.ToString();

	[return: NotNullIfNotNull("instance")]
	internal static FileVersionInfoWrapper? FromFileVersionInfo(
		FileVersionInfo? instance, IFileSystem fileSystem)
	{
		if (instance == null)
		{
			return null;
		}

		return new FileVersionInfoWrapper(instance, fileSystem);
	}
}

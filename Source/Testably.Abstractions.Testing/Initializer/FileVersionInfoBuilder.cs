using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Initializer;

/// <summary>
///     The builder for registering a <see cref="IFileVersionInfo" /> on the <see cref="MockFileSystem" />.
/// </summary>
public sealed class FileVersionInfoBuilder
{
	private readonly FileVersionInfoContainer _container = new();

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.Comments" />.
	/// </summary>
	public FileVersionInfoBuilder WithComments(string? comments)
	{
		_container.Comments = comments;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.CompanyName" />.
	/// </summary>
	public FileVersionInfoBuilder WithCompanyName(string? companyName)
	{
		_container.CompanyName = companyName;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.FileDescription" />.
	/// </summary>
	public FileVersionInfoBuilder WithFileDescription(string? fileDescription)
	{
		_container.FileDescription = fileDescription;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.FileVersion" />.
	/// </summary>
	public FileVersionInfoBuilder WithFileVersion(string? fileVersion)
	{
		_container.FileVersion = fileVersion;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.InternalName" />.
	/// </summary>
	public FileVersionInfoBuilder WithInternalName(string? internalName)
	{
		_container.InternalName = internalName;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.IsDebug" />.
	/// </summary>
	public FileVersionInfoBuilder WithIsDebug(bool isDebug)
	{
		_container.IsDebug = isDebug;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.IsPatched" />.
	/// </summary>
	public FileVersionInfoBuilder WithIsPatched(bool isPatched)
	{
		_container.IsPatched = isPatched;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.IsPreRelease" />.
	/// </summary>
	public FileVersionInfoBuilder WithIsPreRelease(bool isPreRelease)
	{
		_container.IsPreRelease = isPreRelease;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.IsPrivateBuild" />.
	/// </summary>
	public FileVersionInfoBuilder WithIsPrivateBuild(bool isPrivateBuild)
	{
		_container.IsPrivateBuild = isPrivateBuild;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.IsSpecialBuild" />.
	/// </summary>
	public FileVersionInfoBuilder WithIsSpecialBuild(bool isSpecialBuild)
	{
		_container.IsSpecialBuild = isSpecialBuild;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.Language" />.
	/// </summary>
	public FileVersionInfoBuilder WithLanguage(string? language)
	{
		_container.Language = language;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.LegalCopyright" />.
	/// </summary>
	public FileVersionInfoBuilder WithLegalCopyright(string? legalCopyright)
	{
		_container.LegalCopyright = legalCopyright;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.LegalTrademarks" />.
	/// </summary>
	public FileVersionInfoBuilder WithLegalTrademarks(string? legalTrademarks)
	{
		_container.LegalTrademarks = legalTrademarks;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.OriginalFilename" />.
	/// </summary>
	public FileVersionInfoBuilder WithOriginalFilename(string? originalFilename)
	{
		_container.OriginalFilename = originalFilename;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.PrivateBuild" />.
	/// </summary>
	public FileVersionInfoBuilder WithPrivateBuild(string? privateBuild)
	{
		_container.PrivateBuild = privateBuild;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.ProductName" />.
	/// </summary>
	public FileVersionInfoBuilder WithProductName(string? productName)
	{
		_container.ProductName = productName;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.ProductVersion" />.
	/// </summary>
	public FileVersionInfoBuilder WithProductVersion(string? productVersion)
	{
		_container.ProductVersion = productVersion;
		return this;
	}

	/// <summary>
	///     Sets the <see cref="IFileVersionInfo.SpecialBuild" />.
	/// </summary>
	public FileVersionInfoBuilder WithSpecialBuild(string? specialBuild)
	{
		_container.SpecialBuild = specialBuild;
		return this;
	}

	internal FileVersionInfoContainer Create()
		=> _container;
}

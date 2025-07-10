namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class FileVersionInfoBuilderTests
{
	[Theory]
	[AutoData]
	public async Task ShouldBePossibleToChainMethods(
		string comments,
		string companyName,
		string fileDescription,
		string internalName,
		bool isDebug,
		bool isPatched,
		bool isPreRelease,
		bool isPrivateBuild,
		bool isSpecialBuild,
		string language,
		string legalCopyright,
		string legalTrademarks,
		string originalFilename,
		string privateBuild,
		string productName,
		string specialBuild)
	{
		string fileVersion = "1.2.3.4-foo";
		string productVersion = "255.255.255.65432+bar";
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b
			.SetComments(comments)
			.SetCompanyName(companyName)
			.SetFileDescription(fileDescription)
			.SetFileVersion(fileVersion)
			.SetInternalName(internalName)
			.SetIsDebug(isDebug)
			.SetIsPatched(isPatched)
			.SetIsPreRelease(isPreRelease)
			.SetIsPrivateBuild(isPrivateBuild)
			.SetIsSpecialBuild(isSpecialBuild)
			.SetLanguage(language)
			.SetLegalCopyright(legalCopyright)
			.SetLegalTrademarks(legalTrademarks)
			.SetOriginalFilename(originalFilename)
			.SetPrivateBuild(privateBuild)
			.SetProductName(productName)
			.SetProductVersion(productVersion)
			.SetSpecialBuild(specialBuild)
			.SetComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.Comments).IsEqualTo(comments);
		await That(result.CompanyName).IsEqualTo(companyName);
		await That(result.FileDescription).IsEqualTo(fileDescription);
		await That(result.FileVersion).IsEqualTo(fileVersion);
		await That(result.InternalName).IsEqualTo(internalName);
		await That(result.IsDebug).IsEqualTo(isDebug);
		await That(result.IsPatched).IsEqualTo(isPatched);
		await That(result.IsPreRelease).IsEqualTo(isPreRelease);
		await That(result.IsSpecialBuild).IsEqualTo(isSpecialBuild);
		await That(result.Language).IsEqualTo(language);
		await That(result.LegalCopyright).IsEqualTo(legalCopyright);
		await That(result.LegalTrademarks).IsEqualTo(legalTrademarks);
		await That(result.OriginalFilename).IsEqualTo(originalFilename);
		await That(result.PrivateBuild).IsEqualTo(privateBuild);
		await That(result.ProductName).IsEqualTo(productName);
		await That(result.ProductVersion).IsEqualTo(productVersion);
		await That(result.SpecialBuild).IsEqualTo(specialBuild);
	}

	[Theory]
	[AutoData]
	public async Task WithComments_ShouldSetComments(string comments)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.Comments).IsEqualTo(comments);
	}

	[Theory]
	[AutoData]
	public async Task WithCompanyName_ShouldSetCompanyName(string? companyName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetCompanyName(companyName));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.CompanyName).IsEqualTo(companyName);
	}

	[Theory]
	[AutoData]
	public async Task WithFileDescription_ShouldSetFileDescription(string? fileDescription)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetFileDescription(fileDescription));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.FileDescription).IsEqualTo(fileDescription);
	}

	[Theory]
	[InlineData("1", 1, 0)]
	[InlineData("0.1", 0, 1)]
	[InlineData("1.2", 1, 2)]
	[InlineData("1.2.3", 1, 2, 3)]
	[InlineData("1.2.3.4", 1, 2, 3, 4)]
	public async Task WithFileVersion_ShouldSetFileVersion(
		string? fileVersion,
		int fileMajorPart, int fileMinorPart, int fileBuildPart = 0, int filePrivatePart = 0)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*",
			b => b.SetFileVersion("9.8.7.6").SetFileVersion(fileVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		await That(result.FileVersion).IsEqualTo(fileVersion);
		await That(result.FileMajorPart).IsEqualTo(fileMajorPart);
		await That(result.FileMinorPart).IsEqualTo(fileMinorPart);
		await That(result.FileBuildPart).IsEqualTo(fileBuildPart);
		await That(result.FilePrivatePart).IsEqualTo(filePrivatePart);
	}

	[Theory]
	[InlineData("1-foo", 1, 0)]
	[InlineData("1+bar", 1, 0)]
	[InlineData("1some-text", 1, 0)]
	[InlineData("0.1-foo", 0, 1)]
	[InlineData("0.1+bar", 0, 1)]
	[InlineData("0.1some-text", 0, 1)]
	[InlineData("1.2-foo", 1, 2)]
	[InlineData("1.2+bar", 1, 2)]
	[InlineData("1.2some-text", 1, 2)]
	[InlineData("1.2.3-foo", 1, 2, 3)]
	[InlineData("1.2.3+bar", 1, 2, 3)]
	[InlineData("1.2.3some-text", 1, 2, 3)]
	[InlineData("1.2.3.4-foo", 1, 2, 3, 4)]
	[InlineData("1.2.3.4+bar", 1, 2, 3, 4)]
	[InlineData("1.2.3.4some-text", 1, 2, 3, 4)]
	public async Task WithFileVersion_WhenContainsPreReleaseInfo_ShouldIgnorePreReleaseInfo(
		string? fileVersion,
		int fileMajorPart, int fileMinorPart, int fileBuildPart = 0, int filePrivatePart = 0)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetFileVersion(fileVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		await That(result.FileVersion).IsEqualTo(fileVersion);
		await That(result.FileMajorPart).IsEqualTo(fileMajorPart);
		await That(result.FileMinorPart).IsEqualTo(fileMinorPart);
		await That(result.FileBuildPart).IsEqualTo(fileBuildPart);
		await That(result.FilePrivatePart).IsEqualTo(filePrivatePart);
	}

	[Theory]
	[InlineData("")]
	[InlineData("-1")]
	[InlineData("+1.2.3-bar")]
	[InlineData("abc")]
	public async Task WithFileVersion_WhenStringIsInvalid_ShouldNotSetFileVersionParts(
		string? fileVersion)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetFileVersion("1.2.3.4")
			.SetFileVersion(fileVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		await That(result.FileVersion).IsEqualTo(fileVersion);
		await That(result.FileMajorPart).IsEqualTo(0);
		await That(result.FileMinorPart).IsEqualTo(0);
		await That(result.FileBuildPart).IsEqualTo(0);
		await That(result.FilePrivatePart).IsEqualTo(0);
	}

	[Theory]
	[AutoData]
	public async Task WithInternalName_ShouldSetInternalName(string? internalName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetInternalName(internalName));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.InternalName).IsEqualTo(internalName);
	}

	[Theory]
	[AutoData]
	public async Task WithIsDebug_ShouldSetIsDebug(bool isDebug)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsDebug(isDebug));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.IsDebug).IsEqualTo(isDebug);
	}

	[Theory]
	[AutoData]
	public async Task WithIsPatched_ShouldSetIsPatched(bool isPatched)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsPatched(isPatched));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.IsPatched).IsEqualTo(isPatched);
	}

	[Theory]
	[AutoData]
	public async Task WithIsPreRelease_ShouldSetIsPreRelease(bool isPreRelease)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsPreRelease(isPreRelease));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.IsPreRelease).IsEqualTo(isPreRelease);
	}

	[Theory]
	[AutoData]
	public async Task WithIsPrivateBuild_ShouldSetIsPrivateBuild(bool isPrivateBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsPrivateBuild(isPrivateBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.IsPrivateBuild).IsEqualTo(isPrivateBuild);
	}

	[Theory]
	[AutoData]
	public async Task WithIsSpecialBuild_ShouldSetIsSpecialBuild(bool isSpecialBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsSpecialBuild(isSpecialBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.IsSpecialBuild).IsEqualTo(isSpecialBuild);
	}

	[Theory]
	[AutoData]
	public async Task WithLanguage_ShouldSetLanguage(string? language)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetLanguage(language));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.Language).IsEqualTo(language);
	}

	[Theory]
	[AutoData]
	public async Task WithLegalCopyright_ShouldSetLegalCopyright(string? legalCopyright)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetLegalCopyright(legalCopyright));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.LegalCopyright).IsEqualTo(legalCopyright);
	}

	[Theory]
	[AutoData]
	public async Task WithLegalTrademarks_ShouldSetLegalTrademarks(string? legalTrademarks)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetLegalTrademarks(legalTrademarks));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.LegalTrademarks).IsEqualTo(legalTrademarks);
	}

	[Theory]
	[AutoData]
	public async Task WithOriginalFilename_ShouldSetOriginalFilename(string? originalFilename)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetOriginalFilename(originalFilename));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.OriginalFilename).IsEqualTo(originalFilename);
	}

	[Theory]
	[AutoData]
	public async Task WithPrivateBuild_ShouldSetPrivateBuild(string? privateBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetPrivateBuild(privateBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.PrivateBuild).IsEqualTo(privateBuild);
	}

	[Theory]
	[AutoData]
	public async Task WithProductName_ShouldSetProductName(string? productName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetProductName(productName));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.ProductName).IsEqualTo(productName);
	}

	[Theory]
	[InlineData("1", 1, 0)]
	[InlineData("0.1", 0, 1)]
	[InlineData("1.2", 1, 2)]
	[InlineData("1.2.3", 1, 2, 3)]
	[InlineData("1.2.3.4", 1, 2, 3, 4)]
	public async Task WithProductVersion_ShouldSetProductVersion(
		string? productVersion,
		int fileMajorPart, int fileMinorPart, int fileBuildPart = 0, int filePrivatePart = 0)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*",
			b => b.SetProductVersion("9.8.7.6").SetProductVersion(productVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		await That(result.ProductVersion).IsEqualTo(productVersion);
		await That(result.ProductMajorPart).IsEqualTo(fileMajorPart);
		await That(result.ProductMinorPart).IsEqualTo(fileMinorPart);
		await That(result.ProductBuildPart).IsEqualTo(fileBuildPart);
		await That(result.ProductPrivatePart).IsEqualTo(filePrivatePart);
	}

	[Theory]
	[InlineData("1-foo", 1, 0)]
	[InlineData("1+bar", 1, 0)]
	[InlineData("1some-text", 1, 0)]
	[InlineData("0.1-foo", 0, 1)]
	[InlineData("0.1+bar", 0, 1)]
	[InlineData("0.1some-text", 0, 1)]
	[InlineData("1.2-foo", 1, 2)]
	[InlineData("1.2+bar", 1, 2)]
	[InlineData("1.2some-text", 1, 2)]
	[InlineData("1.2.3-foo", 1, 2, 3)]
	[InlineData("1.2.3+bar", 1, 2, 3)]
	[InlineData("1.2.3some-text", 1, 2, 3)]
	[InlineData("1.2.3.4-foo", 1, 2, 3, 4)]
	[InlineData("1.2.3.4+bar", 1, 2, 3, 4)]
	[InlineData("1.2.3.4some-text", 1, 2, 3, 4)]
	public async Task WithProductVersion_WhenContainsPreReleaseInfo_ShouldIgnorePreReleaseInfo(
		string? productVersion,
		int fileMajorPart, int fileMinorPart, int fileBuildPart = 0, int filePrivatePart = 0)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetProductVersion(productVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		await That(result.ProductVersion).IsEqualTo(productVersion);
		await That(result.ProductMajorPart).IsEqualTo(fileMajorPart);
		await That(result.ProductMinorPart).IsEqualTo(fileMinorPart);
		await That(result.ProductBuildPart).IsEqualTo(fileBuildPart);
		await That(result.ProductPrivatePart).IsEqualTo(filePrivatePart);
	}

	[Theory]
	[InlineData("")]
	[InlineData("-1")]
	[InlineData("+1.2.3-bar")]
	[InlineData("abc")]
	public async Task WithProductVersion_WhenStringIsInvalid_ShouldNotSetProductVersionParts(
		string? productVersion)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetProductVersion("1.2.3.4")
			.SetProductVersion(productVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		await That(result.ProductVersion).IsEqualTo(productVersion);
		await That(result.ProductMajorPart).IsEqualTo(0);
		await That(result.ProductMinorPart).IsEqualTo(0);
		await That(result.ProductBuildPart).IsEqualTo(0);
		await That(result.ProductPrivatePart).IsEqualTo(0);
	}

	[Theory]
	[AutoData]
	public async Task WithSpecialBuild_ShouldSetSpecialBuild(string? specialBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetSpecialBuild(specialBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		await That(result.SpecialBuild).IsEqualTo(specialBuild);
	}
}

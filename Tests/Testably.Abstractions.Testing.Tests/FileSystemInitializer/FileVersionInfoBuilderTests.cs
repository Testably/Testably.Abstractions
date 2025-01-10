namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class FileVersionInfoBuilderTests
{
	[Theory]
	[AutoData]
	public void WithComments_ShouldSetComments(string comments)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.Comments.Should().Be(comments);
	}

	[Theory]
	[AutoData]
	public void WithCompanyName_ShouldSetCompanyName(string? companyName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetCompanyName(companyName));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.CompanyName.Should().Be(companyName);
	}

	[Theory]
	[AutoData]
	public void WithFileDescription_ShouldSetFileDescription(string? fileDescription)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetFileDescription(fileDescription));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.FileDescription.Should().Be(fileDescription);
	}

	[Theory]
	[InlineData("1", 1, 0)]
	[InlineData("0.1", 0, 1)]
	[InlineData("1.2", 1, 2)]
	[InlineData("1.2.3", 1, 2, 3)]
	[InlineData("1.2.3.4", 1, 2, 3, 4)]
	public void WithFileVersion_ShouldSetFileVersion(
		string? fileVersion,
		int fileMajorPart, int fileMinorPart, int fileBuildPart = 0, int filePrivatePart = 0)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetFileVersion("9.8.7.6").SetFileVersion(fileVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		result.FileVersion.Should().Be(fileVersion);
		result.FileMajorPart.Should().Be(fileMajorPart);
		result.FileMinorPart.Should().Be(fileMinorPart);
		result.FileBuildPart.Should().Be(fileBuildPart);
		result.FilePrivatePart.Should().Be(filePrivatePart);
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
	public void WithFileVersion_WhenContainsPreReleaseInfo_ShouldIgnorePreReleaseInfo(
		string? fileVersion,
		int fileMajorPart, int fileMinorPart, int fileBuildPart = 0, int filePrivatePart = 0)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetFileVersion(fileVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		result.FileVersion.Should().Be(fileVersion);
		result.FileMajorPart.Should().Be(fileMajorPart);
		result.FileMinorPart.Should().Be(fileMinorPart);
		result.FileBuildPart.Should().Be(fileBuildPart);
		result.FilePrivatePart.Should().Be(filePrivatePart);
	}

	[Theory]
	[InlineData("")]
	[InlineData("-1")]
	[InlineData("+1.2.3-bar")]
	[InlineData("abc")]
	public void WithFileVersion_WhenStringIsInvalid_ShouldNotSetFileVersionParts(
		string? fileVersion)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetFileVersion("1.2.3.4")
			.SetFileVersion(fileVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		result.FileVersion.Should().Be(fileVersion);
		result.FileMajorPart.Should().Be(0);
		result.FileMinorPart.Should().Be(0);
		result.FileBuildPart.Should().Be(0);
		result.FilePrivatePart.Should().Be(0);
	}

	[Theory]
	[AutoData]
	public void WithInternalName_ShouldSetInternalName(string? internalName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetInternalName(internalName));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.InternalName.Should().Be(internalName);
	}

	[Theory]
	[AutoData]
	public void WithIsDebug_ShouldSetIsDebug(bool isDebug)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsDebug(isDebug));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsDebug.Should().Be(isDebug);
	}

	[Theory]
	[AutoData]
	public void WithIsPatched_ShouldSetIsPatched(bool isPatched)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsPatched(isPatched));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsPatched.Should().Be(isPatched);
	}

	[Theory]
	[AutoData]
	public void WithIsPreRelease_ShouldSetIsPreRelease(bool isPreRelease)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsPreRelease(isPreRelease));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsPreRelease.Should().Be(isPreRelease);
	}

	[Theory]
	[AutoData]
	public void WithIsPrivateBuild_ShouldSetIsPrivateBuild(bool isPrivateBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsPrivateBuild(isPrivateBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsPrivateBuild.Should().Be(isPrivateBuild);
	}

	[Theory]
	[AutoData]
	public void WithIsSpecialBuild_ShouldSetIsSpecialBuild(bool isSpecialBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetIsSpecialBuild(isSpecialBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsSpecialBuild.Should().Be(isSpecialBuild);
	}

	[Theory]
	[AutoData]
	public void WithLanguage_ShouldSetLanguage(string? language)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetLanguage(language));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.Language.Should().Be(language);
	}

	[Theory]
	[AutoData]
	public void WithLegalCopyright_ShouldSetLegalCopyright(string? legalCopyright)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetLegalCopyright(legalCopyright));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.LegalCopyright.Should().Be(legalCopyright);
	}

	[Theory]
	[AutoData]
	public void WithLegalTrademarks_ShouldSetLegalTrademarks(string? legalTrademarks)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetLegalTrademarks(legalTrademarks));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.LegalTrademarks.Should().Be(legalTrademarks);
	}

	[Theory]
	[AutoData]
	public void WithOriginalFilename_ShouldSetOriginalFilename(string? originalFilename)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetOriginalFilename(originalFilename));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.OriginalFilename.Should().Be(originalFilename);
	}

	[Theory]
	[AutoData]
	public void WithPrivateBuild_ShouldSetPrivateBuild(string? privateBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetPrivateBuild(privateBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.PrivateBuild.Should().Be(privateBuild);
	}

	[Theory]
	[AutoData]
	public void WithProductName_ShouldSetProductName(string? productName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetProductName(productName));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.ProductName.Should().Be(productName);
	}

	[Theory]
	[InlineData("1", 1, 0)]
	[InlineData("0.1", 0, 1)]
	[InlineData("1.2", 1, 2)]
	[InlineData("1.2.3", 1, 2, 3)]
	[InlineData("1.2.3.4", 1, 2, 3, 4)]
	public void WithProductVersion_ShouldSetProductVersion(
		string? productVersion,
		int fileMajorPart, int fileMinorPart, int fileBuildPart = 0, int filePrivatePart = 0)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetProductVersion("9.8.7.6").SetProductVersion(productVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		result.ProductVersion.Should().Be(productVersion);
		result.ProductMajorPart.Should().Be(fileMajorPart);
		result.ProductMinorPart.Should().Be(fileMinorPart);
		result.ProductBuildPart.Should().Be(fileBuildPart);
		result.ProductPrivatePart.Should().Be(filePrivatePart);
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
	public void WithProductVersion_WhenContainsPreReleaseInfo_ShouldIgnorePreReleaseInfo(
		string? productVersion,
		int fileMajorPart, int fileMinorPart, int fileBuildPart = 0, int filePrivatePart = 0)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetProductVersion(productVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		result.ProductVersion.Should().Be(productVersion);
		result.ProductMajorPart.Should().Be(fileMajorPart);
		result.ProductMinorPart.Should().Be(fileMinorPart);
		result.ProductBuildPart.Should().Be(fileBuildPart);
		result.ProductPrivatePart.Should().Be(filePrivatePart);
	}

	[Theory]
	[InlineData("")]
	[InlineData("-1")]
	[InlineData("+1.2.3-bar")]
	[InlineData("abc")]
	public void WithProductVersion_WhenStringIsInvalid_ShouldNotSetProductVersionParts(
		string? productVersion)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetProductVersion("1.2.3.4")
			.SetProductVersion(productVersion));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");
		result.ProductVersion.Should().Be(productVersion);
		result.ProductMajorPart.Should().Be(0);
		result.ProductMinorPart.Should().Be(0);
		result.ProductBuildPart.Should().Be(0);
		result.ProductPrivatePart.Should().Be(0);
	}

	[Theory]
	[AutoData]
	public void WithSpecialBuild_ShouldSetSpecialBuild(string? specialBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersionInfo("*", b => b.SetSpecialBuild(specialBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.SpecialBuild.Should().Be(specialBuild);
	}
	
	[Theory]
	[AutoData]
	public void ShouldBePossibleToChainMethods(
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

		result.Comments.Should().Be(comments);
		result.CompanyName.Should().Be(companyName);
		result.FileDescription.Should().Be(fileDescription);
		result.FileVersion.Should().Be(fileVersion);
		result.InternalName.Should().Be(internalName);
		result.IsDebug.Should().Be(isDebug);
		result.IsPatched.Should().Be(isPatched);
		result.IsPreRelease.Should().Be(isPreRelease);
		result.IsSpecialBuild.Should().Be(isSpecialBuild);
		result.Language.Should().Be(language);
		result.LegalCopyright.Should().Be(legalCopyright);
		result.LegalTrademarks.Should().Be(legalTrademarks);
		result.OriginalFilename.Should().Be(originalFilename);
		result.PrivateBuild.Should().Be(privateBuild);
		result.ProductName.Should().Be(productName);
		result.ProductVersion.Should().Be(productVersion);
		result.SpecialBuild.Should().Be(specialBuild);
	}
}

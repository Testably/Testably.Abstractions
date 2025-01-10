namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class FileVersionInfoBuilderTests
{
	[Theory]
	[AutoData]
	public void WithComments_ShouldSetComments(string comments)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.Comments.Should().Be(comments);
	}

	[Theory]
	[AutoData]
	public void WithCompanyName_ShouldSetCompanyName(string? companyName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithCompanyName(companyName));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.CompanyName.Should().Be(companyName);
	}

	[Theory]
	[AutoData]
	public void WithFileDescription_ShouldSetFileDescription(string? fileDescription)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithFileDescription(fileDescription));

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
		fileSystem.WithFileVersion("*", b => b.WithFileVersion("9.8.7.6").WithFileVersion(fileVersion));

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
		fileSystem.WithFileVersion("*", b => b.WithFileVersion(fileVersion));

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
		fileSystem.WithFileVersion("*", b => b.WithFileVersion("1.2.3.4")
			.WithFileVersion(fileVersion));

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
		fileSystem.WithFileVersion("*", b => b.WithInternalName(internalName));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.InternalName.Should().Be(internalName);
	}

	[Theory]
	[AutoData]
	public void WithIsDebug_ShouldSetIsDebug(bool isDebug)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithIsDebug(isDebug));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsDebug.Should().Be(isDebug);
	}

	[Theory]
	[AutoData]
	public void WithIsPatched_ShouldSetIsPatched(bool isPatched)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithIsPatched(isPatched));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsPatched.Should().Be(isPatched);
	}

	[Theory]
	[AutoData]
	public void WithIsPreRelease_ShouldSetIsPreRelease(bool isPreRelease)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithIsPreRelease(isPreRelease));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsPreRelease.Should().Be(isPreRelease);
	}

	[Theory]
	[AutoData]
	public void WithIsPrivateBuild_ShouldSetIsPrivateBuild(bool isPrivateBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithIsPrivateBuild(isPrivateBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsPrivateBuild.Should().Be(isPrivateBuild);
	}

	[Theory]
	[AutoData]
	public void WithIsSpecialBuild_ShouldSetIsSpecialBuild(bool isSpecialBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithIsSpecialBuild(isSpecialBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.IsSpecialBuild.Should().Be(isSpecialBuild);
	}

	[Theory]
	[AutoData]
	public void WithLanguage_ShouldSetLanguage(string? language)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithLanguage(language));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.Language.Should().Be(language);
	}

	[Theory]
	[AutoData]
	public void WithLegalCopyright_ShouldSetLegalCopyright(string? legalCopyright)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithLegalCopyright(legalCopyright));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.LegalCopyright.Should().Be(legalCopyright);
	}

	[Theory]
	[AutoData]
	public void WithLegalTrademarks_ShouldSetLegalTrademarks(string? legalTrademarks)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithLegalTrademarks(legalTrademarks));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.LegalTrademarks.Should().Be(legalTrademarks);
	}

	[Theory]
	[AutoData]
	public void WithOriginalFilename_ShouldSetOriginalFilename(string? originalFilename)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithOriginalFilename(originalFilename));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.OriginalFilename.Should().Be(originalFilename);
	}

	[Theory]
	[AutoData]
	public void WithPrivateBuild_ShouldSetPrivateBuild(string? privateBuild)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithPrivateBuild(privateBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.PrivateBuild.Should().Be(privateBuild);
	}

	[Theory]
	[AutoData]
	public void WithProductName_ShouldSetProductName(string? productName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "");
		fileSystem.WithFileVersion("*", b => b.WithProductName(productName));

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
		fileSystem.WithFileVersion("*", b => b.WithProductVersion("9.8.7.6").WithProductVersion(productVersion));

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
		fileSystem.WithFileVersion("*", b => b.WithProductVersion(productVersion));

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
		fileSystem.WithFileVersion("*", b => b.WithProductVersion("1.2.3.4")
			.WithProductVersion(productVersion));

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
		fileSystem.WithFileVersion("*", b => b.WithSpecialBuild(specialBuild));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("foo");

		result.SpecialBuild.Should().Be(specialBuild);
	}
}

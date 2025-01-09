using Testably.Abstractions.Testing.Tests.TestHelpers;
// ReSharper disable MethodSupportsCancellation

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileVersionInfoStatisticsTests
{
	[SkippableFact]
	public void Property_Comments_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").Comments;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.Comments));
	}

	[SkippableFact]
	public void Property_CompanyName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").CompanyName;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.CompanyName));
	}

	[SkippableFact]
	public void Property_FileBuildPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileBuildPart;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.FileBuildPart));
	}

	[SkippableFact]
	public void Property_FileDescription_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileDescription;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.FileDescription));
	}

	[SkippableFact]
	public void Property_FileMajorPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileMajorPart;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.FileMajorPart));
	}

	[SkippableFact]
	public void Property_FileMinorPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileMinorPart;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.FileMinorPart));
	}

	[SkippableFact]
	public void Property_FileName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileName;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.FileName));
	}

	[SkippableFact]
	public void Property_FilePrivatePart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FilePrivatePart;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.FilePrivatePart));
	}

	[SkippableFact]
	public void Property_FileVersion_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileVersion;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.FileVersion));
	}

	[SkippableFact]
	public void Property_InternalName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").InternalName;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.InternalName));
	}

	[SkippableFact]
	public void Property_IsDebug_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsDebug;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.IsDebug));
	}

	[SkippableFact]
	public void Property_IsPatched_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsPatched;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.IsPatched));
	}

	[SkippableFact]
	public void Property_IsPreRelease_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsPreRelease;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.IsPreRelease));
	}

	[SkippableFact]
	public void Property_IsPrivateBuild_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsPrivateBuild;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.IsPrivateBuild));
	}

	[SkippableFact]
	public void Property_IsSpecialBuild_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsSpecialBuild;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.IsSpecialBuild));
	}

	[SkippableFact]
	public void Property_Language_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").Language;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.Language));
	}

	[SkippableFact]
	public void Property_LegalCopyright_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").LegalCopyright;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.LegalCopyright));
	}

	[SkippableFact]
	public void Property_LegalTrademarks_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").LegalTrademarks;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.LegalTrademarks));
	}

	[SkippableFact]
	public void Property_OriginalFilename_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").OriginalFilename;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.OriginalFilename));
	}

	[SkippableFact]
	public void Property_PrivateBuild_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").PrivateBuild;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.PrivateBuild));
	}

	[SkippableFact]
	public void Property_ProductBuildPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductBuildPart;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.ProductBuildPart));
	}

	[SkippableFact]
	public void Property_ProductMajorPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductMajorPart;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.ProductMajorPart));
	}

	[SkippableFact]
	public void Property_ProductMinorPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductMinorPart;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.ProductMinorPart));
	}

	[SkippableFact]
	public void Property_ProductName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductName;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.ProductName));
	}

	[SkippableFact]
	public void Property_ProductPrivatePart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductPrivatePart;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.ProductPrivatePart));
	}

	[SkippableFact]
	public void Property_ProductVersion_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductVersion;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.ProductVersion));
	}

	[SkippableFact]
	public void Property_SpecialBuild_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.FileVersionInfo.GetVersionInfo("foo").SpecialBuild;

		sut.Statistics.FileVersionInfo["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileVersionInfo.SpecialBuild));
	}
}

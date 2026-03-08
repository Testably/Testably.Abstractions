using Testably.Abstractions.Testing.Tests.TestHelpers;
// ReSharper disable MethodSupportsCancellation

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileVersionInfoStatisticsTests
{
	[Test]
	public async Task Property_Comments_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").Comments;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.Comments));
	}

	[Test]
	public async Task Property_CompanyName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").CompanyName;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.CompanyName));
	}

	[Test]
	public async Task Property_FileBuildPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileBuildPart;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.FileBuildPart));
	}

	[Test]
	public async Task Property_FileDescription_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileDescription;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.FileDescription));
	}

	[Test]
	public async Task Property_FileMajorPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileMajorPart;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.FileMajorPart));
	}

	[Test]
	public async Task Property_FileMinorPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileMinorPart;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.FileMinorPart));
	}

	[Test]
	public async Task Property_FileName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileName;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.FileName));
	}

	[Test]
	public async Task Property_FilePrivatePart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FilePrivatePart;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.FilePrivatePart));
	}

	[Test]
	public async Task Property_FileVersion_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").FileVersion;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.FileVersion));
	}

	[Test]
	public async Task Property_InternalName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").InternalName;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.InternalName));
	}

	[Test]
	public async Task Property_IsDebug_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsDebug;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.IsDebug));
	}

	[Test]
	public async Task Property_IsPatched_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsPatched;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.IsPatched));
	}

	[Test]
	public async Task Property_IsPreRelease_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsPreRelease;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.IsPreRelease));
	}

	[Test]
	public async Task Property_IsPrivateBuild_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsPrivateBuild;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.IsPrivateBuild));
	}

	[Test]
	public async Task Property_IsSpecialBuild_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").IsSpecialBuild;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.IsSpecialBuild));
	}

	[Test]
	public async Task Property_Language_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").Language;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.Language));
	}

	[Test]
	public async Task Property_LegalCopyright_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").LegalCopyright;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.LegalCopyright));
	}

	[Test]
	public async Task Property_LegalTrademarks_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").LegalTrademarks;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.LegalTrademarks));
	}

	[Test]
	public async Task Property_OriginalFilename_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").OriginalFilename;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.OriginalFilename));
	}

	[Test]
	public async Task Property_PrivateBuild_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").PrivateBuild;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.PrivateBuild));
	}

	[Test]
	public async Task Property_ProductBuildPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductBuildPart;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.ProductBuildPart));
	}

	[Test]
	public async Task Property_ProductMajorPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductMajorPart;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.ProductMajorPart));
	}

	[Test]
	public async Task Property_ProductMinorPart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductMinorPart;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.ProductMinorPart));
	}

	[Test]
	public async Task Property_ProductName_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductName;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.ProductName));
	}

	[Test]
	public async Task Property_ProductPrivatePart_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductPrivatePart;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.ProductPrivatePart));
	}

	[Test]
	public async Task Property_ProductVersion_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").ProductVersion;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.ProductVersion));
	}

	[Test]
	public async Task Property_SpecialBuild_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile("foo");

		_ = sut.FileVersionInfo.GetVersionInfo("foo").SpecialBuild;

		await That(sut.Statistics.FileVersionInfo["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileVersionInfo.SpecialBuild));
	}
}

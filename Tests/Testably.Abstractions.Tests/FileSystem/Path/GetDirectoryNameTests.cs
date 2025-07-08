namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetDirectoryNameTests
{
	[Theory]
	[InlineData((string?)null)]
#if !NETFRAMEWORK
	[InlineData("")]
#endif
	public async Task GetDirectoryName_NullOrEmpty_ShouldReturnNull(string? path)
	{
		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsNull();
	}

#if NETFRAMEWORK
	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("    ")]
	[InlineData("\t")]
	[InlineData("\n")]
	public async Task GetDirectoryName_EmptyOrWhiteSpace_ShouldThrowArgumentException(string path)
	{
		void Act()
		{
			_ = FileSystem.Path.GetDirectoryName(path);
		}

		await That(Act).Throws<ArgumentException>();
	}
#endif

#if !NETFRAMEWORK
	[Theory]
	[InlineData(" ")]
	[InlineData("    ")]
	public async Task GetDirectoryName_Spaces_ShouldReturnNullOnWindowsOtherwiseEmpty(string? path)
	{
		string? result = FileSystem.Path.GetDirectoryName(path);

		if (Test.RunsOnWindows)
		{
			await That(result).IsNull();
		}
		else
		{
			await That(result).IsEqualTo("");
		}
	}
#endif

#if !NETFRAMEWORK
	[Theory]
	[InlineData("\t")]
	[InlineData("\n")]
	[InlineData(" \t")]
	[InlineData("\n  ")]
	public async Task GetDirectoryName_TabOrNewline_ShouldReturnEmptyString(string? path)
	{
		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo("");
	}
#endif

	[Theory]
	[AutoData]
	public async Task GetDirectoryName_ShouldReturnDirectory(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo(directory);
	}

	[Theory]
	[AutoData]
	public async Task GetDirectoryName_ShouldReplaceAltDirectorySeparator(
		string parentDirectory, string directory, string filename)
	{
		string path = parentDirectory + FileSystem.Path.AltDirectorySeparatorChar + directory +
					  FileSystem.Path.AltDirectorySeparatorChar + filename;
		string expected = parentDirectory + FileSystem.Path.DirectorySeparatorChar + directory;

		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo(expected);
	}

	[Theory]
	[InlineData("foo//bar/file", "foo/bar", TestOS.All)]
	[InlineData("foo///bar/file", "foo/bar", TestOS.All)]
	[InlineData("//foo//bar/file", "/foo/bar", TestOS.Linux | TestOS.Mac)]
	[InlineData(@"foo\\bar/file", "foo/bar", TestOS.Windows)]
	[InlineData(@"foo\\\bar/file", "foo/bar", TestOS.Windows)]
	public async Task GetDirectoryName_ShouldNormalizeDirectorySeparators(
		string path, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		expected = expected.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo(expected);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task GetDirectoryName_Span_ShouldReturnDirectory(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetDirectoryName(path.AsSpan());

		await That(result.ToString()).IsEqualTo(directory);
	}
#endif

	[Theory]
	[InlineData("//", null, TestOS.Windows)]
	[InlineData(@"\\", null, TestOS.Windows)]
	[InlineData(@"\\", "", TestOS.Linux | TestOS.Mac)]
	[InlineData(@"\", "", TestOS.Linux | TestOS.Mac)]
	[InlineData(@"/", null, TestOS.Linux | TestOS.Mac)]
	[InlineData(@"/a", "/", TestOS.Linux | TestOS.Mac)]
	[InlineData(@"/a\b", @"/", TestOS.Linux | TestOS.Mac)]
	[InlineData(@"/a\b/c", @"/a\b", TestOS.Linux | TestOS.Mac)]
	[InlineData(@"/a/b/c", @"/a/b", TestOS.Linux | TestOS.Mac)]
	[InlineData(@"/a/b", "/a", TestOS.Linux | TestOS.Mac)]
	[InlineData("//?/G:/", null, TestOS.Windows)]
	[InlineData("/??/H:/", @"\??\H:", TestOS.Windows)]
	[InlineData("//?/I:/a", @"\\?\I:\", TestOS.Windows)]
	[InlineData("/??/J:/a", @"\??\J:", TestOS.Windows)]
	[InlineData(@"\\?\K:\", null, TestOS.Windows)]
	[InlineData(@"\??\L:\", null, TestOS.Windows)]
	[InlineData(@"\\?\M:\a", @"\\?\M:\", TestOS.Windows)]
	[InlineData(@"\??\N:\a", @"\??\N:\", TestOS.Windows)]
	[InlineData(@"\\?\UNC\", null, TestOS.Windows)]
	[InlineData(@"//?/UNC/", null, TestOS.Windows)]
	[InlineData(@"\??\UNC\", null, TestOS.Windows)]
	[InlineData(@"/??/UNC/", @"\??\UNC", TestOS.Windows)]
	[InlineData(@"\\?\UNC\a", null, TestOS.Windows)]
	[InlineData(@"//?/UNC/a", null, TestOS.Windows)]
	[InlineData(@"\??\UNC\a", null, TestOS.Windows)]
	[InlineData(@"/??/UNC/a", @"\??\UNC", TestOS.Windows)]
	[InlineData(@"\\?\ABC\", null, TestOS.Windows)]
	[InlineData(@"//?/ABC/", null, TestOS.Windows)]
	[InlineData(@"\??\XYZ\", null, TestOS.Windows)]
	[InlineData(@"/??/XYZ/", @"\??\XYZ", TestOS.Windows)]
	[InlineData(@"\\?\unc\a", @"\\?\unc\", TestOS.Windows)]
	[InlineData(@"//?/unc/a", @"\\?\unc\", TestOS.Windows)]
	[InlineData(@"\??\unc\a", @"\??\unc\", TestOS.Windows)]
	[InlineData(@"/??/unc/a", @"\??\unc", TestOS.Windows)]
	[InlineData("//./", null, TestOS.Windows)]
	[InlineData(@"\\.\", null, TestOS.Windows)]
	[InlineData("//?/", null, TestOS.Windows)]
	[InlineData(@"\\?\", null, TestOS.Windows)]
	[InlineData("//a/", null, TestOS.Windows)]
	[InlineData(@"\\a\", null, TestOS.Windows)]
	[InlineData(@"C:", null, TestOS.Windows)]
	[InlineData(@"D:\", null, TestOS.Windows)]
	[InlineData(@"E:/", null, TestOS.Windows)]
	[InlineData(@"F:\a", @"F:\", TestOS.Windows)]
	[InlineData(@"F:\b\c", @"F:\b", TestOS.Windows)]
	[InlineData(@"F:\d/e", @"F:\d", TestOS.Windows)]
	[InlineData(@"G:/f", @"G:\", TestOS.Windows)]
	[InlineData(@"F:/g\h", @"F:\g", TestOS.Windows)]
	[InlineData(@"G:/i/j", @"G:\i", TestOS.Windows)]
	public async Task GetDirectoryName_SpecialCases_ShouldReturnExpectedValue(
		string path, string? expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo(expected);
	}
}

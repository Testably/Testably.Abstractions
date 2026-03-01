namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class GetDirectoryNameTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[Arguments((string?)null)]
#if !NETFRAMEWORK
	[Arguments("")]
#endif
	public async Task GetDirectoryName_NullOrEmpty_ShouldReturnNull(string? path)
	{
		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsNull();
	}

#if NETFRAMEWORK
	[Test]
	[Arguments("")]
	[Arguments(" ")]
	[Arguments("    ")]
	[Arguments("\t")]
	[Arguments("\n")]
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
	[Test]
	[Arguments(" ")]
	[Arguments("    ")]
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
	[Test]
	[Arguments("\t")]
	[Arguments("\n")]
	[Arguments(" \t")]
	[Arguments("\n  ")]
	public async Task GetDirectoryName_TabOrNewline_ShouldReturnEmptyString(string? path)
	{
		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo("");
	}
#endif

	[Test]
	[AutoArguments]
	public async Task GetDirectoryName_ShouldReturnDirectory(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo(directory);
	}

	[Test]
	[AutoArguments]
	public async Task GetDirectoryName_ShouldReplaceAltDirectorySeparator(
		string parentDirectory, string directory, string filename)
	{
		string path = parentDirectory + FileSystem.Path.AltDirectorySeparatorChar + directory +
		              FileSystem.Path.AltDirectorySeparatorChar + filename;
		string expected = parentDirectory + FileSystem.Path.DirectorySeparatorChar + directory;

		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[Arguments("foo//bar/file", "foo/bar", TestOS.All)]
	[Arguments("foo///bar/file", "foo/bar", TestOS.All)]
	[Arguments("//foo//bar/file", "/foo/bar", TestOS.Linux | TestOS.Mac)]
	[Arguments(@"foo\\bar/file", "foo/bar", TestOS.Windows)]
	[Arguments(@"foo\\\bar/file", "foo/bar", TestOS.Windows)]
	public async Task GetDirectoryName_ShouldNormalizeDirectorySeparators(
		string path, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		expected = expected.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo(expected);
	}

#if FEATURE_SPAN
	[Test]
	[AutoArguments]
	public async Task GetDirectoryName_Span_ShouldReturnDirectory(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetDirectoryName(path.AsSpan());

		await That(result.ToString()).IsEqualTo(directory);
	}
#endif

	[Test]
	[Arguments("//", null, TestOS.Windows)]
	[Arguments(@"\\", null, TestOS.Windows)]
	[Arguments(@"\\", "", TestOS.Linux | TestOS.Mac)]
	[Arguments(@"\", "", TestOS.Linux | TestOS.Mac)]
	[Arguments(@"/", null, TestOS.Linux | TestOS.Mac)]
	[Arguments(@"/a", "/", TestOS.Linux | TestOS.Mac)]
	[Arguments(@"/a\b", @"/", TestOS.Linux | TestOS.Mac)]
	[Arguments(@"/a\b/c", @"/a\b", TestOS.Linux | TestOS.Mac)]
	[Arguments(@"/a/b/c", @"/a/b", TestOS.Linux | TestOS.Mac)]
	[Arguments(@"/a/b", "/a", TestOS.Linux | TestOS.Mac)]
	[Arguments("//?/G:/", null, TestOS.Windows)]
	[Arguments("/??/H:/", @"\??\H:", TestOS.Windows)]
	[Arguments("//?/I:/a", @"\\?\I:\", TestOS.Windows)]
	[Arguments("/??/J:/a", @"\??\J:", TestOS.Windows)]
	[Arguments(@"\\?\K:\", null, TestOS.Windows)]
	[Arguments(@"\??\L:\", null, TestOS.Windows)]
	[Arguments(@"\\?\M:\a", @"\\?\M:\", TestOS.Windows)]
	[Arguments(@"\??\N:\a", @"\??\N:\", TestOS.Windows)]
	[Arguments(@"\\?\UNC\", null, TestOS.Windows)]
	[Arguments(@"//?/UNC/", null, TestOS.Windows)]
	[Arguments(@"\??\UNC\", null, TestOS.Windows)]
	[Arguments(@"/??/UNC/", @"\??\UNC", TestOS.Windows)]
	[Arguments(@"\\?\UNC\a", null, TestOS.Windows)]
	[Arguments(@"//?/UNC/a", null, TestOS.Windows)]
	[Arguments(@"\??\UNC\a", null, TestOS.Windows)]
	[Arguments(@"/??/UNC/a", @"\??\UNC", TestOS.Windows)]
	[Arguments(@"\\?\ABC\", null, TestOS.Windows)]
	[Arguments(@"//?/ABC/", null, TestOS.Windows)]
	[Arguments(@"\??\XYZ\", null, TestOS.Windows)]
	[Arguments(@"/??/XYZ/", @"\??\XYZ", TestOS.Windows)]
	[Arguments(@"\\?\unc\a", @"\\?\unc\", TestOS.Windows)]
	[Arguments(@"//?/unc/a", @"\\?\unc\", TestOS.Windows)]
	[Arguments(@"\??\unc\a", @"\??\unc\", TestOS.Windows)]
	[Arguments(@"/??/unc/a", @"\??\unc", TestOS.Windows)]
	[Arguments("//./", null, TestOS.Windows)]
	[Arguments(@"\\.\", null, TestOS.Windows)]
	[Arguments("//?/", null, TestOS.Windows)]
	[Arguments(@"\\?\", null, TestOS.Windows)]
	[Arguments("//a/", null, TestOS.Windows)]
	[Arguments(@"\\a\", null, TestOS.Windows)]
	[Arguments(@"C:", null, TestOS.Windows)]
	[Arguments(@"D:\", null, TestOS.Windows)]
	[Arguments(@"E:/", null, TestOS.Windows)]
	[Arguments(@"F:\a", @"F:\", TestOS.Windows)]
	[Arguments(@"F:\b\c", @"F:\b", TestOS.Windows)]
	[Arguments(@"F:\d/e", @"F:\d", TestOS.Windows)]
	[Arguments(@"G:/f", @"G:\", TestOS.Windows)]
	[Arguments(@"F:/g\h", @"F:\g", TestOS.Windows)]
	[Arguments(@"G:/i/j", @"G:\i", TestOS.Windows)]
	public async Task GetDirectoryName_SpecialCases_ShouldReturnExpectedValue(
		string path, string? expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string? result = FileSystem.Path.GetDirectoryName(path);

		await That(result).IsEqualTo(expected);
	}
}

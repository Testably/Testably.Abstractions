using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void AltDirectorySeparatorChar_ShouldReturnSlash()
	{
		char result = FileSystem.Path.AltDirectorySeparatorChar;

		result.Should().Be('/');
	}

	[SkippableFact]
	public void DirectorySeparatorChar_WhenNotOnWindows_ShouldReturnSlash()
	{
		Skip.If(Test.RunsOnWindows);

		char result = FileSystem.Path.DirectorySeparatorChar;

		result.Should().Be('/');
	}

	[SkippableFact]
	public void DirectorySeparatorChar_WhenOnWindows_ShouldReturnBackslash()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char result = FileSystem.Path.DirectorySeparatorChar;

		result.Should().Be('\\');
	}

	[SkippableFact]
	public void GetInvalidFileNameChars_WhenNotOnWindows_ShouldReturnCorrectValues()
	{
		Skip.If(Test.RunsOnWindows);

		char[] expected = ['\0', '/'];

		char[] result = FileSystem.Path.GetInvalidFileNameChars();

		result.Should().BeEquivalentTo(expected);
	}

	[SkippableFact]
	public void GetInvalidFileNameChars_WhenOnWindows_ShouldReturnCorrectValues()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char[] expected =
		[
			'\"', '<', '>', '|', '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6,
			(char)7, (char)8, (char)9, (char)10, (char)11, (char)12, (char)13, (char)14, (char)15,
			(char)16, (char)17, (char)18, (char)19, (char)20, (char)21, (char)22, (char)23,
			(char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30, (char)31, ':',
			'*', '?', '\\', '/'
		];

		char[] result = FileSystem.Path.GetInvalidFileNameChars();

		result.Should().BeEquivalentTo(expected);
	}

	[SkippableFact]
	public void GetInvalidPathChars_WhenNotOnWindows_ShouldReturnCorrectValues()
	{
		Skip.If(Test.RunsOnWindows);

		char[] expected = ['\0'];

		char[] result = FileSystem.Path.GetInvalidPathChars();

		result.Should().BeEquivalentTo(expected);
	}

	[SkippableFact]
	public void GetInvalidPathChars_WhenOnWindows_ShouldReturnCorrectValues()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char[] expected =
		[
			'|', '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8,
			(char)9, (char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17,
			(char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25,
			(char)26, (char)27, (char)28, (char)29, (char)30, (char)31
		];
		if (Test.IsNetFramework)
		{
			expected = expected.Concat(['"', '<', '>']).ToArray();
		}

		char[] result = FileSystem.Path.GetInvalidPathChars();

		result.Should().BeEquivalentTo(expected);
	}

	[SkippableFact]
	public void PathSeparator_WhenNotOnWindows_ShouldReturnColon()
	{
		Skip.If(Test.RunsOnWindows);

		char result = FileSystem.Path.PathSeparator;

		result.Should().Be(':');
	}

	[SkippableFact]
	public void PathSeparator_WhenOnWindows_ShouldReturnSemicolon()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char result = FileSystem.Path.PathSeparator;

		result.Should().Be(';');
	}

	[SkippableFact]
	public void VolumeSeparatorChar_WhenNotOnWindows_ShouldReturnSlash()
	{
		Skip.If(Test.RunsOnWindows);

		char result = FileSystem.Path.VolumeSeparatorChar;

		result.Should().Be('/');
	}

	[SkippableFact]
	public void VolumeSeparatorChar_WhenOnWindows_ShouldReturnColon()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char result = FileSystem.Path.VolumeSeparatorChar;

		result.Should().Be(':');
	}
}

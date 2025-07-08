using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class Tests
{
	[Fact]
	public async Task AltDirectorySeparatorChar_ShouldReturnSlash()
	{
		char result = FileSystem.Path.AltDirectorySeparatorChar;

		await That(result).IsEqualTo('/');
	}

	[Fact]
	public async Task DirectorySeparatorChar_WhenNotOnWindows_ShouldReturnSlash()
	{
		Skip.If(Test.RunsOnWindows);

		char result = FileSystem.Path.DirectorySeparatorChar;

		await That(result).IsEqualTo('/');
	}

	[Fact]
	public async Task DirectorySeparatorChar_WhenOnWindows_ShouldReturnBackslash()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char result = FileSystem.Path.DirectorySeparatorChar;

		await That(result).IsEqualTo('\\');
	}

	[Fact]
	public async Task GetInvalidFileNameChars_WhenNotOnWindows_ShouldReturnCorrectValues()
	{
		Skip.If(Test.RunsOnWindows);

		char[] expected = ['\0', '/'];

		char[] result = FileSystem.Path.GetInvalidFileNameChars();

		await That(result).IsEqualTo(expected).InAnyOrder();
	}

	[Fact]
	public async Task GetInvalidFileNameChars_WhenOnWindows_ShouldReturnCorrectValues()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char[] expected =
		[
			'\"', '<', '>', '|', '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6,
			(char)7, (char)8, (char)9, (char)10, (char)11, (char)12, (char)13, (char)14, (char)15,
			(char)16, (char)17, (char)18, (char)19, (char)20, (char)21, (char)22, (char)23,
			(char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30, (char)31, ':',
			'*', '?', '\\', '/',
		];

		char[] result = FileSystem.Path.GetInvalidFileNameChars();

		await That(result).IsEqualTo(expected).InAnyOrder();
	}

	[Fact]
	public async Task GetInvalidPathChars_WhenNotOnWindows_ShouldReturnCorrectValues()
	{
		Skip.If(Test.RunsOnWindows);

		char[] expected = ['\0'];

		char[] result = FileSystem.Path.GetInvalidPathChars();

		await That(result).IsEqualTo(expected).InAnyOrder();
	}

	[Fact]
	public async Task GetInvalidPathChars_WhenOnWindows_ShouldReturnCorrectValues()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char[] expected =
		[
			'|', '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8,
			(char)9, (char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17,
			(char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25,
			(char)26, (char)27, (char)28, (char)29, (char)30, (char)31,
		];
		if (Test.IsNetFramework)
		{
			expected = expected.Concat(['"', '<', '>']).ToArray();
		}

		char[] result = FileSystem.Path.GetInvalidPathChars();

		await That(result).IsEqualTo(expected).InAnyOrder();
	}

	[Fact]
	public async Task PathSeparator_WhenNotOnWindows_ShouldReturnColon()
	{
		Skip.If(Test.RunsOnWindows);

		char result = FileSystem.Path.PathSeparator;

		await That(result).IsEqualTo(':');
	}

	[Fact]
	public async Task PathSeparator_WhenOnWindows_ShouldReturnSemicolon()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char result = FileSystem.Path.PathSeparator;

		await That(result).IsEqualTo(';');
	}

	[Fact]
	public async Task VolumeSeparatorChar_WhenNotOnWindows_ShouldReturnSlash()
	{
		Skip.If(Test.RunsOnWindows);

		char result = FileSystem.Path.VolumeSeparatorChar;

		await That(result).IsEqualTo('/');
	}

	[Fact]
	public async Task VolumeSeparatorChar_WhenOnWindows_ShouldReturnColon()
	{
		Skip.IfNot(Test.RunsOnWindows);

		char result = FileSystem.Path.VolumeSeparatorChar;

		await That(result).IsEqualTo(':');
	}
}

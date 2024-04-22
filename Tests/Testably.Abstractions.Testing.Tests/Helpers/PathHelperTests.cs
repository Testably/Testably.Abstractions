using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class PathHelperTests
{
	[Fact]
	public void
		EnsureValidFormat_WithWhiteSpaceAndIncludeIsEmptyCheck_ShouldThrowArgumentException()
	{
		string whiteSpace = " ";
		MockFileSystem fileSystem = new();
		Exception? exception = Record.Exception(() =>
		{
			whiteSpace.EnsureValidFormat(fileSystem, "foo", true);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
	}

	[Theory]
	[AutoData]
	public void GetFullPathOrWhiteSpace_NormalPath_ShouldReturnFullPath(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize();
		string expectedPath = fileSystem.Path.GetFullPath(path);

		string result = path.GetFullPathOrWhiteSpace(fileSystem);

		result.Should().Be(expectedPath);
	}

	[Fact]
	public void GetFullPathOrWhiteSpace_Null_ShouldReturnEmptyString()
	{
		MockFileSystem fileSystem = new();
		string? sut = null;

		string result = sut.GetFullPathOrWhiteSpace(fileSystem);

		result.Should().Be("");
	}

	[Theory]
	[InlineData("  ")]
	[InlineData("\t")]
	public void GetFullPathOrWhiteSpace_WhiteSpace_ShouldReturnPath(string path)
	{
		MockFileSystem fileSystem = new();

		string result = path.GetFullPathOrWhiteSpace(fileSystem);

		result.Should().Be(path);
	}

	[Theory]
	[AutoData]
	public void IsUncPath_AltDirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.AltDirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath(new MockFileSystem());

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void IsUncPath_DirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.DirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath(new MockFileSystem());

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void IsUncPath_MixedDirectorySeparatorChars_ShouldReturnFalse(string path)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Mac and Linux don't have distinctive directory separator chars.");

		path = $"{Path.AltDirectorySeparatorChar}{Path.DirectorySeparatorChar}{path}";

		bool result = path.IsUncPath(new MockFileSystem());

		result.Should().BeFalse();
	}

	[Fact]
	public void IsUncPath_Null_ShouldReturnFalse()
	{
		string? path = null;

		bool result = path!.IsUncPath(new MockFileSystem());

		result.Should().BeFalse();
	}

	[Fact]
	public void
		ThrowCommonExceptionsIfPathIsInvalid_StartWithNull_ShouldThrowArgumentException()
	{
		string path = "\0foo";

		Exception? exception = Record.Exception(() =>
		{
			path.EnsureValidFormat(new MockFileSystem());
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.Message.Should().Contain($"'{path}'");
	}

#if !NETFRAMEWORK
	[SkippableTheory]
	[InlineData('|')]
	[InlineData((char)1)]
	[InlineData((char)31)]
	public void ThrowCommonExceptionsIfPathIsInvalid_WithInvalidCharacters(
		char invalidChar)
	{
		MockFileSystem fileSystem = new(i => i
			.SimulatingOperatingSystem(SimulationMode.Windows));
		string path = invalidChar + "path";

		Exception? exception = Record.Exception(() =>
		{
			path.EnsureValidFormat(fileSystem);
		});
		exception.Should().BeOfType<IOException>()
			.Which.Message.Should().Contain(path);
	}
#endif

	[Fact]
	public void
		ThrowCommonExceptionsIfPathToTargetIsInvalid_NullCharacter_ShouldThrowArgumentException()
	{
		MockFileSystem fileSystem = new();
		string path = "path-with\0 invalid character";

		Exception? exception = Record.Exception(() =>
		{
			path.ThrowCommonExceptionsIfPathToTargetIsInvalid(fileSystem);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.Message.Should().Contain($"'{path}'");
	}
}

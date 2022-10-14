using Moq;
using System.IO;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Internal;

public class PathHelperTests
{
	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(PathHelper))]
	public void IsUncPath_AltDirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.AltDirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath();

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(PathHelper))]
	public void IsUncPath_DirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.DirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath();

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	[Trait(nameof(Testing), nameof(PathHelper))]
	public void IsUncPath_MixedDirectorySeparatorChars_ShouldReturnFalse(string path)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Mac and Linux don't have distinctive directory separator chars.");

		path = $"{Path.AltDirectorySeparatorChar}{Path.DirectorySeparatorChar}{path}";

		bool result = path.IsUncPath();

		result.Should().BeFalse();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(PathHelper))]
	public void IsUncPath_Null_ShouldReturnFalse()
	{
		string? path = null;

		bool result = path.IsUncPath();

		result.Should().BeFalse();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(PathHelper))]
	public void RemoveLeadingDot_MultipleLocalDirectories_ShouldBeRemoved()
	{
		string path = Path.Combine(".", ".", ".", "foo");

		string result = path.RemoveLeadingDot();

		result.Should().Be("foo");
	}

	[Fact]
	[Trait(nameof(Testing), nameof(PathHelper))]
	public void
		ThrowCommonExceptionsIfPathIsInvalid_StartWithNull_ShouldThrowArgumentException()
	{
		string path = "\0foo";

		Exception? exception = Record.Exception(() =>
		{
			path.ThrowCommonExceptionsIfPathIsInvalid(new FileSystem());
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain($"'{path}'");
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(PathHelper))]
	public void ThrowCommonExceptionsIfPathIsInvalid_WithInvalidCharacters(
		char[] invalidChars)
	{
		Mock<IFileSystem> fileSystemMock = new();
		Mock<IFileSystem.IPath> pathSystemMock = new();
		fileSystemMock.Setup(m => m.Path).Returns(pathSystemMock.Object);
		pathSystemMock.Setup(m => m.GetInvalidPathChars()).Returns(invalidChars);
		pathSystemMock
		   .Setup(m => m.GetFullPath(It.IsAny<string>()))
		   .Returns<string>(s => s);
		string path = invalidChars[0] + "foo";

		Exception? exception = Record.Exception(() =>
		{
			path.ThrowCommonExceptionsIfPathIsInvalid(fileSystemMock.Object);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain($"'{path}'");
#else
		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should().Contain($"'{path}'");
#endif
	}
}
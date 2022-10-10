using Moq;
using System.IO;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing.Tests.Internal;

public class PathHelperTests
{
	[Fact]
	public void RemoveLeadingDot_MultipleLocalDirectories_ShouldBeRemoved()
	{
		string path = Path.Combine(".", ".", ".", "foo");

		string result = path.RemoveLeadingDot();

		result.Should().Be("foo");
	}

	[Fact]
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
using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class FileDescriptionTests
{
	[Theory]
	[AutoData]
	public void Constructor_WithBytes_ShouldSetBytes(byte[] bytes)
	{
		FileDescription sut = new("foo", bytes);

		sut.Content.Should().BeNull();
		sut.Bytes.Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void Constructor_WithContent_ShouldSetContent(string content)
	{
		FileDescription sut = new("foo", content);

		sut.Content.Should().Be(content);
		sut.Bytes.Should().BeNull();
	}

	[Fact]
	public void Index_AccessShouldThrowTestingException()
	{
		FileDescription sut = new("foo");

		Exception? exception = Record.Exception(() =>
		{
			_ = sut["bar"];
		});

		exception.Should().BeOfType<TestingException>()
			.Which.Message.Should().Be("Files cannot have children.");
	}
}

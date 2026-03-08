using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class FileDescriptionTests
{
	[Test]
	[AutoArguments]
	public async Task Constructor_WithBytes_ShouldSetBytes(byte[] bytes)
	{
		FileDescription sut = new("foo", bytes);

		await That(sut.Content).IsNull();
		await That(sut.Bytes).IsEqualTo(bytes).InAnyOrder();
	}

	[Test]
	[AutoArguments]
	public async Task Constructor_WithContent_ShouldSetContent(string content)
	{
		FileDescription sut = new("foo", content);

		await That(sut.Content).IsEqualTo(content);
		await That(sut.Bytes).IsNull();
	}

	[Test]
	public async Task Index_AccessShouldThrowTestingException()
	{
		FileDescription sut = new("foo");

		void Act()
		{
			_ = sut["bar"];
		}

		await That(Act).ThrowsExactly<TestingException>()
			.WithMessage("Files cannot have children.");
	}
}

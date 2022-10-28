namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class InterceptionHandlerTests
{
	public Testing.MockFileSystem FileSystem { get; }

	public InterceptionHandlerTests()
	{
		FileSystem = new Testing.MockFileSystem();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_CustomException_ShouldNotCreateDirectory(
		string path, Exception exceptionToThrow)
	{
		FileSystem.Intercept.Event(_ =>
		{
			FileSystem.Directory.Exists(path).Should().BeFalse();
			throw exceptionToThrow;
		});
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		FileSystem.Directory.Exists(path).Should().BeFalse();
		exception.Should().Be(exceptionToThrow);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_CustomException_ShouldOnlyTriggerChangeOccurring(
		string path, Exception exceptionToThrow)
	{
		string? receivedPath = null;
		FileSystem.Intercept.Event(_ => throw exceptionToThrow);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		exception.Should().Be(exceptionToThrow);
		receivedPath.Should().BeNull();
	}
}
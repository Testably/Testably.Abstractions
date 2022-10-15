namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class InterceptionHandlerTests
{
	public Testing.FileSystemMock FileSystem { get; }

	public InterceptionHandlerTests()
	{
		FileSystem = new Testing.FileSystemMock();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateDirectory_CustomException_ShouldNotCreateDirectory(
		string path, Exception exceptionToThrow)
	{
		FileSystem.Intercept.Changing(_ =>
		{
			FileSystem.Directory.Exists(path).Should().BeFalse();
			throw exceptionToThrow;
		});
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnChange()
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
			   .Wait(timeout: 500);
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
		FileSystem.Intercept.Changing(_ => throw exceptionToThrow);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnChange(c => receivedPath = c.Path)
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
			   .Wait(timeout: 500);
		});

		exception.Should().Be(exceptionToThrow);
		receivedPath.Should().BeNull();
	}
}
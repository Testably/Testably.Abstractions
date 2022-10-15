namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class FileSystemMockExtensionsTests
{
	#region Test Setup

	public Testing.FileSystemMock FileSystem { get; }

	public FileSystemMockExtensionsTests()
	{
		FileSystem = new Testing.FileSystemMock();
	}

	#endregion

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryCreated_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDirectoryCreated(_ => isNotified = true)
			   .Execute(() =>
				{
					FileSystem.Directory.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryCreated_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDirectoryCreated(_ => isNotified = true, path2)
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path1);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineData("foo", "f*o", true)]
	[InlineData("foo", "*fo", false)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryCreated_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDirectoryCreated(_ => isNotified = true, searchPattern: searchPattern)
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().BeNull();
		}
		else
		{
			exception.Should().BeOfType<TimeoutException>();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryCreated_ShouldNotifyWhenDirectoryIsCreated(string path)
	{
		bool isNotified = false;

		FileSystem.Notify
		   .OnDirectoryCreated(_ => isNotified = true)
		   .Execute(() =>
			{
				FileSystem.Directory.CreateDirectory(path);
			})
		   .Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryCreated_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDirectoryCreated(_ => isNotified = true, predicate: _ => expectedResult)
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().BeNull();
		}
		else
		{
			exception.Should().BeOfType<TimeoutException>();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryDeleted_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDirectoryDeleted(_ => isNotified = true)
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryDeleted_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path1);
		FileSystem.Directory.CreateDirectory(path2);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDirectoryDeleted(_ => isNotified = true, path2)
			   .Execute(() =>
				{
					FileSystem.Directory.Delete(path1);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineData("foo", "f*o", true)]
	[InlineData("foo", "*fo", false)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryDeleted_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDirectoryDeleted(_ => isNotified = true, searchPattern: searchPattern)
			   .Execute(() =>
				{
					FileSystem.Directory.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().BeNull();
		}
		else
		{
			exception.Should().BeOfType<TimeoutException>();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryDeleted_ShouldNotifyWhenDirectoryIsDeleted(string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Notify
		   .OnDirectoryDeleted(_ => isNotified = true)
		   .Execute(() =>
			{
				FileSystem.Directory.Delete(path);
			})
		   .Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDirectoryDeleted_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDirectoryDeleted(_ => isNotified = true, predicate: _ => expectedResult)
			   .Execute(() =>
				{
					FileSystem.Directory.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().BeNull();
		}
		else
		{
			exception.Should().BeOfType<TimeoutException>();
		}

		isNotified.Should().Be(expectedResult);
	}
}
namespace Testably.Abstractions.Testing.Tests;

public class NotificationHandlerExtensionsTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Theory]
	[AutoData]
	public void OnChanged_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnChanged(FileSystemTypes.File, _ => isNotified = true)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.WriteAllText(path, null);
				})
				.Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void OnChanged_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path1, null);
		FileSystem.File.WriteAllText(path2, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnChanged(FileSystemTypes.File, _ => isNotified = true, path2)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.AppendAllText(path1, "foo");
				})
				.Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public void OnChanged_File_ShouldConsiderGlobPattern(
		string directoryPath, string fileName, string globPattern, bool expectedResult)
	{
		bool isNotified = false;
		string filePath = FileSystem.Path.Combine(directoryPath, fileName);
		FileSystem.Directory.CreateDirectory(directoryPath);
		FileSystem.File.WriteAllText(filePath, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnChanged(FileSystemTypes.File, _ => isNotified = true, globPattern)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.AppendAllText(filePath, "foo");
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnChanged_File_ShouldNotifyWhenFileIsChanged(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		FileSystem.Notify
			.OnChanged(FileSystemTypes.File, _ => isNotified = true)
			.ExecuteWhileWaiting(() =>
			{
				FileSystem.File.AppendAllText(path, "foo");
			})
			.Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public void OnChanged_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnChanged(FileSystemTypes.File, _ => isNotified = true,
					predicate: _ => expectedResult)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.AppendAllText(path, "foo");
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnCreated_Directory_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnCreated(FileSystemTypes.Directory, _ => isNotified = true)
				.ExecuteWhileWaiting(() =>
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
	public void OnCreated_Directory_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnCreated(FileSystemTypes.Directory, _ => isNotified = true, path2)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.Directory.CreateDirectory(path1);
				})
				.Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public void OnCreated_Directory_ShouldConsiderGlobPattern(
		string directoryPath, string fileName, string globPattern, bool expectedResult)
	{
		string filePath = FileSystem.Path.Combine(directoryPath, fileName);
		FileSystem.Directory.CreateDirectory(directoryPath);
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnCreated(FileSystemTypes.Directory, _ => isNotified = true,
					globPattern)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.Directory.CreateDirectory(filePath);
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnCreated_Directory_ShouldNotifyWhenDirectoryIsCreated(string path)
	{
		bool isNotified = false;

		FileSystem.Notify
			.OnCreated(FileSystemTypes.Directory, _ => isNotified = true)
			.ExecuteWhileWaiting(() =>
			{
				FileSystem.Directory.CreateDirectory(path);
			})
			.Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public void OnCreated_Directory_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnCreated(FileSystemTypes.Directory, _ => isNotified = true,
					predicate: _ => expectedResult)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnCreated_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnCreated(FileSystemTypes.File, _ => isNotified = true)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.Delete(path);
				})
				.Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void OnCreated_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnCreated(FileSystemTypes.File, _ => isNotified = true, path2)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.WriteAllText(path1, null);
				})
				.Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public void OnCreated_File_ShouldConsiderGlobPattern(
		string directoryPath, string fileName, string globPattern, bool expectedResult)
	{
		string filePath = FileSystem.Path.Combine(directoryPath, fileName);
		FileSystem.Directory.CreateDirectory(directoryPath);
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnCreated(FileSystemTypes.File, _ => isNotified = true,
					globPattern)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.WriteAllText(filePath, null);
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnCreated_File_ShouldNotifyWhenFileIsCreated(string path)
	{
		bool isNotified = false;

		FileSystem.Notify
			.OnCreated(FileSystemTypes.File, _ => isNotified = true)
			.ExecuteWhileWaiting(() =>
			{
				FileSystem.File.WriteAllText(path, null);
			})
			.Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public void OnCreated_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnCreated(FileSystemTypes.File, _ => isNotified = true,
					predicate: _ => expectedResult)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.WriteAllText(path, null);
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnDeleted_Directory_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true)
				.ExecuteWhileWaiting(() =>
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
	public void OnDeleted_Directory_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path1);
		FileSystem.Directory.CreateDirectory(path2);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true, path2)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.Directory.Delete(path1);
				})
				.Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public void OnDeleted_Directory_ShouldConsiderGlobPattern(
		string basePath, string directoryName, string globPattern, bool expectedResult)
	{
		bool isNotified = false;
		string directoryPath = FileSystem.Path.Combine(basePath, directoryName);
		FileSystem.Directory.CreateDirectory(basePath);
		FileSystem.Directory.CreateDirectory(directoryPath);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true,
					globPattern)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.Directory.Delete(directoryPath);
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnDeleted_Directory_ShouldNotifyWhenDirectoryIsDeleted(string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Notify
			.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true)
			.ExecuteWhileWaiting(() =>
			{
				FileSystem.Directory.Delete(path);
			})
			.Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public void OnDeleted_Directory_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true,
					predicate: _ => expectedResult)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.Directory.Delete(path);
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnDeleted_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnDeleted(FileSystemTypes.File, _ => isNotified = true)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.WriteAllText(path, null);
				})
				.Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void OnDeleted_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path1, null);
		FileSystem.File.WriteAllText(path2, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnDeleted(FileSystemTypes.File, _ => isNotified = true, path2)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.Delete(path1);
				})
				.Wait(timeout: 50);
		});

		exception.Should().BeOfType<TimeoutException>();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public void OnDeleted_File_ShouldConsiderGlobPattern(
		string directoryPath, string fileName, string globPattern, bool expectedResult)
	{
		bool isNotified = false;
		string filePath = FileSystem.Path.Combine(directoryPath, fileName);
		FileSystem.Directory.CreateDirectory(directoryPath);
		FileSystem.File.WriteAllText(filePath, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnDeleted(FileSystemTypes.File, _ => isNotified = true,
					globPattern)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.Delete(filePath);
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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
	public void OnDeleted_File_ShouldNotifyWhenFileIsDeleted(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		FileSystem.Notify
			.OnDeleted(FileSystemTypes.File, _ => isNotified = true)
			.ExecuteWhileWaiting(() =>
			{
				FileSystem.File.Delete(path);
			})
			.Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public void OnDeleted_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
				.OnDeleted(FileSystemTypes.File, _ => isNotified = true,
					predicate: _ => expectedResult)
				.ExecuteWhileWaiting(() =>
				{
					FileSystem.File.Delete(path);
				})
				.Wait(timeout: expectedResult ? 30000 : 50);
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

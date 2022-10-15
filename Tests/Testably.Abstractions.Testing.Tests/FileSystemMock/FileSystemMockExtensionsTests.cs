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
	public void OnCreated_Directory_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnCreated(FileSystemTypes.Directory, _ => isNotified = true)
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
	public void OnCreated_Directory_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnCreated(FileSystemTypes.Directory, _ => isNotified = true, path2)
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
	public void OnCreated_Directory_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnCreated(FileSystemTypes.Directory, _ => isNotified = true,
					searchPattern: searchPattern)
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
	public void OnCreated_Directory_ShouldNotifyWhenDirectoryIsCreated(string path)
	{
		bool isNotified = false;

		FileSystem.Notify
		   .OnCreated(FileSystemTypes.Directory, _ => isNotified = true)
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
	public void OnCreated_Directory_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnCreated(FileSystemTypes.Directory, _ => isNotified = true,
					predicate: _ => expectedResult)
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
	public void OnCreated_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnCreated(FileSystemTypes.File, _ => isNotified = true)
			   .Execute(() =>
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
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnCreated_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnCreated(FileSystemTypes.File, _ => isNotified = true, path2)
			   .Execute(() =>
				{
					FileSystem.File.WriteAllText(path1, null);
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
	public void OnCreated_File_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnCreated(FileSystemTypes.File, _ => isNotified = true,
					searchPattern: searchPattern)
			   .Execute(() =>
				{
					FileSystem.File.WriteAllText(path, null);
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
	public void OnCreated_File_ShouldNotifyWhenFileIsCreated(string path)
	{
		bool isNotified = false;

		FileSystem.Notify
		   .OnCreated(FileSystemTypes.File, _ => isNotified = true)
		   .Execute(() =>
			{
				FileSystem.File.WriteAllText(path, null);
			})
		   .Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnCreated_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnCreated(FileSystemTypes.File, _ => isNotified = true,
					predicate: _ => expectedResult)
			   .Execute(() =>
				{
					FileSystem.File.WriteAllText(path, null);
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
	public void OnDeleted_Directory_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDeleted(FileSystemTypes.Directory, _ => isNotified = true)
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
	public void OnDeleted_Directory_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path1);
		FileSystem.Directory.CreateDirectory(path2);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDeleted(FileSystemTypes.Directory, _ => isNotified = true, path2)
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
	public void OnDeleted_Directory_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDeleted(FileSystemTypes.Directory, _ => isNotified = true,
					searchPattern: searchPattern)
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
	public void OnDeleted_Directory_ShouldNotifyWhenDirectoryIsDeleted(string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Notify
		   .OnDeleted(FileSystemTypes.Directory, _ => isNotified = true)
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
	public void OnDeleted_Directory_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDeleted(FileSystemTypes.Directory, _ => isNotified = true,
					predicate: _ => expectedResult)
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
	public void OnDeleted_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDeleted(FileSystemTypes.File, _ => isNotified = true)
			   .Execute(() =>
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
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDeleted_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path1, null);
		FileSystem.File.WriteAllText(path2, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDeleted(FileSystemTypes.File, _ => isNotified = true, path2)
			   .Execute(() =>
			   {
				   FileSystem.File.Delete(path1);
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
	public void OnDeleted_File_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDeleted(FileSystemTypes.File, _ => isNotified = true,
					searchPattern: searchPattern)
			   .Execute(() =>
			   {
				   FileSystem.File.Delete(path);
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
	public void OnDeleted_File_ShouldNotifyWhenFileIsDeleted(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		FileSystem.Notify
		   .OnDeleted(FileSystemTypes.File, _ => isNotified = true)
		   .Execute(() =>
		   {
			   FileSystem.File.Delete(path);
		   })
		   .Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnDeleted_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnDeleted(FileSystemTypes.File, _ => isNotified = true,
					predicate: _ => expectedResult)
			   .Execute(() =>
			   {
				   FileSystem.File.Delete(path);
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
	public void OnChanged_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnChanged(FileSystemTypes.File, _ => isNotified = true)
			   .Execute(() =>
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
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnChanged_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path1, null);
		FileSystem.File.WriteAllText(path2, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnChanged(FileSystemTypes.File, _ => isNotified = true, path2)
			   .Execute(() =>
			   {
				   FileSystem.File.AppendAllText(path1, "foo");
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
	public void OnChanged_File_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnChanged(FileSystemTypes.File, _ => isNotified = true,
					searchPattern: searchPattern)
			   .Execute(() =>
			   {
				   FileSystem.File.AppendAllText(path, "foo");
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
	public void OnChanged_File_ShouldNotifyWhenFileIsChanged(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		FileSystem.Notify
		   .OnChanged(FileSystemTypes.File, _ => isNotified = true)
		   .Execute(() =>
		   {
			   FileSystem.File.AppendAllText(path, "foo");
		   })
		   .Wait();

		isNotified.Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void OnChanged_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnChanged(FileSystemTypes.File, _ => isNotified = true,
					predicate: _ => expectedResult)
			   .Execute(() =>
			   {
				   FileSystem.File.AppendAllText(path, "foo");
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
namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class InterceptionHandlerExtensionsTests
{
	#region Test Setup

	public Testing.FileSystemMock FileSystem { get; }

	public InterceptionHandlerExtensionsTests()
	{
		FileSystem = new Testing.FileSystemMock();
	}

	#endregion

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Changing_File_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);
		FileSystem.Intercept.Changing(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Changing_File_ShouldConsiderBasePath(string path1, string path2,
	                                                 Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path1, null);
		FileSystem.Intercept.Changing(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.Delete(path1);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Changing_File_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);
		FileSystem.Intercept.Changing(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path, searchPattern: searchPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.AppendAllText(path, "foo");
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Changing_File_ShouldUsePredicate(bool expectedResult, string path,
	                                             Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);
		FileSystem.Intercept.Changing(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, predicate: _ => expectedResult);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.AppendAllText(path, "foo");
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Creating_Directory_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);
		FileSystem.Intercept.Creating(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.Directory.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Creating_Directory_ShouldConsiderBasePath(
		string path1, string path2, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path1);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Creating_Directory_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path, searchPattern: searchPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Creating_Directory_ShouldUsePredicate(bool expectedResult, string path,
	                                                  Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, predicate: _ => expectedResult);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Creating_File_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);
		FileSystem.Intercept.Creating(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Creating_File_ShouldConsiderBasePath(string path1, string path2,
	                                                 Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.WriteAllText(path1, null);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Creating_File_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path, searchPattern: searchPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.WriteAllText(path, null);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Creating_File_ShouldUsePredicate(bool expectedResult, string path,
	                                             Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, predicate: _ => expectedResult);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.WriteAllText(path, null);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Deleting_Directory_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Deleting(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.Directory.CreateDirectory(path);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Deleting_Directory_ShouldConsiderBasePath(
		string path1, string path2, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path1);
		FileSystem.Intercept.Deleting(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.Directory.Delete(path1);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Deleting_Directory_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);
		FileSystem.Intercept.Deleting(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path, searchPattern: searchPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.Directory.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Deleting_Directory_ShouldUsePredicate(bool expectedResult, string path,
	                                                  Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);
		FileSystem.Intercept.Deleting(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, predicate: _ => expectedResult);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.Directory.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Deleting_File_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Deleting(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.WriteAllText(path, null);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Deleting_File_ShouldConsiderBasePath(string path1, string path2,
	                                                 Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path1, null);
		FileSystem.Intercept.Deleting(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.Delete(path1);
				})
			   .Wait(timeout: 50);
		});

		exception.Should().BeNull();
		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Deleting_File_ShouldConsiderSearchPattern(
		string path, string searchPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);
		FileSystem.Intercept.Deleting(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path, searchPattern: searchPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	[Trait(nameof(Testing), nameof(FileSystemMockExtensions))]
	public void Deleting_File_ShouldUsePredicate(bool expectedResult, string path,
	                                             Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);
		FileSystem.Intercept.Deleting(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, predicate: _ => expectedResult);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Notify
			   .OnEvent()
			   .Execute(() =>
				{
					FileSystem.File.Delete(path);
				})
			   .Wait(timeout: 50);
		});

		if (expectedResult)
		{
			exception.Should().Be(exceptionToThrow);
		}
		else
		{
			exception.Should().BeNull();
		}

		isNotified.Should().Be(expectedResult);
	}
}
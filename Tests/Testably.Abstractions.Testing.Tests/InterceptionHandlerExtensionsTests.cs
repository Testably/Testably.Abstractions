using System.Threading;

namespace Testably.Abstractions.Testing.Tests;

[Collection(nameof(ThreadPool))]
public class InterceptionHandlerExtensionsTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; }

	public InterceptionHandlerExtensionsTests()
	{
		FileSystem = new MockFileSystem();
	}

	#endregion

	[Theory]
	[AutoData]
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

		FileSystem.File.Delete(path);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
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

		FileSystem.File.Delete(path1);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
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
			FileSystem.File.AppendAllText(path, "foo");
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
			FileSystem.File.AppendAllText(path, "foo");
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

		FileSystem.Directory.Delete(path);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Creating_Directory_ShouldConsiderBasePath(
		string path1, string path2, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		FileSystem.Directory.CreateDirectory(path1);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
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
			FileSystem.Directory.CreateDirectory(path);
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
			FileSystem.Directory.CreateDirectory(path);
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

		FileSystem.File.Delete(path);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Creating_File_ShouldConsiderBasePath(string path1, string path2,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		FileSystem.File.WriteAllText(path1, null);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
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
			FileSystem.File.WriteAllText(path, null);
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
			FileSystem.File.WriteAllText(path, null);
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
	public void Deleting_Directory_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Deleting(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		FileSystem.Directory.CreateDirectory(path);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
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

		FileSystem.Directory.Delete(path1);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
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
			FileSystem.Directory.Delete(path);
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
			FileSystem.Directory.Delete(path);
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
	public void Deleting_File_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Deleting(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		FileSystem.File.WriteAllText(path, null);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[AutoData]
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

		FileSystem.File.Delete(path1);

		isNotified.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("foo", "f*o", true)]
	[InlineAutoData("foo", "*fo", false)]
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
			FileSystem.File.Delete(path);
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
			FileSystem.File.Delete(path);
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

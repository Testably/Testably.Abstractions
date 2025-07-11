namespace Testably.Abstractions.Testing.Tests;

public class InterceptionHandlerExtensionsTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Theory]
	[AutoData]
	public async Task Changing_File_OtherEvent_ShouldNotTrigger(
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

		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Changing_File_ShouldConsiderBasePath(string path1, string path2,
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

		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineAutoData(".", "foo", "f*o", true)]
	[InlineAutoData(".", "foo", "*fo", false)]
	[InlineAutoData("bar", "foo", "f*o", true)]
	[InlineAutoData("bar", "foo", "baz/f*o", false)]
	[InlineAutoData("bar", "foo", "/f*o", false)]
	[InlineAutoData("bar", "foo", "**/f*o", true)]
	public async Task Changing_File_ShouldConsiderGlobPattern(
		string basePath, string fileName, string globPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		string filePath = FileSystem.Path.Combine(basePath, fileName);
		FileSystem.Directory.CreateDirectory(basePath);
		FileSystem.File.WriteAllText(filePath, null);
		FileSystem.Intercept.Changing(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, globPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllText(filePath, "foo");
		});

		if (expectedResult)
		{
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task Changing_File_ShouldUsePredicate(bool expectedResult, string path,
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
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Creating_Directory_OtherEvent_ShouldNotTrigger(
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

		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Creating_Directory_ShouldConsiderBasePath(
		string path1, string path2, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		FileSystem.Directory.CreateDirectory(path1);

		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineAutoData(".", "foo", "f*o", true)]
	[InlineAutoData(".", "foo", "*fo", false)]
	[InlineAutoData("bar", "foo", "f*o", true)]
	[InlineAutoData("bar", "foo", "baz/f*o", false)]
	[InlineAutoData("bar", "foo", "/f*o", false)]
	[InlineAutoData("bar", "foo", "**/f*o", true)]
	public async Task Creating_Directory_ShouldConsiderGlobPattern(
		string basePath, string fileName, string globPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		string filePath = FileSystem.Path.Combine(basePath, fileName);
		FileSystem.Directory.CreateDirectory(basePath);

		FileSystem.Intercept.Creating(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, globPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(filePath);
		});

		if (expectedResult)
		{
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task Creating_Directory_ShouldUsePredicate(bool expectedResult, string path,
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
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Creating_File_OtherEvent_ShouldNotTrigger(
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

		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Creating_File_ShouldConsiderBasePath(string path1, string path2,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Creating(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, path2);

		FileSystem.File.WriteAllText(path1, null);

		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineAutoData(".", "foo", "f*o", true)]
	[InlineAutoData(".", "foo", "*fo", false)]
	[InlineAutoData("bar", "foo", "f*o", true)]
	[InlineAutoData("bar", "foo", "baz/f*o", false)]
	[InlineAutoData("bar", "foo", "/f*o", false)]
	[InlineAutoData("bar", "foo", "**/f*o", true)]
	public async Task Creating_File_ShouldConsiderGlobPattern(
		string basePath, string fileName, string globPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		string filePath = FileSystem.Path.Combine(basePath, fileName);
		FileSystem.Directory.CreateDirectory(basePath);

		FileSystem.Intercept.Creating(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, globPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllText(filePath, null);
		});

		if (expectedResult)
		{
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task Creating_File_ShouldUsePredicate(bool expectedResult, string path,
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
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Deleting_Directory_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Deleting(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		FileSystem.Directory.CreateDirectory(path);

		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Deleting_Directory_ShouldConsiderBasePath(
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

		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineAutoData(".", "foo", "f*o", true)]
	[InlineAutoData(".", "foo", "*fo", false)]
	[InlineAutoData("bar", "foo", "f*o", true)]
	[InlineAutoData("bar", "foo", "baz/f*o", false)]
	[InlineAutoData("bar", "foo", "/f*o", false)]
	[InlineAutoData("bar", "foo", "**/f*o", true)]
	public async Task Deleting_Directory_ShouldConsiderGlobPattern(
		string basePath, string directoryName, string globPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		string directoryPath = FileSystem.Path.Combine(basePath, directoryName);
		FileSystem.Directory.CreateDirectory(basePath);
		FileSystem.Directory.CreateDirectory(directoryPath);
		FileSystem.Intercept.Deleting(FileSystemTypes.Directory, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, globPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(directoryPath);
		});

		if (expectedResult)
		{
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task Deleting_Directory_ShouldUsePredicate(bool expectedResult, string path,
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
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Deleting_File_OtherEvent_ShouldNotTrigger(
		string path, Exception exceptionToThrow)
	{
		bool isNotified = false;
		FileSystem.Intercept.Deleting(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		});

		FileSystem.File.WriteAllText(path, null);

		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Deleting_File_ShouldConsiderBasePath(string path1, string path2,
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

		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineAutoData(".", "foo", "f*o", true)]
	[InlineAutoData(".", "foo", "*fo", false)]
	[InlineAutoData("bar", "foo", "f*o", true)]
	[InlineAutoData("bar", "foo", "baz/f*o", false)]
	[InlineAutoData("bar", "foo", "/f*o", false)]
	[InlineAutoData("bar", "foo", "**/f*o", true)]
	public async Task Deleting_File_ShouldConsiderGlobPattern(
		string basePath, string fileName, string globPattern, bool expectedResult,
		Exception exceptionToThrow)
	{
		bool isNotified = false;
		string filePath = FileSystem.Path.Combine(basePath, fileName);
		FileSystem.Directory.CreateDirectory(basePath);
		FileSystem.File.WriteAllText(filePath, null);
		FileSystem.Intercept.Deleting(FileSystemTypes.File, _ =>
		{
			isNotified = true;
			throw exceptionToThrow;
		}, globPattern);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(filePath);
		});

		if (expectedResult)
		{
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task Deleting_File_ShouldUsePredicate(bool expectedResult, string path,
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
			await That(exception).IsEqualTo(exceptionToThrow);
		}
		else
		{
			await That(exception).IsNull();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}
}

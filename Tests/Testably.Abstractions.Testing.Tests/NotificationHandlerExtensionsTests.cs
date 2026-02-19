using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests;

public class NotificationHandlerExtensionsTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Theory]
	[AutoData]
	public async Task OnChanged_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onChanged =
			FileSystem.Notify
				.OnChanged(FileSystemTypes.File, _ => isNotified = true);

		FileSystem.File.WriteAllText(path, null);
		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onChanged.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task OnChanged_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path1, null);
		FileSystem.File.WriteAllText(path2, null);

		using IAwaitableCallback<ChangeDescription> onChanged = FileSystem.Notify
			.OnChanged(FileSystemTypes.File, _ => isNotified = true, path2);
		FileSystem.File.AppendAllText(path1, "foo");
		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onChanged.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public async Task OnChanged_File_ShouldConsiderGlobPattern(
		string directoryPath, string fileName, string globPattern, bool expectedResult)
	{
		bool isNotified = false;
		string filePath = FileSystem.Path.Combine(directoryPath, fileName);
		FileSystem.Directory.CreateDirectory(directoryPath);
		FileSystem.File.WriteAllText(filePath, null);
		using IAwaitableCallback<ChangeDescription> onChanged = FileSystem.Notify
			.OnChanged(FileSystemTypes.File, _ => isNotified = true, globPattern);
		FileSystem.File.AppendAllText(filePath, "foo");

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onChanged.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnChanged_File_ShouldNotifyWhenFileIsChanged(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		using IAwaitableCallback<ChangeDescription> onChanged = FileSystem.Notify
			.OnChanged(FileSystemTypes.File, _ => isNotified = true);

		FileSystem.File.AppendAllText(path, "foo");

		onChanged.Wait();

		await That(isNotified).IsTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task OnChanged_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		using IAwaitableCallback<ChangeDescription> onChanged = FileSystem.Notify
			.OnChanged(FileSystemTypes.File, _ => isNotified = true,
				predicate: _ => expectedResult);
		FileSystem.File.AppendAllText(path, "foo");

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onChanged.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnCreated_Directory_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.Directory, _ => isNotified = true);
		FileSystem.Directory.Delete(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onCreated.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task OnCreated_Directory_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.Directory, _ => isNotified = true, path2);
		FileSystem.Directory.CreateDirectory(path1);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onCreated.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public async Task OnCreated_Directory_ShouldConsiderGlobPattern(
		string directoryPath, string fileName, string globPattern, bool expectedResult)
	{
		string filePath = FileSystem.Path.Combine(directoryPath, fileName);
		FileSystem.Directory.CreateDirectory(directoryPath);
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.Directory, _ => isNotified = true,
				globPattern);
		FileSystem.Directory.CreateDirectory(filePath);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onCreated.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnCreated_Directory_ShouldNotifyWhenDirectoryIsCreated(string path)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.Directory, _ => isNotified = true);
		FileSystem.Directory.CreateDirectory(path);

		onCreated.Wait();

		await That(isNotified).IsTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task OnCreated_Directory_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.Directory, _ => isNotified = true,
				predicate: _ => expectedResult);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onCreated.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnCreated_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.File, _ => isNotified = true);
		FileSystem.File.Delete(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onCreated.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task OnCreated_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.File, _ => isNotified = true, path2);
		FileSystem.File.WriteAllText(path1, null);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onCreated.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public async Task OnCreated_File_ShouldConsiderGlobPattern(
		string directoryPath, string fileName, string globPattern, bool expectedResult)
	{
		string filePath = FileSystem.Path.Combine(directoryPath, fileName);
		FileSystem.Directory.CreateDirectory(directoryPath);
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.File, _ => isNotified = true,
				globPattern);
		FileSystem.File.WriteAllText(filePath, null);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onCreated.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnCreated_File_ShouldNotifyWhenFileIsCreated(string path)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.File, _ => isNotified = true);
		FileSystem.File.WriteAllText(path, null);

		onCreated.Wait();

		await That(isNotified).IsTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task OnCreated_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onCreated = FileSystem.Notify
			.OnCreated(FileSystemTypes.File, _ => isNotified = true,
				predicate: _ => expectedResult);
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onCreated.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnDeleted_Directory_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onDeleted.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task OnDeleted_Directory_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path1);
		FileSystem.Directory.CreateDirectory(path2);

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true, path2);
		FileSystem.Directory.Delete(path1);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onDeleted.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public async Task OnDeleted_Directory_ShouldConsiderGlobPattern(
		string basePath, string directoryName, string globPattern, bool expectedResult)
	{
		bool isNotified = false;
		string directoryPath = FileSystem.Path.Combine(basePath, directoryName);
		FileSystem.Directory.CreateDirectory(basePath);
		FileSystem.Directory.CreateDirectory(directoryPath);

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true,
				globPattern);
		FileSystem.Directory.Delete(directoryPath);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onDeleted.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnDeleted_Directory_ShouldNotifyWhenDirectoryIsDeleted(string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true);
		FileSystem.Directory.Delete(path);

		onDeleted.Wait();

		await That(isNotified).IsTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task OnDeleted_Directory_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.Directory.CreateDirectory(path);

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.Directory, _ => isNotified = true,
				predicate: _ => expectedResult);
		FileSystem.Directory.Delete(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onDeleted.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnDeleted_File_OtherEvent_ShouldNotTrigger(string path)
	{
		bool isNotified = false;

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.File, _ => isNotified = true);
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onDeleted.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task OnDeleted_File_ShouldConsiderBasePath(string path1, string path2)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path1, null);
		FileSystem.File.WriteAllText(path2, null);

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.File, _ => isNotified = true, path2);
		FileSystem.File.Delete(path1);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onDeleted.Wait(timeout: 50);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isNotified).IsFalse();
	}

	[Theory]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
	public async Task OnDeleted_File_ShouldConsiderGlobPattern(
		string directoryPath, string fileName, string globPattern, bool expectedResult)
	{
		bool isNotified = false;
		string filePath = FileSystem.Path.Combine(directoryPath, fileName);
		FileSystem.Directory.CreateDirectory(directoryPath);
		FileSystem.File.WriteAllText(filePath, null);

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.File, _ => isNotified = true,
				globPattern);
		FileSystem.File.Delete(filePath);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onDeleted.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task OnDeleted_File_ShouldNotifyWhenFileIsDeleted(string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.File, _ => isNotified = true);
		FileSystem.File.Delete(path);

		onDeleted.Wait();

		await That(isNotified).IsTrue();
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task OnDeleted_File_ShouldUsePredicate(bool expectedResult, string path)
	{
		bool isNotified = false;
		FileSystem.File.WriteAllText(path, null);

		using IAwaitableCallback<ChangeDescription> onDeleted = FileSystem.Notify
			.OnDeleted(FileSystemTypes.File, _ => isNotified = true,
				predicate: _ => expectedResult);
		FileSystem.File.Delete(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			onDeleted.Wait(timeout: expectedResult ? 30000 : 50);
		});

		if (expectedResult)
		{
			await That(exception).IsNull();
		}
		else
		{
			await That(exception).IsExactly<TimeoutException>();
		}

		await That(isNotified).IsEqualTo(expectedResult);
	}
}

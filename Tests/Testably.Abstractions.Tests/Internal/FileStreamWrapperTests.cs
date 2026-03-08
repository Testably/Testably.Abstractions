using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.Internal;

[NotInParallel(nameof(RealFileSystem))]
public sealed class FileStreamWrapperTests : IDisposable
{
	#region Test Setup

	public RealFileSystem FileSystem { get; }

	private readonly IDirectoryCleaner _directoryCleaner;

	public FileStreamWrapperTests()
	{
		FileSystem = new RealFileSystem();
		_directoryCleaner = FileSystem
			.SetCurrentDirectoryToEmptyTemporaryDirectory(
				$"Testably.Abstractions.Tests.Internal{FileSystem.Path.DirectorySeparatorChar}FileStreamWrapperTests-",
				Console.WriteLine);
	}

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[Test]
	[Arguments("my-path", "my-key", "my-value")]
	public async Task RetrieveMetadata_ShouldReturnStoredValue(string path, string key,
		object value)
	{
		using FileSystemStream sut = FileSystem.FileStream.New(path, FileMode.OpenOrCreate);

		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		extensibility!.StoreMetadata(key, value);

		object? result = extensibility.RetrieveMetadata<object>(key);

		await That(result).IsEqualTo(value);
	}

	[Test]
	[Arguments("my-path")]
	public async Task TryGetWrappedInstance_ShouldReturnWrappedInstance(string path)
	{
		using FileSystemStream sut = FileSystem.FileStream.New(path, FileMode.OpenOrCreate);

		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		bool result = extensibility!.TryGetWrappedInstance(out FileStream? fileStream);

		await That(result).IsTrue();
		await That(fileStream).IsNotNull();
	}
}

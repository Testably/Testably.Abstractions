using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.Internal;

[Collection("RealFileSystemTests")]
public sealed class FileStreamWrapperTests : IDisposable
{
	#region Test Setup

	public RealFileSystem FileSystem { get; }

	private readonly IDirectoryCleaner _directoryCleaner;

	public FileStreamWrapperTests(ITestOutputHelper testOutputHelper)
	{
		FileSystem = new RealFileSystem();
		_directoryCleaner = FileSystem
			.SetCurrentDirectoryToEmptyTemporaryDirectory(
				$"Testably.Abstractions.Tests.Internal{FileSystem.Path.DirectorySeparatorChar}FileStreamWrapperTests-",
				testOutputHelper.WriteLine);
	}

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[Theory]
	[AutoData]
	public async Task RetrieveMetadata_ShouldReturnStoredValue(string path, string key,
		object value)
	{
		using FileSystemStream sut = FileSystem.FileStream.New(path, FileMode.OpenOrCreate);

		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		extensibility!.StoreMetadata(key, value);

		object? result = extensibility.RetrieveMetadata<object>(key);

		await That(result).IsEqualTo(value);
	}

	[Theory]
	[AutoData]
	public async Task TryGetWrappedInstance_ShouldReturnWrappedInstance(string path)
	{
		using FileSystemStream sut = FileSystem.FileStream.New(path, FileMode.OpenOrCreate);

		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		bool result = extensibility!.TryGetWrappedInstance(out FileStream? fileStream);

		await That(result).IsTrue();
		await That(fileStream).IsNotNull();
	}
}

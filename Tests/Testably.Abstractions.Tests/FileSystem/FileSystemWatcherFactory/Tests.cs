using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcherFactory;

[FileSystemTests]
public partial class Tests
{
	[Fact]
	public async Task New_ShouldInitializeWithDefaultValues()
	{
		using IFileSystemWatcher result =
			FileSystem.FileSystemWatcher.New();

		await That(result.Path).IsEqualTo("");
#if NETFRAMEWORK
		await That(result.Filter).IsEqualTo("*.*");
#else
		await That(result.Filter).IsEqualTo("*");
#endif
		await That(result.IncludeSubdirectories).IsFalse();
		await That(result.InternalBufferSize).IsEqualTo(8192);
		await That(result.NotifyFilter).IsEqualTo(NotifyFilters.FileName |
										NotifyFilters.DirectoryName |
										NotifyFilters.LastWrite);
		await That(result.EnableRaisingEvents).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task New_WithPath_ShouldInitializeWithDefaultValues(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		using IFileSystemWatcher result =
			FileSystem.FileSystemWatcher.New(path);

		await That(result.Path).IsEqualTo(path);
#if NETFRAMEWORK
		await That(result.Filter).IsEqualTo("*.*");
#else
		await That(result.Filter).IsEqualTo("*");
#endif
		await That(result.IncludeSubdirectories).IsFalse();
		await That(result.InternalBufferSize).IsEqualTo(8192);
		await That(result.NotifyFilter).IsEqualTo(NotifyFilters.FileName |
										NotifyFilters.DirectoryName |
										NotifyFilters.LastWrite);
		await That(result.EnableRaisingEvents).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task New_WithPathAndFilter_ShouldInitializeWithDefaultValues(
		string path, string filter)
	{
		FileSystem.Directory.CreateDirectory(path);
		using IFileSystemWatcher result =
			FileSystem.FileSystemWatcher.New(path, filter);

		await That(result.Path).IsEqualTo(path);
		await That(result.Filter).IsEqualTo(filter);
		await That(result.IncludeSubdirectories).IsFalse();
		await That(result.InternalBufferSize).IsEqualTo(8192);
		await That(result.NotifyFilter).IsEqualTo(NotifyFilters.FileName |
										NotifyFilters.DirectoryName |
										NotifyFilters.LastWrite);
		await That(result.EnableRaisingEvents).IsFalse();
	}

	[Fact]
	public async Task Wrap_Null_ShouldReturnNull()
	{
		using IFileSystemWatcher? result = FileSystem.FileSystemWatcher.Wrap(null);

		await That(result).IsNull();
	}
}

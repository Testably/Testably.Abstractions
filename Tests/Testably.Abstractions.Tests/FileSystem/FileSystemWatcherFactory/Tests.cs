using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcherFactory;

[FileSystemTests]
public partial class Tests
{
	[Fact]
	public void New_ShouldInitializeWithDefaultValues()
	{
		using IFileSystemWatcher result =
			FileSystem.FileSystemWatcher.New();

		result.Path.Should().Be("");
#if NETFRAMEWORK
		result.Filter.Should().Be("*.*");
#else
		result.Filter.Should().Be("*");
#endif
		result.IncludeSubdirectories.Should().BeFalse();
		result.InternalBufferSize.Should().Be(8192);
		result.NotifyFilter.Should().Be(NotifyFilters.FileName |
		                                NotifyFilters.DirectoryName |
		                                NotifyFilters.LastWrite);
		result.EnableRaisingEvents.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void New_WithPath_ShouldInitializeWithDefaultValues(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		using IFileSystemWatcher result =
			FileSystem.FileSystemWatcher.New(path);

		result.Path.Should().Be(path);
#if NETFRAMEWORK
		result.Filter.Should().Be("*.*");
#else
		result.Filter.Should().Be("*");
#endif
		result.IncludeSubdirectories.Should().BeFalse();
		result.InternalBufferSize.Should().Be(8192);
		result.NotifyFilter.Should().Be(NotifyFilters.FileName |
		                                NotifyFilters.DirectoryName |
		                                NotifyFilters.LastWrite);
		result.EnableRaisingEvents.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void New_WithPathAndFilter_ShouldInitializeWithDefaultValues(
		string path, string filter)
	{
		FileSystem.Directory.CreateDirectory(path);
		using IFileSystemWatcher result =
			FileSystem.FileSystemWatcher.New(path, filter);

		result.Path.Should().Be(path);
		result.Filter.Should().Be(filter);
		result.IncludeSubdirectories.Should().BeFalse();
		result.InternalBufferSize.Should().Be(8192);
		result.NotifyFilter.Should().Be(NotifyFilters.FileName |
		                                NotifyFilters.DirectoryName |
		                                NotifyFilters.LastWrite);
		result.EnableRaisingEvents.Should().BeFalse();
	}

	[Fact]
	public void Wrap_Null_ShouldReturnNull()
	{
		using IFileSystemWatcher? result = FileSystem.FileSystemWatcher.Wrap(null);

		result.Should().BeNull();
	}
}

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed partial class StatisticsTests
{
	[Fact]
	public void FileSystem_Initialize_ShouldNotRegisterStatistics()
	{
		MockFileSystem sut = new();
		sut.Initialize()
			.WithSubdirectory("d0").Initialized(d => d.WithASubdirectory())
			.WithSubdirectories("d1", "d2")
			.WithASubdirectory()
			.WithFile("f0").Which(f => f.HasBytesContent(Encoding.UTF8.GetBytes("bar")))
			.WithAFile().Which(f => f.HasStringContent("foo"));

		sut.Statistics.TotalCount.Should().Be(0);
		sut.Statistics.Directory.Methods.Should().BeEmpty();
		sut.Statistics.File.Methods.Should().BeEmpty();
	}

	[Theory]
	[InlineData(nameof(MockFileSystem.Directory), false,
		typeof(IDirectory), typeof(DirectoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.DirectoryInfo), false,
		typeof(IDirectoryInfoFactory), typeof(DirectoryInfoFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.DirectoryInfo), true,
		typeof(IDirectoryInfo), typeof(DirectoryInfoStatisticsTests))]
	[InlineData(nameof(MockFileSystem.DriveInfo), false,
		typeof(IDriveInfoFactory), typeof(DriveInfoFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.DriveInfo), true,
		typeof(IDriveInfo), typeof(DriveInfoStatisticsTests))]
	[InlineData(nameof(MockFileSystem.File), false,
		typeof(IFile), typeof(FileStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileInfo), false,
		typeof(IFileInfoFactory), typeof(FileInfoFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileInfo), true,
		typeof(IFileInfo), typeof(FileInfoStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileStream), false,
		typeof(IFileStreamFactory), typeof(FileStreamFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileStream), true,
		typeof(FileSystemStream), typeof(FileStreamStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileSystemWatcher), false,
		typeof(IFileSystemWatcherFactory), typeof(FileSystemWatcherFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileSystemWatcher), true,
		typeof(IFileSystemWatcher), typeof(FileSystemWatcherStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileVersionInfo), false,
		typeof(IFileVersionInfoFactory), typeof(FileVersionInfoFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileVersionInfo), true,
		typeof(IFileVersionInfo), typeof(FileVersionInfoStatisticsTests))]
	[InlineData(nameof(MockFileSystem.Path), false,
		typeof(IPath), typeof(FileSystem.PathStatisticsTests))]
	public void ShouldHaveTestedAllFileSystemMethods(string className, bool requireInstance,
		Type mockType, Type testType)
	{
		string result =
			Helper.CheckPropertiesAndMethods(className, requireInstance, mockType, testType);

		result.Should().BeEmpty();
	}

	[Fact]
	public void Statistics_ShouldIncrementCallOrder()
	{
		MockFileSystem sut = new();
		string[] directories = Enumerable.Range(1, 20)
			.Select(i => $"foo-{i}")
			.ToArray();

		foreach (string directory in directories)
		{
			sut.Directory.CreateDirectory(directory);
		}

		for (int i = 0; i < directories.Length; i++)
		{
			sut.Statistics.Directory.Methods[i]
				.Parameters[0].Is(directories[i]).Should().BeTrue();
		}
	}

	[Fact]
	public void Statistics_ShouldKeepCallOrder()
	{
		MockFileSystem sut = new();
		string[] directories = Enumerable.Range(1, 20)
			.Select(i => $"foo-{i}")
			.ToArray();

		foreach (string directory in directories)
		{
			sut.Directory.CreateDirectory(directory);
		}

		for (int i = 0; i < directories.Length; i++)
		{
			sut.Statistics.Directory.Methods
				.OrderBy(x => x.Counter)
				.Skip(i)
				.First()
				.Parameters[0].Is(directories[i]).Should().BeTrue();
		}
	}

	[Fact]
	public async Task Statistics_ShouldSupportParallelCalls()
	{
		int parallelTasks = 100;
		MockFileSystem sut = new();
		string[] directories = Enumerable.Range(1, parallelTasks)
			.Select(i => $"foo-{i}")
			.ToArray();
		Task[] tasks = new Task[parallelTasks];

		for (int i = 0; i < directories.Length; i++)
		{
			int taskId = i;
			tasks[taskId] = Task.Run(() =>
			{
				sut.Directory.CreateDirectory(directories[taskId]);
			}, TestContext.Current.CancellationToken);
		}

		await Task.WhenAll(tasks);

		foreach (string directory in directories)
		{
			sut.Statistics.Directory.Methods
				.Should().ContainSingle(x =>
					x.Name == nameof(Directory.CreateDirectory) &&
					x.Parameters.Length == 1 &&
					x.Parameters[0].Is(directory));
		}

		sut.Statistics.Directory.Methods.Select(x => x.Counter).Should()
			.BeEquivalentTo(Enumerable.Range(1, directories.Length));
	}

	[Fact]
	public void Statistics_ShouldUseGlobalIncrement()
	{
		MockFileSystem sut = new();
		sut.Directory.CreateDirectory("foo");
		sut.File.WriteAllText("bar.txt", null);
		IFileInfo fileInfo = sut.FileInfo.New("bar.txt");
		using FileSystemStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read);
		_ = new StreamReader(stream).ReadToEnd();

		sut.Statistics.Directory.Methods[0]
			.Should().Match<MethodStatistic>(m =>
				m.Name == nameof(IDirectory.CreateDirectory) &&
				m.Counter == 1);
		sut.Statistics.File.Methods[0]
			.Should().Match<MethodStatistic>(m =>
				m.Name == nameof(IFile.WriteAllText) &&
				m.Counter == 2);
		sut.Statistics.FileInfo.Methods[0]
			.Should().Match<MethodStatistic>(m =>
				m.Name == nameof(IFileInfoFactory.New) &&
				m.Counter == 3);
		sut.Statistics.FileInfo["bar.txt"].Methods[0]
			.Should().Match<MethodStatistic>(m =>
				m.Name == nameof(IFileInfo.Open) &&
				// Note: Index 4 could be used internally for creating the full path of the file info.
				m.Counter >= 4);
	}
}

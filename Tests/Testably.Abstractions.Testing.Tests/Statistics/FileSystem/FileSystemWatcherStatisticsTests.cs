using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;
// ReSharper disable MethodSupportsCancellation

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileSystemWatcherStatisticsTests
{
	[Fact]
	public void Method_BeginInit_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");

		fileSystemWatcher.BeginInit();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.BeginInit));
	}

	[Fact]
	public void Method_EndInit_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");

		fileSystemWatcher.EndInit();

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.EndInit));
	}

	[Fact]
	public void Method_WaitForChanged_WatcherChangeTypes_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
		CancellationToken token = cts.Token;
		_ = Task.Run(async () =>
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(10, token);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;
		fileSystemWatcher.EnableRaisingEvents = true;
		int timeout = 42;

		fileSystemWatcher.WaitForChanged(changeType, timeout);
		cts.Cancel();

		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType,
				timeout);
	}

	[Fact]
	public void Method_WaitForChanged_WatcherChangeTypes_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		using CancellationTokenSource cts = new();
		CancellationToken token = cts.Token;
		_ = Task.Run(async () =>
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(10, token);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;
		fileSystemWatcher.EnableRaisingEvents = true;

		fileSystemWatcher.WaitForChanged(changeType);
		cts.Cancel();

		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType);
	}

#if FEATURE_FILESYSTEM_NET7
	[Fact]
	public void Method_WaitForChanged_WatcherChangeTypes_TimeSpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
		CancellationToken token = cts.Token;
		_ = Task.Run(async () =>
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(10, token);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;
		fileSystemWatcher.EnableRaisingEvents = true;
		TimeSpan timeout = TimeSpan.FromSeconds(2);

		fileSystemWatcher.WaitForChanged(changeType, timeout);
		cts.Cancel();

		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType,
				timeout);
	}
#endif

	[Fact]
	public void Property_EnableRaisingEvents_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").EnableRaisingEvents;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.EnableRaisingEvents));
	}

	[Fact]
	public void Property_EnableRaisingEvents_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool value = false;

		sut.FileSystemWatcher.New("foo").EnableRaisingEvents = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.EnableRaisingEvents));
	}

	[Fact]
	public void Property_Filter_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Filter;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.Filter));
	}

	[Fact]
	public void Property_Filter_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string value = "foo";

		sut.FileSystemWatcher.New("foo").Filter = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.Filter));
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[Fact]
	public void Property_Filters_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Filters;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.Filters));
	}
#endif

	[Fact]
	public void Property_IncludeSubdirectories_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").IncludeSubdirectories;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.IncludeSubdirectories));
	}

	[Fact]
	public void Property_IncludeSubdirectories_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool value = true;

		sut.FileSystemWatcher.New("foo").IncludeSubdirectories = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.IncludeSubdirectories));
	}

	[Fact]
	public void Property_InternalBufferSize_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").InternalBufferSize;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.InternalBufferSize));
	}

	[Fact]
	public void Property_InternalBufferSize_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		int value = 42;

		sut.FileSystemWatcher.New("foo").InternalBufferSize = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.InternalBufferSize));
	}

	[Fact]
	public void Property_NotifyFilter_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").NotifyFilter;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.NotifyFilter));
	}

	[Fact]
	public void Property_NotifyFilter_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		NotifyFilters value = new();

		sut.FileSystemWatcher.New("foo").NotifyFilter = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.NotifyFilter));
	}

	[Fact]
	public void Property_Path_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Path;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.Path));
	}

	[Fact]
	public void Property_Path_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string value = "foo";

		sut.FileSystemWatcher.New("foo").Path = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.Path));
	}

	[Fact]
	public void Property_Site_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Site;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.Site));
	}

	[Fact]
	public void Property_Site_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		ISite value = null!;

		sut.FileSystemWatcher.New("foo").Site = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.Site));
	}

	[Fact]
	public void Property_SynchronizingObject_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").SynchronizingObject;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.SynchronizingObject));
	}

	[Fact]
	public void Property_SynchronizingObject_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		ISynchronizeInvoke value = null!;

		sut.FileSystemWatcher.New("foo").SynchronizingObject = value;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.SynchronizingObject));
	}

	[Fact]
	public void ToString_ShouldBeFileSystemWatcherWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.FileSystemWatcher[@"\\some\path"];

		string? result = sut.ToString();

		result.Should().Be(@"FileSystemWatcher[\\some\path]");
	}
}

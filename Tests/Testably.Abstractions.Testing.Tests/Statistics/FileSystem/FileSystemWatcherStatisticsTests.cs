using System.ComponentModel;
using System.IO;
using System.Threading;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;
// ReSharper disable MethodSupportsCancellation

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileSystemWatcherStatisticsTests
{
	[Fact]
	public async Task Method_BeginInit_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");

		fileSystemWatcher.BeginInit();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsMethodCall(nameof(IFileSystemWatcher.BeginInit));
	}

	[Fact]
	public async Task Method_EndInit_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");

		fileSystemWatcher.EndInit();

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsMethodCall(nameof(IFileSystemWatcher.EndInit));
	}

	[Fact]
	public async Task Method_WaitForChanged_WatcherChangeTypes_Int_ShouldRegisterCall()
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

		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType,
				timeout);
	}

	[Fact]
	public async Task Method_WaitForChanged_WatcherChangeTypes_ShouldRegisterCall()
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

		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType);
	}

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
	[Fact]
	public async Task Method_WaitForChanged_WatcherChangeTypes_TimeSpan_ShouldRegisterCall()
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

		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType,
				timeout);
	}
#endif

	[Fact]
	public async Task Property_EnableRaisingEvents_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").EnableRaisingEvents;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.EnableRaisingEvents));
	}

	[Fact]
	public async Task Property_EnableRaisingEvents_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool value = false;

		sut.FileSystemWatcher.New("foo").EnableRaisingEvents = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileSystemWatcher.EnableRaisingEvents));
	}

	[Fact]
	public async Task Property_Filter_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Filter;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.Filter));
	}

	[Fact]
	public async Task Property_Filter_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string value = "foo";

		sut.FileSystemWatcher.New("foo").Filter = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileSystemWatcher.Filter));
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[Fact]
	public async Task Property_Filters_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Filters;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.Filters));
	}
#endif

	[Fact]
	public async Task Property_IncludeSubdirectories_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").IncludeSubdirectories;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.IncludeSubdirectories));
	}

	[Fact]
	public async Task Property_IncludeSubdirectories_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool value = true;

		sut.FileSystemWatcher.New("foo").IncludeSubdirectories = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileSystemWatcher.IncludeSubdirectories));
	}

	[Fact]
	public async Task Property_InternalBufferSize_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").InternalBufferSize;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.InternalBufferSize));
	}

	[Fact]
	public async Task Property_InternalBufferSize_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		int value = 42;

		sut.FileSystemWatcher.New("foo").InternalBufferSize = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileSystemWatcher.InternalBufferSize));
	}

	[Fact]
	public async Task Property_NotifyFilter_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").NotifyFilter;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.NotifyFilter));
	}

	[Fact]
	public async Task Property_NotifyFilter_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		NotifyFilters value = new();

		sut.FileSystemWatcher.New("foo").NotifyFilter = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileSystemWatcher.NotifyFilter));
	}

	[Fact]
	public async Task Property_Path_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Path;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.Path));
	}

	[Fact]
	public async Task Property_Path_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string value = "foo";

		sut.FileSystemWatcher.New("foo").Path = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileSystemWatcher.Path));
	}

	[Fact]
	public async Task Property_Site_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Site;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.Site));
	}

	[Fact]
	public async Task Property_Site_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		ISite value = null!;

		sut.FileSystemWatcher.New("foo").Site = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileSystemWatcher.Site));
	}

	[Fact]
	public async Task Property_SynchronizingObject_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").SynchronizingObject;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertyGetAccess(nameof(IFileSystemWatcher.SynchronizingObject));
	}

	[Fact]
	public async Task Property_SynchronizingObject_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		ISynchronizeInvoke value = null!;

		sut.FileSystemWatcher.New("foo").SynchronizingObject = value;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.FileSystemWatcher["foo"])
			.OnlyContainsPropertySetAccess(nameof(IFileSystemWatcher.SynchronizingObject));
	}

	[Fact]
	public async Task ToString_ShouldBeFileSystemWatcherWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.FileSystemWatcher[@"\\some\path"];

		string? result = sut.ToString();

		await That(result).IsEqualTo(@"FileSystemWatcher[\\some\path]");
	}
}

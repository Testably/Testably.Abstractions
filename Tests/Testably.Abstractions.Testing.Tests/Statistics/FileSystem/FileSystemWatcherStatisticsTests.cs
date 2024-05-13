﻿using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileSystemWatcherStatisticsTests
{
	[SkippableFact]
	public void Method_BeginInit_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");

		fileSystemWatcher.BeginInit();

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.BeginInit));
	}

	[SkippableFact]
	public void Method_EndInit_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");

		fileSystemWatcher.EndInit();

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.EndInit));
	}

	[SkippableFact]
	public void Method_WaitForChanged_WatcherChangeTypes_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(1));
		CancellationToken token = cts.Token;
		_ = Task.Run(() =>
		{
			while (!token.IsCancellationRequested)
			{
				Thread.Sleep(10);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;
		int timeout = 42;

		fileSystemWatcher.WaitForChanged(changeType, timeout);
		cts.Cancel();

		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType,
				timeout);
	}

	[SkippableFact]
	public void Method_WaitForChanged_WatcherChangeTypes_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(1));
		CancellationToken token = cts.Token;
		_ = Task.Run(() =>
		{
			while (!token.IsCancellationRequested)
			{
				Thread.Sleep(10);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;

		fileSystemWatcher.WaitForChanged(changeType);
		cts.Cancel();

		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType);
	}

#if FEATURE_FILESYSTEM_NET7
	[SkippableFact]
	public void Method_WaitForChanged_WatcherChangeTypes_TimeSpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		using IFileSystemWatcher fileSystemWatcher = sut.FileSystemWatcher.New("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(1));
		CancellationToken token = cts.Token;
		_ = Task.Run(() =>
		{
			while (!token.IsCancellationRequested)
			{
				Thread.Sleep(10);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;
		TimeSpan timeout = TimeSpan.FromSeconds(2);

		fileSystemWatcher.WaitForChanged(changeType, timeout);
		cts.Cancel();

		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainMethodCall(nameof(IFileSystemWatcher.WaitForChanged), changeType,
				timeout);
	}
#endif

	[SkippableFact]
	public void Property_EnableRaisingEvents_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").EnableRaisingEvents;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.EnableRaisingEvents));
	}

	[SkippableFact]
	public void Property_EnableRaisingEvents_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool value = false;

		sut.FileSystemWatcher.New("foo").EnableRaisingEvents = value;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.EnableRaisingEvents));
	}

	[SkippableFact]
	public void Property_Filter_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Filter;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.Filter));
	}

	[SkippableFact]
	public void Property_Filter_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string value = "foo";

		sut.FileSystemWatcher.New("foo").Filter = value;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.Filter));
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[SkippableFact]
	public void Property_Filters_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Filters;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.Filters));
	}
#endif

	[SkippableFact]
	public void Property_IncludeSubdirectories_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").IncludeSubdirectories;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.IncludeSubdirectories));
	}

	[SkippableFact]
	public void Property_IncludeSubdirectories_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		bool value = true;

		sut.FileSystemWatcher.New("foo").IncludeSubdirectories = value;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.IncludeSubdirectories));
	}

	[SkippableFact]
	public void Property_InternalBufferSize_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").InternalBufferSize;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.InternalBufferSize));
	}

	[SkippableFact]
	public void Property_InternalBufferSize_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		int value = 42;

		sut.FileSystemWatcher.New("foo").InternalBufferSize = value;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.InternalBufferSize));
	}

	[SkippableFact]
	public void Property_NotifyFilter_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").NotifyFilter;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.NotifyFilter));
	}

	[SkippableFact]
	public void Property_NotifyFilter_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		NotifyFilters value = new();

		sut.FileSystemWatcher.New("foo").NotifyFilter = value;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.NotifyFilter));
	}

	[SkippableFact]
	public void Property_Path_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Path;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.Path));
	}

	[SkippableFact]
	public void Property_Path_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string value = "foo";

		sut.FileSystemWatcher.New("foo").Path = value;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.Path));
	}

	[SkippableFact]
	public void Property_Site_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").Site;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.Site));
	}

	[SkippableFact]
	public void Property_Site_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		ISite value = null!;

		sut.FileSystemWatcher.New("foo").Site = value;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.Site));
	}

	[SkippableFact]
	public void Property_SynchronizingObject_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		_ = sut.FileSystemWatcher.New("foo").SynchronizingObject;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IFileSystemWatcher.SynchronizingObject));
	}

	[SkippableFact]
	public void Property_SynchronizingObject_Set_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		ISynchronizeInvoke value = null!;

		sut.FileSystemWatcher.New("foo").SynchronizingObject = value;

		sut.StatisticsRegistration.TotalCount.Should().Be(2);
		sut.Statistics.FileSystemWatcher["foo"]
			.ShouldOnlyContainPropertySetAccess(nameof(IFileSystemWatcher.SynchronizingObject));
	}

	[SkippableFact]
	public void ToString_ShouldBeFileSystemWatcherWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.FileSystemWatcher[@"\\some\path"];

		string? result = sut.ToString();

		result.Should().Be(@"FileSystemWatcher[\\some\path]");
	}
}

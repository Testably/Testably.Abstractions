using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class WaitForChangedTests
{
	[Theory]
	[AutoData]
	public void WaitForChanged_ShouldBlockUntilEventHappens(string path)
	{
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		try
		{
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						await Task.Delay(10, TestContext.Current.CancellationToken);
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			});

			using (CancellationTokenSource cts = new(ExpectSuccess))
			{
				cts.Token.Register(() => throw new TimeoutException());
				IWaitForChangedResult result =
					fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created);
				fileSystemWatcher.EnableRaisingEvents.Should().BeFalse();
				result.TimedOut.Should().BeFalse();
				result.ChangeType.Should().Be(WatcherChangeTypes.Created);
				result.Name.Should().Be(path);
				result.OldName.Should().BeNull();
			}
		}
		finally
		{
			ms.Set();
		}
	}

	[Theory]
	[MemberData(nameof(GetWaitForChangedTimeoutParameters))]
	public void WaitForChanged_Timeout_ShouldReturnTimedOut(string path,
		Func<IFileSystemWatcher, IWaitForChangedResult> callback)
	{
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		string fullPath = FileSystem.Path.GetFullPath(path);

		try
		{
			fileSystemWatcher.EnableRaisingEvents = true;
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						await Task.Delay(10, TestContext.Current.CancellationToken);
						FileSystem.Directory.CreateDirectory(fullPath);
						FileSystem.Directory.Delete(fullPath);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			});
			IWaitForChangedResult result = callback(fileSystemWatcher);

			fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
			result.TimedOut.Should().BeTrue();
			result.ChangeType.Should().Be(0);
			result.Name.Should().BeNull();
			result.OldName.Should().BeNull();
		}
		finally
		{
			ms.Set();
		}
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<string, Func<IFileSystemWatcher, IWaitForChangedResult>>
		GetWaitForChangedTimeoutParameters()
	{
		TheoryData<string, Func<IFileSystemWatcher, IWaitForChangedResult>> theoryData = new()
		{
			{
				"foo.dll", fileSystemWatcher
					=> fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed, 100)
			},
		};
#if FEATURE_FILESYSTEM_NET7
		theoryData.Add(
			"bar.txt",
			fileSystemWatcher
				=> fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed,
					TimeSpan.FromMilliseconds(100))
		);
#endif
		return theoryData;
	}
	#pragma warning restore MA0018

	#endregion
}

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WaitForChangedTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	#region Test Setup

	public static IEnumerable<object?[]> WaitForChangedTimeoutParameters
	{
		get
		{
			yield return new object?[]
			{
				"foo.dll",
				new Func<IFileSystemWatcher, IWaitForChangedResult>(fileSystemWatcher
					=> fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed, 100))
			};
#if FEATURE_FILESYSTEM_NET7
			yield return new object?[]
			{
				"bar.txt",
				new Func<IFileSystemWatcher, IWaitForChangedResult>(fileSystemWatcher
					=> fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed,
						TimeSpan.FromMilliseconds(100)))
			};
#endif
		}
	}

	#endregion

	[SkippableTheory]
	[AutoData]
	public void WaitForChanged_ShouldBlockUntilEventHappens(string path)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		try
		{
			_ = Task.Run(async () =>
			{
				while (!ms.IsSet)
				{
					await Task.Delay(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});

			using (CancellationTokenSource cts = new(5000))
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

	[SkippableTheory]
	[MemberData(nameof(WaitForChangedTimeoutParameters))]
	public void WaitForChanged_Timeout_ShouldReturnTimedOut(string path,
		Func<IFileSystemWatcher, IWaitForChangedResult> callback)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		try
		{
			fileSystemWatcher.EnableRaisingEvents = true;
			_ = Task.Run(async () =>
			{
				while (!ms.IsSet)
				{
					await Task.Delay(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
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
}

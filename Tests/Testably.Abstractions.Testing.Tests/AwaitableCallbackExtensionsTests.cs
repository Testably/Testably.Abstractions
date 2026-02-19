using System.IO;
using System.Linq;
using System.Threading;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests;

public class AwaitableCallbackExtensionsTests
{
#if NET6_0_OR_GREATER
	public sealed class ToAsyncEnumerableTests
	{
		[Fact]
		public async Task ShouldReturnAllEvents()
		{
			MockFileSystem fileSystem = new();
			IAwaitableCallback<ChangeDescription> sut = fileSystem.Notify.OnEvent();

			fileSystem.Directory.CreateDirectory("Test");
			fileSystem.File.WriteAllText("Test/abc.txt", "foo");
			fileSystem.File.Delete("Test/abc.txt");
			fileSystem.Directory.Delete("Test");

			ChangeDescription[] results = await sut
				.ToAsyncEnumerable(cancellationToken: TestContext.Current.CancellationToken)
				.Take(6)
				.ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

			await That(results[0]).IsEquivalentTo(new
			{
				ChangeType = WatcherChangeTypes.Created,
				FileSystemType = FileSystemTypes.Directory,
				Name = "Test",
			});
			await That(results[1]).IsEquivalentTo(new
			{
				ChangeType = WatcherChangeTypes.Created,
				FileSystemType = FileSystemTypes.File,
				Name = "Test/abc.txt",
			});
			await That(results[4]).IsEquivalentTo(new
			{
				ChangeType = WatcherChangeTypes.Deleted,
				FileSystemType = FileSystemTypes.File,
				Name = "Test/abc.txt",
			});
			await That(results[5]).IsEquivalentTo(new
			{
				ChangeType = WatcherChangeTypes.Deleted,
				FileSystemType = FileSystemTypes.Directory,
				Name = "Test",
			});
		}

		[Fact]
		public async Task WithCancelledToken_ShouldAbort()
		{
			MockFileSystem fileSystem = new();
			IAwaitableCallback<ChangeDescription> sut = fileSystem.Notify
				.OnEvent(predicate: p => p.FileSystemType == FileSystemTypes.Directory);
			using CancellationTokenSource cts = new();
			CancellationToken token = cts.Token;

			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					fileSystem.Directory.CreateDirectory($"Test{i}");
					await Task.Delay(100, TestContext.Current.CancellationToken);
					if (i == 5)
					{
						// ReSharper disable once AccessToDisposedClosure
						cts.Cancel();
					}
				}
			}, TestContext.Current.CancellationToken);

			ChangeDescription[] results = await sut.ToAsyncEnumerable(cancellationToken: token)
				.Take(10)
				.ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

			await That(results.Length).IsEqualTo(6);
		}

		[Fact]
		public async Task WithFilter_ShouldOnlyReturnMatchingEvents()
		{
			MockFileSystem fileSystem = new();
			IAwaitableCallback<ChangeDescription> sut = fileSystem.Notify
				.OnEvent(predicate: p => p.FileSystemType == FileSystemTypes.Directory);

			fileSystem.Directory.CreateDirectory("Test");
			fileSystem.File.WriteAllText("Test/abc.txt", "foo");
			fileSystem.File.Delete("Test/abc.txt");
			fileSystem.Directory.Delete("Test");

			ChangeDescription[] results = await sut
				.ToAsyncEnumerable(cancellationToken: TestContext.Current.CancellationToken)
				.Take(2)
				.ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

			await That(results[0]).IsEquivalentTo(new
			{
				ChangeType = WatcherChangeTypes.Created,
				FileSystemType = FileSystemTypes.Directory,
				Name = "Test",
			});
			await That(results[1]).IsEquivalentTo(new
			{
				ChangeType = WatcherChangeTypes.Deleted,
				FileSystemType = FileSystemTypes.Directory,
				Name = "Test",
			});
		}

		[Fact]
		public async Task WithIntTimeout_ShouldAbortAfterwards()
		{
			MockFileSystem fileSystem = new();
			IAwaitableCallback<ChangeDescription> sut = fileSystem.Notify
				.OnEvent(predicate: p => p.FileSystemType == FileSystemTypes.Directory);

			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					fileSystem.Directory.CreateDirectory($"Test{i}");
					await Task.Delay(100, TestContext.Current.CancellationToken);
				}
			}, TestContext.Current.CancellationToken);

			ChangeDescription[] results = await sut
				.ToAsyncEnumerable(150, cancellationToken: TestContext.Current.CancellationToken)
				.Take(10)
				.ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

			await That(results.Length).IsLessThan(9);
		}

		[Fact]
		public async Task WithTimeout_ShouldAbortAfterwards()
		{
			MockFileSystem fileSystem = new();
			IAwaitableCallback<ChangeDescription> sut = fileSystem.Notify
				.OnEvent(predicate: p => p.FileSystemType == FileSystemTypes.Directory);

			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					fileSystem.Directory.CreateDirectory($"Test{i}");
					await Task.Delay(100, TestContext.Current.CancellationToken);
				}
			}, TestContext.Current.CancellationToken);

			ChangeDescription[] results = await sut
				.ToAsyncEnumerable(TimeSpan.FromMilliseconds(150),
					cancellationToken: TestContext.Current.CancellationToken)
				.Take(10)
				.ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

			await That(results.Length).IsLessThan(9);
		}
	}
#endif
}

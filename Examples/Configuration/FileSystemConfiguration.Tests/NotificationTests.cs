using aweXpect;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.Configuration.FileSystemConfiguration.Tests;

public class NotificationTests
{
	/// <summary>
	///     Notifications allow reacting to an event after it occurred.
	/// </summary>
	[Fact]
	public async Task Notify_ManualWait()
	{
		ManualResetEventSlim ms = new();
		bool isNotified = false;
		MockFileSystem fileSystem = new();
		fileSystem.Notify.OnCreated(FileSystemTypes.File,
			_ =>
			{
				isNotified = true;
				ms.Set();
			});

		fileSystem.Directory.CreateDirectory("foo");
		fileSystem.File.Create("foo/bar.txt");

		ms.Wait(TestContext.Current.CancellationToken);
		await Expect.That(fileSystem.File.Exists("foo/bar.txt")).IsTrue();
		await Expect.That(isNotified).IsTrue();
	}

	/// <summary>
	///     Notifications allow reacting to an event after it occurred.
	/// </summary>
	[Fact]
	public async Task Notify_UseAwaitableCallback()
	{
		bool isNotified = false;
		MockFileSystem fileSystem = new();
		fileSystem.Notify
			.OnCreated(FileSystemTypes.File,
				_ =>
				{
					isNotified = true;
				})
			.ExecuteWhileWaiting(() =>
			{
				fileSystem.Directory.CreateDirectory("foo");
				fileSystem.File.Create("foo/bar.txt");
			})
			// If a timeout is provided, this will throw a TimeoutException if no event was triggered within 1000ms
			.Wait(timeout: 1000);

		await Expect.That(fileSystem.File.Exists("foo/bar.txt")).IsTrue();
		await Expect.That(isNotified).IsTrue();
	}
}

using FluentAssertions;
using System.Threading;
using Testably.Abstractions.Testing;
using Xunit;

namespace FileSystemConfiguration.Tests;

public class NotificationTests
{
	/// <summary>
	///     Notifications allow reacting to an event after it occurred.
	/// </summary>
	[Fact]
	public void Notify_ManualWait()
	{
		ManualResetEventSlim ms = new();
		bool isNotified = false;
		FileSystemMock fileSystem = new();
		fileSystem.Notify.OnCreated(FileSystemTypes.File,
			_ =>
			{
				isNotified = true;
				ms.Set();
			});

		fileSystem.Directory.CreateDirectory("foo");
		fileSystem.File.Create("foo/bar.txt");

		ms.Wait();
		fileSystem.File.Exists("foo/bar.txt").Should().BeTrue();
		isNotified.Should().BeTrue();
	}

	/// <summary>
	///     Notifications allow reacting to an event after it occurred.
	/// </summary>
	[Fact]
	public void Notify_UseAwaitableCallback()
	{
		bool isNotified = false;
		FileSystemMock fileSystem = new();
		fileSystem.Notify
			.OnCreated(FileSystemTypes.File, _ =>
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

		fileSystem.File.Exists("foo/bar.txt").Should().BeTrue();
		isNotified.Should().BeTrue();
	}
}
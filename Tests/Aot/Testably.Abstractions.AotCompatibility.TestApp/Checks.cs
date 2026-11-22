using System.IO.Abstractions;
using System.Security.AccessControl;
using Testably.Abstractions.Testing;

namespace Testably.Abstractions.AotCompatibility.TestApp;

internal static class Checks
{
	public static readonly Check[] All =
	[
		new("the real file system writes and reads a file", () =>
		{
			RealFileSystem sut = new();
			string directory = sut.Path.Combine(sut.Path.GetTempPath(), $"aot-{Guid.NewGuid()}");
			try
			{
				sut.Directory.CreateDirectory(directory);
				string path = sut.Path.Combine(directory, "a.txt");
				sut.File.WriteAllText(path, "foo");
				return Equal(sut.File.ReadAllText(path), "foo");
			}
			finally
			{
				sut.Directory.Delete(directory, true);
			}
		}),
		new("the real time and random systems return values", () =>
		{
			RealTimeSystem timeSystem = new();
			RealRandomSystem randomSystem = new();
			return Equal((timeSystem.DateTime.UtcNow > DateTime.MinValue,
				randomSystem.Random.Shared.Next(10) < 10,
				randomSystem.Guid.NewGuid() != Guid.Empty), (true, true, true));
		}),
		new("the mock file system writes and reads a file", () =>
		{
			MockFileSystem sut = new();
			sut.Directory.CreateDirectory("foo");
			sut.File.WriteAllText("foo/a.txt", "bar");
			return Equal((sut.File.ReadAllText("foo/a.txt"), sut.Directory.GetFiles("foo").Length), ("bar", 1));
		}),
		new("the watcher of a mock file system that simulates another OS uses its separator", async () =>
		{
			MockFileSystem sut = new(o => o.SimulatingOperatingSystem(
				OperatingSystem.IsWindows() ? SimulationMode.Linux : SimulationMode.Windows));
			string directory = sut.Directory.CreateDirectory("foo").FullName;
			string expected = sut.Path.Combine(directory, "a.txt");
			TaskCompletionSource<string> created = new(TaskCreationOptions.RunContinuationsAsynchronously);
			TaskCompletionSource<string> renamed = new(TaskCreationOptions.RunContinuationsAsynchronously);
			using IFileSystemWatcher watcher = sut.FileSystemWatcher.New(directory);
			watcher.Created += (_, e) => created.TrySetResult(e.FullPath);
			watcher.Renamed += (_, e) => renamed.TrySetResult(e.OldFullPath);
			watcher.EnableRaisingEvents = true;

			sut.File.WriteAllText(expected, "bar");
			sut.File.Move(expected, sut.Path.Combine(directory, "b.txt"));

			return Equal((await created.Task.WaitAsync(TimeSpan.FromSeconds(10)),
				await renamed.Task.WaitAsync(TimeSpan.FromSeconds(10))), (expected, expected));
		}),
		new("the mock random system returns values", () =>
		{
			MockRandomSystem sut = new();
			return Equal((sut.Random.Shared.Next(10) < 10, sut.Guid.NewGuid() != Guid.Empty), (true, true));
		}),
		new("a mock timer fires when the clock passes its due time", () =>
		{
			MockTimeSystem sut = new(o => o.DisableAutoAdvance());
			using ManualResetEventSlim fired = new();
			using Testably.Abstractions.TimeSystem.ITimer timer = sut.Timer.New(_ => fired.Set(), null,
				TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
			sut.TimeProvider.AdvanceBy(TimeSpan.FromSeconds(5));
			return Equal(fired.Wait(TimeSpan.FromSeconds(10)), true);
		}),
		new("a delay with a cancellation token on the mock TimeProvider moves the clock", async () =>
		{
			MockTimeSystem timeSystem = new();
			System.TimeProvider sut = timeSystem.ToTimeProvider();
			DateTimeOffset before = sut.GetUtcNow();
			using CancellationTokenSource cts = new();
			await Task.Delay(TimeSpan.FromSeconds(5), sut, cts.Token).WaitAsync(TimeSpan.FromSeconds(10));
			return Equal(sut.GetUtcNow(), before + TimeSpan.FromSeconds(5));
		}),
		new("the timeout of WaitAsync on the mock TimeProvider does not move the clock", async () =>
		{
			MockTimeSystem timeSystem = new();
			System.TimeProvider sut = timeSystem.ToTimeProvider();
			DateTimeOffset before = sut.GetUtcNow();
			TaskCompletionSource<bool> neverCompleted = new();
			Task waiting = neverCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10), sut);
			await Task.Delay(TimeSpan.FromMilliseconds(50), System.TimeProvider.System);
			return Equal((waiting.IsCompleted, sut.GetUtcNow()), (false, before));
		}),
		new("a zip file is created and extracted in the mock file system", () =>
		{
			MockFileSystem sut = new();
			sut.Directory.CreateDirectory("source");
			sut.File.WriteAllText("source/a.txt", "foo");
			sut.ZipFile.CreateFromDirectory("source", "archive.zip");
			sut.ZipFile.ExtractToDirectory("archive.zip", "destination");
			return Equal(sut.File.ReadAllText("destination/a.txt"), "foo");
		}),
		new("a zip file is created and extracted in the real file system", () =>
		{
			RealFileSystem sut = new();
			string directory = sut.Path.Combine(sut.Path.GetTempPath(), $"aot-{Guid.NewGuid()}");
			try
			{
				string source = sut.Directory.CreateDirectory(sut.Path.Combine(directory, "source")).FullName;
				sut.File.WriteAllText(sut.Path.Combine(source, "a.txt"), "foo");
				string archive = sut.Path.Combine(directory, "archive.zip");
				string destination = sut.Path.Combine(directory, "destination");
				sut.ZipFile.CreateFromDirectory(source, archive);
				sut.ZipFile.ExtractToDirectory(archive, destination);
				return Equal(sut.File.ReadAllText(sut.Path.Combine(destination, "a.txt")), "foo");
			}
			finally
			{
				sut.Directory.Delete(directory, true);
			}
		}),
		new("an anonymous memory mapped file is created on the real file system", () =>
		{
			RealFileSystem sut = new();
			using IMemoryMappedFile file = sut.MemoryMappedFile.CreateNew(null, 16);
			using IMemoryMappedViewAccessor accessor = file.CreateViewAccessor();
			accessor.Write(4, 42);
			return Equal(accessor.ReadInt32(4), 42);
		}),
		new("a memory mapped file writes a struct to the mock file system", () =>
		{
			MockFileSystem sut = new();
			sut.File.WriteAllBytes("data.bin", new byte[16]);
			using (IMemoryMappedFile file = sut.MemoryMappedFile.CreateFromFile("data.bin"))
			using (IMemoryMappedViewAccessor accessor = file.CreateViewAccessor())
			{
				Point point = new(1, 2);
				accessor.Write(4, ref point);
			}

			byte[] bytes = sut.File.ReadAllBytes("data.bin");
			return Equal((BitConverter.ToInt32(bytes, 4), BitConverter.ToInt32(bytes, 8)), (1, 2));
		}),
		new("the access control of a directory in the mock file system is stored", () =>
		{
			if (!OperatingSystem.IsWindows())
			{
				return null;
			}

			MockFileSystem sut = new();
			DirectorySecurity security = new();
			security.SetSecurityDescriptorSddlForm("O:WD");
			sut.Directory.CreateDirectory("foo");
			sut.Directory.SetAccessControl("foo", security);
			return Equal(sut.Directory.GetAccessControl("foo")
				.GetSecurityDescriptorSddlForm(AccessControlSections.Owner), "O:WD");
		}),
	];

	private static string? Equal<T>(T actual, T expected)
		=> EqualityComparer<T>.Default.Equals(actual, expected) ? null : $"expected {expected}, but was {actual}";

	private readonly record struct Point(int X, int Y);
}

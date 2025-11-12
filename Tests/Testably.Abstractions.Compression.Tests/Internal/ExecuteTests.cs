using Testably.Abstractions.Internal;

namespace Testably.Abstractions.Compression.Tests.Internal;

public sealed class ExecuteTests
{
	[Fact]
	public async Task
		WhenRealFileSystem_MockFileSystem_WithActionCallback_ShouldExecuteOnMockFileSystem()
	{
		bool onRealFileSystemExecuted = false;
		bool onMockFileSystemExecuted = false;
		MockFileSystem fileSystem = new();
		Execute.WhenRealFileSystem(fileSystem,
			() =>
			{
				onRealFileSystemExecuted = true;
			},
			() =>
			{
				onMockFileSystemExecuted = true;
			});

		await That(onRealFileSystemExecuted).IsFalse();
		await That(onMockFileSystemExecuted).IsTrue();
	}

	[Fact]
	public async Task
		WhenRealFileSystem_MockFileSystem_WithFuncCallback_ShouldExecuteOnMockFileSystem()
	{
		bool onRealFileSystemExecuted = false;
		bool onMockFileSystemExecuted = false;
		MockFileSystem fileSystem = new();
		Execute.WhenRealFileSystem(fileSystem,
			() =>
			{
				return onRealFileSystemExecuted = true;
			},
			() =>
			{
				return onMockFileSystemExecuted = true;
			});

		await That(onRealFileSystemExecuted).IsFalse();
		await That(onMockFileSystemExecuted).IsTrue();
	}

	[Fact]
	public async Task
		WhenRealFileSystem_RealFileSystem_WithActionCallback_ShouldExecuteOnRealFileSystem()
	{
		bool onRealFileSystemExecuted = false;
		bool onMockFileSystemExecuted = false;
		RealFileSystem fileSystem = new();
		Execute.WhenRealFileSystem(fileSystem,
			() =>
			{
				onRealFileSystemExecuted = true;
			},
			() =>
			{
				onMockFileSystemExecuted = true;
			});

		await That(onRealFileSystemExecuted).IsTrue();
		await That(onMockFileSystemExecuted).IsFalse();
	}

	[Fact]
	public async Task
		WhenRealFileSystem_RealFileSystem_WithFuncCallback_ShouldExecuteOnRealFileSystem()
	{
		bool onRealFileSystemExecuted = false;
		bool onMockFileSystemExecuted = false;
		RealFileSystem fileSystem = new();
		Execute.WhenRealFileSystem(fileSystem,
			() =>
			{
				return onRealFileSystemExecuted = true;
			},
			() =>
			{
				return onMockFileSystemExecuted = true;
			});

		await That(onRealFileSystemExecuted).IsTrue();
		await That(onMockFileSystemExecuted).IsFalse();
	}

	[Fact]
	public async Task
		WhenRealFileSystemAsync_MockFileSystem_WithActionCallback_ShouldExecuteOnMockFileSystem()
	{
		bool onRealFileSystemExecuted = false;
		bool onMockFileSystemExecuted = false;
		MockFileSystem fileSystem = new();
		await Execute.WhenRealFileSystemAsync(fileSystem,
			() =>
			{
				onRealFileSystemExecuted = true;
				return Task.CompletedTask;
			},
			() =>
			{
				onMockFileSystemExecuted = true;
			});

		await That(onRealFileSystemExecuted).IsFalse();
		await That(onMockFileSystemExecuted).IsTrue();
	}

	[Fact]
	public async Task
		WhenRealFileSystemAsync_MockFileSystem_WithFuncCallback_ShouldExecuteOnMockFileSystem()
	{
		bool onRealFileSystemExecuted = false;
		bool onMockFileSystemExecuted = false;
		MockFileSystem fileSystem = new();
		await Execute.WhenRealFileSystemAsync(fileSystem,
			async () =>
			{
				await Task.Yield();
				return onRealFileSystemExecuted = true;
			},
			() =>
			{
				return onMockFileSystemExecuted = true;
			});

		await That(onRealFileSystemExecuted).IsFalse();
		await That(onMockFileSystemExecuted).IsTrue();
	}

	[Fact]
	public async Task
		WhenRealFileSystemAsync_RealFileSystem_WithActionCallback_ShouldExecuteOnRealFileSystem()
	{
		bool onRealFileSystemExecuted = false;
		bool onMockFileSystemExecuted = false;
		RealFileSystem fileSystem = new();
		await Execute.WhenRealFileSystemAsync(fileSystem,
			() =>
			{
				onRealFileSystemExecuted = true;
				return Task.CompletedTask;
			},
			() =>
			{
				onMockFileSystemExecuted = true;
			});

		await That(onRealFileSystemExecuted).IsTrue();
		await That(onMockFileSystemExecuted).IsFalse();
	}

	[Fact]
	public async Task
		WhenRealFileSystemAsync_RealFileSystem_WithFuncCallback_ShouldExecuteOnRealFileSystem()
	{
		bool onRealFileSystemExecuted = false;
		bool onMockFileSystemExecuted = false;
		RealFileSystem fileSystem = new();
		await Execute.WhenRealFileSystemAsync(fileSystem,
			async () =>
			{
				await Task.Yield();
				return onRealFileSystemExecuted = true;
			},
			() =>
			{
				return onMockFileSystemExecuted = true;
			});

		await That(onRealFileSystemExecuted).IsTrue();
		await That(onMockFileSystemExecuted).IsFalse();
	}
}

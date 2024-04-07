using Testably.Abstractions.Internal;

namespace Testably.Abstractions.Compression.Tests.Internal;

public sealed class ExecuteTests
{
	[Fact]
	public void WhenRealFileSystem_MockFileSystem_WithActionCallback_ShouldExecuteOnMockFileSystem()
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

		onRealFileSystemExecuted.Should().BeFalse();
		onMockFileSystemExecuted.Should().BeTrue();
	}

	[Fact]
	public void WhenRealFileSystem_MockFileSystem_WithFuncCallback_ShouldExecuteOnMockFileSystem()
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

		onRealFileSystemExecuted.Should().BeFalse();
		onMockFileSystemExecuted.Should().BeTrue();
	}

	[Fact]
	public void WhenRealFileSystem_RealFileSystem_WithActionCallback_ShouldExecuteOnRealFileSystem()
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

		onRealFileSystemExecuted.Should().BeTrue();
		onMockFileSystemExecuted.Should().BeFalse();
	}

	[Fact]
	public void WhenRealFileSystem_RealFileSystem_WithFuncCallback_ShouldExecuteOnRealFileSystem()
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

		onRealFileSystemExecuted.Should().BeTrue();
		onMockFileSystemExecuted.Should().BeFalse();
	}
}

using aweXpect;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.UnixFileMode.Tests;

public class DefaultUnixFileModeStrategyTests
{
	[Fact]
	public async Task GroupRead_WhenSameGroup_ShouldNotThrow()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateGroup("some group");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.GroupRead);
		sut.SimulateGroup("some other group");
		fileSystem.Directory.CreateDirectory("another directory", System.IO.UnixFileMode.GroupRead);
		sut.SimulateGroup("some group");

		void Act()
		{
			fileSystem.Directory.GetFiles("foo");
		}

		await Expect.That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task GroupRead_WhenSimulatingOtherGroup_ShouldThrowUnauthorizedAccessException()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateGroup("some group");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.GroupRead);
		sut.SimulateGroup("some other group");

		void Act()
		{
			fileSystem.Directory.GetFiles("foo");
		}

		await Expect.That(Act).Throws<UnauthorizedAccessException>();
	}

	[Fact]
	public async Task GroupWrite_WhenSameGroup_ShouldNotThrow()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateGroup("some group");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.GroupWrite);
		sut.SimulateGroup("some other group");
		fileSystem.Directory.CreateDirectory("another directory",
			System.IO.UnixFileMode.GroupWrite);
		sut.SimulateGroup("some group");

		void Act()
		{
			fileSystem.Directory.CreateDirectory("foo/bar");
		}

		await Expect.That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task GroupWrite_WhenSimulatingOtherGroup_ShouldThrowUnauthorizedAccessException()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateGroup("some group");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.GroupWrite);
		sut.SimulateGroup("some other group");

		void Act()
		{
			fileSystem.Directory.CreateDirectory("foo/bar");
		}

		await Expect.That(Act).Throws<UnauthorizedAccessException>();
	}

	[Fact]
	public async Task OtherRead_WhenSimulatingOtherUser_ShouldNotThrow()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateUser("some user");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.OtherRead);
		sut.SimulateUser("some other user");

		void Act()
		{
			fileSystem.Directory.GetFiles("foo");
		}

		await Expect.That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task OtherWrite_WhenSimulatingOtherUser_ShouldNotThrow()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateUser("some user");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.OtherWrite);
		sut.SimulateUser("some other user");

		void Act()
		{
			fileSystem.Directory.CreateDirectory("foo/bar");
		}

		await Expect.That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task ShouldInitializeWithEmptyStringForUserAndGroup()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		fileSystem.Directory.CreateDirectory("foo",
			System.IO.UnixFileMode.GroupRead | System.IO.UnixFileMode.UserRead);

		sut.SimulateUser("foo")
			.SimulateGroup("bar");

		void ActWithDifferentUserAndGroup()
		{
			fileSystem.Directory.GetFiles("foo");
		}

		await Expect.That(ActWithDifferentUserAndGroup).Throws<UnauthorizedAccessException>();

		sut.SimulateGroup("")
			.SimulateUser("");

		void ActWithEmptyUserAndGroup()
		{
			fileSystem.Directory.GetFiles("foo");
		}

		await Expect.That(ActWithEmptyUserAndGroup).DoesNotThrow();
	}

	[Fact]
	public async Task UserRead_WhenSameUser_ShouldNotThrow()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateUser("some user");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.UserRead);
		sut.SimulateUser("some other user");
		fileSystem.Directory.CreateDirectory("another directory", System.IO.UnixFileMode.UserRead);
		sut.SimulateUser("some user");

		void Act()
		{
			fileSystem.Directory.GetFiles("foo");
		}

		await Expect.That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task UserRead_WhenSimulatingOtherUser_ShouldThrowUnauthorizedAccessException()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateUser("some user");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.UserRead);
		sut.SimulateUser("some other user");

		void Act()
		{
			fileSystem.Directory.GetFiles("foo");
		}

		await Expect.That(Act).Throws<UnauthorizedAccessException>();
	}

	[Fact]
	public async Task UserWrite_WhenSameUser_ShouldNotThrow()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateUser("some user");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.UserWrite);
		sut.SimulateUser("some other user");
		fileSystem.Directory.CreateDirectory("another directory", System.IO.UnixFileMode.UserWrite);
		sut.SimulateUser("some user");

		void Act()
		{
			fileSystem.Directory.CreateDirectory("foo/bar");
		}

		await Expect.That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task UserWrite_WhenSimulatingOtherUser_ShouldThrowUnauthorizedAccessException()
	{
		Skip.When(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
			"Unix file mode is not supported on Windows.");

		DefaultUnixFileModeStrategy sut = new();
		MockFileSystem fileSystem = new MockFileSystem().WithUnixFileModeStrategy(sut);
		sut.SimulateUser("some user");

		fileSystem.Directory.CreateDirectory("foo", System.IO.UnixFileMode.UserWrite);
		sut.SimulateUser("some other user");

		void Act()
		{
			fileSystem.Directory.CreateDirectory("foo/bar");
		}

		await Expect.That(Act).Throws<UnauthorizedAccessException>();
	}
}

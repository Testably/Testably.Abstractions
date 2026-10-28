using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DefaultAccessControlStrategyTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Test]
	public async Task Constructor_NullCallback_ShouldThrowArgumentNullException()
	{
		void Act()
		{
			_ = new DefaultAccessControlStrategy(
				(Func<string, IFileSystemExtensibility, bool>)null!);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("callback");
	}

	[Test]
	public async Task Constructor_NullCallbackWithRequestedAccess_ShouldThrowArgumentNullException()
	{
		void Act()
		{
			_ = new DefaultAccessControlStrategy(
				(Func<string, IFileSystemExtensibility, FileAccess, bool>)null!);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("callback");
	}

	[Test]
	public async Task IsAccessGranted_ShouldUseCallback()
	{
		DefaultAccessControlStrategy sut = new((p, _) => p.StartsWith('a', StringComparison.Ordinal));

		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility())).IsTrue();
		await That(sut.IsAccessGranted("xyz", new FileSystemExtensibility())).IsFalse();
		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(), FileAccess.Write)).IsTrue();
		await That(sut.IsAccessGranted("xyz", new FileSystemExtensibility(), FileAccess.Read)).IsFalse();
	}

	[Test]
	public async Task IsAccessGranted_WithRequestedAccess_ShouldUseCallback()
	{
		DefaultAccessControlStrategy sut = new((_, _, access) => access == FileAccess.Read);

		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(), FileAccess.Read)).IsTrue();
		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(), FileAccess.Write)).IsFalse();
	}

	[Test]
	public async Task IsAccessGranted_WithoutRequestedAccess_ShouldRequestReadWrite()
	{
		FileAccess? requestedAccess = null;
		DefaultAccessControlStrategy sut = new((_, _, access) =>
		{
			requestedAccess = access;
			return true;
		});

		sut.IsAccessGranted("abc", new FileSystemExtensibility());

		await That(requestedAccess).IsEqualTo(FileAccess.ReadWrite);
	}
}

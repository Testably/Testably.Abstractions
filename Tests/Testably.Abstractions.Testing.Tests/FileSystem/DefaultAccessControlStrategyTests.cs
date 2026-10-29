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
				(Func<string, IFileSystemExtensibility, AccessRequest, bool>)null!);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("callback");
	}

	[Test]
	public async Task IsAccessGranted_ShouldUseCallback()
	{
		DefaultAccessControlStrategy sut = new((p, _) => p.StartsWith('a', StringComparison.Ordinal));

		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility())).IsTrue();
		await That(sut.IsAccessGranted("xyz", new FileSystemExtensibility())).IsFalse();
		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.Write, isDelete: true))).IsTrue();
		await That(sut.IsAccessGranted("xyz", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.Read, isDelete: false))).IsFalse();
	}

	[Test]
	public async Task IsAccessGranted_WithRequestedAccess_ShouldUseCallback()
	{
		DefaultAccessControlStrategy sut = new((_, _, request)
			=> !request.Access.HasFlag(FileAccess.Write) && !request.IsDelete);

		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.Read, isDelete: false))).IsTrue();
		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.ReadWrite, isDelete: false))).IsFalse();
		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.Read, isDelete: true))).IsFalse();
	}

	[Test]
	public async Task IsAccessGranted_WithoutRequestedAccess_ShouldRequestReadWrite()
	{
		AccessRequest? receivedRequest = null;
		DefaultAccessControlStrategy sut = new((_, _, request) =>
		{
			receivedRequest = request;
			return true;
		});

		sut.IsAccessGranted("abc", new FileSystemExtensibility());

		await That(receivedRequest?.Access).IsEqualTo(FileAccess.ReadWrite);
		await That(receivedRequest?.IsDelete).IsEqualTo(false);
	}
}

using System.IO;
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
			_ = new DefaultAccessControlStrategy(null!);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("callback");
	}

	[Test]
	public async Task IsAccessGranted_ShouldUseCallback()
	{
		DefaultAccessControlStrategy sut = new((p, _, request)
			=> p.StartsWith('a', StringComparison.Ordinal)
			   && !request.Access.HasFlag(FileAccess.Write)
			   && !request.IsDelete);

		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.Read, isDelete: false))).IsTrue();
		await That(sut.IsAccessGranted("xyz", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.Read, isDelete: false))).IsFalse();
		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.ReadWrite, isDelete: false))).IsFalse();
		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility(),
			new AccessRequest(FileAccess.Read, isDelete: true))).IsFalse();
	}
}

using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DefaultAccessControlStrategyTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Fact]
	public async Task Constructor_NullCallback_ShouldThrowArgumentNullException()
	{
		void Act()
		{
			_ = new DefaultAccessControlStrategy(null!);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("callback");
	}

	[Fact]
	public async Task IsAccessGranted_ShouldUseCallback()
	{
		DefaultAccessControlStrategy sut = new((p, _) => p.StartsWith('a'));

		await That(sut.IsAccessGranted("abc", new FileSystemExtensibility())).IsTrue();
		await That(sut.IsAccessGranted("xyz", new FileSystemExtensibility())).IsFalse();
	}
}

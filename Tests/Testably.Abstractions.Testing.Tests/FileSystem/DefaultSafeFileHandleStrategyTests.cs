#if NET6_0_OR_GREATER
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DefaultSafeFileHandleStrategyTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Fact]
	public async Task Constructor_NullCallback_ShouldThrowArgumentNullException()
	{
		void Act()
		{
			_ = new DefaultSafeFileHandleStrategy(null!);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("callback");
	}

	[Fact]
	public async Task MapSafeFileHandle_ShouldReturnExpectedValue()
	{
		SafeFileHandle fooSafeFileHandle = new();
		SafeFileHandle barSafeFileHandle = new();
		Dictionary<SafeFileHandle, SafeFileHandleMock> mapping = new()
		{
			{
				fooSafeFileHandle, new SafeFileHandleMock("foo")
			},
			{
				barSafeFileHandle, new SafeFileHandleMock("bar")
			},
		};
		FileSystem.File.WriteAllText("foo", "foo-content");
		FileSystem.File.WriteAllText("bar", "bar-content");

		DefaultSafeFileHandleStrategy sut = new(fileHandle => mapping[fileHandle]);

		await That(sut.MapSafeFileHandle(fooSafeFileHandle).Path).IsEqualTo("foo");
		await That(sut.MapSafeFileHandle(barSafeFileHandle).Path).IsEqualTo("bar");
	}
}
#endif

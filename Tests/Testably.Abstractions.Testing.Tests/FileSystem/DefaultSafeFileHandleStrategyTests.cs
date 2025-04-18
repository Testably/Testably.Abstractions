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
	public void Constructor_NullCallback_ShouldThrowArgumentNullException()
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = new DefaultSafeFileHandleStrategy(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("callback");
	}

	[Fact]
	public void MapSafeFileHandle_ShouldReturnExpectedValue()
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

		sut.MapSafeFileHandle(fooSafeFileHandle).Path.Should().Be("foo");
		sut.MapSafeFileHandle(barSafeFileHandle).Path.Should().Be("bar");
	}
}
#endif

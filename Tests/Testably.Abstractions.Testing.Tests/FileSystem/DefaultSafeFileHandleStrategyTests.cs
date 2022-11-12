#if NET6_0_OR_GREATER
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DefaultSafeFileHandleStrategyTests
{
	public MockFileSystem FileSystem { get; }

	public DefaultSafeFileHandleStrategyTests()
	{
		FileSystem = new MockFileSystem();
	}

	[SkippableTheory]
	[AutoData]
	public void MapSafeFileHandle_NullCallback_ShouldThrowArgumentNullException(
		string path, Exception exceptionToThrow)
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = new DefaultSafeFileHandleStrategy(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("callback");
	}

	[SkippableTheory]
	[AutoData]
	public void MapSafeFileHandle_ShouldReturnExpectedValue(
		string path, Exception exceptionToThrow)
	{
		SafeFileHandle fooSafeFileHandle = new();
		SafeFileHandle barSafeFileHandle = new();
		Dictionary<SafeFileHandle, SafeFileHandleMock> mapping = new();
		mapping.Add(fooSafeFileHandle, new SafeFileHandleMock("foo"));
		mapping.Add(barSafeFileHandle, new SafeFileHandleMock("bar"));
		FileSystem.File.WriteAllText("foo", "foo-content");
		FileSystem.File.WriteAllText("bar", "bar-content");

		DefaultSafeFileHandleStrategy sut = new(fileHandle => mapping[fileHandle]);

		sut.MapSafeFileHandle(fooSafeFileHandle).Path.Should().Be("foo");
		sut.MapSafeFileHandle(barSafeFileHandle).Path.Should().Be("bar");
	}
}
#endif
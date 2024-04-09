using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryLocationTests
{
	[Theory]
	[AutoData]
	public void
		Equals_AsObject_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
			string path1, string path2)
	{
		MockFileSystem fileSystem = new();
		object location1 = InMemoryLocation.New(fileSystem, null, Path.GetFullPath(path1));
		object location2 = InMemoryLocation.New(fileSystem, null, Path.GetFullPath(path2));

		bool result = location1.Equals(location2);

		result.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Equals_ForDummyLocation_ShouldCompareFullPath(
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location1 = InMemoryLocation.New(new MockFileSystem(), null, fullPath);
		IStorageLocation location2 = new DummyLocation(fullPath);

		bool result = location1.Equals(location2);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Equals_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
		string path)
	{
		MockFileSystem fileSystem = new();
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location1 = InMemoryLocation.New(fileSystem, null,
			fullPath);
		IStorageLocation location2 = InMemoryLocation.New(fileSystem, null,
			fullPath + Path.DirectorySeparatorChar);

		bool result = location1.Equals(location2);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Equals_Null_ShouldReturnFalse(string path)
	{
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, path);

		bool result = location.Equals(null!);

		result.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Equals_Object_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
		string path)
	{
		MockFileSystem fileSystem = new();
		string fullPath = Path.GetFullPath(path);
		object location1 = InMemoryLocation.New(fileSystem, null,
			fullPath);
		object location2 = InMemoryLocation.New(fileSystem, null,
			fullPath + Path.DirectorySeparatorChar);

		bool result = location1.Equals(location2);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Equals_Object_Null_ShouldReturnFalse(string path)
	{
		object location = InMemoryLocation.New(new MockFileSystem(), null, path);

		bool result = location.Equals(null);

		result.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Equals_Object_SameInstance_ShouldReturnTrue(string path)
	{
		object location = InMemoryLocation.New(new MockFileSystem(), null, path);
		object other = location;

		bool result = location.Equals(other);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Equals_SameInstance_ShouldReturnTrue(string path)
	{
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, path);

		bool result = location.Equals(location);

		result.Should().BeTrue();
	}

	[Fact]
	public void GetParent_Root_ShouldReturnNull()
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location =
			InMemoryLocation.New(fileSystem, null, "".PrefixRoot(fileSystem));

		IStorageLocation? result = location.GetParent();

		result.Should().BeNull();
	}

	[Fact]
	public void New_EmptyPath_ShouldThrowArgumentException()
	{
		Exception? exception = Record.Exception(() =>
		{
			InMemoryLocation.New(new MockFileSystem(), null, "");
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
	}

	[Theory]
	[AutoData]
	public void ToString_ShouldReturnPath(string path)
	{
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, path);

		location.ToString().Should().Be(path);
	}

	private sealed class DummyLocation(string fullPath) : IStorageLocation
	{
		#region IStorageLocation Members

		/// <inheritdoc cref="IStorageLocation.Drive" />
		public IStorageDrive? Drive => null;

		/// <inheritdoc cref="IStorageLocation.FriendlyName" />
		public string FriendlyName { get; } = fullPath;

		/// <inheritdoc cref="IStorageLocation.FullPath" />
		public string FullPath { get; } = fullPath;

		/// <inheritdoc cref="IStorageLocation.IsRooted" />
		public bool IsRooted
			=> false;

		/// <inheritdoc cref="IEquatable{IStorageLocation}.Equals(IStorageLocation)" />
		public bool Equals(IStorageLocation? other)
		{
			_ = other;
			return false;
		}

		/// <inheritdoc cref="IStorageLocation.GetParent()" />
		public IStorageLocation GetParent()
			=> throw new NotSupportedException();

		#endregion
	}
}

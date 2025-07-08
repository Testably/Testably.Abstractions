using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryLocationTests
{
	[Theory]
	[AutoData]
	public async Task Equals_AsObject_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
			string path1, string path2)
	{
		MockFileSystem fileSystem = new();
		object location1 = InMemoryLocation.New(fileSystem, null, Path.GetFullPath(path1));
		object location2 = InMemoryLocation.New(fileSystem, null, Path.GetFullPath(path2));

		bool result = location1.Equals(location2);

		await That(result).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Equals_ForDummyLocation_ShouldCompareFullPath(
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location1 = InMemoryLocation.New(new MockFileSystem(), null, fullPath);
		IStorageLocation location2 = new DummyLocation(fullPath);

		bool result = location1.Equals(location2);

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Equals_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
		string path)
	{
		MockFileSystem fileSystem = new();
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location1 = InMemoryLocation.New(fileSystem, null,
			fullPath);
		IStorageLocation location2 = InMemoryLocation.New(fileSystem, null,
			fullPath + Path.DirectorySeparatorChar);

		bool result = location1.Equals(location2);

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Equals_Null_ShouldReturnFalse(string path)
	{
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, path);

		bool result = location.Equals(null!);

		await That(result).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Equals_Object_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
		string path)
	{
		MockFileSystem fileSystem = new();
		string fullPath = Path.GetFullPath(path);
		object location1 = InMemoryLocation.New(fileSystem, null,
			fullPath);
		object location2 = InMemoryLocation.New(fileSystem, null,
			fullPath + Path.DirectorySeparatorChar);

		bool result = location1.Equals(location2);

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Equals_Object_Null_ShouldReturnFalse(string path)
	{
		object location = InMemoryLocation.New(new MockFileSystem(), null, path);

		bool result = location.Equals(null);

		await That(result).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Equals_Object_SameInstance_ShouldReturnTrue(string path)
	{
		object location = InMemoryLocation.New(new MockFileSystem(), null, path);
		object other = location;

		bool result = location.Equals(other);

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Equals_SameInstance_ShouldReturnTrue(string path)
	{
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, path);

		bool result = location.Equals(location);

		await That(result).IsTrue();
	}

	[Fact]
	public async Task GetParent_Root_ShouldReturnNull()
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location =
			InMemoryLocation.New(fileSystem, null, "".PrefixRoot(fileSystem));

		IStorageLocation? result = location.GetParent();

		await That(result).IsNull();
	}

	[Fact]
	public async Task New_EmptyPath_ShouldThrowArgumentException()
	{
		void Act()
		{
			InMemoryLocation.New(new MockFileSystem(), null, "");
		}

		await That(Act).ThrowsExactly<ArgumentException>().WithHResult(-2147024809);
	}

	[Theory]
	[AutoData]
	public async Task ToString_ShouldReturnPath(string path)
	{
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, path);

		await That(location.ToString()).IsEqualTo(path);
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

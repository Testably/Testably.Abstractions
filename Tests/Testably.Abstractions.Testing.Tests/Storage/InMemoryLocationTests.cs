using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryLocationTests
{
	[Theory]
	[AutoData]
	public void Equals_ForDummyLocation_ShouldCompareFullPath(
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location1 = InMemoryLocation.New(null, fullPath);
		IStorageLocation location2 = new DummyLocation(fullPath);

		bool result = location1.Equals(location2);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Equals_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location1 = InMemoryLocation.New(null, fullPath);
		IStorageLocation location2 =
			InMemoryLocation.New(null, fullPath + Path.DirectorySeparatorChar);

		bool result = location1.Equals(location2);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void
		Equals_AsObject_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
			string path1, string path2)
	{
		object location1 = InMemoryLocation.New(null, Path.GetFullPath(path1));
		object location2 = InMemoryLocation.New(null, Path.GetFullPath(path2));

		bool result = location1.Equals(location2);

		result.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Equals_Object_ForInMemoryLocation_ShouldIgnoreTrailingDirectorySeparator(
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		object location1 = InMemoryLocation.New(null, fullPath);
		object location2 =
			InMemoryLocation.New(null, fullPath + Path.DirectorySeparatorChar);

		bool result = location1.Equals(location2);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Equals_Null_ShouldReturnFalse(string path)
	{
		IStorageLocation location = InMemoryLocation.New(null, path);

		bool result = location.Equals(null!);

		result.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Equals_Object_Null_ShouldReturnFalse(string path)
	{
		object location = InMemoryLocation.New(null, path);

		bool result = location.Equals(null);

		result.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Equals_SameInstance_ShouldReturnTrue(string path)
	{
		IStorageLocation location = InMemoryLocation.New(null, path);

		bool result = location.Equals(location);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Equals_Object_SameInstance_ShouldReturnTrue(string path)
	{
		object location = InMemoryLocation.New(null, path);

		// ReSharper disable once EqualExpressionComparison
		bool result = location.Equals(location);

		result.Should().BeTrue();
	}

	private class DummyLocation : IStorageLocation
	{
		public DummyLocation(string fullPath)
		{
			FullPath = fullPath;
			FriendlyName = fullPath;
		}

		#region IStorageLocation Members

		/// <inheritdoc cref="IStorageLocation.Drive" />
		public IStorageDrive? Drive => null;

		/// <inheritdoc cref="IStorageLocation.FriendlyName" />
		public string FriendlyName { get; }

		/// <inheritdoc cref="IStorageLocation.FullPath" />
		public string FullPath { get; }

		/// <inheritdoc cref="IEquatable{IStorageLocation}.Equals(IStorageLocation)" />
		public bool Equals(IStorageLocation? other)
			=> throw new NotSupportedException();

		/// <inheritdoc cref="IStorageLocation.GetParent()" />
		public IStorageLocation GetParent()
			=> throw new NotSupportedException();

		#endregion
	}
}
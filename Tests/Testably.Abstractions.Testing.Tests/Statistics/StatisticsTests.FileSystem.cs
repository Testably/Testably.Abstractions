using System.Threading.Tasks;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed partial class StatisticsTests
{
	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IDirectory>))]
	public void FileSystem_Directory_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.Directory.ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IDirectoryInfoFactory>))]
	public void FileSystem_DirectoryInfoFactory_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.DirectoryInfo.ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IDirectoryInfo>))]
	public void FileSystem_DirectoryInfo_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.DirectoryInfo[DummyPath].ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IDriveInfoFactory>))]
	public void FileSystem_DriveInfoFactory_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.DriveInfo.ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory(Skip = "DriveInfo contains no methods")]
	[ClassData(typeof(SynchronousFileSystemMethods<IDriveInfo>))]
	public void FileSystem_DriveInfo_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.DriveInfo[DummyPath].ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IFile>))]
	public void FileSystem_File_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.File.ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IFileInfoFactory>))]
	public void FileSystem_FileInfoFactory_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.FileInfo.ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IFileInfo>))]
	public void FileSystem_FileInfo_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.FileInfo[DummyPath].ShouldOnlyContain(name, parameters, expectation);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[ClassData(typeof(AsynchronousFileSystemMethods<IFile>))]
	public async Task FileSystem_File_AsynchronousMethods_ShouldRegisterCall(string expectation,
		Func<MockFileSystem, Task> asynchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		await asynchronousCall(sut);

		sut.Statistics.File.ShouldOnlyContain(name, parameters, expectation);
	}
#endif

	[Fact]
	public void FileSystem_ShouldInitializeWithEmptyStatistics()
	{
		MockFileSystem sut = new();

		sut.Statistics.File.Calls.Should().BeEmpty();
	}

	public class SynchronousFileSystemMethods<T> : GetMethods<Action<MockFileSystem>>
	{
		public SynchronousFileSystemMethods()
		{
			Func<MockFileSystem, T> accessor = GetFileSystemAccessor<T>();

			foreach ((string Expectation,
				Action<T> Action,
				string Name,
				object?[] Parameters) item in EnumerateSynchronousMethods<T>(Fixture))
			{
				Add(item.Expectation,
					m =>
					{
						try
						{
							item.Action.Invoke(accessor(m));
						}
						catch (Exception)
						{
							// Ignore any exception called here, as we only care about the statistics call registration.
						}
					},
					item.Name,
					item.Parameters);
			}
		}
	}

	public class AsynchronousFileSystemMethods<T> : GetMethods<Func<MockFileSystem, Task>>
	{
		public AsynchronousFileSystemMethods()
		{
			Func<MockFileSystem, T> accessor = GetFileSystemAccessor<T>();

			foreach ((string Expectation,
				Func<T, Task> Action,
				string Name,
				object?[] Parameters) item in EnumerateAsynchronousMethods<T>(Fixture))
			{
				Add(item.Expectation,
					async m =>
					{
						try
						{
							await item.Action.Invoke(accessor(m));
						}
						catch (Exception)
						{
							// Ignore any exception called here, as we only care about the statistics call registration.
						}
					},
					item.Name,
					item.Parameters);
			}
		}
	}

	private static Func<MockFileSystem, TProperty> GetFileSystemAccessor<TProperty>()
	{
		if (typeof(TProperty) == typeof(IDirectory))
		{
			return m => (TProperty)m.Directory;
		}

		if (typeof(TProperty) == typeof(IDirectoryInfoFactory))
		{
			return m => (TProperty)m.DirectoryInfo;
		}

		if (typeof(TProperty) == typeof(IDirectoryInfo))
		{
			return m => (TProperty)m.DirectoryInfo.New(DummyPath);
		}

		if (typeof(TProperty) == typeof(IDriveInfoFactory))
		{
			return m => (TProperty)m.DriveInfo;
		}

		if (typeof(TProperty) == typeof(IDriveInfo))
		{
			return m => (TProperty)m.DriveInfo.New(DummyPath);
		}

		if (typeof(TProperty) == typeof(IFile))
		{
			return m => (TProperty)m.File;
		}

		if (typeof(TProperty) == typeof(IFileInfoFactory))
		{
			return m => (TProperty)m.FileInfo;
		}

		if (typeof(TProperty) == typeof(IFileInfo))
		{
			return m => (TProperty)m.FileInfo.New(DummyPath);
		}

		throw new NotSupportedException($"The type {typeof(TProperty)} is not supported!");
	}
}

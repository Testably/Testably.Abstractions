using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Statistics;
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
	[ClassData(typeof(SynchronousFileSystemMethods<IDirectoryInfo>))]
	public void FileSystem_DirectoryInfo_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.DirectoryInfo[DummyPath].ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IDirectoryInfoFactory>))]
	public void FileSystem_DirectoryInfoFactory_SynchronousMethods_ShouldRegisterCall(
		string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.DirectoryInfo.ShouldOnlyContain(name, parameters, expectation);
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
	[ClassData(typeof(SynchronousFileSystemMethods<IDriveInfoFactory>))]
	public void FileSystem_DriveInfoFactory_SynchronousMethods_ShouldRegisterCall(
		string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.DriveInfo.ShouldOnlyContain(name, parameters, expectation);
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
	[ClassData(typeof(SynchronousFileSystemMethods<IFileInfo>))]
	public void FileSystem_FileInfo_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.FileInfo[DummyPath].ShouldOnlyContain(name, parameters, expectation);
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

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[ClassData(typeof(AsynchronousFileSystemMethods<FileSystemStream>))]
	public async Task FileSystem_FileStream_AsynchronousMethods_ShouldRegisterCall(
		string expectation,
		Func<MockFileSystem, Task> asynchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		await asynchronousCall(sut);

		sut.Statistics.FileStream[DummyPath].ShouldOnlyContain(name, parameters, expectation);
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_FileStream_Read_WithSpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileSystemStream stream = sut.FileStream.New(DummyPath, FileMode.OpenOrCreate);
		byte[] buffer = "foo"u8.ToArray();
		Span<byte> span = buffer.AsSpan();

		_ = stream.Read(span);

		sut.Statistics.FileStream[DummyPath].Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(FileSystemStream.Read) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<byte>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<FileSystemStream>))]
	public void FileSystem_FileStream_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.FileStream[DummyPath].ShouldOnlyContain(name, parameters, expectation);
	}

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_FileStream_Write_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileSystemStream stream = sut.FileStream.New(DummyPath, FileMode.OpenOrCreate);
		byte[] buffer = "foo"u8.ToArray();
		ReadOnlySpan<byte> span = buffer.AsSpan();

		stream.Write(span);

		sut.Statistics.FileStream[DummyPath].Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(FileSystemStream.Write) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<byte>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IFileStreamFactory>))]
	public void FileSystem_FileStreamFactory_SynchronousMethods_ShouldRegisterCall(
		string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.FileStream.ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IFileSystemWatcher>))]
	public void FileSystem_FileSystemWatcher_SynchronousMethods_ShouldRegisterCall(
		string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.FileSystemWatcher[DummyPath]
			.ShouldOnlyContain(name, parameters, expectation);
	}

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IFileSystemWatcherFactory>))]
	public void FileSystem_FileSystemWatcherFactory_SynchronousMethods_ShouldRegisterCall(
		string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.FileSystemWatcher.ShouldOnlyContain(name, parameters, expectation);
	}

#if FEATURE_PATH_ADVANCED
	[Fact]
	public void FileSystem_Path_EndsInDirectorySeparator_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.EndsInDirectorySeparator(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.EndsInDirectorySeparator) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_GetDirectoryName_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.GetDirectoryName(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.GetDirectoryName) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_GetExtension_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.GetExtension(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.GetExtension) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_GetFileName_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.GetFileName(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.GetFileName) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_GetFileNameWithoutExtension_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.GetFileNameWithoutExtension(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.GetFileNameWithoutExtension) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_GetPathRoot_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.GetPathRoot(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.GetPathRoot) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_HasExtension_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.HasExtension(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.HasExtension) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_IsPathFullyQualified_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.IsPathFullyQualified(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.IsPathFullyQualified) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_IsPathRooted_WithReadOnlySpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer = "foo".ToCharArray();
		ReadOnlySpan<char> span = buffer.AsSpan();

		sut.Path.IsPathRooted(span);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.IsPathRooted) &&
				c.Parameters.Length == 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_Join_WithFourReadOnlySpans_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer1 = "foo".ToCharArray();
		char[] buffer2 = "bar".ToCharArray();
		char[] buffer3 = "xyz".ToCharArray();
		char[] buffer4 = "abc".ToCharArray();
		ReadOnlySpan<char> span1 = buffer1.AsSpan();
		ReadOnlySpan<char> span2 = buffer2.AsSpan();
		ReadOnlySpan<char> span3 = buffer3.AsSpan();
		ReadOnlySpan<char> span4 = buffer4.AsSpan();

		sut.Path.Join(span1, span2, span3, span4);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length >= 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer1, o => o.WithStrictOrdering());

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length >= 2)
			.Which.Parameters[1].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer2, o => o.WithStrictOrdering());

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length >= 3)
			.Which.Parameters[2].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer3, o => o.WithStrictOrdering());

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length == 4)
			.Which.Parameters[3].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer4, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_Join_WithThreeReadOnlySpans_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer1 = "foo".ToCharArray();
		char[] buffer2 = "bar".ToCharArray();
		char[] buffer3 = "xyz".ToCharArray();
		ReadOnlySpan<char> span1 = buffer1.AsSpan();
		ReadOnlySpan<char> span2 = buffer2.AsSpan();
		ReadOnlySpan<char> span3 = buffer3.AsSpan();

		sut.Path.Join(span1, span2, span3);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length >= 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer1, o => o.WithStrictOrdering());

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length >= 2)
			.Which.Parameters[1].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer2, o => o.WithStrictOrdering());

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length == 3)
			.Which.Parameters[2].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer3, o => o.WithStrictOrdering());
	}
#endif

#if FEATURE_SPAN
	[Fact]
	public void FileSystem_Path_Join_WithTwoReadOnlySpans_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		char[] buffer1 = "foo".ToCharArray();
		char[] buffer2 = "bar".ToCharArray();
		ReadOnlySpan<char> span1 = buffer1.AsSpan();
		ReadOnlySpan<char> span2 = buffer2.AsSpan();

		sut.Path.Join(span1, span2);

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length >= 1)
			.Which.Parameters[0].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer1, o => o.WithStrictOrdering());

		sut.Statistics.Path.Calls
			.Should().ContainSingle(c =>
				c.Name == nameof(Path.Join) &&
				c.Parameters.Length == 2)
			.Which.Parameters[1].Should().BeOfType<SpanProvider<char>>()
			.Which.Values.Should().BeEquivalentTo(buffer2, o => o.WithStrictOrdering());
	}
#endif

	[Theory]
	[ClassData(typeof(SynchronousFileSystemMethods<IPath>))]
	public void FileSystem_Path_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		synchronousCall(sut);

		sut.Statistics.Path.ShouldOnlyContain(name, parameters, expectation);
	}

	[Fact]
	public void FileSystem_ShouldInitializeWithEmptyStatistics()
	{
		MockFileSystem sut = new();

		sut.Statistics.File.Calls.Should().BeEmpty();
	}

	#region Helpers

	private static Func<MockFileSystem, TProperty> GetFileSystemAccessor<TProperty>()
	{
		if (typeof(TProperty) == typeof(IDirectory))
		{
			return m => (TProperty)m.Directory;
		}

		if (typeof(TProperty) == typeof(IDirectoryInfo))
		{
			return m => (TProperty)m.DirectoryInfo.New(DummyPath);
		}

		if (typeof(TProperty) == typeof(IDirectoryInfoFactory))
		{
			return m => (TProperty)m.DirectoryInfo;
		}

		if (typeof(TProperty) == typeof(IDriveInfo))
		{
			return m => (TProperty)m.DriveInfo.New(DummyPath);
		}

		if (typeof(TProperty) == typeof(IDriveInfoFactory))
		{
			return m => (TProperty)m.DriveInfo;
		}

		if (typeof(TProperty) == typeof(IFile))
		{
			return m => (TProperty)m.File;
		}

		if (typeof(TProperty) == typeof(IFileInfo))
		{
			return m => (TProperty)m.FileInfo.New(DummyPath);
		}

		if (typeof(TProperty) == typeof(IFileInfoFactory))
		{
			return m => (TProperty)m.FileInfo;
		}

		if (typeof(TProperty) == typeof(FileSystemStream))
		{
			return m => (TProperty)(object)m.FileStream.New(DummyPath, FileMode.OpenOrCreate);
		}

		if (typeof(TProperty) == typeof(IFileStreamFactory))
		{
			return m => (TProperty)m.FileStream;
		}

		if (typeof(TProperty) == typeof(IFileSystemWatcher))
		{
			return m =>
			{
				m.Directory.CreateDirectory(DummyPath);
				// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
				CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
				_ = Task.Run(async () =>
				{
					while (!cts.Token.IsCancellationRequested)
					{
						await Task.Delay(10, cts.Token);
						m.Directory.CreateDirectory(m.Path.Combine(DummyPath, "some-directory"));
						m.Directory.Delete(m.Path.Combine(DummyPath, "some-directory"));
					}
				}, cts.Token);
				return (TProperty)m.FileSystemWatcher.New(DummyPath);
			};
		}

		if (typeof(TProperty) == typeof(IFileSystemWatcherFactory))
		{
			return m => (TProperty)m.FileSystemWatcher;
		}

		if (typeof(TProperty) == typeof(IPath))
		{
			return m => (TProperty)m.Path;
		}

		throw new NotSupportedException($"The type {typeof(TProperty)} is not supported!");
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
						T element = accessor(m);
						try
						{
							await item.Action.Invoke(element);
						}
						catch (Exception)
						{
							// Ignore any exception called here, as we only care about the statistics call registration.
						}
						finally
						{
							if (element is IDisposable disposable)
							{
								disposable.Dispose();
							}
						}
					},
					item.Name,
					item.Parameters);
			}
		}
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
						T element = accessor(m);
						try
						{
							item.Action.Invoke(element);
						}
						catch (Exception)
						{
							// Ignore any exception called here, as we only care about the statistics call registration.
						}
						finally
						{
							if (element is IDisposable disposable)
							{
								disposable.Dispose();
							}
						}
					},
					item.Name,
					item.Parameters);
			}
		}
	}

	#endregion
}

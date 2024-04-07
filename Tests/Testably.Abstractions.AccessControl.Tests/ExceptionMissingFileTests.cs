using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionMissingFileTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks),
		parameters: (int)(BaseTypes.Directory | BaseTypes.DirectoryInfo))]
	public void DirectoryOperations_WhenDirectoryIsMissing_ShouldThrowDirectoryNotFoundException(
		Action<IFileSystem, string> callback, BaseTypes baseType, MethodType exceptionType)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string path = FileSystem.Path.Combine("missing-directory", "file.txt");

		Exception? exception = Record.Exception(() =>
		{
			callback.Invoke(FileSystem, path);
		});

		switch (exceptionType)
		{
			case MethodType.Create:
				exception.Should()
					.BeNull(
						$"\n{exceptionType} on {baseType}\n was called with a missing directory");
				break;
			case MethodType.GetAccessControl:
				exception.Should()
					.BeOfType<DirectoryNotFoundException>(
						$"\n{exceptionType} on {baseType}\n was called with a missing directory")
					.Which.HResult.Should().Be(-2147024893);
				break;
			case MethodType.SetAccessControl:
				exception.Should()
					.BeNull(
						$"\n{exceptionType} on {baseType}\n was called with a missing directory");
				break;
			default:
				throw new NotSupportedException();
		}
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks),
		parameters: (int)(BaseTypes.Directory | BaseTypes.DirectoryInfo))]
	public void DirectoryOperations_WhenFileIsMissing_ShouldThrowFileNotFoundException(
		Action<IFileSystem, string> callback, BaseTypes baseType, MethodType exceptionType)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string path = "missing-file.txt";

		Exception? exception = Record.Exception(() =>
		{
			callback.Invoke(FileSystem, path);
		});

		switch (exceptionType)
		{
			case MethodType.Create:
				exception.Should()
					.BeNull($"\n{exceptionType} on {baseType}\n was called with a missing file");
				break;
			case MethodType.GetAccessControl:
				exception.Should()
					.BeOfType<DirectoryNotFoundException>(
						$"\n{exceptionType} on {baseType}\n was called with a missing file")
					.Which.HResult.Should().Be(-2147024893);
				break;
			case MethodType.SetAccessControl:
				exception.Should()
					.BeNull($"\n{exceptionType} on {baseType}\n was called with a missing file");
				break;
			default:
				throw new NotSupportedException();
		}
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks),
		parameters: (int)(BaseTypes.File | BaseTypes.FileInfo | BaseTypes.FileStream))]
	public void FileOperations_WhenDirectoryIsMissing_ShouldThrowDirectoryNotFoundException(
		Action<IFileSystem, string> callback, BaseTypes baseType, MethodType exceptionType)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string path = FileSystem.Path.Combine("missing-directory", "file.txt");

		Exception? exception = Record.Exception(() =>
		{
			callback.Invoke(FileSystem, path);
		});

		switch (exceptionType)
		{
			case MethodType.Create:
				exception.Should()
					.BeOfType<UnauthorizedAccessException>(
						$"\n{exceptionType} on {baseType}\n was called with a missing directory")
					.Which.HResult.Should().Be(-2147024891);
				break;
			case MethodType.GetAccessControl:
				exception.Should()
					.BeOfType<FileNotFoundException>(
						$"\n{exceptionType} on {baseType}\n was called with a missing directory")
					.Which.HResult.Should().Be(-2147024894);
				break;
			case MethodType.SetAccessControl:
				exception.Should()
					.BeNull(
						$"\n{exceptionType} on {baseType}\n was called with a missing directory");
				break;
			default:
				throw new NotSupportedException();
		}
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks),
		parameters: (int)(BaseTypes.File | BaseTypes.FileInfo | BaseTypes.FileStream))]
	public void FileOperations_WhenFileIsMissing_ShouldThrowFileNotFoundException(
		Action<IFileSystem, string> callback, BaseTypes baseType, MethodType exceptionType)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string path = "missing-file.txt";

		Exception? exception = Record.Exception(() =>
		{
			callback.Invoke(FileSystem, path);
		});

		switch (exceptionType)
		{
			case MethodType.Create:
				exception.Should()
					.BeNull($"\n{exceptionType} on {baseType}\n was called with a missing file");
				break;
			case MethodType.GetAccessControl:
				exception.Should()
					.BeOfType<FileNotFoundException>(
						$"\n{exceptionType} on {baseType}\n was called with a missing file")
					.Which.HResult.Should().Be(-2147024894);
				break;
			case MethodType.SetAccessControl:
				exception.Should()
					.BeNull($"\n{exceptionType} on {baseType}\n was called with a missing file");
				break;
			default:
				throw new NotSupportedException();
		}
	}

	#region Helpers

	#pragma warning disable MA0018 // Do not declare static members on generic types
	public static TheoryData<Action<IFileSystem, string>, BaseTypes, MethodType> GetFileCallbacks(
		int baseType)
	{
		TheoryData<Action<IFileSystem, string>, BaseTypes, MethodType> theoryData = new();
		foreach ((BaseTypes BaseType, MethodType MethodType, Action<IFileSystem, string> Callback)
			item in GetFileCallbackTestParameters()
				.Where(item => (item.BaseType & (BaseTypes)baseType) > 0))
		{
			theoryData.Add(item.Callback, item.BaseType, item.MethodType);
		}

		return theoryData;
	}
	#pragma warning restore MA0018 // Do not declare static members on generic types

	private static
		IEnumerable<(
			BaseTypes BaseType,
			MethodType MethodType,
			Action<IFileSystem, string> Callback
			)> GetFileCallbackTestParameters()
	{
		#pragma warning disable CA1416
		yield return (BaseTypes.Directory, MethodType.Create,
			(fileSystem, path)
				=> fileSystem.Directory.CreateDirectory(path,
					fileSystem.CreateDirectorySecurity()));
		yield return (BaseTypes.Directory, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.Directory.GetAccessControl(path));

		yield return (BaseTypes.Directory, MethodType.SetAccessControl,
			(fileSystem, path)
				=> fileSystem.Directory.SetAccessControl(path,
					fileSystem.CreateDirectorySecurity()));

		yield return (BaseTypes.DirectoryInfo, MethodType.Create,
			(fileSystem, path)
				=> fileSystem.DirectoryInfo.New(path).Create(fileSystem.CreateDirectorySecurity()));
		yield return (BaseTypes.DirectoryInfo, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.DirectoryInfo.New(path).GetAccessControl());

		yield return (BaseTypes.DirectoryInfo, MethodType.SetAccessControl,
			(fileSystem, path)
				=> fileSystem.DirectoryInfo.New(path)
					.SetAccessControl(fileSystem.CreateDirectorySecurity()));

		yield return (BaseTypes.File, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.File.GetAccessControl(path));

		yield return (BaseTypes.File, MethodType.SetAccessControl,
			(fileSystem, path)
				=> fileSystem.File.SetAccessControl(path, fileSystem.CreateFileSecurity()));

		yield return (BaseTypes.FileInfo, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.FileInfo.New(path).GetAccessControl());

		yield return (BaseTypes.FileInfo, MethodType.SetAccessControl,
			(fileSystem, path)
				=> fileSystem.FileInfo.New(path).SetAccessControl(fileSystem.CreateFileSecurity()));
		#pragma warning restore CA1416
	}

	#endregion

	#pragma warning disable MA0062 // Non-flags enums should not be marked with "FlagsAttribute"
	[Flags]
	public enum BaseTypes
	{
		Directory = 1,
		DirectoryInfo = 2,
		File = 4,
		FileInfo = 8,
		FileStream = 16,
		None = 0,
		All = ~None
	}
	#pragma warning restore MA0062 // Non-flags enums should not be marked with "FlagsAttribute"

	public enum MethodType
	{
		Create,
		GetAccessControl,
		SetAccessControl
	}
}

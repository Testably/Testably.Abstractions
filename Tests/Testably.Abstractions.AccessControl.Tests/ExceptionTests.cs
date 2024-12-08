using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Skip = Xunit.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks),
		parameters: (int)BaseTypes.All)]
	public async Task Operations_WhenPathIsEmpty_ShouldThrowArgumentException(
		Action<IFileSystem, string> callback, BaseTypes baseType, MethodType exceptionType)
	{
		Skip.IfNot(Test.RunsOnWindows || exceptionType == MethodType.GetAccessControl);

		await That(() => callback.Invoke(FileSystem, "")).Should()
			.Throw<ArgumentException>().WithHResult(-2147024809)
			.Because($"\n{exceptionType} on {baseType}\n was called with an empty path");
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks),
		parameters: (int)BaseTypes.All)]
	public async Task Operations_WhenPathIsNull_ShouldThrowArgumentNullException(
		Action<IFileSystem, string> callback, BaseTypes baseType, MethodType exceptionType)
	{
		Skip.IfNot(Test.RunsOnWindows || exceptionType == MethodType.GetAccessControl);

		await That(() => callback.Invoke(FileSystem, null!)).Should()
			.Throw<ArgumentNullException>()
			.Because($"\n{exceptionType} on {baseType}\n was called with a null path");
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks),
		parameters: (int)BaseTypes.All)]
	public async Task Operations_WhenPathIsWhiteSpace_ShouldThrowArgumentException(
		Action<IFileSystem, string> callback, BaseTypes baseType, MethodType exceptionType)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await That(() => callback.Invoke(FileSystem, "  ")).Should()
			.Throw<ArgumentException>().WithHResult(-2147024809)
			.Because($"\n{exceptionType} on {baseType}\n was called with a whitespace path");
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
				=> fileSystem.Directory.CreateDirectory(path, new DirectorySecurity()));
		yield return (BaseTypes.Directory, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.Directory.GetAccessControl(path));

		yield return (BaseTypes.Directory, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.Directory.GetAccessControl(path, AccessControlSections.None));

		yield return (BaseTypes.Directory, MethodType.SetAccessControl,
			(fileSystem, path)
				=> fileSystem.Directory.SetAccessControl(path, new DirectorySecurity()));

		yield return (BaseTypes.DirectoryInfo, MethodType.Create,
			(fileSystem, path)
				=> fileSystem.DirectoryInfo.New(path).Create(new DirectorySecurity()));
		yield return (BaseTypes.DirectoryInfo, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.DirectoryInfo.New(path).GetAccessControl());

		yield return (BaseTypes.DirectoryInfo, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.DirectoryInfo.New(path)
					.GetAccessControl(AccessControlSections.None));

		yield return (BaseTypes.DirectoryInfo, MethodType.SetAccessControl,
			(fileSystem, path)
				=> fileSystem.DirectoryInfo.New(path).SetAccessControl(new DirectorySecurity()));

		yield return (BaseTypes.File, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.File.GetAccessControl(path));

		yield return (BaseTypes.File, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.File.GetAccessControl(path, AccessControlSections.None));

		yield return (BaseTypes.File, MethodType.SetAccessControl,
			(fileSystem, path)
				=> fileSystem.File.SetAccessControl(path, new FileSecurity()));

		yield return (BaseTypes.FileInfo, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.FileInfo.New(path).GetAccessControl());

		yield return (BaseTypes.FileInfo, MethodType.GetAccessControl,
			(fileSystem, path)
				=> fileSystem.FileInfo.New(path).GetAccessControl(AccessControlSections.None));
		yield return (BaseTypes.FileInfo, MethodType.SetAccessControl,
			(fileSystem, path)
				=> fileSystem.FileInfo.New(path).SetAccessControl(new FileSecurity()));
		#pragma warning restore CA1416
	}

	#endregion

	#pragma warning disable MA0062
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
	#pragma warning restore MA0062

	public enum MethodType
	{
		Create,
		GetAccessControl,
		SetAccessControl
	}
}

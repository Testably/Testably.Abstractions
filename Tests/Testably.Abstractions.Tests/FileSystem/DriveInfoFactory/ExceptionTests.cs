using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfoFactory;

[FileSystemTests]
public partial class ExceptionTests
{
	[Theory]
	[InlineData("?invalid-drive-name")]
	[InlineData("invalid")]
	[InlineData(" ")]
	public void New_WhenDriveNameIsInvalid_ShouldThrowArgumentException(
		string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DriveInfo.New(driveName);
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[Theory]
	[MemberData(nameof(GetDriveInfoFactoryCallbacks), "")]
	public void Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IDriveInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DriveInfo);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Theory]
	[MemberData(nameof(GetDriveInfoFactoryCallbacks), (string?)null)]
	public void Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IDriveInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DriveInfo);
		});

		exception.Should().BeException<ArgumentNullException>(
			paramName: ignoreParamCheck ? null : paramName,
			because:
			$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Expression<Action<IDriveInfoFactory>>, string, bool>
		GetDriveInfoFactoryCallbacks(string? path)
	{
		TheoryData<Expression<Action<IDriveInfoFactory>>, string, bool> theoryData = new();
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IDriveInfoFactory>> Callback) item in
			GetDriveInfoFactoryCallbackTestParameters(path!)
				.Where(item => item.TestType.HasFlag(path.ToTestType())))
		{
			theoryData.Add(
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes.IgnoreParamNameCheck));
		}

		return theoryData;
	}
	#pragma warning restore MA0018

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string ParamName,
			Expression<Action<IDriveInfoFactory>> Callback)>
		GetDriveInfoFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "driveName", driveInfoFactory
			=> driveInfoFactory.New(value));
	}

	#endregion
}

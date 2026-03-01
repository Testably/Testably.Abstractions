using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfoFactory;

[FileSystemTests]
public class ExceptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	public static IEnumerable<(Expression<Action<IDriveInfoFactory>>, string, bool)>
		GetDriveInfoFactoryCallbacksWithEmptyPath()
		=> GetDriveInfoFactoryCallbacks("");

	public static IEnumerable<(Expression<Action<IDriveInfoFactory>>, string, bool)>
		GetDriveInfoFactoryCallbacksWithNullPath()
		=> GetDriveInfoFactoryCallbacks(null);

	[Test]
	[Arguments("?invalid-drive-name")]
	[Arguments("invalid")]
	[Arguments(" ")]
	public async Task New_WhenDriveNameIsInvalid_ShouldThrowArgumentException(
		string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			FileSystem.DriveInfo.New(driveName);
		}

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Test]
	[MethodDataSource(nameof(GetDriveInfoFactoryCallbacksWithEmptyPath))]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IDriveInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.DriveInfo);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetDriveInfoFactoryCallbacksWithNullPath))]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IDriveInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.DriveInfo);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	private static IEnumerable<(Expression<Action<IDriveInfoFactory>>, string, bool)>
		GetDriveInfoFactoryCallbacks(string? path)
	{
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IDriveInfoFactory>> Callback) item in
			GetDriveInfoFactoryCallbackTestParameters(path!)
				.Where(item => item.TestType.HasFlag(path.ToTestType())))
		{
			yield return (
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes.IgnoreParamNameCheck));
		}
	}

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string ParamName,
			Expression<Action<IDriveInfoFactory>> Callback)>
		GetDriveInfoFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "driveName", driveInfoFactory
			=> driveInfoFactory.New(value));
	}
}

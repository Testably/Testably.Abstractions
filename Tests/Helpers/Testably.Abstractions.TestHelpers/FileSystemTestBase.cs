using System;
using System.IO.Abstractions;
using System.Threading;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.TestHelpers;

[Retry(1)]
public abstract class FileSystemTestBase : IDisposable
{
	/// <summary>
	///     The delay in milliseconds when wanting to ensure a timeout in the test.
	/// </summary>
	public const int EnsureTimeout = 500;

	/// <summary>
	///     The delay in milliseconds when expecting a success in the test.
	/// </summary>
	public const int ExpectSuccess = 30000;

	/// <summary>
	///     The delay in milliseconds when expecting a timeout in the test.
	/// </summary>
	public const int ExpectTimeout = 30;

	/// <summary>
	///     The base path of the <see cref="FileSystem" />, which is used for all file system operations in the test.
	/// </summary>
	/// <remarks>
	///     It will be set to a random temporary path for each test, which is automatically cleaned up after the test
	///     execution. You can use it to create files and directories for testing. It is guaranteed that the base path is empty
	///     at the beginning of the test, so you can safely create files and directories without worrying about conflicts with
	///     existing files or directories.
	/// </remarks>
	public string BasePath { get; private set; }

	/// <summary>
	///     A cancellation token that can be used to cancel the test execution when it takes too long.
	/// </summary>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	///     The file system to test.
	/// </summary>
	public IFileSystem FileSystem { get; }

	/// <summary>
	///     The test environment used when simulating other operating systems.
	/// </summary>
	public Test Test { get; }

	/// <summary>
	///     The time system associdated with the <see cref="FileSystem" />.
	/// </summary>
	public ITimeSystem TimeSystem { get; }

	private readonly IDirectoryCleaner _directoryCleaner;

	protected FileSystemTestBase(FileSystemTestData testData)
	{
		(FileSystem, TimeSystem) = testData.GetAbstractions();
		_directoryCleaner = testData.GetDirectoryCleaner(FileSystem);
		BasePath = _directoryCleaner.BasePath;
		Test = testData.GetTest();
		CancellationToken = TestContext.Current!.Execution.CancellationToken;
	}

	#region IDisposable Members

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	#endregion

	/// <summary>
	///     Specifies, if brittle tests should be skipped on the real file system.
	/// </summary>
	/// <param name="condition">
	///     (optional) A condition that must be <see langword="true" /> for the test to be skipped on the
	///     real file system.
	/// </param>
	public static void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
	{
#if DEBUG
		Skip.If(Settings.BrittleTests == Settings.TestSettingStatus.AlwaysDisabled,
			$"Brittle tests are {Settings.BrittleTests}. You can enable them by setting the corresponding settings in Testably.Abstractions.TestHelpers.Settings.");
#else
		Skip.If(Settings.BrittleTests != Settings.TestSettingStatus.AlwaysEnabled,
			$"Brittle tests are {Settings.BrittleTests}. You can enable them by setting the corresponding settings in Testably.Abstractions.TestHelpers.Settings.");
#endif
	}

	/// <summary>
	///     Specifies, if long-running tests should be skipped on the real file system.
	/// </summary>
	public static void SkipIfLongRunningTestsShouldBeSkipped()
	{
#if DEBUG
		Skip.If(Settings.LongRunningTests == Settings.TestSettingStatus.AlwaysDisabled,
			$"Long-running tests are {Settings.LongRunningTests}. You can enable them by setting the corresponding settings in Testably.Abstractions.TestHelpers.Settings.");
#else
		Skip.If(Settings.LongRunningTests != Settings.TestSettingStatus.AlwaysEnabled,
			$"Long-running tests are {Settings.LongRunningTests}. You can enable them by setting the corresponding settings in Testably.Abstractions.TestHelpers.Settings.");
#endif
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_directoryCleaner.Dispose();
		}
	}
}

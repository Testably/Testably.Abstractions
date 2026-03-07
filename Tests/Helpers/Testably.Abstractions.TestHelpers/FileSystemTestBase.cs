using System;
using System.IO.Abstractions;
using System.Threading;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.TestHelpers;

[Retry(1)]
public abstract class FileSystemTestBase : IDisposable
{
	private readonly IDirectoryCleaner _directoryCleaner;

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
		                
	public string BasePath { get; private set; }
	public Test Test { get; }
	public ITimeSystem TimeSystem { get; }

	public CancellationToken CancellationToken { get; }

	public IFileSystem FileSystem { get; }

	protected FileSystemTestBase(FileSystemTestData testData)
	{
		(FileSystem, TimeSystem) = testData.GetAbstractions();
		_directoryCleaner = testData.GetDirectoryCleaner(FileSystem);
		BasePath = _directoryCleaner.BasePath;
		Test = testData.GetTest();
		CancellationToken = TestContext.Current!.Execution.CancellationToken;
	}

	/// <summary>
	///     Specifies, if brittle tests should be skipped on the real file system.
	/// </summary>
	/// <param name="condition">
	///     (optional) A condition that must be <see langword="true" /> for the test to be skipped on the
	///     real file system.
	/// </param>
	public void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
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
	public void SkipIfLongRunningTestsShouldBeSkipped()
	{
#if DEBUG
		Skip.If(Settings.LongRunningTests == Settings.TestSettingStatus.AlwaysDisabled,
			$"Long-running tests are {Settings.LongRunningTests}. You can enable them by setting the corresponding settings in Testably.Abstractions.TestHelpers.Settings.");
#else
		Skip.If(Settings.LongRunningTests != Settings.TestSettingStatus.AlwaysEnabled,
			$"Long-running tests are {Settings.LongRunningTests}. You can enable them by setting the corresponding settings in Testably.Abstractions.TestHelpers.Settings.");
#endif
	}

	/// <inheritdoc />
	public virtual void Dispose()
	{
		_directoryCleaner.Dispose();
	}
}

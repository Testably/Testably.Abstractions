﻿using System;
using System.IO.Abstractions;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     If referencing this base class, the source generator will automatically create two classes implementing your class:
///     <br />
///     - one will provide a `RealFileSystem`<br />
///     - one will provide a `MockFileSystem`<br />
///     Thus your tests run on both systems identically.
/// </summary>
/// <remarks>
///     Important: You have to mark your class as ´partial`!
/// </remarks>
public abstract class FileSystemTestBase<TFileSystem> : TestBase
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public Test Test { get; }
	public ITimeSystem TimeSystem { get; }

	// ReSharper disable once UnusedMember.Global
	protected FileSystemTestBase(
		Test test,
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		Test = test;
		FileSystem = fileSystem;
		TimeSystem = timeSystem;
	}

	protected FileSystemTestBase()
	{
		throw new NotSupportedException(
			"The SourceGenerator didn't create the corresponding files!");
	}

	/// <summary>
	///     Specifies, if brittle tests should be skipped on the real file system.
	/// </summary>
	/// <param name="condition">
	///     (optional) A condition that must be <see langword="true" /> for the test to be skipped on the
	///     real file system.
	/// </param>
	public abstract void SkipIfBrittleTestsShouldBeSkipped(bool condition = true);

	/// <summary>
	///     Specifies, if long-running tests should be skipped on the real file system.
	/// </summary>
	public abstract void SkipIfLongRunningTestsShouldBeSkipped();
}

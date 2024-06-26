﻿using System;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     If referencing this base class, the source generator will automatically create two classes implementing your class:
///     <br />
///     - one will provide a `RealTimeSystem`<br />
///     - one will provide a `MockTimeSystem`<br />
///     Thus your tests run on both systems identically.
/// </summary>
/// <remarks>
///     Important: You have to mark your class as ´partial`!
/// </remarks>
public abstract class TimeSystemTestBase<TTimeSystem> : TestBase
	where TTimeSystem : ITimeSystem
{
	public TTimeSystem TimeSystem { get; }

	// ReSharper disable once UnusedMember.Global
	protected TimeSystemTestBase(TTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	protected TimeSystemTestBase()
	{
		throw new NotSupportedException(
			"The SourceGenerator didn't create the corresponding files!");
	}

	/// <summary>
	///     Specifies, if brittle tests should be skipped on the real time system.
	/// </summary>
	/// <param name="condition">
	///     (optional) A condition that must be <see langword="true" /> for the test to be skipped on the
	///     real time system.
	/// </param>
	public abstract void SkipIfBrittleTestsShouldBeSkipped(bool condition = true);
}

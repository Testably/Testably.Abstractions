using System.Runtime.CompilerServices;

namespace Testably.Abstractions.TestHelpers;

public static class Skip
{
	public static void If(bool condition,
		[CallerArgumentExpression("condition")]
		string reason = "")
	{
		TUnit.Core.Skip.When(condition, reason);
	}

	public static void IfNot(bool condition,
		[CallerArgumentExpression("condition")]
		string reason = "")
	{
		TUnit.Core.Skip.Unless(condition, reason);
	}
}

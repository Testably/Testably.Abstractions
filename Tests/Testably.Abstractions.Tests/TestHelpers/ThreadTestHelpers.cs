using System.Threading;

namespace Testably.Abstractions.Tests.TestHelpers;

public static class ThreadTestHelpers
{
	/// <summary>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v7.0.0/src/libraries/Common/tests/System/Threading/ThreadTestHelpers.cs#L27" />
	/// </summary>
	public static Thread CreateGuardedThread(out Action waitForThread, Action start)
	{
		Exception? backgroundEx = null;
		Thread t =
			new(() =>
			{
				try
				{
					start();
				}
				catch (Exception ex)
				{
					backgroundEx = ex;
				}
			});

		void LocalCheckForThreadErrors()
		{
			if (backgroundEx != null)
			{
				throw new AggregateException(backgroundEx);
			}
		}

		waitForThread =
			() =>
			{
				Assert.True(t.Join(60000));
				LocalCheckForThreadErrors();
			};
		return t;
	}
}

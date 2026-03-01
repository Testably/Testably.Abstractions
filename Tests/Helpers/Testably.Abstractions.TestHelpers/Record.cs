using System;
using System.Threading.Tasks;

namespace Testably.Abstractions.TestHelpers;

public static class Record
{
	public static Exception? Exception(Action action)
	{
		try
		{
			action();
			return null;
		}
		catch (Exception exception)
		{
			return exception;
		}
	}

	public static async Task<Exception?> ExceptionAsync(Func<Task> action)
	{
		try
		{
			await action();
			return null;
		}
		catch (Exception exception)
		{
			return exception;
		}
	}
}

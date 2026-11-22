namespace Testably.Abstractions.AotCompatibility.TestApp;

internal static class Program
{
	/// <summary>
	///     Runs every check and returns a non-zero exit code when one of them fails.
	/// </summary>
	/// <remarks>
	///     The same program runs from the compiled output and published with Native AOT, so a check that fails only in
	///     the published run points to code that behaves differently without the JIT.
	/// </remarks>
	public static async Task<int> Main()
	{
		bool isPublished = AppContext.GetData("APP_CONTEXT_DEPS_FILES") is null;
		Console.WriteLine($"published: {isPublished}");
		int failures = 0;
		foreach (Check check in Checks.All)
		{
			string? failure = await check.Run();
			Console.WriteLine(failure is null ? $"PASS {check.Name}" : $"FAIL {check.Name}: {failure}");
			if (failure is not null)
			{
				failures++;
			}
		}

		Console.WriteLine($"{Checks.All.Length - failures} of {Checks.All.Length} checks passed");
		return failures == 0 ? 0 : 1;
	}
}

internal sealed class Check
{
	private readonly Func<Task<string?>> _run;

	public Check(string name, Func<string?> run) : this(name, () => Task.FromResult(run()))
	{
	}

	public Check(string name, Func<Task<string?>> run)
	{
		Name = name;
		_run = run;
	}

	public string Name { get; }

	/// <summary>
	///     Returns <see langword="null" /> when the check holds, otherwise a description of what went wrong.
	/// </summary>
	public async Task<string?> Run()
	{
		try
		{
			return await _run();
		}
		catch (Exception ex)
		{
			return $"unexpected {ex.GetType().Name}: {ex.Message}";
		}
	}
}

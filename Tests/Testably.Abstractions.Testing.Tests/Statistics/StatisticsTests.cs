using AutoFixture;
using AutoFixture.Kernel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed partial class StatisticsTests
{
	[Fact]
	public void MockFileSystem_ShouldInitializeWithEmptyStatistics()
	{
		MockFileSystem sut = new();

		sut.Statistics.Calls.Should().BeEmpty();
	}

	#region Helpers

	private static IEnumerable<(string Expectation,
			Action<T> Action,
			string Name,
			object?[]Parameters)>
		EnumerateSynchronousMethods<T>(Fixture fixture)
	{
		foreach (MethodInfo methodInfo in typeof(T).GetMethods()
			.Where(m => m.IsPublic && !typeof(Task).IsAssignableFrom(m.ReturnType)))
		{
			object?[] parameters = CreateMethodParameters(
				fixture, methodInfo.GetParameters()).ToArray();
			yield return (
				$"{methodInfo.Name}({string.Join(",", methodInfo.GetParameters().Select(x => GetName(x.ParameterType)))})",
				x => methodInfo.Invoke(x, parameters),
				methodInfo.Name, parameters);
		}
	}

	private static IEnumerable<(string Expectation,
		Func<T, Task> Action,
		string Name,
		object?[] Parameters)> EnumerateAsynchronousMethods<T>(Fixture fixture)
	{
		foreach (MethodInfo methodInfo in typeof(T).GetMethods()
			.Where(m => m.IsPublic && typeof(Task).IsAssignableFrom(m.ReturnType)))
		{
			object?[] parameters = CreateMethodParameters(
				fixture, methodInfo.GetParameters()).ToArray();
			yield return (
				$"{methodInfo.Name}({string.Join(",", methodInfo.GetParameters().Select(x => GetName(x.ParameterType)))})",
				x => (Task)methodInfo.Invoke(x, parameters)!,
				methodInfo.Name, parameters);
		}
	}

	private static Fixture CreateFixture()
	{
		Fixture fixture = new();
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		fixture.Register(() => new FileStreamOptions());
#endif
		return fixture;
	}

	private static IEnumerable<object?> CreateMethodParameters(
		Fixture fixture,
		ParameterInfo[] parameterInfos)
	{
		foreach (ParameterInfo parameterInfo in parameterInfos)
		{
			yield return fixture.Create(parameterInfo.ParameterType, new SpecimenContext(fixture));
		}
	}

	private static string GetName(Type type)
	{
		if (type == typeof(int))
		{
			return "int";
		}

		if (type == typeof(bool))
		{
			return "bool";
		}

		if (type == typeof(string))
		{
			return "string";
		}

		if (type.IsGenericType)
		{
			int idx = type.Name.IndexOf("`", StringComparison.Ordinal);
			if (idx > 0)
			{
				return
					$"{type.Name.Substring(0, idx)}<{string.Join(",", type.GenericTypeArguments.Select(GetName))}>";
			}

			return type.ToString();
		}

		return type.Name;
	}

	#endregion
}

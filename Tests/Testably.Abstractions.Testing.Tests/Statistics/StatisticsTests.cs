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
	public const string DummyPath = "foo";

	#region Helpers

	public abstract class GetMethods<T>
		: TheoryData<string, T, string, object?[]>
	{
		protected Fixture Fixture { get; }

		protected GetMethods()
		{
			Fixture = new Fixture();
			Fixture.Register(() => new DirectoryInfo(DummyPath));
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
			Fixture.Register(() => new FileStreamOptions());
#endif
		}

		protected IEnumerable<(string Expectation,
				Action<TProperty> Action,
				string Name,
				object?[] Parameters)>
			EnumerateSynchronousMethods<TProperty>(Fixture fixture)
		{
			foreach (MethodInfo methodInfo in typeof(TProperty).GetMethods()
				.Where(m => m is { IsPublic: true, IsSpecialName: false } && !typeof(Task).IsAssignableFrom(m.ReturnType)))
			{
				object?[] parameters = CreateMethodParameters(
					fixture, methodInfo.GetParameters()).ToArray();
				yield return (
					$"{methodInfo.Name}({string.Join(",", methodInfo.GetParameters().Select(x => GetName(x.ParameterType)))})",
					x => methodInfo.Invoke(x, parameters),
					methodInfo.Name, parameters);
			}
		}

		protected IEnumerable<(string Expectation,
			Func<TProperty, Task> Action,
			string Name,
			object?[] Parameters)> EnumerateAsynchronousMethods<TProperty>(Fixture fixture)
		{
			foreach (MethodInfo methodInfo in typeof(TProperty).GetMethods()
				.Where(m => m is { IsPublic: true, IsSpecialName: false } && typeof(Task).IsAssignableFrom(m.ReturnType)))
			{
				object?[] parameters = CreateMethodParameters(
					fixture, methodInfo.GetParameters()).ToArray();
				yield return (
					$"{methodInfo.Name}({string.Join(",", methodInfo.GetParameters().Select(x => GetName(x.ParameterType)))})",
					x => (Task)methodInfo.Invoke(x, parameters)!,
					methodInfo.Name, parameters);
			}
		}

		private static IEnumerable<object?> CreateMethodParameters(
			Fixture fixture,
			ParameterInfo[] parameterInfos)
		{
			foreach (ParameterInfo parameterInfo in parameterInfos)
			{
				yield return fixture.Create(parameterInfo.ParameterType,
					new SpecimenContext(fixture));
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
	}

	#endregion
}

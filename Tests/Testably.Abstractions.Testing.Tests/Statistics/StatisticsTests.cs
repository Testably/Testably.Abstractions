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

	[Fact]
	public async Task Statistics_ShouldSupportParallelCalls()
	{
		int parallelTasks = 100;
		MockFileSystem sut = new();
		string[] directories = Enumerable.Range(1, parallelTasks)
			.Select(i => $"foo-{i}")
			.ToArray();
		Task[] tasks = new Task[parallelTasks];

		for (int i = 0; i < directories.Length; i++)
		{
			int taskId = i;
			tasks[taskId] = Task.Run(() =>
			{
				sut.Directory.CreateDirectory(directories[taskId]);
			});
		}

		await Task.WhenAll(tasks);

		foreach (string directory in directories)
		{
			sut.Statistics.Directory.Calls
				.Should().ContainSingle(x =>
					x.Name == nameof(Directory.CreateDirectory) &&
					x.Parameters.Length == 1 &&
					x.Parameters[0].Is(directory));
		}
	}

	[Fact]
	public void Statistics_ShouldKeepCallOrder()
	{
		MockFileSystem sut = new();
		string[] directories = Enumerable.Range(1, 20)
			.Select(i => $"foo-{i}")
			.ToArray();

		foreach (string directory in directories)
		{
			sut.Directory.CreateDirectory(directory);
		}

		for (int i = 0; i < directories.Length; i++)
		{
			sut.Statistics.Directory.Calls
				.Skip(i)
				.First()
				.Parameters[0].Is(directories[i]).Should().BeTrue();
		}
	}

	public abstract class GetMethods<T>
		: TheoryData<string, T, string, object?[]>
	{
		protected Fixture Fixture { get; }

		protected GetMethods()
		{
			Fixture = new Fixture();
			Fixture.Register(() => WatcherChangeTypes.Created);
			Fixture.Register(() => 100);
			Fixture.Register(() => TimeSpan.FromMilliseconds(100));
			Fixture.Register(() => DriveInfo.GetDrives().First());
			Fixture.Register(() => new DirectoryInfo(DummyPath));
			Fixture.Register(() => new FileInfo(DummyPath));
			Fixture.Register(() => new FileStream(DummyPath, FileMode.OpenOrCreate));
			Fixture.Register(() => new FileSystemWatcher("."));
			Fixture.Register(() => (IntPtr)null!);
			Fixture.Register<Stream>(() => new MemoryStream());
			Fixture.Register(() => new MockFileSystem().FileStream
				.New("foo", FileMode.OpenOrCreate)
				.BeginRead(Array.Empty<byte>(), 0, 0, null, null));
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
			Fixture.Register(() => new FileStreamOptions());
#endif
		}

		protected IEnumerable<(string Expectation,
			Func<TProperty, Task> Action,
			string Name,
			object?[] Parameters)> EnumerateAsynchronousMethods<TProperty>(Fixture fixture)
		{
			foreach (MethodInfo methodInfo in
				typeof(TProperty).GetInterfaces().Where(i => i != typeof(IDisposable))
					.SelectMany(i => i.GetMethods())
					.Concat(typeof(TProperty).GetMethods(BindingFlags.DeclaredOnly |
					                                     BindingFlags.Public |
					                                     BindingFlags.Instance))
					.Where(m => m is { IsPublic: true, IsSpecialName: false } &&
					            typeof(Task).IsAssignableFrom(m.ReturnType)))
			{
				if (methodInfo.GetCustomAttribute<ObsoleteAttribute>() != null)
				{
					continue;
				}

				if (methodInfo.GetParameters().Any(p
					=> p.ParameterType.Name.StartsWith("Span") ||
					   p.ParameterType.Name.StartsWith("ReadOnlySpan")))
				{
					continue;
				}

				object?[] parameters = CreateMethodParameters(
					fixture, methodInfo.GetParameters()).ToArray();
				yield return (
					$"{methodInfo.Name}({string.Join(",", methodInfo.GetParameters().Select(x => GetName(x.ParameterType)))})",
					x => (Task)methodInfo.Invoke(x, parameters)!,
					methodInfo.Name, parameters);
			}
		}

		protected IEnumerable<(string Expectation,
				Action<TProperty> Action,
				string Name,
				object?[] Parameters)>
			EnumerateSynchronousMethods<TProperty>(Fixture fixture)
		{
			foreach (MethodInfo methodInfo in
				typeof(TProperty).GetInterfaces().Where(i => i != typeof(IDisposable))
					.SelectMany(i => i.GetMethods())
					.Concat(typeof(TProperty).GetMethods(BindingFlags.DeclaredOnly |
					                                     BindingFlags.Public |
					                                     BindingFlags.Instance))
					.Where(m => m is { IsPublic: true, IsSpecialName: false } &&
					            !typeof(Task).IsAssignableFrom(m.ReturnType) &&
					            !typeof(ValueTask).IsAssignableFrom(m.ReturnType)))
			{
				if (methodInfo.GetCustomAttribute<ObsoleteAttribute>() != null)
				{
					continue;
				}

				if (methodInfo.GetParameters().Any(p
					=> p.ParameterType.Name.StartsWith("Span") ||
					   p.ParameterType.Name.StartsWith("ReadOnlySpan")))
				{
					continue;
				}

				object?[] parameters = CreateMethodParameters(
					fixture, methodInfo.GetParameters()).ToArray();
				yield return (
					$"{methodInfo.Name}({string.Join(",", methodInfo.GetParameters().Select(x => GetName(x.ParameterType)))})",
					x => methodInfo.Invoke(x, parameters),
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
}

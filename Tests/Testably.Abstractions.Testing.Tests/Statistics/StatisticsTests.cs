using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.Statistics.FileSystem;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class StatisticsTests
{
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
			sut.Statistics.Directory.Methods
				.Should().ContainSingle(x =>
					x.Name == nameof(Directory.CreateDirectory) &&
					x.Parameters.Length == 1 &&
					x.Parameters[0].Is(directory));
		}

		sut.Statistics.Directory.Methods.Select(x => x.Counter).Should()
			.BeEquivalentTo(Enumerable.Range(1, directories.Length));
	}

	[Fact]
	public void Statistics_ShouldIncrementCallOrder()
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
			sut.Statistics.Directory.Methods[i]
				.Parameters[0].Is(directories[i]).Should().BeTrue();
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
			sut.Statistics.Directory.Methods
				.OrderBy(x => x.Counter)
				.Skip(i)
				.First()
				.Parameters[0].Is(directories[i]).Should().BeTrue();
		}
	}

	[Fact]
	public void Statistics_ShouldUseGlobalIncrement()
	{
		MockFileSystem sut = new();
		sut.Directory.CreateDirectory("foo");
		sut.File.WriteAllText("bar.txt", null);
		IFileInfo fileInfo = sut.FileInfo.New("bar.txt");
		using FileSystemStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read);
		_ = new StreamReader(stream).ReadToEnd();

		sut.Statistics.Directory.Methods.First()
			.Should().Match<MethodStatistic>(m => 
				m.Name == nameof(IDirectory.CreateDirectory) &&
				m.Counter == 1);
		sut.Statistics.File.Methods.First()
			.Should().Match<MethodStatistic>(m =>
				m.Name == nameof(IFile.WriteAllText) &&
				m.Counter == 2);
		sut.Statistics.FileInfo.Methods.First()
			.Should().Match<MethodStatistic>(m =>
				m.Name == nameof(IFileInfoFactory.New) &&
				m.Counter == 3);
		sut.Statistics.FileInfo["bar.txt"].Methods.First()
			.Should().Match<MethodStatistic>(m =>
				m.Name == nameof(IFileInfo.Open) &&
				// Note: Index 4 could be used internally for creating the full path of the file info.
				m.Counter >= 4);
	}

	[Fact]
	public void FileSystem_Initialize_ShouldNotRegisterStatistics()
	{
		MockFileSystem sut = new();
		sut.Initialize()
			.WithSubdirectory("d0").Initialized(d => d.WithASubdirectory())
			.WithSubdirectories("d1", "d2")
			.WithASubdirectory()
			.WithFile("f0").Which(f => f.HasBytesContent(Encoding.UTF8.GetBytes("bar")))
			.WithAFile().Which(f => f.HasStringContent("foo"));

		sut.Statistics.Directory.Methods.Should().BeEmpty();
		sut.Statistics.File.Methods.Should().BeEmpty();
	}

	[Theory]
	[InlineData(nameof(MockFileSystem.Directory), false,
		typeof(IDirectory), typeof(DirectoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.DirectoryInfo), false,
		typeof(IDirectoryInfoFactory), typeof(DirectoryInfoFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.DirectoryInfo), true,
		typeof(IDirectoryInfo), typeof(DirectoryInfoStatisticsTests))]
	[InlineData(nameof(MockFileSystem.DriveInfo), false,
		typeof(IDriveInfoFactory), typeof(DriveInfoFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.DriveInfo), true,
		typeof(IDriveInfo), typeof(DriveInfoStatisticsTests))]
	[InlineData(nameof(MockFileSystem.File), false,
		typeof(IFile), typeof(FileStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileInfo), false,
		typeof(IFileInfoFactory), typeof(FileInfoFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileInfo), true,
		typeof(IFileInfo), typeof(FileInfoStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileStream), false,
		typeof(IFileStreamFactory), typeof(FileStreamFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileStream), true,
		typeof(FileSystemStream), typeof(FileStreamStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileSystemWatcher), false,
		typeof(IFileSystemWatcherFactory), typeof(FileSystemWatcherFactoryStatisticsTests))]
	[InlineData(nameof(MockFileSystem.FileSystemWatcher), true,
		typeof(IFileSystemWatcher), typeof(FileSystemWatcherStatisticsTests))]
	[InlineData(nameof(MockFileSystem.Path), false,
		typeof(IPath), typeof(PathStatisticsTests))]
	public void ShouldHaveTestedAllFileSystemMethods(string className, bool requireInstance,
		Type mockType, Type testType)
	{
		string result = CheckMethods(className, requireInstance, mockType, testType);

		result.Should().BeEmpty();
	}

	private static string CheckMethods(string className, bool requireInstance,
		Type mockType, Type testType)
	{
		StringBuilder builder = new();

		string FirstCharToUpperAsSpan(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return string.Empty;
			}

			return
				$"{input[0].ToString().ToUpper(CultureInfo.InvariantCulture)}{input.Substring(1)}";
		}

		string GetName(Type type, bool firstCharUpperCase)
		{
			if (type.Name == "Int32&")
			{
				return "OutInt";
			}

			if (type.IsGenericType)
			{
				int idx = type.Name.IndexOf('`', StringComparison.Ordinal);
				if (idx > 0)
				{
					return
						$"{type.Name.Substring(0, idx)}<{string.Join(",", type.GenericTypeArguments.Select(x => GetName(x, firstCharUpperCase)))}>";
				}

				return type.ToString();
			}

			if (firstCharUpperCase)
			{
				if (type == typeof(int))
				{
					return "Int";
				}

				if (type == typeof(bool))
				{
					return "Bool";
				}
			}
			else
			{
				if (type == typeof(int))
				{
					return "int";
				}

				if (type == typeof(bool))
				{
					return "bool";
				}

				if (type == typeof(byte))
				{
					return "byte";
				}

				if (type == typeof(string))
				{
					return "string";
				}

				if (type == typeof(byte[]))
				{
					return "byte[]";
				}

				if (type == typeof(string[]))
				{
					return "string[]";
				}
			}

			return type.Name;
		}

		string GetDefaultValue(Type type)
		{
			if (type == typeof(string))
			{
				return "\"foo\"";
			}

			if (type == typeof(int))
			{
				return "42";
			}

			if (type == typeof(bool))
			{
				return "true";
			}

			if (type == typeof(SearchOption))
			{
				return "SearchOption.AllDirectories";
			}

			if (type == typeof(FileMode))
			{
				return "FileMode.OpenOrCreate";
			}

			if (type == typeof(FileAccess))
			{
				return "FileAccess.ReadWrite";
			}

			if (type == typeof(FileShare))
			{
				return "FileShare.ReadWrite";
			}

			if (type == typeof(SearchOption))
			{
				return "SearchOption.AllDirectories";
			}

			if (type == typeof(IEnumerable<string>) ||
			    type == typeof(string[]))
			{
				return "[\"foo\", \"bar\"]";
			}

			if (type == typeof(byte[]))
			{
				return "\"foo\"u8.ToArray()";
			}

			if (type == typeof(Encoding))
			{
				return "Encoding.UTF8";
			}

			if (type == typeof(Stream))
			{
				return "new MemoryStream()";
			}

			if (type == typeof(CancellationToken))
			{
				return "CancellationToken.None";
			}

			return "new()";
		}

		foreach (MethodInfo methodInfo in
			mockType.GetInterfaces()
				.Where(i => i != typeof(IDisposable) && i != typeof(IAsyncDisposable))
				.SelectMany(i => i.GetMethods())
				.Concat(mockType.GetMethods(BindingFlags.DeclaredOnly |
				                            BindingFlags.Public |
				                            BindingFlags.Instance))
				.Where(m => m is { IsPublic: true, IsSpecialName: false })
				.OrderBy(m => m.Name)
				.ThenBy(m => m.GetParameters().Length))
		{
			if (methodInfo.GetCustomAttribute<ObsoleteAttribute>() != null)
			{
				continue;
			}

			ParameterInfo[] parameters = methodInfo.GetParameters();

			if (Test.IsNetFramework &&
			    parameters.Any(p => p.ParameterType == typeof(SafeFileHandle)))
			{
				// SafeFileHandle cannot be instantiated on .NET Framework
				continue;
			}

			string expectedName = $"{methodInfo.Name}_{string.Join("_", methodInfo
				.GetParameters()
				.Select(x => FirstCharToUpperAsSpan(GetName(x.ParameterType, true)
					.Replace("<", "")
					.Replace(">", "")
					.Replace("IEnumerablestring", "IEnumerableString")
					.Replace("[]", "Array"))))}{(parameters.Length > 0 ? "_" : "")}ShouldRegisterCall";
			if (testType.GetMethod(expectedName) != null)
			{
				continue;
			}

			bool isAsync = typeof(Task).IsAssignableFrom(methodInfo.ReturnType);
			builder.AppendLine("\t[SkippableFact]");
			builder.Append(isAsync ? "\tpublic async Task " : "\tpublic void ");
			builder.Append(expectedName);
			builder.AppendLine("()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tMockFileSystem sut = new();");
			foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
			{
				builder.AppendLine(
					$"\t\t{GetName(parameterInfo.ParameterType, false)} {parameterInfo.Name} = {GetDefaultValue(parameterInfo.ParameterType)};");
			}

			builder.AppendLine();
			builder.AppendLine(
				$"\t\t{(isAsync ? "await " : "")}sut.{className}{(requireInstance ? ".New(\"foo\")" : "")}.{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(p => p.Name))});");
			builder.AppendLine();
			builder.AppendLine(
				$"\t\tsut.Statistics.{className}{(requireInstance ? "[\"foo\"]" : "")}.ShouldOnlyContain(nameof({mockType.Name}.{methodInfo.Name}){(parameters.Length > 0 ? ",\n\t\t" : "")}{string.Join(", ", methodInfo.GetParameters().Select(p => p.Name))});");
			builder.AppendLine("\t}");
			builder.AppendLine();
		}

		return builder.ToString();
	}
}

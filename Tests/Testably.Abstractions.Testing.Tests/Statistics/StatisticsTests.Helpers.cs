using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed partial class StatisticsTests
{
	private static class Helper
	{
		public static string CheckPropertiesAndMethods(string className, bool requireInstance,
			Type mockType, Type testType)
		{
			StringBuilder builder = new();

			foreach (PropertyInfo propertyInfo in
				mockType.GetInterfaces()
					.Where(i => i != typeof(IDisposable) && i != typeof(IAsyncDisposable))
					.SelectMany(i => i.GetProperties())
					.Concat(mockType.GetProperties(BindingFlags.DeclaredOnly |
					                               BindingFlags.Public |
					                               BindingFlags.Instance))
					.Where(p => p.IsSpecialName == false && (p.CanRead || p.CanWrite))
					.OrderBy(m => m.Name))
			{
				if (propertyInfo.GetCustomAttribute<ObsoleteAttribute>() != null ||
				    propertyInfo.Name == nameof(IFileSystemEntity.FileSystem))
				{
					continue;
				}

				if (propertyInfo.PropertyType == typeof(IContainer))
				{
					// Container cannot be overridden
					continue;
				}

				if (propertyInfo.CanRead)
				{
					CheckPropertyGetAccess(builder, propertyInfo, className, requireInstance, mockType, testType);
				}

				if (propertyInfo.CanWrite)
				{
					CheckPropertySetAccess(builder, propertyInfo, className, requireInstance, mockType, testType);
				}
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

				CheckMethodCall(builder, methodInfo, parameters, className, requireInstance, mockType, testType);
			}

			return builder.ToString();
		}

		private static void CheckMethodCall(StringBuilder builder,
			MethodInfo methodInfo,
			ParameterInfo[] parameters,
			string className,
			bool requireInstance,
			Type mockType,
			Type testType)
		{
			string expectedName = $"Method_{methodInfo.Name}_{string.Join("_", methodInfo
				.GetParameters()
				.Select(x => FirstCharToUpperAsSpan(GetName(x.ParameterType, true)
					.Replace("<", "")
					.Replace(">", "")
					.Replace("IEnumerablestring", "IEnumerableString")
					.Replace("[]", "Array"))))}{(parameters.Length > 0 ? "_" : "")}ShouldRegisterCall";
			if (testType.GetMethod(expectedName) != null)
			{
				return;
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
				$"\t\tsut.Statistics.{className}{(requireInstance ? "[\"foo\"]" : "")}.ShouldOnlyContainMethodCall(nameof({mockType.Name}.{methodInfo.Name}){(parameters.Length > 0 ? ",\n\t\t\t" : "")}{string.Join(", ", methodInfo.GetParameters().Select(p => p.Name))});");
			builder.AppendLine("\t}");
			builder.AppendLine();
		}

		private static void CheckPropertyGetAccess(StringBuilder builder,
			PropertyInfo propertyInfo,
			string className,
			bool requireInstance,
			Type mockType,
			Type testType)
		{
			string expectedName = $"Property_{propertyInfo.Name}_Get_ShouldRegisterPropertyAccess";
			if (testType.GetMethod(expectedName) != null)
			{
				return;
			}

			builder.AppendLine("\t[SkippableFact]");
			builder.Append("\tpublic void ");
			builder.Append(expectedName);
			builder.AppendLine("()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tMockFileSystem sut = new();");
			builder.AppendLine();
			builder.AppendLine(
				$"\t\t_ = sut.{className}{(requireInstance ? ".New(\"foo\")" : "")}.{propertyInfo.Name};");
			builder.AppendLine();
			builder.AppendLine(
				$"\t\tsut.Statistics.{className}{(requireInstance ? "[\"foo\"]" : "")}.ShouldOnlyContainPropertyGetAccess(nameof({mockType.Name}.{propertyInfo.Name}));");
			builder.AppendLine("\t}");
			builder.AppendLine();
		}

		private static void CheckPropertySetAccess(StringBuilder builder,
			PropertyInfo propertyInfo,
			string className,
			bool requireInstance,
			Type mockType,
			Type testType)
		{
			string expectedName = $"Property_{propertyInfo.Name}_Set_ShouldRegisterPropertyAccess";
			if (testType.GetMethod(expectedName) != null)
			{
				return;
			}

			builder.AppendLine("\t[SkippableFact]");
			builder.Append("\tpublic void ");
			builder.Append(expectedName);
			builder.AppendLine("()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tMockFileSystem sut = new();");
			builder.AppendLine(
				$"\t\t{GetName(propertyInfo.PropertyType, false)} value = {GetDefaultValue(propertyInfo.PropertyType)};");
			builder.AppendLine();
			builder.AppendLine(
				$"\t\tsut.{className}{(requireInstance ? ".New(\"foo\")" : "")}.{propertyInfo.Name} = value;");
			builder.AppendLine();
			builder.AppendLine(
				$"\t\tsut.Statistics.{className}{(requireInstance ? "[\"foo\"]" : "")}.ShouldOnlyContainPropertySetAccess(nameof({mockType.Name}.{propertyInfo.Name}));");
			builder.AppendLine("\t}");
			builder.AppendLine();
		}


		private static string FirstCharToUpperAsSpan(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return string.Empty;
			}

			return
				$"{input[0].ToString().ToUpper(CultureInfo.InvariantCulture)}{input.Substring(1)}";
		}

		private static string GetDefaultValue(Type type)
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

		private static string GetName(Type type, bool firstCharUpperCase)
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
	}
}

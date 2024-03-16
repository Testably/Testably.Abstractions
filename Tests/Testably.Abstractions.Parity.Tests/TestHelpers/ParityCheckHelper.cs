using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.Parity.Tests.TestHelpers;

internal static class ParityCheckHelper
{
	public static bool ContainsEquivalentExtensionMethod(this Type abstractionType,
		MethodInfo systemMethod)
	{
		if (abstractionType.GetMethods(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
			.Where(x => x.Name == systemMethod.Name)
			.Any(abstractionMethod => AreExtensionMethodsEqual(systemMethod, abstractionMethod)))
		{
			return true;
		}

		if (abstractionType.GetInterfaces()
			.Any(@interface => @interface.ContainsEquivalentMethod(systemMethod)))
		{
			return true;
		}

		return false;
	}

	public static bool ContainsEquivalentMethod(this Type abstractionType,
		MethodInfo systemMethod)
	{
		if (abstractionType.GetMethods(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
			.Where(x => x.Name == systemMethod.Name)
			.Any(abstractionMethod => AreMethodsEqual(systemMethod, abstractionMethod)))
		{
			return true;
		}

		if (abstractionType.GetInterfaces()
			.Any(@interface => @interface.ContainsEquivalentMethod(systemMethod)))
		{
			return true;
		}

		return false;
	}

	public static bool ContainsEquivalentMethod(this Type abstractionType,
		ConstructorInfo systemConstructor)
	{
		if (abstractionType.GetMethods(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
			.Where(x => x.Name == "New")
			.Any(abstractionMethod => AreMethodsEqual(systemConstructor, abstractionMethod)))
		{
			return true;
		}

		return false;
	}

	public static bool ContainsEquivalentProperty(this Type abstractionType,
		PropertyInfo systemProperty)
	{
		if (abstractionType.GetProperties(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
			.Where(x => x.Name == systemProperty.Name)
			.Any(abstractionProperty => ArePropertiesEqual(systemProperty, abstractionProperty)))
		{
			return true;
		}

		if (abstractionType.GetInterfaces()
			.Any(@interface => @interface.ContainsEquivalentProperty(systemProperty)))
		{
			return true;
		}

		return false;
	}

	public static bool ContainsEquivalentProperty(this Type abstractionType,
		FieldInfo systemField)
	{
		if (abstractionType.GetProperties(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
			.Where(x => x.Name == systemField.Name)
			.Any(abstractionProperty => ArePropertiesEqual(systemField, abstractionProperty)))
		{
			return true;
		}

		if (abstractionType.GetInterfaces()
			.Any(@interface => @interface.ContainsEquivalentProperty(systemField)))
		{
			return true;
		}

		return false;
	}

	public static string PrintConstructor(this ConstructorInfo constructor)
	{
		return
			$"new {constructor.DeclaringType!.Name}({string.Join(", ", constructor.GetParameters().Select(x => x.ParameterType.PrintType() + " " + x.Name))})";
	}

	public static string PrintField(this FieldInfo property, string namePrefix = "")
	{
		return
			$"{property.FieldType.PrintType()} {namePrefix}{property.Name}{{ {(property.IsInitOnly ? "get; " : "get; set; ")}}}";
	}

	public static string PrintMethod(this MethodInfo method, string namePrefix = "",
		string firstParameterPrefix = "")
	{
		return
			$"{method.ReturnType.PrintType()} {namePrefix}{method.Name}({firstParameterPrefix}{string.Join(", ", method.GetParameters().Select(x => x.ParameterType.PrintType() + " " + x.Name))})";
	}

	public static string PrintProperty(this PropertyInfo property, string namePrefix = "")
	{
		return
			$"{property.PropertyType.PrintType()} {namePrefix}{property.Name}{{ {(property.CanRead ? "get; " : "")}{(property.CanRead ? "set; " : "")}}}";
	}

	public static string PrintType(this Type type)
	{
		if (type == typeof(void))
		{
			return "void";
		}

		if (type == typeof(string))
		{
			return "string";
		}

		if (type.GenericTypeArguments.Length > 0)
		{
			return type.Name.Substring(0, type.Name.Length - 2) +
			       "<" + string.Join(",",
				       type.GenericTypeArguments.Select(x => x.PrintType())) + ">";
		}

		return type.Name;
	}

	internal static bool IsTypeNameEqual(string? systemTypeName,
		string? abstractionTypeName)
	{
		if (abstractionTypeName == null)
		{
			return systemTypeName == null;
		}

		return systemTypeName != null &&
		       (abstractionTypeName.Equals(systemTypeName) ||
		        abstractionTypeName.Equals("I" + systemTypeName) ||
		        (Parity.AcceptedTypeMapping.TryGetValue(systemTypeName,
			         out string? acceptedName) &&
		         acceptedName.Equals(abstractionTypeName)));
	}

	private static bool AreExtensionMethodsEqual(MethodInfo systemMethod,
		MethodInfo abstractionMethod)
	{
		ParameterInfo[] systemParameters = systemMethod.GetParameters();
		ParameterInfo[] abstractionParameters = abstractionMethod.GetParameters();
		if (systemParameters.Length - 1 != abstractionParameters.Length)
		{
			return false;
		}

		for (int i = 0; i < systemParameters.Length - 1; i++)
		{
			if (!string.Equals(systemParameters[i + 1].Name,
				abstractionParameters[i].Name))
			{
				return false;
			}

			if (!Equals(systemParameters[i + 1].DefaultValue,
				abstractionParameters[i].DefaultValue))
			{
				return false;
			}

			if (!IsTypeNameEqual(systemParameters[i + 1].ParameterType.Name,
				abstractionParameters[i].ParameterType.Name))
			{
				return false;
			}
		}

		if (!IsTypeNameEqual(systemMethod.ReturnParameter.ParameterType.Name,
			abstractionMethod.ReturnParameter.ParameterType.Name))
		{
			return false;
		}

		return true;
	}

	private static bool AreMethodsEqual(MethodInfo systemMethod,
		MethodInfo abstractionMethod)
	{
		ParameterInfo[] systemParameters = systemMethod.GetParameters();
		ParameterInfo[] abstractionParameters = abstractionMethod.GetParameters();
		if (systemParameters.Length != abstractionParameters.Length)
		{
			return false;
		}

		for (int i = 0; i < systemParameters.Length; i++)
		{
			if (!string.Equals(systemParameters[i].Name, abstractionParameters[i].Name))
			{
				return false;
			}

			if (!Equals(systemParameters[i].DefaultValue,
				abstractionParameters[i].DefaultValue))
			{
				return false;
			}

			if (!IsTypeNameEqual(systemParameters[i].ParameterType.Name,
				abstractionParameters[i].ParameterType.Name))
			{
				return false;
			}
		}

		if (!IsTypeNameEqual(systemMethod.ReturnParameter.ParameterType.Name,
			abstractionMethod.ReturnParameter.ParameterType.Name))
		{
			return false;
		}

		return true;
	}

	private static bool AreMethodsEqual(ConstructorInfo systemConstructor,
		MethodInfo abstractionMethod)
	{
		ParameterInfo[] systemParameters = systemConstructor.GetParameters();
		ParameterInfo[] abstractionParameters = abstractionMethod.GetParameters();
		if (systemParameters.Length != abstractionParameters.Length)
		{
			return false;
		}

		for (int i = 0; i < systemParameters.Length; i++)
		{
			if (!string.Equals(systemParameters[i].Name, abstractionParameters[i].Name,
				StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (!Equals(systemParameters[i].DefaultValue,
				abstractionParameters[i].DefaultValue))
			{
				return false;
			}

			if (!IsTypeNameEqual(systemParameters[i].ParameterType.Name,
				abstractionParameters[i].ParameterType.Name))
			{
				return false;
			}
		}

		if (!IsTypeNameEqual(systemConstructor.DeclaringType!.Name,
			abstractionMethod.ReturnParameter.ParameterType.Name))
		{
			return false;
		}

		return true;
	}

	private static bool ArePropertiesEqual(FieldInfo systemField,
		PropertyInfo abstractionProperty)
	{
		if (!IsTypeNameEqual(systemField.FieldType.Name,
			abstractionProperty.PropertyType.Name))
		{
			return false;
		}

		if (systemField.IsInitOnly)
		{
			return abstractionProperty.CanRead && !abstractionProperty.CanWrite;
		}

		if (systemField.IsPublic)
		{
			return abstractionProperty.CanRead && abstractionProperty.CanWrite;
		}

		return true;
	}

	private static bool ArePropertiesEqual(PropertyInfo systemProperty,
		PropertyInfo abstractionProperty)
	{
		if (!IsTypeNameEqual(systemProperty.PropertyType.Name,
			abstractionProperty.PropertyType.Name))
		{
			return false;
		}

		bool canRead = systemProperty.CanRead &&
		               systemProperty.GetMethod?.Attributes
			               .HasFlag(MethodAttributes.Private) == false;
		if (canRead != abstractionProperty.CanRead)
		{
			return false;
		}

		bool canWrite = systemProperty.CanWrite &&
		                systemProperty.SetMethod?.Attributes
			                .HasFlag(MethodAttributes.Private) == false;
		if (canWrite != abstractionProperty.CanWrite)
		{
			return false;
		}

		return true;
	}
}

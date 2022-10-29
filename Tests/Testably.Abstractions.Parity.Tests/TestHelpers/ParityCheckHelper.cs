using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.Parity.Tests.TestHelpers;

internal static class ParityCheckHelper
{
	public static bool ContainsEquivalentMethod(this Type abstractionType,
	                                            MethodInfo systemMethod)
	{
		foreach (MethodInfo abstractionMethod in abstractionType
		   .GetMethods(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
		   .Where(x => x.Name == systemMethod.Name))
		{
			if (AreMethodsEqual(systemMethod, abstractionMethod))
			{
				return true;
			}
		}

		foreach (Type @interface in abstractionType.GetInterfaces())
		{
			if (@interface.ContainsEquivalentMethod(systemMethod))
			{
				return true;
			}
		}

		return false;
	}

	public static bool ContainsEquivalentMethod(this Type abstractionType,
	                                            ConstructorInfo systemConstructor)
	{
		foreach (MethodInfo abstractionMethod in abstractionType
		   .GetMethods(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
		   .Where(x => x.Name == "New"))
		{
			if (AreMethodsEqual(systemConstructor, abstractionMethod))
			{
				return true;
			}
		}

		return false;
	}

	public static bool ContainsEquivalentProperty(this Type abstractionType,
	                                              PropertyInfo systemProperty)
	{
		foreach (PropertyInfo abstractionProperty in abstractionType
		   .GetProperties(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
		   .Where(x => x.Name == systemProperty.Name))
		{
			if (ArePropertiesEqual(systemProperty, abstractionProperty))
			{
				return true;
			}
		}

		foreach (Type @interface in abstractionType.GetInterfaces())
		{
			if (@interface.ContainsEquivalentProperty(systemProperty))
			{
				return true;
			}
		}

		return false;
	}

	public static bool ContainsEquivalentProperty(this Type abstractionType,
	                                              FieldInfo systemField)
	{
		foreach (PropertyInfo abstractionProperty in abstractionType
		   .GetProperties(
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy)
		   .Where(x => x.Name == systemField.Name))
		{
			if (ArePropertiesEqual(systemField, abstractionProperty))
			{
				return true;
			}
		}

		foreach (Type @interface in abstractionType.GetInterfaces())
		{
			if (@interface.ContainsEquivalentProperty(systemField))
			{
				return true;
			}
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

	public static string PrintMethod(this MethodInfo method, string namePrefix = "")
	{
		return
			$"{method.ReturnType.PrintType()} {namePrefix}{method.Name}({string.Join(", ", method.GetParameters().Select(x => x.ParameterType.PrintType() + " " + x.Name))})";
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

		var canRead = systemProperty.CanRead &&
		               !systemProperty.GetMethod.Attributes
			              .HasFlag(MethodAttributes.Private);
		if (canRead != abstractionProperty.CanRead)
		{
			return false;
		}

		var canWrite = systemProperty.CanWrite &&
		               !systemProperty.SetMethod.Attributes
			              .HasFlag(MethodAttributes.Private);
		if (canWrite != abstractionProperty.CanWrite)
		{
			return false;
		}

		return true;
	}

	private static bool IsTypeNameEqual(string? systemTypeName,
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
}
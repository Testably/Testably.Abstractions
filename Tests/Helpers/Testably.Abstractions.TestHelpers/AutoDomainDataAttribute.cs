using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     Extension of <see cref="AutoDataAttribute" /> that uses applies domain-specific customizations.
/// </summary>
public class AutoDomainDataAttribute : AutoDataAttribute
{
	/// <summary>
	///     Adds an additional <see cref="ICustomization" /> that is applied for only this test.
	/// </summary>
	public Type? CustomizeWith
	{
		get;
		set
		{
			field = value;
			_fixtureFactory.CustomizeWith(value);
		}
	}

	private readonly DomainFixtureFactory _fixtureFactory;

	/// <summary>
	///     Extension of <see cref="AutoDataAttribute" /> that uses applies domain-specific customizations.
	/// </summary>
	public AutoDomainDataAttribute() : this(new DomainFixtureFactory())
	{
	}

	private AutoDomainDataAttribute(DomainFixtureFactory fixtureFactory)
		: base(fixtureFactory.GetFixtureFactory)
	{
		_fixtureFactory = fixtureFactory;
	}

	private sealed class DomainFixtureFactory
	{
		private static Lazy<ICustomization[]> _domainCustomisation { get; } = new(Initialize);
		private ICustomization? _customizeWith;

		public void CustomizeWith(Type? type)
		{
			Type customizationInterface = typeof(ICustomization);
			if (type != null &&
			    customizationInterface.IsAssignableFrom(type))
			{
				try
				{
					_customizeWith = (ICustomization?)Activator.CreateInstance(type);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(
						$"Could not instantiate customization with '{type.Name}'!", ex);
				}
			}
		}

		public Fixture GetFixtureFactory()
		{
			Fixture fixture = new();
			fixture.Customize(new AutoNSubstituteCustomization());
			foreach (ICustomization domainCustomization in _domainCustomisation.Value)
			{
				domainCustomization.Customize(fixture);
			}

			if (_customizeWith != null)
			{
				fixture.Customize(_customizeWith);
			}

			return fixture;
		}

		private static ICustomization[] Initialize()
		{
			List<ICustomization> domainCustomizations = new();
			Type autoDataCustomizationInterface = typeof(IAutoDataCustomization);
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					foreach (Type type in assembly.GetTypes()
						.Where(x => x.IsClass &&
						            autoDataCustomizationInterface.IsAssignableFrom(x)))
					{
						try
						{
							IAutoDataCustomization? domainCustomization =
								(IAutoDataCustomization?)Activator.CreateInstance(type);
							if (domainCustomization != null)
							{
								domainCustomizations.Add(domainCustomization);
							}
						}
						catch (Exception ex)
						{
							throw new InvalidOperationException(
								$"Could not instantiate auto data customization '{type.Name}'!",
								ex);
						}
					}
				}
				catch (ReflectionTypeLoadException)
				{
					// Ignore assemblies that can't be loaded
				}
			}

			return domainCustomizations.ToArray();
		}
	}
}

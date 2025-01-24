using AutoFixture;
using AutoFixture.Xunit3;
using AutoFixture.AutoNSubstitute;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     Extension of <see cref="AutoDataAttribute"/> that uses applies domain-specific customizations.
/// </summary>
public class AutoDomainDataAttribute : AutoDataAttribute
{
	private Type? _customizeWith;
	private readonly DomainFixtureFactory _fixtureFactory;

	/// <summary>
	///     Extension of <see cref="AutoDataAttribute"/> that uses applies domain-specific customizations.
	/// </summary>
	public AutoDomainDataAttribute() : this(new DomainFixtureFactory())
	{
	}

	private AutoDomainDataAttribute(DomainFixtureFactory fixtureFactory)
		: base(fixtureFactory.GetFixtureFactory)
	{
		_fixtureFactory = fixtureFactory;
	}

	/// <summary>
	///     Adds an additional <see cref="ICustomization"/> that is applied for only this test.
	/// </summary>
	public Type? CustomizeWith
	{
		get
		{
			return _customizeWith;
		}
		set
		{
			_customizeWith = value;
			_fixtureFactory.CustomizeWith(value);
		}
	}

	private sealed class DomainFixtureFactory
	{
		private ICustomization? _customizeWith;
		private static Lazy<ICustomization[]> _domainCustomisation { get; } = new(Initialize);

		public Fixture GetFixtureFactory()
		{
			var fixture = new Fixture();
			fixture.Customize(new AutoNSubstituteCustomization());
			foreach (var domainCustomization in _domainCustomisation.Value)
			{
				domainCustomization.Customize(fixture);
			}
			if (_customizeWith != null)
			{
				fixture.Customize(_customizeWith);
			}
			return fixture;
		}

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

		private static ICustomization[] Initialize()
		{
			List<ICustomization> domainCustomizations = new();
			Type autoDataCustomizationInterface = typeof(IAutoDataCustomization);
			foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(x => x.IsClass && autoDataCustomizationInterface.IsAssignableFrom(x)))
			{
				try
				{
					IAutoDataCustomization? domainCustomization = (IAutoDataCustomization?)Activator.CreateInstance(type);
					if (domainCustomization != null)
					{
						domainCustomizations.Add(domainCustomization);
					}
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(
						$"Could not instantiate auto data customization '{type.Name}'!", ex);
				}
			}
			return domainCustomizations.ToArray();
		}
	}
}

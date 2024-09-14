using AutoFixture;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     Marks customizations of <see cref="IFixture"/> that should always be applied when using the <see cref="AutoDomainDataAttribute"/>.
/// </summary>
public interface IAutoDataCustomization : ICustomization
{
}

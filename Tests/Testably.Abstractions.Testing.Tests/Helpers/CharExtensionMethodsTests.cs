using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class CharExtensionMethodsTests
{
	[Theory]
	[InlineData('A')]
	[InlineData('Z')]
	[InlineData('a')]
	[InlineData('z')]
	[InlineData('M')]
	[InlineData('d')]
	public void IsAsciiLetter_WithAsciiLetterChar_ShouldReturnTrue(char c)
	{
		bool result = c.IsAsciiLetter();

		result.Should().BeTrue();
	}

	[Theory]
	[InlineData((char)64)]
	[InlineData((char)91)]
	[InlineData((char)96)]
	[InlineData((char)123)]
	[InlineData((char)55)]
	[InlineData((char)0)]
	[InlineData((char)127)]
	public void IsAsciiLetter_WithNonAsciiLetterChar_ShouldReturnFalse(char c)
	{
		bool result = c.IsAsciiLetter();

		result.Should().BeFalse();
	}
}

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
	public async Task IsAsciiLetter_WithAsciiLetterChar_ShouldReturnTrue(char c)
	{
		bool result = c.IsAsciiLetter();

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData((char)64)]
	[InlineData((char)91)]
	[InlineData((char)96)]
	[InlineData((char)123)]
	[InlineData((char)55)]
	[InlineData((char)0)]
	[InlineData((char)127)]
	public async Task IsAsciiLetter_WithNonAsciiLetterChar_ShouldReturnFalse(char c)
	{
		bool result = c.IsAsciiLetter();

		await That(result).IsFalse();
	}
}

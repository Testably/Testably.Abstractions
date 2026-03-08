using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class CharExtensionMethodsTests
{
	[Test]
	[Arguments('A')]
	[Arguments('Z')]
	[Arguments('a')]
	[Arguments('z')]
	[Arguments('M')]
	[Arguments('d')]
	public async Task IsAsciiLetter_WithAsciiLetterChar_ShouldReturnTrue(char c)
	{
		bool result = c.IsAsciiLetter();

		await That(result).IsTrue();
	}

	[Test]
	[Arguments((char)64)]
	[Arguments((char)91)]
	[Arguments((char)96)]
	[Arguments((char)123)]
	[Arguments((char)55)]
	[Arguments((char)0)]
	[Arguments((char)127)]
	public async Task IsAsciiLetter_WithNonAsciiLetterChar_ShouldReturnFalse(char c)
	{
		bool result = c.IsAsciiLetter();

		await That(result).IsFalse();
	}
}

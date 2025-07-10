using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

public class TestDataGetEncodingDifference : IEnumerable<TheoryDataRow<string, Encoding, Encoding>>
{
	private const string SpecialCharactersContent = "_€_Ä_Ö_Ü";

	#region IEnumerable<TheoryDataRow<string,Encoding,Encoding>> Members

	public IEnumerator<TheoryDataRow<string, Encoding, Encoding>> GetEnumerator()
	{
		yield return new TheoryDataRow<string, Encoding, Encoding>(SpecialCharactersContent,
			Encoding.ASCII, Encoding.UTF8);
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

	#endregion
}

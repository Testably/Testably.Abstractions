using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

public class TestDataGetEncodingDifference : IEnumerable<object[]>
{
	private const string SpecialCharactersContent = "_€_Ä_Ö_Ü";

	#region IEnumerable<object[]> Members

	public IEnumerator<object[]> GetEnumerator()
	{
		yield return new object[]
		{
			SpecialCharactersContent, Encoding.ASCII, Encoding.UTF8
		};
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion
}

﻿using NUnit.Framework;
using System;

namespace Testably.Abstractions.Api.Tests;

public sealed class ApiAcceptance
{
	/// <summary>
	///     Execute this test to update the expected public API to the current API surface.
	/// </summary>
	[TestCase]
	[Explicit]
	public void AcceptApiChanges()
	{
		string[] assemblyNames =
		[
			"Testably.Abstractions",
			"Testably.Abstractions.AccessControl",
			"Testably.Abstractions.Compression",
			"Testably.Abstractions.FileSystem.Interface",
			"Testably.Abstractions.Interface",
			"Testably.Abstractions.Testing",
		];

		foreach (string assemblyName in assemblyNames)
		{
			foreach (string framework in Helper.GetTargetFrameworks())
			{
				string publicApi = Helper.CreatePublicApi(framework, assemblyName)
					.Replace("\n", Environment.NewLine);
				Helper.SetExpectedApi(framework, assemblyName, publicApi);
			}
		}

		Assert.That(assemblyNames, Is.Not.Empty);
	}
}

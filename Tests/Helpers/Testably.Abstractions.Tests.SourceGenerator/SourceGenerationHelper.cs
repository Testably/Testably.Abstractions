﻿using System;
using System.Text;

namespace Testably.Abstractions.Tests.SourceGenerator;

#pragma warning disable MA0051
internal static partial class SourceGenerationHelper
{
	public static string GenerateCollectionFixtures()
	{
		StringBuilder? sb = GetSourceBuilder();
		sb.Append("""
		          using Xunit;

		          namespace Testably.Abstractions.TestHelpers.Settings;

		          [CollectionDefinition("RealFileSystemTests")]
		          public class FileSystemTestSettingsFixture : ICollectionFixture<TestSettingsFixture>
		          {
		          	// This class has no code, and is never created. Its purpose is simply
		          	// to be the place to apply [CollectionDefinition] and all the
		          	// ICollectionFixture<> interfaces.
		          }

		          [CollectionDefinition("RealTimeSystemTests")]
		          public class TimeSystemTestSettingsFixture : ICollectionFixture<TestSettingsFixture>
		          {
		          	// This class has no code, and is never created. Its purpose is simply
		          	// to be the place to apply [CollectionDefinition] and all the
		          	// ICollectionFixture<> interfaces.
		          }
		          """);
		return sb.ToString();
	}
	public static string GenerateMarkerAttributes()
	{
		StringBuilder? sb = GetSourceBuilder();
		sb.Append("""
		          namespace Testably.Abstractions.TestHelpers
		          {
		              /// <summary>
		              ///     Marks a class to contain tests for the <see cref="IFileSystem"/> that runs against mock and real implementations.
		              /// </summary>
		              /// <remarks>
		              ///     The class must be abstract and partial and will get an `IFileSystem FileSystem` property injected
		              /// </remarks>
		              [System.AttributeUsage(System.AttributeTargets.Class)]
		              public class FileSystemTestsAttribute : System.Attribute
		              {
		              }
		              
		              /// <summary>
		              ///     Marks a class to contain tests for the <see cref="ITimeSystem"/> that runs against mock and real implementations.
		              /// </summary>
		              /// <remarks>
		              ///     The class must be abstract and partial and will get an `ITimeSystem TimeSystem` property injected
		              /// </remarks>
		              [System.AttributeUsage(System.AttributeTargets.Class)]
		              public class TimeSystemTestsAttribute : System.Attribute
		              {
		              }
		          
		              /// <summary>
		              ///     Marks a class to contain tests for the <see cref="IRandomSystem"/> that runs against mock and real implementations.
		              /// </summary>
		              /// <remarks>
		              ///     The class must be abstract and partial and will get an `IRandomSystem RandomSystem` property injected
		              /// </remarks>
		              [System.AttributeUsage(System.AttributeTargets.Class)]
		              public class RandomSystemTestsAttribute : System.Attribute
		              {
		              }
		          }
		          """);
		return sb.ToString();
	}

	public static string GenerateTestClasses(ClassModel model)
		=> model.Type switch
		{
			ClassModelType.FileSystem => GenerateFileSystemTestClasses(model),
			ClassModelType.TimeSystem => GenerateTimeSystemTestClasses(model),
			ClassModelType.RandomSystem => GenerateRandomSystemTestClasses(model),
			_ => throw new NotSupportedException(),
		};

	private static StringBuilder GetSourceBuilder()
		=> new(
			@"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by ""Testably.Abstractions.Tests.SourceGenerator"".
// 
//   Changes to this file may cause incorrect behavior
//   and will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
");
}
#pragma warning restore MA0051
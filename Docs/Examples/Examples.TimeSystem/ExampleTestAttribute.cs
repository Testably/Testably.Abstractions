#if EXECUTE_EXAMPLE_TESTS
using Xunit;

namespace Examples.TimeSystem;

/// <summary>
///     Activated example test attribute will execute the example unit tests. This was achieved by setting the
///     "<c>EXECUTE_EXAMPLE_TESTS</c>" variable.
///     <para />
///     Remove this variable, if you no longer want to execute the example tests.
/// </summary>
public class ExampleTestAttribute
    : FactAttribute
{
}
#else
using System;

namespace Examples.TimeSystem;

/// <summary>
///     Used to skip example unit tests to illustrate code examples in this project.
///     <para />
///     In order for these tests to execute, set the variable "<c>EXECUTE_EXAMPLE_TESTS</c>", e.g. in the .csproj file
///     <br />
///     see example for a snippet to add to the .csproj file.
/// </summary>
/// <example>
///     <PropertyGroup>
///         <DefineConstants>$(DefineConstants);EXECUTE_EXAMPLE_TESTS</DefineConstants>
///     </PropertyGroup>
/// </example>
public class ExampleTestAttribute
    : Attribute
{
}

#endif
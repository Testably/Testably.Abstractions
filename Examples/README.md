# Timer Example
This example illustrates the use of the timer extensions in Testably.Abstractions.Extensions.

It uses the `CreateTimer` extension methods on the `ITimeSystem` interface to start a background timer that executes a callback in a fixed interval.
The corresponding test class includes two example tests that validate, that the callback is executed multiple times and how the exception handling is working.
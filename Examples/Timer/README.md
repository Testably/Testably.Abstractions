# Timer example
This example illustrates the use of the timer extensions in Testably.Abstractions.Extensions.

It uses the `CreateTimer` extension methods on the `ITimeSystem` interface to start a background timer that executes a callback in a fixed interval.

```csharp
var timer = _timeSystem.CreateTimer(
    TimeSpan.FromSeconds(2),
    cancellationToken => Console.WriteLine("Executed!"));
    
timer.Start(cancellationToken);
```

By cancelling the `cancellationToken` (or by disposing of the timer), the background thread stops.

## Testing
The corresponding test class includes two example tests that validate, that the callback is executed multiple times and how the exception handling is working.

Both test cases execute in a matter of milliseconds, even if the interval is defined as 10 seconds.

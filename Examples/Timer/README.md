# Working with timers
Example implementation of a timer on top of `ITimeSystem`. It uses the `ITask.Delay` methods, to implementing a customer timer class which can be unit tested.

## Example
Included is a simple example implementation and corresponding unit tests to verify the behaviour.

```csharp
        ITimer timer = timeSystem.CreateTimer(
            interval,
            _ =>
            {
                // Do something
            });
        timer.Start();
```

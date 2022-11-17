# Working with timers
Example implementation of a timer on top of `ITimeSystem`. It uses the `ITask.Delay` methods, to implementing a customer timer class which can be unit tested.

## Example
Included is a simple example implementation and corresponding unit tests to verify the behaviour.

```csharp
    TimeSpan interval = TimeSpan.FromMinutes(5);
    ITimer timer = timeSystem.CreateTimer(
        interval,
        _ =>
        {
            // This callback is executed every 5 minutes until `timer.Stop()` is called.
        });
    timer.Start();
```

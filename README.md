# AwaitableObject
A library that lets you wrap and await any awaitable object in C#.

**NOTE: This library is slower than direct await about 10~12 times, this will cause a huge performance impact when sensitive to async/await performance .**

# Usage

```csharp
object obj = ...;
await AwaitableWrapper.Wrap(obj);

object obj = Task.FromResult(1);
int result = await AwaitableWrapper.Wrap<int>(obj);
```

# How is works?

1. Check object type, throw if incoming object is not awaitable.
2. Call 'GetAwaiter' method and box return value to `object`.
3. Wrap, generate and cache `OnComplete`,`get_IsCompleted`,`GetResult` from the awaiter which returned from `GetAwaiter` method.
4. Create `AwaiterWrapper` object and pass cached methods to it.

# Benchmark
```
| Method                       | Mean       | Error     | StdDev    |
|----------------------------- |-----------:|----------:|----------:|
| DirectAwaitTask              |   9.996 ns | 0.0746 ns | 0.0661 ns |
| WrappedTask                  | 101.225 ns | 2.0640 ns | 2.1196 ns |
| DirectAwaitValueTask         |   9.233 ns | 0.0876 ns | 0.0732 ns |
| WrappedValueTask             | 111.555 ns | 2.1622 ns | 2.1236 ns |
```

using System.Runtime.CompilerServices;

namespace AwaitableObject;

public readonly struct AwaiterWrapper<T>(
    Action<Action> wrappedOnComplete, 
    Func<bool> wrappedIsCompleted, 
    Func<T> wrappedGetResult) : INotifyCompletion
{
    public void OnCompleted(Action continuation)
    {
        wrappedOnComplete(continuation);
    }
    
    public bool IsCompleted => wrappedIsCompleted();
    public T GetResult() => wrappedGetResult();
}
namespace AwaitableObject;

public readonly struct AwaitableWrapper<TReturn>
{
    
    private readonly AwaiterWrapper<TReturn> _wrapper;
    
    public AwaitableWrapper(object obj)
    {
        var awaiter = AwaitableWrapperHelper<TReturn>.GetAwaiterGetter(obj.GetType())(obj);
        var methods = AwaitableWrapperHelper<TReturn>.GetWrappedAwaiterMethods(awaiter.GetType());

        _wrapper = new AwaiterWrapper<TReturn>(
            methods.OnCompleteGetter(awaiter),
            methods.IsCompleteGetter(awaiter),
            methods.GetResult(awaiter));
    }
    
    public AwaiterWrapper<TReturn> GetAwaiter()
    {
        return _wrapper;
    }
}

public readonly struct AwaitableWrapper(object obj)
{
    
    private readonly AwaitableWrapper<object> _wrapper = new(obj);

    public AwaiterWrapper<object> GetAwaiter()
    {
        return _wrapper.GetAwaiter();
    }

    public static AwaitableWrapper Wrap(object obj) => new(obj);

    public static AwaitableWrapper<TReturn> Wrap<TReturn>(object obj) => new(obj);
}

using System.Linq.Expressions;
using System.Reflection;

namespace AwaitableObject;

public static class AwaitableWrapperHelper<TReturn>
{
    public readonly record struct WrappedAwaitableDelegates(
        Func<object, Action<Action>> OnCompleteGetter, 
        Func<object, Func<bool>> IsCompleteGetter, 
        Func<object, Func<TReturn>> GetResult);

    private const BindingFlags PublicInstanceMethodFlags = BindingFlags.Public | BindingFlags.Instance;

    private static readonly Dictionary<Type, Func<object, object>> AwaiterGetterCache = [];
    public static Func<object, object> GetAwaiterGetter(Type awaitableType)
    {
        if (AwaiterGetterCache.TryGetValue(awaitableType, out var getter)) return getter;
        
        var getAwaiterMethod = awaitableType.GetMethod("GetAwaiter", PublicInstanceMethodFlags, Type.EmptyTypes)
                               ?? throw new InvalidOperationException($"Can't find stable GetAwaiter method in type {awaitableType}");

        var self = Expression.Parameter(typeof(object));
        var cast = Expression.Convert(self, awaitableType);

        var getAwaiterCall = Expression.Call(cast, getAwaiterMethod);

        getter = Expression.Lambda<Func<object, object>>(Expression.Convert(getAwaiterCall, typeof(object)), self).Compile();
        AwaiterGetterCache.Add(awaitableType, getter);

        return getter;
    }
    
    private static readonly Dictionary<Type, WrappedAwaitableDelegates> AwaiterMethodCache = [];

    public static WrappedAwaitableDelegates GetWrappedAwaiterMethods(Type awaiterType)
    {
        if (AwaiterMethodCache.TryGetValue(awaiterType, out var methods)) return methods;
        
        var onCompleteMethod = awaiterType.GetMethod("OnCompleted", PublicInstanceMethodFlags, [typeof(Action)])
                               ?? throw new InvalidOperationException("Can't find stable GetAwaiter");
        var isCompletedMethod = (awaiterType.GetProperty("IsCompleted")
                                 ?? throw new InvalidOperationException("Can't find stable GetAwaiter")).GetMethod
                                ?? throw new InvalidOperationException("Can't find stable GetAwaiter");
        var getResultMethod = awaiterType.GetMethod("GetResult", PublicInstanceMethodFlags, Type.EmptyTypes)
                              ?? throw new InvalidOperationException("Can't find stable GetAwaiter");

        var param = Expression.Parameter(typeof(object));
        var castedParam = Expression.Convert(param, awaiterType);

        var onCompleteParam = Expression.Parameter(typeof(Action));

        // (continuation) => ((awaiter)obj).OnComplete();
        var callOnCompleteExpr = Expression.Call(castedParam, onCompleteMethod, onCompleteParam);
        var onCompleteCallLambda = Expression
            .Lambda<Action<Action>>(callOnCompleteExpr, onCompleteParam);

        // (obj) => ...onCompleteCallLambda...
        var onCompleteWrappedMethod = Expression
            .Lambda<Action<object, Action>>(Expression.Invoke(onCompleteCallLambda, onCompleteParam), [param, onCompleteParam])
            .Compile();

        var isCompletedProperty = Expression.Property(castedParam, isCompletedMethod);
        // (obj) => ((awaiter)obj).IsCompleted;
        var isCompletedWrappedMethod = Expression
            .Lambda<Func<object, bool>>(isCompletedProperty, [param])
            .Compile();

        // () => ((awaiter)obj).GetResult();
        Expression getResultCallMethodExpr = Expression.Call(castedParam, getResultMethod);
        if (getResultMethod.ReturnType == typeof(void))
        {
            getResultCallMethodExpr = Expression.Block(getResultCallMethodExpr, Expression.Constant(null));
        }
        else
        {
            getResultCallMethodExpr = Expression.Convert(getResultCallMethodExpr, typeof(TReturn));
        }
        var getResultCallLambda = Expression
            .Lambda<Func<object, TReturn>>(getResultCallMethodExpr, [param])
            .Compile();

        methods = new WrappedAwaitableDelegates(OnComplete, IsCompleted, GetResult);
        AwaiterMethodCache.Add(awaiterType, methods);

        return methods;

        Func<TReturn> GetResult(object obj) => () => getResultCallLambda(obj);

        Func<bool> IsCompleted(object obj) => () => isCompletedWrappedMethod(obj);

        Action<Action> OnComplete(object obj) => action => onCompleteWrappedMethod(obj, action);
    }

}
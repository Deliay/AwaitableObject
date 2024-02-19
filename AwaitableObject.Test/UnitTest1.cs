namespace AwaitableObject.Test;

public class UnitTest1
{
    [Fact]
    public async Task CanWrapGenericTaskAndGetCorrectValue()
    {
        const int expected = 100;
        
        var result = await AwaitableWrapper.Wrap<int>(Task.FromResult(expected));
        var result2 = await AwaitableWrapper.Wrap<int>(Task.FromResult(expected));
        
        Assert.Equal(expected, result);
        Assert.Equal(expected, result2);
    }
    [Fact]
    public async Task CanWrapTask()
    {
        var flag = false;
        var task = Task.Run(() => { flag = true; });
        
        var result = await AwaitableWrapper.Wrap(task);
        
        Assert.Null(result);
        Assert.True(flag);
    }

    [Fact]
    public void ThrowIfObjectIsNotAwaitable()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            AwaitableWrapper.Wrap(1);
        });
    }

    
    class Foo
    {
        public int GetAwaiter() => 1;
    }
    
    [Fact]
    public void ThrowIfObjectIsNotAwaitableGetAwaiterReturnNotAAwaiter()
    {
        
        Assert.Throws<InvalidOperationException>(() =>
        {
            AwaitableWrapper.Wrap(new Foo());
        });
    }
}
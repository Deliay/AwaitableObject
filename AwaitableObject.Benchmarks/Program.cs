using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AwaitableObject.Benchmarks;

public class BenchmarkAwaitableObject
{

    [Benchmark]
    public async Task NonWrappedTask()
    {
        await Task.CompletedTask;
    }

    [Benchmark]
    public async Task WrappedTask()
    {
        await AwaitableWrapper.Wrap(Task.CompletedTask);
    }
    
    [Benchmark]
    public async Task NonWrappedValueTask()
    {
        await ValueTask.CompletedTask;
    }
    
    [Benchmark]
    public async Task WrappedValueTask()
    {
        await AwaitableWrapper.Wrap(ValueTask.CompletedTask);
    }

}

public class BenchmarkAwaitableObjectWithResult
{
    [Benchmark]
    public async Task NonWrappedTaskWithValue()
    {
        await Task.FromResult(1);
    }

    [Benchmark]
    public async Task WrappedTaskWithValue()
    {
        await AwaitableWrapper.Wrap<int>(Task.FromResult(1));
    }
    
    [Benchmark]
    public async Task NonWrappedValueTaskWithValue()
    {
        await ValueTask.FromResult(1);
    }
    
    [Benchmark]
    public async Task WrappedValueTaskWithValue()
    {
        await AwaitableWrapper.Wrap<int>(ValueTask.FromResult(1));
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<BenchmarkAwaitableObject>();
        BenchmarkRunner.Run<BenchmarkAwaitableObjectWithResult>();
    }
}
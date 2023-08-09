namespace Chatify.Shared.Infrastructure.Common.Extensions;

public static class TaskExtensions
{
    public static async Task<(T1, T2)> WhenAll<T1, T2>(
        this (Task<T1> taskOne, Task<T2> taskTwo) tasks)
    {
        var (one, two) = tasks;
        await Task.WhenAll(one, two);
        
        return ( one.Result, two.Result );
    }
    
    public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(
        this (Task<T1> taskOne, Task<T2> taskTwo, Task<T3> taskThree) tasks)
    {
        var (one, two, three) = tasks;
        await Task.WhenAll(one, two, three);
        
        return ( one.Result, two.Result, three.Result );
    }
    
    public static async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3 , T4>(
        this (Task<T1> taskOne, Task<T2> taskTwo, Task<T3> taskThree, Task<T4> taskFour) tasks)
    {
        var (one, two, three, four) = tasks;
        await Task.WhenAll(one, two, three, four);
        
        return ( one.Result, two.Result, three.Result, four.Result );
    }
}
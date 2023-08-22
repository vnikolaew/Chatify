using Castle.DynamicProxy;
using Metalama.Documentation.QuickStart;
using Metalama.Framework;

namespace Playgrounds;

public interface IModel
{
    List<(string, object?)> PropertyChangeList { get; }
}

public class ModelInterceptor : IInterceptor
{
    public List<(string, object?)> PropertyChangeList { get; } = new();
    
    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();

        var method = invocation.Method.Name;

        if ( method.StartsWith("set_") )
        {
            var newValue = invocation.Arguments[0];
            var field = method.Replace("set_", "");

            PropertyChangeList.Add((field, newValue));
        }
    }
}

[NotifyPropertyChanged]
public partial class Person : IModel
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    [Retry]
    [Log]
    public Person Test()
    {
        Console.WriteLine("Hello");
        return this;
    }
    
    public List<(string, object?)> PropertyChangeList { get; } = new();
}

public class ProxyFactory : IProxyFactory
{
    private readonly IProxyGenerator _proxyGenerator;
    private readonly IInterceptor _interceptor;

    public ProxyFactory(IProxyGenerator proxyGenerator, IInterceptor interceptor)
    {
        _proxyGenerator = proxyGenerator;
        _interceptor = interceptor;
    }

    public T GetModelProxy<T>(T source) where T : class
    {
        var person = _proxyGenerator.CreateClassProxy<Person>(_interceptor);
        
        var proxy = _proxyGenerator.CreateClassProxyWithTarget(source.GetType(),
            source, _interceptor) as T;
        return proxy;
    }
}
using Castle.DynamicProxy;

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
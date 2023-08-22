namespace Playgrounds;

public interface IProxyFactory
{
    T GetModelProxy<T>(T source) where T : class;
}
using System.Reflection;

namespace Chatify.Domain.Common;

public abstract class ValueObject : IEquatable<ValueObject>
{
    private static readonly BindingFlags BindingFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public bool Equals(ValueObject? other) => Equals(( object )other);

    public override bool Equals(object? obj)
    {
        if ( obj == null || GetType() != obj.GetType() )
            return false;
        foreach ( var field in ( obj as ValueObject ).GetFields() )
        {
            var obj1 = field.GetValue(this);
            var obj2 = field.GetValue(obj);
            
            if ( obj1 == null )
            {
                if ( obj2 != null ) return false;
            }
            else if ( !obj1.Equals(obj2) ) return false;
        }

        return true;
    }

    public override int GetHashCode()
        => GetFields()
            .Select(fi => fi.GetValue(this))
            .Where(value => value != null)
            .Aggregate(31, (acc, current) => acc * 57 + current.GetHashCode());

    private List<FieldInfo> GetFields()
    {
        var fields = new List<FieldInfo>();

        for ( var type = GetType(); type != typeof(object) && type != null; type = type.BaseType )
            fields.AddRange(type.GetFields(BindingFlags));

        return fields;
    }

    public static bool operator ==(ValueObject first, ValueObject second)
        => first.Equals(second);

    public static bool operator !=(ValueObject first, ValueObject second)
        => !( first == second );
}
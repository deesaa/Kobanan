namespace Kobanan;

public class ReactAddAttribute : Attribute
{
    public Type Type;
    public ReactAddAttribute(Type type)
    {
        Type = type;
    }
}
using System.Drawing;

namespace Kobanan;

public interface ISetName :  IComponent<ISetName>
{
    public void Set(string name);
    //IWorld World => Entity.World;
}

public interface ISetColor : IComponent<ISetColor>
{
    public void Set(Color color);
    //IWorld World => Entity.World;
}
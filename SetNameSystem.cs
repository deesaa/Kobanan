namespace Kobanan;

public struct PlayerTag : IComponent<PlayerTag>
{
    public IEntity Entity { get; set; }
}

public struct DamageComponent : IComponent<DamageComponent>
{
    public IEntity Entity { get; set; }
}

public class SetNameSystem : ISystem
{
    public void Update(ISetName setName, SetName name)
    {
        IFilter f = setName.World.Filter<ISetName>().Inc<SetName>().End();
        IFilter f2 = setName.World.Filter<PlayerTag>().Des<DamageComponent>().End();
        IFilter f3 = setName.World.Filter<PlayerTag>().Child<DamageComponent>().End();

        var e1 = setName.Entity;

        foreach (var e in f)
        {
            var c = e.Components;
            var s = e.Get<ISetName>();
            var euid = e.Euid;

            IEntity newE = e.World.NewEntity("MyEntity");
            PlayerCube arwa = new PlayerCube();
            newE.Add<ISetName>(arwa);
            newE.Add<ISetColor>(arwa);
            IEntity e2 = setName.World.Get(euid);
        }
    }

    public IWorld World { get; }
}
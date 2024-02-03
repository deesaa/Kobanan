namespace Kobanan;

public class DeathCounterComponent : BaseComponent<DeathCounterComponent>
{
    public int count;

    public DeathCounterComponent(IEntity entity) : base(entity)
    {
    }
}
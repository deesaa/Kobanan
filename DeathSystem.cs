namespace Kobanan;

public class DeathSystem : BaseSystem
{
    private void Update(TransformComponent transformComponent)
    {
        if (transformComponent.x > 10000)
        {
            transformComponent.Entity.Get<DeathTagComponent>();
        }
    }
}
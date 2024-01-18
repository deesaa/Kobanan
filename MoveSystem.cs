namespace Kobanan;

public class MoveSystem : BaseSystem
{
    [Inject] private LogSystem LogSystem;
    
    private void UpdateMove(TransformComponent transformComponent)
    {
        transformComponent.x += 1;
        World.Fire("DistancePassed", 1.5f);
        transformComponent.Entity.Command("Blink", 10);
    }
}
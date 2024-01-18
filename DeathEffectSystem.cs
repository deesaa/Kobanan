namespace Kobanan;

public class DeathEffectSystem : BaseSystem
{
    [Inject] private LogSystem LogSystem;
    
    [ReactAdd(typeof(DeathTagComponent))]
    public void OnDeath(Entity entity)
    {
        LogSystem.Log($"Death on x = {entity.Get<TransformComponent>().x}");
        entity.Del<DeathTagComponent>();
        entity.Get<TransformComponent>().x = 0;
    }
    
    [ReactDel(typeof(DeathTagComponent))]
    public void OnDeathDel(Entity entity)
    {
        entity.Get<DeathCounterComponent>().count += 1;
    }

    [ReactEvent("DistancePassed")]
    public void OnDistancePassedEvent(float distance)
    {
        
    }
    
    [ReactCommand("Blink")]
    public void OnCommand(Entity entity, int count)
    {
        
    }
}
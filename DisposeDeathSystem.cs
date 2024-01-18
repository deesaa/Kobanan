namespace Kobanan;

public class DisposeDeathSystem : BaseSystem
{
    private void Update(DeathCounterComponent deathCounterComponent)
    {
        if (deathCounterComponent.count > 5)
        {
            deathCounterComponent.Entity.Dispose();
        }
    }
}
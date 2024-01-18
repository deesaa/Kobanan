namespace Kobanan;

public class SpawnSystem : BaseSystem
{
    [UpdateEveryFrame]
    public void Update()
    {
        if (World.Count<TransformComponent>() <= 0)
        {
            World.NewEntity().Get<TransformComponent>();
        }
    }
}
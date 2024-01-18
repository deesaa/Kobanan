namespace Kobanan;

public class EcsInstaller : BaseInstaller
{
    public override void Install()
    {
        World.Inject<LogSystem>();
        World.Inject<DeathSystem>();
    }
}
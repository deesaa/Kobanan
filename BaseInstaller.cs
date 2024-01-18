namespace Kobanan;

public abstract class BaseInstaller : IInstaller
{
    protected World World => Kobanan.World;
    public abstract void Install();
}
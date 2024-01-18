namespace Kobanan;

public class Bootstrap
{
    public void Main()
    {
        Kobanan.AddInstaller<EcsInstaller>();
        Kobanan.Start();
    }
}
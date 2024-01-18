namespace Kobanan;

public static class Kobanan
{
    private static List<Func<IInstaller>> _installers = new List<Func<IInstaller>>();

    private static World _world;
    public static World World
    {
        get
        {
            if (_world == null)
            {
                _world = new World();
            }
            return _world;
        }
    }

    public static void AddInstaller<T>() where T : IInstaller, new()
    {
        _installers.Add((() => new T()));
    }

    public static void Start()
    {
        foreach (var installer in _installers)
        {
            installer.Invoke().Install();
        }

        World.ProcessInjects();
    }
}
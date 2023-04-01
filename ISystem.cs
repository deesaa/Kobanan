using System.Reflection;

namespace Kobanan;

public class Bootstrap
{
    public void Main()
    {
        Kobanan.AddInstaller<EcsInstaller>();
        Kobanan.Start();
    }
}

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

public interface ISystem
{
    
}

public interface IComponent
{
    
}

public interface IInstaller
{
    public void Install();
}

public abstract class BaseInstaller : IInstaller
{
    protected World World => Kobanan.World;
    public abstract void Install();
}

public class EcsInstaller : BaseInstaller
{
    public override void Install()
    {
        World.Inject<LogSystem>();
        World.Inject<DeathSystem>();
    }
}

public abstract class BaseComponent : IComponent
{
    public Entity Entity { get; private set; }
}

public class TransformComponent : BaseComponent
{
    public int x;
}

public class DeathTagComponent : BaseComponent
{
   
}

public class Entity : IDisposable
{
    private World World;
    public Guid Guid { get; private set; }
    private Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();
    public Entity(World world)
    {
        World = world;
        Guid = Guid.NewGuid();
    }

    public T Get<T>() where T : IComponent, new()
    {
        if (_components.TryGetValue(typeof(T), out var component))
            return (T) component;

        var newComponent = new T();
        World.AddComponent(this, newComponent);
        _components.Add(typeof(T), newComponent);
        return newComponent;
    }
    
    public void Del<T>() where T : IComponent
    {
        if (!_components.TryGetValue(typeof(T), out var component))
            return;
        
        World.RemoveComponent(this, component);
        _components.Remove(typeof(T));
    }

    public void Dispose()
    {
        
    }

    public void Command(string name, object command)
    {
        
    }
}

public abstract class BaseSystem : ISystem
{
    public World World { get; private set; }
}

public class Filter
{
    public void Invoke()
    {
        
    }
}

public class World
{
    private List<Entity> _entities = new List<Entity>();
    private List<Filter> _filters = new List<Filter>();
    private Queue<Type> _toInject = new Queue<Type>();
    private Dictionary<Type, object> _instances = new Dictionary<Type, object>();
    private Dictionary<Type, List<Action<Entity, IComponent>>> _reactComponentAddMethods = new Dictionary<Type, List<Action<Entity, IComponent>>> ();
    public int Count<T>() where T : IComponent
    {
        return 0;
    }

    public Entity NewEntity()
    {
        var entity = new Entity(this);
        _entities.Add(entity);
        return entity;
    }

    public void Update()
    {
        foreach (Filter filter in _filters)
        {
            filter.Invoke();
        }
    }

    public void Fire(string name, object @event)
    {
        
    }

    public void Inject<T>() where T : ISystem
    {
        _toInject.Enqueue(typeof(T));
    }

    public void ProcessInjects()
    {
        foreach (var type in _toInject)
        {
            var constructor = type.GetConstructor(Type.EmptyTypes);
            var instance = constructor.Invoke(Array.Empty<object>());
            _instances.Add(type, instance);
        }

        foreach (var pair in _instances)
        {
            InjectFields(pair);
            InjectMethods(pair);
        }
    }

    protected void InjectMethods(KeyValuePair<Type, object> keyValuePair)
    {
        var methods = keyValuePair.Value.GetType().GetMethods();

        foreach (var methodInfo in methods)
        {
            TryAddEntityFilter(methodInfo, keyValuePair.Value);
        }
        
        var componentAddMethods = methods.Select(info => (info, info.GetCustomAttribute(typeof(ReactAddAttribute), false)))
            .Where(method => method.Item2 != null).Select(method => (method.info, (ReactAddAttribute)method.Item2));

        foreach (var componentAddMethod in componentAddMethods)
        {
            var reactOnType = componentAddMethod.Item2.Type;
            if(!_reactComponentAddMethods.TryGetValue(reactOnType, out var actions))
                _reactComponentAddMethods.Add(reactOnType, actions = new List<Action<Entity, IComponent>>());

            actions.Add((Entity entity, IComponent component) =>
                componentAddMethod.info.Invoke(keyValuePair.Value, new[] {(object)entity, (object)component}));
        }
    }

    private void TryAddEntityFilter(MethodInfo methodInfo, object @object)
    {
        var @params = methodInfo.GetParameters();
        if (!@params.All(x => x.ParameterType.IsAssignableFrom(typeof(IComponent))))
            return;

        var targetTypes = @params.Select(x => x.ParameterType);
    }

    private void InjectFields(KeyValuePair<Type, object> keyValuePair)
    {
        var fields = keyValuePair.Value.GetType().GetFields();
        var injectFields = fields.Where(x => x.IsDefined(typeof(InjectAttribute), false));

        foreach (var field in injectFields)
        {
            var fieldType = field.FieldType;
            if(_instances.TryGetValue(fieldType, out var instance))
                field.SetValue(keyValuePair.Value, instance);
        }
    }

    public void AddComponent<T>(Entity entity, T newComponent) where T : IComponent, new()
    {
        if(!_reactComponentAddMethods.TryGetValue(typeof(T), out var actions))
            return;

        foreach (var action in actions)
        {
            action.Invoke(entity, newComponent);
        }
    }
}

public class InjectAttribute : Attribute
{
    
}

public class ReactAddAttribute : Attribute
{
    public Type Type;
    public ReactAddAttribute(Type type)
    {
        Type = type;
    }
}

public class ReactDelAttribute : Attribute
{
    public ReactDelAttribute(Type type)
    {
        
    }
}

public class ReactEventAttribute : Attribute
{
    public ReactEventAttribute(string @event)
    {
        
    }
}

public class ReactCommandAttribute : Attribute
{
    public ReactCommandAttribute(string command)
    {
        
    }
}

public interface IEvent{}
public interface ICommand{}

public interface IReactEvent<T> where T : IEvent
{
    public void OnEvent(T @event);
}

public interface IReactAdd<T> where T : IComponent
{
    public void OnAdd(T component);
}

public interface IReactDel<T> where T : IComponent
{
    public void OnDel(T component);
}

public struct EntityDisposeEvent : IEvent{}

public class UpdateEveryFrameAttribute : Attribute
{
}




public class LogSystem : BaseSystem
{
    public void Log(string text){}
}

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

public class DeathCounterComponent : BaseComponent
{
    public int count;
}

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

public class Filter<T> where T : IComponent
{
    
}

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
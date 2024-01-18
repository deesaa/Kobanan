using System.Reflection;

namespace Kobanan;

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Unity.VisualScripting.FullSerializer.Internal;

namespace Kobanan
{
    public interface IEntityCreatedListener
    {
        public void OnCreated(IEntity entity);
    }
    public interface IEntityDestroyedListener
    {
        public void OnDestroyed(IEntity entity);
    }
    public interface IComponentAddedListener<T> where T : IComponent<T>
    {
        public void OnAdded(T component);
    }
    public interface IComponentDeletedListener<T> where T : IComponent<T>
    {
        public void OnDeleted(T component);
    }


    public class World : IWorld
    {
        private Dictionary<Euid, IEntity> _entities = new();
        private Dictionary<BigInteger, List<IEntity>> _entitiesByMask = new();
        public IDictionary<Euid, IEntity> Entities => _entities;

        private List<IEntityCreatedListener> _entityCreatedListeners = new();
        private List<IEntityDestroyedListener> _entityDestroyedListeners = new();

        public string Name { get; }

        public World(string name = "Default")
        {
            Name = name;
        }

        public IFilterBuilder Filter<T>()
        {
            return default;

        }

        public IEntity Get(Euid id)
        {
            return default;

        }

        public IEntity NewEntity(string name)
        {
            var newE = new Entity(this, name);
            return newE;
        }

        public IEntity NewEntityWithEuid(Euid euid)
        {
            return default;

        }

        public void OnEntityCreated(IEntity entity)
        {
            if (!Entities.TryAdd(entity.Euid, entity))
                throw new Exception($"Entity with Euid:{entity.Euid} already exists in world:{Name}");

            foreach (var entityCreatedListener in _entityCreatedListeners)
                entityCreatedListener.OnCreated(entity);
        }


        private Dictionary<Type, IService> _services = new();
        public void AddSystem<T>(T system) where T : ISystem
        {
            system.World = this;
            if (system is IService service)
            {
                var serviceInterfaces = system
                    .GetType()
                    .GetInterfaces()
                    .Where(x => typeof(IService).IsAssignableFrom(x) &&
                                x != typeof(IService));
                

                foreach (var serviceInterface in serviceInterfaces)
                {
                    AddService(serviceInterface, service);
                }
            }

            var reflectedSystemMethods = system
                .GetType()
                .GetMethods();
            
            
            foreach (var method in reflectedSystemMethods)
            {
                var args = method.GetParameters();

                var methodInvocationData = new MethodInvocationData()
                {
                    Arguments = new MethodInvocationData.Argument[args.Length],
                    ArgsCount = args.Length
                };

                for (int i = 0; i < args.Length; i++)
                {
                    if (typeof(IComponentBase).IsAssignableFrom(args[i].ParameterType))
                    {
                        methodInvocationData.Arguments[i].Type = args[i].ParameterType;
                        methodInvocationData.Arguments[i].InjectMode = MethodInvocationData.InjectMode.Component;
                        methodInvocationData.ValidArgsCount++;
                        methodInvocationData.FilterIncludeMask += IdProvider.GetIdByType(args[i].ParameterType).MaskId;
                        continue;
                    }
                    if (typeof(IService).IsAssignableFrom(args[i].ParameterType))
                    {
                        methodInvocationData.Arguments[i].Type = args[i].ParameterType;
                        methodInvocationData.Arguments[i].InjectMode = MethodInvocationData.InjectMode.Service;
                        methodInvocationData.ValidArgsCount++;
                    }
                }
                
                if(!methodInvocationData.AnyArgsMatch) continue;
                if (methodInvocationData.IsException) 
                    throw new Exception("Args in method are not valid");

                if (!_updates.TryGetValue(system.GetType(), out var updateMethods))
                    _updates.Add(system.GetType(), updateMethods = new List<Action>());
                
                updateMethods.Add(() =>
                {
                    var filter = Filter(methodInvocationData.FilterIncludeMask);

                    foreach (var entity in filter)
                    {
                        InvokeArguments invokeArguments = new InvokeArguments()
                        {
                            args = new object[methodInvocationData.Arguments.Length]
                        };
                        
                        for (var i = 0; i < methodInvocationData.Arguments.Length; i++)
                        {
                            var argument = methodInvocationData.Arguments[i];
                            if (argument.InjectMode == MethodInvocationData.InjectMode.Component)
                            {
                                var type = argument.Type;
                                var component = entity.Get(type);
                                invokeArguments.args[i] = component;
                                continue;
                            }

                            if (argument.InjectMode == MethodInvocationData.InjectMode.Service)
                            {
                                var service = this.GetService(argument.Type);
                                invokeArguments.args[i] = service;
                                continue;
                            }
                        }

                        method.Invoke(system, invokeArguments.args);
                    }
                });
            }
        }
        
        public struct InvokeArguments
        {
            public object[] args;
        }

        private IFilter Filter(BigInteger includeMask)
        {
            var filterBuilder = new FilterBuilder(this);
            filterBuilder.Inc(includeMask);
            var filter = filterBuilder.End();
            return filter;
        }

        public struct MethodInvocationData
        {
            public enum InjectMode
            {
                None,
                Component,
                Service
            }

            public BigInteger FilterIncludeMask;
            public Argument[] Arguments;
            public int ArgsCount;
            public int ValidArgsCount;

            public bool AnyArgsMatch => ValidArgsCount != 0;
            public bool IsException => ValidArgsCount != 0 && ValidArgsCount != ArgsCount;
            public struct Argument
            {
                public InjectMode InjectMode;
                public Type Type;
            }
        }

        private void AddService(Type @interfaceType, IService service)
        {
            _services[@interfaceType] = service;
        }

        public void RemoveSystem<T>() where T : ISystem
        {

        }


        private Dictionary<Type, List<Action>> _updates = new();
        public void Update()
        {
            foreach (var update in _updates)
            {
                foreach (var method in update.Value)
                {
                    method.Invoke();
                }
            }
        }

        public void Destroy()
        {

        }

        public void OnComponentCreated(IEntity entity, IComponentBase component, BigInteger id)
        {

        }

        public void OnComponentDeleted(IEntity entity, IComponentBase component, BigInteger id)
        {

        }

        public void OnEntityDestroyed(IEntity entity)
        {

        }

        public T GetService<T>() where T : IService
        {
            return (T)_services[typeof(T)];
        }

        public void OnFilterCreated(IFilter filter)
        {
            FilterMask filterMask = filter.GetMask();
            foreach (var entity in _entities)
            {
                BigInteger entityComponentMask = entity.Value.GetComponentsEntityMask();
                if (filterMask.Contains(entityComponentMask))
                    filter.AddEntity(entity.Value);
            }
        }
    }
}

/*public class World
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
}*/
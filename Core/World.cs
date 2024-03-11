using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

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
    public interface IComponentAddedListener<T> where T : IComponentBase
    {
        public void OnAdded(T component);
    }
    public interface IComponentDeletedListener<T> where T : IComponentBase
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
            _filtersPool = new FiltersPool(this);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryInjectAsService<T>(T system) where T : ISystem
        {
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MethodInvocationData CollectInvocationData(MethodInfo method)
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
                    continue;
                }
                if (typeof(Command).IsAssignableFrom(args[i].ParameterType))
                {
                    methodInvocationData.Arguments[i].Type = args[i].ParameterType;
                    methodInvocationData.Arguments[i].InjectMode = MethodInvocationData.InjectMode.Command;
                    methodInvocationData.ValidArgsCount++;
                    methodInvocationData.IsSyncCommandMethod = true;
                }
            }

            return methodInvocationData;
        }
        
        public void AddSystem<T>(T system) where T : ISystem
        {
            system.World = this;
            
            TryInjectAsService(system);
            
            var reflectedSystemMethods = system
                .GetType()
                .GetMethods();
            
            foreach (var method in reflectedSystemMethods)
            {
                var methodInvocationData = CollectInvocationData(method);
                
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

        private IFiltersPool _filtersPool;
        private IFilter Filter(BigInteger includeMask)
        {
            var filterBuilder = new FilterBuilder(this, _filtersPool);
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
                Service,
                Command
            }

            public BigInteger FilterIncludeMask;
            public Argument[] Arguments;
            public int ArgsCount;
            public int ValidArgsCount;

            public bool IsSyncCommandMethod;

            public bool AnyArgsMatch => ValidArgsCount != 0;
            
            // If only some of the args in a method are valid, throw exception
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
        private Dictionary<Type, List<Action>> _syncCommandUpdates = new(); //Have any command type args
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

        public void OnComponentAdded(IEntity entity, IComponentBase component, ComponentId componentId)
        {
            _filtersPool.OnComponentAdded(entity, component, componentId);
        }

        public void OnComponentDeleted(IEntity entity, IComponentBase component, ComponentId componentId)
        {
            _filtersPool.OnComponentDeleted(entity, component, componentId);
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
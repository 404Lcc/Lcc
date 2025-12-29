using Entitas;
using LccModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
    public abstract class ECSWorld
    {
        public IWorldData Data { get; private set; }
        public LogicContext LogicContext { get; private set; }
        public MetaContext MetaContext { get; private set; }
        public List<IContext> ContextList { get; private set; }
        public ECSSystems System { get; protected set; }

        public virtual void InitWorld(IWorldData data)
        {
            Data = data;

            ContextList = new List<IContext>();

            ContextList.Add(LogicContext = CreateLogicContext());
            ContextList.Add(MetaContext = CreateMetaContext());

            CreateSystems();

            //初始化系统
            if (System != null)
            {
                System.Initialize();
                System.ActivateReactiveSystems();
            }
        }

        public virtual void DestroyWorld()
        {
            if (System != null)
            {
                System.TearDown();
                System.DeactivateReactiveSystems();
                System.ClearReactiveSystems();

                System = null;
            }

            foreach (var item in ContextList)
            {
                item.Reset();
            }
        }

        public T GetWorldData<T>() where T : IWorldData
        {
            return (T)Data;
        }

        public abstract void CreateSystems();

        public IEntityIndex GetEntityIndex<T>()
        {
            return LogicContext.GetEntityIndex(typeof(T).Name);
        }

        public TIndex GetEntityIndex<TComponent, TIndex>()
        {
            return (TIndex)GetEntityIndex<TComponent>();
        }

        public virtual void Update()
        {
            if (System != null)
            {
                System.Execute();
                System.Cleanup();
            }
        }

        public virtual void LateUpdate()
        {
            if (System != null)
            {
                System.LateUpdate();
            }
        }

        public virtual void DrawGizmos()
        {
            if (System != null)
            {
                System.DrawGizmos();
            }
        }

        protected virtual LogicContext CreateLogicContext()
        {
            RebuildComponentsLookup(typeof(LogicComponentsLookup), LogicComponentsLookup.typeIndexList, out var componentTypeList, out var componentNameList);
            var contextInfo = new ContextInfo("LogicContext", componentNameList.ToArray(), componentTypeList.ToArray());
            return new LogicContext(componentTypeList.Count, 0, contextInfo, (entity) => new UnsafeAERC(), GetLogicEntityFactory());
        }

        protected virtual Func<LogicEntity> GetLogicEntityFactory()
        {
            return () => new LogicEntity();
        }

        protected virtual MetaContext CreateMetaContext()
        {
            RebuildComponentsLookup(typeof(MetaComponentsLookup), MetaComponentsLookup.typeIndexList, out var componentTypeList, out var componentNameList);
            var contextInfo = new ContextInfo("MetaContext", componentNameList.ToArray(), componentTypeList.ToArray());
            return new MetaContext(componentTypeList.Count, 0, contextInfo, (entity) => new UnsafeAERC(), GetMetaEntityFactory());
        }

        protected virtual Func<MetaEntity> GetMetaEntityFactory()
        {
            return () => new MetaEntity();
        }

        protected virtual void RebuildComponentsLookup(Type lookupType, List<ComponentTypeIndex> typeIndexList, out List<Type> componentTypeList, out List<string> componentNameList)
        {
            List<FieldInfo> componentTypeIndexList = new List<FieldInfo>();
            foreach (FieldInfo item in lookupType.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (item.FieldType == typeof(ComponentTypeIndex))
                {
                    componentTypeIndexList.Add(item);
                }
            }

            //按字段名排序 确保顺序可预测
            componentTypeIndexList = componentTypeIndexList.OrderBy(x => x.Name).ToList();

            componentTypeList = new List<Type>();
            componentNameList = new List<string>();

            //为每个ComponentTypeIndex实例的index字段赋值
            for (int i = 0; i < componentTypeIndexList.Count; i++)
            {
                ComponentTypeIndex obj = componentTypeIndexList[i].GetValue(null) as ComponentTypeIndex;
                if (obj != null)
                {
                    var type = obj.componentType;
                    var name = type.Name;
                    obj.index = i;
                    typeIndexList.Add(obj);
                    componentTypeList.Add(type);
                    componentNameList.Add(name);

                    Debug.Log($"重设组件index typeName={name} index={obj.index}");
                }
                else
                {
                    Debug.LogError("RebuildComponentsLookup出错");
                }
            }
        }
    }
}
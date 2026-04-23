using Entitas;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public interface IWorldCreationInfo
    {
        int ModeLogicID { get; }
    }

    public abstract class ECWorlds
    {
        protected List<IContext> _worldList = new List<IContext>();
        protected ECSystems _rootSystem;
        protected IWorldCreationInfo _creationInfo;

        //逻辑世界（游戏实体）
        public LogicWorld LogicWorld { get; protected set; }

        //抽象世界（World组件）
        public MetaWorld MetaWorld { get; protected set; }

        public virtual void InitWorlds(IWorldCreationInfo creationInfo)
        {
            _creationInfo = creationInfo;

            //构造世界
            _worldList.Clear();
            CreateLogicWorld();
            CreateMetaWorld();

            //构造系统
            CreateSystems();

            //初始化系统
            if (_rootSystem != null)
            {
                _rootSystem.ActivateReactiveSystems();
                _rootSystem.Initialize();
            }
        }

        public virtual void DestroyWorlds()
        {
            if (_rootSystem != null)
            {
                _rootSystem.TearDown();
                _rootSystem.DeactivateReactiveSystems();
                _rootSystem.ClearReactiveSystems();
                _rootSystem = null;
            }

            foreach (var item in _worldList)
            {
                item.Reset();
            }

            _worldList.Clear();
        }

        public virtual void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (_rootSystem != null)
            {
                _rootSystem.Execute();
                _rootSystem.Cleanup();
            }
        }

        public virtual void LateUpdate()
        {
            if (_rootSystem != null)
            {
                _rootSystem.LateUpdate();
            }
        }

        public virtual void Gizmos()
        {
            if (_rootSystem != null)
            {
                _rootSystem.Gizmos();
            }
        }

        protected void RebuildComponentLookUp(Type lookupType, List<ComponentTypeIndex> typeIndexList, out List<Type> componentTypes, out List<string> componentNames)
        {
            FieldInfo[] fieldInfos = lookupType.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            List<FieldInfo> typeIndexFields = new List<FieldInfo>();
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                //筛选出静态字段
                if (fieldInfo.FieldType == typeof(ComponentTypeIndex))
                {
                    typeIndexFields.Add(fieldInfo);
                }
            }

            //按字段名排序(确保顺序可预测)
            typeIndexFields.Sort((a, b) => a.Name.CompareTo(b.Name));

            componentTypes = new List<Type>();
            componentNames = new List<string>();

            //为每个ComponentTypeIndex实例的Index字段赋值，(CmptType字段静态初始化时就赋值了)
            for (int i = 0; i < typeIndexFields.Count; i++)
            {
                //获取字段的值(即ComponentTypeIndex实例)
                var staticFieldInst = typeIndexFields[i].GetValue(null) as ComponentTypeIndex;
                if (staticFieldInst != null)
                {
                    var type = staticFieldInst.ComponentType;
                    var name = type.Name;
                    staticFieldInst.Index = i;
                    typeIndexList.Add(staticFieldInst);
                    componentTypes.Add(type);
                    componentNames.Add(name);
                }
            }
        }

        protected abstract void CreateSystems();


        //扩展可以使用LogicEntity子类
        protected virtual Func<LogicEntity> GetLogicEntityFactory()
        {
            return () => new LogicEntity();
        }

        //扩展可以使用LogicWorld子类
        protected virtual void CreateLogicWorld()
        {
            RebuildComponentLookUp(typeof(LogicComponentsLookup), LogicComponentsLookup.TypeIndexList, out var componentTypes, out var componentNames);
            var contextInfo = new ContextInfo("LogicWorld", componentNames.ToArray(), componentTypes.ToArray());
            LogicWorld = new LogicWorld(contextInfo, componentTypes.Count, GetLogicEntityFactory(), 1, (entity) => new UnsafeAERC());
            _worldList.Add(LogicWorld);
        }

        //扩展可以使用MetaEntity子类
        protected virtual Func<MetaEntity> GetMetaEntityFactory()
        {
            return () => new MetaEntity();
        }

        //扩展可以使用MetaWorld子类
        protected virtual void CreateMetaWorld()
        {
            RebuildComponentLookUp(typeof(MetaComponentsLookup), MetaComponentsLookup.TypeIndexList, out var componentTypes, out var componentNames);
            var contextInfo = new ContextInfo("MetaWorld", componentNames.ToArray(), componentTypes.ToArray());
            MetaWorld = new MetaWorld(contextInfo, componentTypes.Count, GetMetaEntityFactory(), 12300001);
            _worldList.Add(MetaWorld);
        }

        public T GetCreationInfo<T>() where T : IWorldCreationInfo
        {
            return (T)_creationInfo;
        }
    }
}
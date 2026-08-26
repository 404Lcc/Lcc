using System;
using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class IDComponent : LogicComponent
    {
        public long ID { get; private set; }
        public string Name { get; private set; }

        public void Init(long id, string name)
        {
            ID = id;
            Name = name;
        }
        
        public void SetID(long id)
        {
            if (ID == id)
                return;
            ID = id;
            Name = null;
            _owner.ReplaceComponent(LogicComponentsLookup.ComID, this);
        }
        
        public void SetName(string name)
        {
            if (Name == name)
                return;
            Name = name;
            _owner.ReplaceComponent(LogicComponentsLookup.ComID, this);
        }
        
        public override void DisposeOnRemove()
        {
            Name = null;
            ID = 0;
            base.DisposeOnRemove();
        }
    }

    public partial class LogicEntity
    {
        public IDComponent comID { get { return (IDComponent)GetComponent(LogicComponentsLookup.ComID); } }
        public bool hasComID { get { return HasComponent(LogicComponentsLookup.ComID); } }

        public void AddComID(long newId, string name = null)
        {
            var index = LogicComponentsLookup.ComID;
            if (index < 0)
            {
                UnityEngine.Debug.LogError("AddComID 未初始化的组件索引 LogicComponentsLookup.ComID");
                return;
            }
            var component = (IDComponent)CreateComponent(index, typeof(IDComponent));
            component.Init(newId, name);
            AddComponent(index, component);
        }

        public long ID
        {
            get
            {
                if (hasComID)
                {
                    return comID.ID;
                }
                return creationIndex;
            }
        } 
    }

    public class EntityIndex_Name : PrimaryEntityIndex<LogicEntity, string>
    {
        public EntityIndex_Name(string name, IGroup<LogicEntity> group, Func<LogicEntity, IComponent, string> getKey) : base(name, group, getKey){}
        public EntityIndex_Name(string name, IGroup<LogicEntity> group, Func<LogicEntity, IComponent, string[]> getKeys) : base(name, group, getKeys) { }
        public EntityIndex_Name(string name, IGroup<LogicEntity> group, Func<LogicEntity, IComponent, string> getKey, IEqualityComparer<string> comparer) : base(name, group, getKey, comparer) { }
        public EntityIndex_Name(string name, IGroup<LogicEntity> group, Func<LogicEntity, IComponent, string[]> getKeys, IEqualityComparer<string> comparer) : base(name, group, getKeys, comparer) { }
        protected override void addEntity(string key, LogicEntity entity)
        {
            if (string.IsNullOrEmpty(key))
                return;
            base.addEntity(key, entity);
        }
        protected override void removeEntity(string key, LogicEntity entity) {
            if (string.IsNullOrEmpty(key))
                return;
            base.removeEntity(key, entity);
        }
    }
    
    
    //////////////////////////////////////////////////////////////////////////
    /// EntityIndex: ComID
    public static partial class WorldExtension
    {
        public static void AddEntityIndex_Name(this LogicWorld world)
        {
            var index = new EntityIndex_Name(
                "EntityIndex_Name",
                world.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComID)),
                (e, c) => ((IDComponent) c).Name);
            world.AddEntityIndex(index);
        }

        public static LogicEntity GetEntityWithName(this LogicWorld world, string name)
        {
            var index = world.GetEntityIndex("EntityIndex_Name") as PrimaryEntityIndex<LogicEntity, string>;
            if (index == null)
            {
                return null;
            }
            return index.GetEntity(name);
        }

        public static LogicEntity GetEntity(this LogicWorld world, long id)
        {
            return world.GetEntityWithComID(id);
        }
    }
    
    
    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComIDIndex = new (typeof(IDComponent));
        public static int ComID => _ComIDIndex.Index;
    }

}

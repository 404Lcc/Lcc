using System;
using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class TagComponent : LogicComponent
    {
        public uint Tags { get; protected set; }

        public void SetTags(uint tags)
        {
            Tags = tags;
        }

        public void AddTags(uint tags)
        {
            Tags |= tags;
        }

        public void RemoveTags(uint tags)
        {
            uint mask = ~tags;
            Tags &= mask;
        }

        public bool HasTags(uint tags)
        {
            return (Tags & tags) != 0;
        }
        
        public int[] GetTagKeys()
        {
            return GetTagKeys(Tags);
        }
        
        public int[] GetTagIndexArray()
        {
            return GetTagIndexArray(Tags);
        }
        
        public static int[] GetTagKeys(uint tags)
        {
            if (tags == 0)
                return Array.Empty<int>();

            var keys = new int[PopCount(tags)];
            var idx = 0;
            while (tags != 0)
            {
                var flag = tags & (uint)-(int)tags;
                keys[idx++] = (int)flag;
                tags &= ~flag;
            }
            return keys;
        }
        
        public static int[] GetTagIndexArray(uint tags)
        {
            if (tags == 0)
                return Array.Empty<int>();

            var keys = new int[PopCount(tags)];
            var idx = 0;
            for (int i = 0; i < 32; i++)
            {
                if ((tags & (1u << i)) != 0)
                    keys[idx++] = i;
            }
            return keys;
        }

        /// <summary>
        /// 统计 uint 中置位个数（population count）。
        /// </summary>
        public static int PopCount(uint value)
        {
            var count = 0;
            while (value != 0)
            {
                count++;
                value &= value - 1;
            }
            return count;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    public partial class LogicEntity
    {
        public TagComponent comTag
        {
            get { return (TagComponent)GetComponent(LogicComponentsLookup.ComTag); }
        }

        public bool hasComTag
        {
            get { return HasComponent(LogicComponentsLookup.ComTag); }
        }

        public void AddComTags(uint newTags)
        {
            var index = LogicComponentsLookup.ComTag;
            if (!hasComTag)
            {
                var component = (TagComponent)CreateComponent(index, typeof(TagComponent));
                component.SetTags(newTags);
                AddComponent(index, component);
            }
            else
            {
                var component = (TagComponent)GetComponent(index);
                component.AddTags(newTags);
                ReplaceComponent(index, component);
            }
        }

        public void RemoveComTag()
        {
            RemoveComponent(LogicComponentsLookup.ComTag);
        }

        public void RemoveTags(uint tags)
        {
            if (!hasComTag)
                return;
            var index = LogicComponentsLookup.ComTag;
            var component = (TagComponent)GetComponent(index);
            component.RemoveTags(tags);
            ReplaceComponent(index, component);
        }

        public bool HasTags(uint tags)
        {
            if (!hasComTag)
                return false;
            var index = LogicComponentsLookup.ComTag;
            var component = (TagComponent)GetComponent(index);
            return component.HasTags(tags);
        }
    }
    
    
    public static partial class WorldExtension
    {
        public static void AddEntityIndex_ComTag(this LogicWorld world)
        {
            var index = new TagEntityIndex<LogicEntity>(
                "EntityIndex_Tag",
                world.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComTag)),
                (e, c) =>
                {
                    var component = c != null
                        ? (TagComponent)c
                        : e.comTag;
                    return component.Tags;
                });
            world.AddEntityIndex(index);
        }

        //////////////////////////////////////////////////////////////////////////
        /// EntityIndex: ComTag
        public static HashSet<LogicEntity> GetEntitiesWithComTag(this LogicWorld world, int tag)
        {
            var index = world.GetEntityIndex("EntityIndex_Tag") as TagEntityIndex<LogicEntity>;
            if (index == null)
            {
                return null;
            }
            return index.GetEntities(tag);
        }
    }

    //////////////////////////////////////////////////////////////////////////
    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComTagIndex = new(typeof(TagComponent));
        public static int ComTag => _ComTagIndex.Index;
    }
}
using System;

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
            Tags &= tags;
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
    }

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

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComTagIndex = new(typeof(TagComponent));
        public static int ComTag => _ComTagIndex.Index;
    }
}
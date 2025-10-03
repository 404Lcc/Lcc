using System;

namespace LccHotfix
{
    [Flags]
    public enum TagType
    {
        Hero = 1 << 1, //英雄对象 1
    }

    public class ComTag : LogicComponent
    {
        public TagType tag;

        public bool HasTag(TagType tag)
        {
            return this.tag.HasFlag(tag);
        }
    }

    public partial class LogicEntity
    {
        public ComTag comTag
        {
            get { return (ComTag)GetComponent(LogicComponentsLookup.ComTag); }
        }

        public bool hasComTag
        {
            get { return HasComponent(LogicComponentsLookup.ComTag); }
        }

        public void AddComTag(TagType newTag)
        {
            var index = LogicComponentsLookup.ComTag;
            var component = (ComTag)CreateComponent(index, typeof(ComTag));
            component.tag = newTag;
            AddComponent(index, component);
        }

        public void ReplaceComTag(TagType newTag)
        {
            var index = LogicComponentsLookup.ComTag;
            var component = (ComTag)CreateComponent(index, typeof(ComTag));
            component.tag = newTag;
            ReplaceComponent(index, component);
        }

        public void RemoveComTag()
        {
            RemoveComponent(LogicComponentsLookup.ComTag);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComTagIndex = new ComponentTypeIndex(typeof(ComTag));
        public static int ComTag => ComTagIndex.index;
    }
}
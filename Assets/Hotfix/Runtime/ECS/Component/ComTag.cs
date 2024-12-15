using System;

namespace LccHotfix
{
    [Flags]
    public enum TagType
    {
        MainPlayer = 1 << 0, //当前客户端操作的对象 1
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
        public ComTag comTag { get { return (ComTag)GetComponent(LogicComponentsLookup.ComTag); } }
        public bool hasComTag { get { return HasComponent(LogicComponentsLookup.ComTag); } }

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

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComTag;

        public static Entitas.IMatcher<LogicEntity> ComTag
        {
            get
            {
                if (_matcherComTag == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComTag);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComTag = matcher;
                }

                return _matcherComTag;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComTag;
    }
}
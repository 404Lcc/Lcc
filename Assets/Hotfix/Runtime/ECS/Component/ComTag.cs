using System;

namespace LccHotfix
{
    [Flags]
    public enum GameEntityTag
    {
        MainPlayer = 1 << 0, //当前客户端操作的对象 1
    }

    public class ComTag : LogicComponent
    {
        public GameEntityTag TagType;

        public bool HasTag(GameEntityTag tag)
        {
            return TagType.HasFlag(tag);
        }
    }

    public partial class LogicEntity
    {
        public ComTag comTag { get { return (ComTag)GetComponent(LogicComponentsLookup.ComTag); } }
        public bool hasComTag { get { return HasComponent(LogicComponentsLookup.ComTag); } }

        public void AddComTag(GameEntityTag newTagType)
        {
            var index = LogicComponentsLookup.ComTag;
            var component = (ComTag)CreateComponent(index, typeof(ComTag));
            component.TagType = newTagType;
            AddComponent(index, component);
        }

        public void ReplaceComTag(GameEntityTag newTagType)
        {
            var index = LogicComponentsLookup.ComTag;
            var component = (ComTag)CreateComponent(index, typeof(ComTag));
            component.TagType = newTagType;
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
                    matcher.ComponentNames = LogicComponentsLookup.componentNames.ToArray();
                    _matcherComTag = matcher;
                }

                return _matcherComTag;
            }
        }
    }
}
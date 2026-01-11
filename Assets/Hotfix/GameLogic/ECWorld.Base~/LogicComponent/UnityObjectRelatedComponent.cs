using System.Collections.Generic;

namespace LccHotfix
{
    public enum GameObjectType
    {
        None,
        Self,
        Part,
    }

    public class UnityObjectRelatedComponent : LogicComponent
    {
        public Dictionary<int, GameObjectType> gameObjectInstanceID;

        public GameObjectType GetGameObjectType(int id)
        {
            if (gameObjectInstanceID.TryGetValue(id, out var type))
            {
                return type;
            }

            return GameObjectType.None;
        }

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();

            gameObjectInstanceID.Clear();
        }
    }

    public partial class LogicEntity
    {
        public UnityObjectRelatedComponent comUnityObjectRelated
        {
            get { return (UnityObjectRelatedComponent)GetComponent(LogicComponentsLookup.ComUnityObjectRelated); }
        }

        public bool hasComUnityObjectRelated
        {
            get { return HasComponent(LogicComponentsLookup.ComUnityObjectRelated); }
        }

        public void AddComUnityObjectRelated(Dictionary<int, GameObjectType> newGameObjectInstanceID)
        {
            var index = LogicComponentsLookup.ComUnityObjectRelated;
            var component = (UnityObjectRelatedComponent)CreateComponent(index, typeof(UnityObjectRelatedComponent));
            component.gameObjectInstanceID = newGameObjectInstanceID;
            AddComponent(index, component);
        }

        public void ReplaceComUnityObjectRelated(Dictionary<int, GameObjectType> newGameObjectInstanceID)
        {
            var index = LogicComponentsLookup.ComUnityObjectRelated;
            var component = (UnityObjectRelatedComponent)CreateComponent(index, typeof(UnityObjectRelatedComponent));
            component.gameObjectInstanceID = newGameObjectInstanceID;
            ReplaceComponent(index, component);
        }

        public void RemoveComUnityObjectRelated()
        {
            RemoveComponent(LogicComponentsLookup.ComUnityObjectRelated);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComUnityObjectRelatedIndex = new ComponentTypeIndex(typeof(UnityObjectRelatedComponent));
        public static int ComUnityObjectRelated => _ComUnityObjectRelatedIndex.Index;
    }
}
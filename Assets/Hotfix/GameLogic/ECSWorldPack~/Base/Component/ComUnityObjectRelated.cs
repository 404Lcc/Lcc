using System.Collections.Generic;

namespace LccHotfix
{
    public enum GameObjectType
    {
        None,
        Self,
        Part,
    }

    public class ComUnityObjectRelated : LogicComponent
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

        public override void Dispose()
        {
            base.Dispose();

            gameObjectInstanceID.Clear();
        }
    }

    public partial class LogicEntity
    {
        public ComUnityObjectRelated comUnityObjectRelated
        {
            get { return (ComUnityObjectRelated)GetComponent(LogicComponentsLookup.ComUnityObjectRelated); }
        }

        public bool hasComUnityObjectRelated
        {
            get { return HasComponent(LogicComponentsLookup.ComUnityObjectRelated); }
        }

        public void AddComUnityObjectRelated(Dictionary<int, GameObjectType> newGameObjectInstanceID)
        {
            var index = LogicComponentsLookup.ComUnityObjectRelated;
            var component = (ComUnityObjectRelated)CreateComponent(index, typeof(ComUnityObjectRelated));
            component.gameObjectInstanceID = newGameObjectInstanceID;
            AddComponent(index, component);
        }

        public void ReplaceComUnityObjectRelated(Dictionary<int, GameObjectType> newGameObjectInstanceID)
        {
            var index = LogicComponentsLookup.ComUnityObjectRelated;
            var component = (ComUnityObjectRelated)CreateComponent(index, typeof(ComUnityObjectRelated));
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
        private static ComponentTypeIndex ComUnityObjectRelatedIndex = new ComponentTypeIndex(typeof(ComUnityObjectRelated));
        public static int ComUnityObjectRelated => ComUnityObjectRelatedIndex.index;
    }
}
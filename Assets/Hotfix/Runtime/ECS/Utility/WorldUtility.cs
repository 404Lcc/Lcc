using UnityEngine;

namespace LccHotfix
{
    public static class WorldUtility
    {
        public static LogicEntity GetEntity(long id)
        {
            return WorldManager.Instance.GetWorld().GetEntityWithComID(id);
        }

        public static LogicEntity AddEntity<T>(GameObject obj) where T : ActorView, new()
        {
            var entity = WorldManager.Instance.GetWorld().LogicContext.CreateEntity();

            entity.AddComID(IdUtility.GenerateId());

            T viewWrapper = new T();
            viewWrapper.Init(obj, true);
            entity.AddView(viewWrapper, ViewCategory.Actor);

            entity.AddComTransform(obj.transform.position, obj.transform.rotation, obj.transform.localScale);


            return entity;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using LccHotfix;
using UnityEngine;

public static class EntityUtility
{
    public static LogicEntity GetEntity(long id)
    {
        return WorldManager.Instance.GetWorld().GetEntityWithComID(id);
    }

    public static LogicEntity GetEntity(GameObject go)
    {
        return WorldManager.Instance.GetWorld().GetEntitiesWithComUnityObjectRelated(go.GetInstanceID());
    }

    public static LogicEntity AddEntity<T>(GameObject obj, bool isPoolRes = false) where T : ActorView, new()
    {
        var entity = WorldManager.Instance.GetWorld().LogicContext.CreateEntity();

        entity.AddComID(IdUtility.GenerateId());

        T viewWrapper = new T();
        viewWrapper.Init(obj, isPoolRes);
        entity.AddView(viewWrapper, ViewCategory.Actor);

        entity.AddComTransform(obj.transform.position, obj.transform.rotation, obj.transform.localScale);

        var dict = new Dictionary<int, GameObjectType>();
        dict.Add(obj.GetInstanceID(), GameObjectType.Self);
        entity.AddComUnityObjectRelated(dict);
        return entity;
    }

    public static List<LogicEntity> GetHeroList()
    {
        var list = WorldManager.Instance.GetWorld().GetEntitiesWithComTag(TagType.Hero);
        return list.ToList();
    }

    public static LogicEntity GetHero(long uid)
    {
        var list = GetHeroList();
        if (list.Count == 0)
        {
            return null;
        }

        foreach (var item in list)
        {
            if (!item.hasComOwnerPlayer)
                continue;

            if (item.comOwnerPlayer.UID == uid)
            {
                return item;
            }
        }

        return null;
    }

    public static LogicEntity GetOwnerPlayerHero(BTAgent agent)
    {
        var entity = agent.GetSelfEntity();
        if (entity == null)
        {
            return null;
        }

        if (entity.hasComHero)
        {
            return entity;
        }

        if (entity.hasComOwnerPlayer)
        {
            return GetHero(entity.comOwnerPlayer.UID);
        }

        return null;
    }


    public static LogicEntity GetOwnerEntity(LogicEntity entity)
    {
        if (!entity.hasComOwnerEntity)
        {
            return null;
        }

        return GetEntity(entity.comOwnerEntity.ownerEntityID);
    }

}
using System.Collections.Generic;
using System.Linq;
using LccHotfix;
using UnityEngine;

public static class EntityUtility
{
    public static bool IsValid(this LogicEntity entity)
    {
        if (entity == null)
        {
            return false;
        }

        if (entity.IsEmpty())
        {
            return false;
        }

        if (entity.hasComLife)
        {
            return false;
        }

        // 属性判断
        if (!entity.hasComProperty)
        {
            return false;
        }

        // view判断
        if (!entity.hasComView)
        {
            return false;
        }

        return true;
    }

    public static LogicEntity GetEntity(long id)
    {
        return Main.WorldService.GetWorld().GetEntityWithComID(id);
    }

    public static LogicEntity GetEntity(GameObject go)
    {
        return Main.WorldService.GetWorld().GetEntitiesWithComUnityObjectRelated(go.GetInstanceID());
    }

    public static LogicEntity AddEntity<T>(GameObjectPoolObject poolObject) where T : ActorView, new()
    {
        var entity = Main.WorldService.GetWorld().LogicContext.CreateEntity();

        entity.AddComID(IdUtility.GenerateId());

        T viewWrapper = new T();
        viewWrapper.Init(poolObject);
        entity.AddView(viewWrapper, ViewCategory.Actor);

        entity.AddComTransform(poolObject.Transform.position, poolObject.Transform.rotation, poolObject.Transform.localScale);

        var dict = new Dictionary<int, GameObjectType>();
        dict.Add(poolObject.GameObject.GetInstanceID(), GameObjectType.Self);
        entity.AddComUnityObjectRelated(dict);
        return entity;
    }

    public static LogicEntity AddEntityWithID<T>(long id, GameObjectPoolObject poolObject) where T : ActorView, new()
    {
        var entity = Main.WorldService.GetWorld().LogicContext.CreateEntity();

        entity.AddComID(id);

        T viewWrapper = new T();
        viewWrapper.Init(poolObject);
        entity.AddView(viewWrapper, ViewCategory.Actor);

        entity.AddComTransform(poolObject.Transform.position, poolObject.Transform.rotation, poolObject.Transform.localScale);

        var dict = new Dictionary<int, GameObjectType>();
        dict.Add(poolObject.GameObject.GetInstanceID(), GameObjectType.Self);
        entity.AddComUnityObjectRelated(dict);
        return entity;
    }

    public static List<LogicEntity> GetHeroList()
    {
        var list = Main.WorldService.GetWorld().GetEntitiesWithComTag(TagType.Hero);
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

    public static LogicEntity GetOwnerEntity(LogicEntity entity)
    {
        if (!entity.hasComOwnerEntity)
        {
            return null;
        }

        return GetEntity(entity.comOwnerEntity.ownerEntityID);
    }

}
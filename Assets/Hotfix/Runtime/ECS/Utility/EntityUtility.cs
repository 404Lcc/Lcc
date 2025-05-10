using System.Collections.Generic;
using System.Linq;
using LccHotfix;
using UnityEngine;

public static class EntityUtility
{
    public static LogicEntity GetEntity(int entityId)
    {
        return WorldManager.Instance.GetWorld().GetEntityWithComID(entityId);
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
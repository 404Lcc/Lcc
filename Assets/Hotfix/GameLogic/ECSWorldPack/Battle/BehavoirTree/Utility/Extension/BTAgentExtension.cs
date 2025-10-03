namespace LccHotfix
{
    public static class BTAgentExtension
    {
        public static LogicEntity GetSelfEntity(this BTAgent agent)
        {
            return EntityUtility.GetEntity(agent.EntityId);
        }

        public static LogicEntity GetOwnerPlayerHero(this BTAgent agent)
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
                return EntityUtility.GetHero(entity.comOwnerPlayer.UID);
            }

            return null;
        }
    }
}
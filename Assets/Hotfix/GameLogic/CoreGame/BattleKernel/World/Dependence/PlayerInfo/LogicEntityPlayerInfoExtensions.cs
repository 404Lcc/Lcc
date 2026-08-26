namespace LccHotfix
{
    public static class LogicEntityPlayerInfoExtensions
    {
        public static InGamePlayerInfo GetPlayerInfo(this LogicEntity entity)
        {
            if (entity == null)
                return null;

            if (!entity.hasComOwnerPlayer)
            {
                if (entity.hasComHolder)
                {
                    var ownerEntity = entity.OwnerWorld?.GetEntityWithComID(entity.comHolder.HolderEntityID);
                    return GetPlayerInfo(ownerEntity);
                }

                return null;
            }

            return entity.comOwnerPlayer.PlayerInfoRef as InGamePlayerInfo;
        }
    }
}

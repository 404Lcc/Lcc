namespace LccHotfix
{
    public static class WorldUtility
    {
        public static LogicEntity GetEntity(long id)
        {
            return WorldManager.Instance.GetWorld().GetEntityWithComID(id);
        }
    }
}
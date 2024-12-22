namespace LccHotfix
{
    public static class WorldUtility
    {
        public static LogicEntity GetEntity(int id)
        {
            return WorldManager.Instance.GetWorld().GetEntityWithComID(id);
        }
    }
}
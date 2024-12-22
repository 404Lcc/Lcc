namespace LccHotfix
{
    public static class LogicAgentExtension
    {
        public static LogicEntity GetSelfEntity(this LogicAgent agent)
        {
            return WorldUtility.GetEntity(agent.EntityId);
        }
    }
}
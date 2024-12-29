namespace LccHotfix
{
    public static class BTAgentExtension
    {
        public static LogicEntity GetSelfEntity(this BTAgent agent)
        {
            return WorldUtility.GetEntity(agent.EntityId);
        }
    }
}
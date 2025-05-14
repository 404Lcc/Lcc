namespace LccHotfix
{
    public static class BTAgentExtension
    {
        public static LogicEntity GetSelfEntity(this BTAgent agent)
        {
            return EntityUtility.GetEntity(agent.EntityId);
        }
    }
}
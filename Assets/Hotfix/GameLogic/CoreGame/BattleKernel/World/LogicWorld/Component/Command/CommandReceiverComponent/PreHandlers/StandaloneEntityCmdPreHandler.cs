namespace LccHotfix
{
    public class StandaloneEntityCmdPreHandler : IEntityCommandPreHandler
    {
        public bool PreHandleCommand(LogicEntity owner, EntityCommand cmd)
        {
            foreach (var component in owner.GetComponents())
            {
                if (component is IEntityCommandHandler commandHandler)
                {
                    commandHandler.HandleEntityCommand(owner, cmd);
                }
            }

            return true;
        }
    }
}

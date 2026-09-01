namespace LccHotfix
{
    public class SubobjectEntityCmdPreHandler : IEntityCommandPreHandler
    {
        public bool PreHandleCommand(LogicEntity owner, EntityCommand cmd)
        {
            if (owner.hasComSubobject)
            {
                owner.comSubobject.HandleEntityCommand(owner, cmd);
            }

            return true;
        }
    }


}

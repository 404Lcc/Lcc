namespace LccHotfix
{
    public class SubObjDestroyNotifyNodeCfg : ICustomNodeCfg
    {
        public System.Type NodeType() { return typeof(SubObjDestroyNotifyNode); }
    }

    public class SubObjDestroyNotifyNode : CustomNode
    {
        public override void Destroy()
        {
            var logicEntity = GetOwnerEntity();
            if (logicEntity != null && logicEntity.hasComTransform)
            {
                GetLogicWorld()?.SubobjectLifecycleEventService?.DispatchDestroy(new EvtSubObjDestroy
                {
                    Where = logicEntity.comTransform.position,
                });
            }

            base.Destroy();
        }
    }
}

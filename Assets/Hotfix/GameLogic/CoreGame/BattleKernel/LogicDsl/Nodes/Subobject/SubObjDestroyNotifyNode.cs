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
            var logicEntity = this.GetOwnerEntity();
            if (logicEntity != null && logicEntity.hasComTransform)
            {
                this.GetLogicWorld()?.SubobjectLifecycleEventService?.DispatchDestroy(new EvtSubObjDestroy
                {
                    Where = logicEntity.comTransform.position,
                });
            }

            base.Destroy();
        }
    }
}

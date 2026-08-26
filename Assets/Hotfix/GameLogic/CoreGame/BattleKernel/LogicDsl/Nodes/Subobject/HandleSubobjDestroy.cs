namespace LccHotfix
{
    public class HandleSubobjDestroyCfg : ICustomNodeCfg
    {
        public System.Type NodeType() { return typeof(HandleSubobjDestroy); }
    }

    public class HandleSubobjDestroy : CustomNode
    {
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, in context);
            this.GetLogicWorld()?.SubobjectLifecycleEventService?.AddDestroyHandler(HandleSubObjDestroy);
        }

        public override void Destroy()
        {
            this.GetLogicWorld()?.SubobjectLifecycleEventService?.RemoveDestroyHandler(HandleSubObjDestroy);
            base.Destroy();
        }

        private void HandleSubObjDestroy(EvtSubObjDestroy evt)
        {
            SetVar(CvKey.CV_SubObjDestroyPos, evt.Where);
        }
    }
}

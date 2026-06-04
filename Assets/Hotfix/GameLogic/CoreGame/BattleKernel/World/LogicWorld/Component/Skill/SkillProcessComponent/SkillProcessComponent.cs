namespace LccHotfix
{
    /// <summary>
    /// 技能执行体
    /// </summary>
    public class SkillLogic : EntityCmdLogic, IForceEnd
    {
        protected bool _isForceEnd = false;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _isForceEnd = false;
        }

        public override void Destroy()
        {
            _isForceEnd = false;
            base.Destroy();
        }

        public void ForceEnd()
        {
            _isForceEnd = true;
        }

        public override bool CanStop()
        {
            if (_isForceEnd)
            {
                return true;
            }

            return base.CanStop();
        }
    }

    public class SkillProcessComponent : LogicComponent, IEntityCommandHandler
    {
        public uint SkillTid { get; set; }
        public CustomLogic SkillProcess { get; set; }

        public override void DisposeOnRemove()
        {
            if (SkillProcess != null)
            {
                Owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.CustomLogicService?.DestroyLogic(SkillProcess);
                SkillTid = 0;
                SkillProcess = null;
            }

            base.DisposeOnRemove();
        }

        public bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            if (SkillProcess is IEntityCommandHandler handler)
            {
                return handler.HandleEntityCommand(entity, cmd);
            }

            return false;
        }
    }


    public partial class LogicEntity
    {
        public SkillProcessComponent comSkillProcess
        {
            get { return (SkillProcessComponent)GetComponent(LogicComponentsLookup.ComSkillProcess); }
        }

        public bool hasComSkillProcess
        {
            get { return HasComponent(LogicComponentsLookup.ComSkillProcess); }
        }

        public void ReplaceComSkillProcess(uint skillTid, SkillLogic skillProcess)
        {
            var index = LogicComponentsLookup.ComSkillProcess;
            var component = (SkillProcessComponent)CreateComponent(index, typeof(SkillProcessComponent));
            component.SkillTid = skillTid;
            component.SkillProcess = skillProcess;
            ReplaceComponent(index, component);
        }

        public void RemoveComSkillProcess()
        {
            RemoveComponent(LogicComponentsLookup.ComSkillProcess);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComSkillProcessIndex = new(typeof(SkillProcessComponent));
        public static int ComSkillProcess => _ComSkillProcessIndex.Index;
    }
}
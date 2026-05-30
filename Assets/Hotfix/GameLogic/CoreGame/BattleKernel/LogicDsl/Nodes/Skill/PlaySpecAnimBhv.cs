using System;

namespace LccHotfix
{

    //////////////////////////////////////////////////////////////////////////
    /// 静态配置:
    public class PlaySpecAnimBhvCfg : ICustomNodeCfg
    {
        public StringCfg AnimName { get; set; }
        
        public bool ClearOnDestroy;

        public EntityVarCfg EntityCfg = new EntityVarCfg();

        public PlaySpecAnimBhvCfg(string animName, string entityVar)
        {
            AnimName = new StringCfg(animName);
            AnimName.ParseByFormatString(animName);
            ClearOnDestroy = false;
            EntityCfg = new EntityVarCfg(entityVar);
        }
        public Type NodeType()
        {
            return typeof(PlaySpecAnimBhv);
        }

        public void SetEntityVar(string varKey)
        {
            EntityCfg = new EntityVarCfg(varKey);
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class PlaySpecAnimBhv : BehaviorNode<PlaySpecAnimBhvCfg>
    {
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
        }
        
        public override void Destroy()
        {
            base.Destroy();
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            var entity = _cfg.EntityCfg.GetEntity(this);
            var animName = _cfg.AnimName;
            var animStateName = animName.GetValue(this);
            PlayAnim_Layer0(entity, animStateName);
        }

        public static void PlayAnim_Layer0(LogicEntity entity, string animStateName)
        {
            if (entity.hasComAnimation)
            {
                var comAnim = entity.comAnimation;
                var newData = comAnim.Data;
                newData.SpecAnim_Layer0 = animStateName;
                comAnim.SetData(newData);
            }
        }
    }
}

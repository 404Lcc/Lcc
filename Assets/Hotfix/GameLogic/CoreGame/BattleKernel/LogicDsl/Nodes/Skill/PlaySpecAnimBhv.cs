using System;
using PBConfig;

namespace LccHotfix
{

    //////////////////////////////////////////////////////////////////////////
    /// 静态配置:
    public class PlaySpecAnimBhvCfg : ICustomNodeCfg
    {
        public StringCfg AnimName { get; set; }
        
        public bool ClearOnDestroy;

        public EntityVarCfg EntityCfg = new EntityVarCfg();
        public int LayerIndex = 0;
        public FloatCfg AnimationSpeed = new FloatCfg(1f);
        public bool ForceRestart;

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

        public PlaySpecAnimBhvCfg SetLayer(int layerIndex)
        {
            LayerIndex = layerIndex;
            return this;
        }

        public PlaySpecAnimBhvCfg WithSpeed(string speedVar)
        {
            AnimationSpeed = new FloatCfg(speedVar);
            return this;
        }

        public PlaySpecAnimBhvCfg WithForceRestart()
        {
            ForceRestart = true;
            return this;
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
            PlayAnim(entity, animStateName, _cfg.LayerIndex);
        }

        public static void PlayAnim_Layer0(LogicEntity entity, string animStateName)
        {
            PlayAnim(entity, animStateName, 0);
        }

        public static void PlayAnim(LogicEntity entity, string animStateName, int layerIndex)
        {
            if (entity == null || !entity.hasComAnimation)
                return;
            var comAnim = entity.comAnimation;
            var newData = comAnim.Data;
            if (layerIndex == 1)
                newData.SpecAnim_Layer1 = animStateName;
            else
                newData.SpecAnim_Layer0 = animStateName;
            comAnim.SetData(newData);
        }


    }
}

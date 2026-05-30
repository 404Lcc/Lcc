using System;

namespace LccHotfix
{

    //////////////////////////////////////////////////////////////////////////
    /// 静态配置:
    public class PlaySpecEntityAnimBhvCfg : ICustomNodeCfg
    {
        public StringCfg AnimName;
        public StringCfg EntityKey;
        public int LayerIndex = 0;
        public bool ClearOnDestroy;
        public PlaySpecEntityAnimBhvCfg(string entityKey, string animName, bool clearOnDestroy = true)
        {
            AnimName = new StringCfg(animName);
            AnimName.ParseByFormatString(animName);
            EntityKey = new StringCfg(entityKey);
            EntityKey.ParseByString(entityKey);
            ClearOnDestroy = clearOnDestroy;
        }
        public Type NodeType()
        {
            return typeof(PlaySpecEntityAnimBhv);
        }

        public PlaySpecEntityAnimBhvCfg SetLayer(int layerIndex)
        {
            LayerIndex = layerIndex;
            return this;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class PlaySpecEntityAnimBhv : BehaviorNode<PlaySpecEntityAnimBhvCfg>
    {
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            var entity = GetVar<LogicEntity>(_cfg.EntityKey.GetValue(this));
            if (entity == null)
                return;
            var animName = _cfg.AnimName;
            if (entity.hasComAnimation)
            {
                var comAnim = entity.comAnimation;
                var newData = comAnim.Data;
                if (_cfg.LayerIndex == 1)
                    newData.SpecAnim_Layer1 = animName.GetValue(this);
                else
                    newData.SpecAnim_Layer0 = animName.GetValue(this);
                comAnim.SetData(newData);
            }
        }

    }
}

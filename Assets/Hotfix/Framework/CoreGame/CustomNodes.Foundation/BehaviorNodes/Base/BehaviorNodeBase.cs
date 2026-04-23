using System;

namespace LccHotfix
{
    public class NoneParamBhvCfg : ICustomNodeCfg
    {
        public Type BhvType { get; protected set; }

        public NoneParamBhvCfg(System.Type bhvType)
        {
            CLHelper.Assert(bhvType.IsSubclassOf(typeof(BehaviorNodeBase)));
            BhvType = bhvType;
        }

        public Type NodeType()
        {
            return BhvType;
        }
    }

    public abstract class BehaviorNodeBase : CustomNode, INeedUpdate
    {
        //需要知道第一次Update的行为
        protected bool _hasUpdate = false;

        public override void Reset()
        {
            //运行前，内部状态的初始化放在这里。（主要用于可以重复多次执行的Behavior）
            _hasUpdate = false;
        }

        public virtual float Update(float dt)
        {
            if (!_hasUpdate)
            {
                _hasUpdate = true;
                OnBegin();
            }

            return OnUpdate(dt);
        }


        public override void Destroy()
        {
            _hasUpdate = false;
            base.Destroy();
        }

        protected virtual void OnBegin()
        {
        }

        //返回消耗后剩余的时间片
        protected virtual float OnUpdate(float dt)
        {
            return dt;
        }
    }

    public abstract class BehaviorNode<T> : BehaviorNodeBase where T : class, ICustomNodeCfg
    {
        protected T _cfg;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _cfg = cfg as T;
            if (_cfg == null)
            {
                CLHelper.LogError(this, $"BehaviorNode mCfg == null node={this}, T={typeof(T)}");
            }
        }

        public override void Destroy()
        {
            _cfg = null;
            base.Destroy();
        }
    }
}
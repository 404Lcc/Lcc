using System;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _EmptyBhvCfg = Register(typeof(NoneParamBhvCfg), NodeCategory.Bhv);
    }

    public class NoneParamBhvCfg : ICustomNodeCfg
    {
        public System.Type BhvType { get; protected set; }

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
        protected bool mHasUpdate = false;

        public override void Reset()
        {
            //运行前，内部状态的初始化放在这里。（主要用于可以重复多次执行的Behavior）
            mHasUpdate = false;
        }

        public virtual float Update(float dt)
        {
            if (!mHasUpdate)
            {
                mHasUpdate = true;
                OnBegin();
            }

            return OnUpdate(dt);
        }


        public override void Destroy()
        {
            mHasUpdate = false;
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
        protected T mCfg;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            mCfg = cfg as T;
            if (mCfg == null)
            {
                CLHelper.LogError(this, $"BehaviorNode mCfg == null node={this}, T={typeof(T)}");
            }
        }

        public override void Destroy()
        {
            mCfg = null;
            base.Destroy();
        }
    }
}
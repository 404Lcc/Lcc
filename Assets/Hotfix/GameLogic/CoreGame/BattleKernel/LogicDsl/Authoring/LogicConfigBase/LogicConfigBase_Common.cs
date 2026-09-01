using System.Runtime.CompilerServices;

namespace LccHotfix
{
    public partial class LogicConfigBase
    {
        /// <summary>
        /// 创建只在节点开始时执行一次的委托节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg BeginCall(NodeParamAction func)
        {
            return new DelegateBhvCfg(func);
        }

        /// <summary>
        /// 创建从节点开始后每帧执行的委托节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg UpdateCall(NodeParamTickAction func)
        {
            return new DelegateBhvCfg(func)
            {
                UpdateCall = true,
                BeginCall = true,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlaySpecAnimBhvCfg PlayAnim_Layer0(string animName, string entityVar = CvKey.CV_OwnerEntity)
        {
            return new PlaySpecAnimBhvCfg(animName, entityVar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlaySpecAnimBhvCfg PlayAnim_Layer1(string animName, string entityVar = CvKey.CV_OwnerEntity)
        {
            return new PlaySpecAnimBhvCfg(animName, entityVar).SetLayer(1);
        }

        /// <summary>
        /// 创建指定持续时间内每帧执行更新回调的节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FiniteTimeUpdateCfg FiniteTimeUpdate(NodeParamTickAction func, float time)
        {
            return new FiniteTimeUpdateCfg(time).WithUpdate(func);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateConditionCfg IsEntityVaild(string varID)
        {
            return new DelegateConditionCfg(node =>
            {
                var cfg = new EntityVarCfg(varID);
                var entity = cfg.GetEntity(node, false);
                return entity.IsValid();
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WaitCheckBhvCfg WaitEnityHasView(string varID = CvKey.CV_OwnerEntity)
        {
            return new WaitCheckBhvCfg(node =>
            {
                var cfg = new EntityVarCfg(varID);
                var entity = cfg.GetEntity(node, false);
                if (entity == null)
                {
                    return false;
                }

                return entity.hasComView;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WaitCheckBhvCfg WaitEntityHasMainGameObjectView(string varID = CvKey.CV_OwnerEntity)
        {
            return new WaitCheckBhvCfg(node =>
            {
                var cfg = new EntityVarCfg(varID);
                var entity = cfg.GetEntity(node, false);
                return entity != null && entity.IsMainGameObjectViewReady();
            });
        }

        public DelegateBhvCfg SetLife(float time)
        {
            return new DelegateBhvCfg(node =>
            {
                var ownerEntity = node.GetOwnerEntity();
                if (ownerEntity == null)
                {
                    return;
                }

                ownerEntity.ReplaceComLife(time);
            });
        }

        public DelegateBhvCfg SetLife(string timeVar)
        {
            return new DelegateBhvCfg(node =>
            {
                var time = node.GetVar<float>(timeVar, -1f);
                if (time < 0)
                {
                    return;
                }

                var ownerEntity = node.GetOwnerEntity();
                if (ownerEntity == null)
                {
                    return;
                }

                ownerEntity.ReplaceComLife(time);
            });
        }
    }
}

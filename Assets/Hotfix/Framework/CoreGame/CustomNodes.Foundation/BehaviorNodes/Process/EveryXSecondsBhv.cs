using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 静态配置：每隔 intervalSeconds 秒执行一次 actionNodes（子序列可包含 UpdateCall 等，可持续多帧）。
    /// </summary>
    public class EveryXSecondsBhvCfg : ICustomNodeCfg
    {
        public float IntervalSeconds { get; protected set; }
        public List<ICustomNodeCfg> ActionNodes { get; protected set; }

        public System.Type NodeType()
        {
            return typeof(EveryXSecondsBhv);
        }

        public EveryXSecondsBhvCfg(float intervalSeconds, List<ICustomNodeCfg> actionNodes)
        {
            IntervalSeconds = intervalSeconds;
            ActionNodes = actionNodes ?? new List<ICustomNodeCfg>();
        }
    }

    /// <summary>
    /// 运行时：用 Update 做计时，每到 interval 秒就创建并运行一次 action 子序列，子序列跑完后再等 interval 秒，循环直至父逻辑结束。
    /// </summary>
    public class EveryXSecondsBhv : BehaviorNodeBase, INeedStopCheck
    {
        private EveryXSecondsBhvCfg _cfg;
        private float _acc;
        private SequenceBhv _actionSeq;
        private SequenceBhvCfg _actionSeqCfg;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _cfg = cfg as EveryXSecondsBhvCfg;
            if (_cfg == null || _cfg.ActionNodes == null || _cfg.ActionNodes.Count == 0)
                return;
            _actionSeqCfg = new SequenceBhvCfg(_cfg.ActionNodes, 1, 0f);
        }

        protected override void OnBegin()
        {
            // 刚进入时先触发一次，再进入 CD（首次 Update 即满足 mAcc >= IntervalSeconds）
            _acc = _cfg != null ? _cfg.IntervalSeconds : 0f;
            _actionSeq = null;
        }

        protected override float OnUpdate(float dt)
        {
            if (_cfg == null || _actionSeqCfg == null)
                return dt;

            if (_actionSeq != null)
            {
                float dtRemain = _actionSeq.Update(dt);
                if (!IsActive)
                    return 0f;
                if (_actionSeq is INeedStopCheck needStop && needStop.CanStop())
                {
                    _context.Factory.DestroyCustomNode(_actionSeq);
                    _actionSeq = null;
                    return dtRemain;
                }

                return 0f;
            }

            _acc += dt;
            if (_acc < _cfg.IntervalSeconds)
                return 0f;

            _acc -= _cfg.IntervalSeconds;
            var seq = _context.Factory.CreateCustomNode(_actionSeqCfg, _context) as SequenceBhv;
            if (seq == null)
                return dt;
            _actionSeq = seq;
            _actionSeq.Activate();
            float remain = _actionSeq.Update(dt);
            if (!IsActive)
                return 0f;
            if (_actionSeq is INeedStopCheck ns && ns.CanStop())
            {
                _context.Factory.DestroyCustomNode(_actionSeq);
                _actionSeq = null;
                return remain;
            }

            return 0f;
        }

        public override void Destroy()
        {
            if (_actionSeq != null)
            {
                _context.Factory.DestroyCustomNode(_actionSeq);
                _actionSeq = null;
            }

            _actionSeqCfg = null;
            _cfg = null;
            base.Destroy();
        }

        public bool CanStop()
        {
            return false;
        }
    }
}
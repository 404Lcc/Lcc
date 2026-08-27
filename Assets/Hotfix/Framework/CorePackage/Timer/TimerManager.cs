using System;
using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 三级分层时间轮实现：MS(100×10ms) / SEC(60×1s) / MIN(60×1min)，双时间基各自独立。
    /// 默认 MaxOneFirePerUpdate：同一 Tick 内每个节点最多回调一次（合并卡顿补票，不降钟）；有限 repeatCount 会摊到后续帧。
    /// </summary>
    public sealed class TimerManager : Module, ITimerService
    {
        // 逻辑刻度：与帧间隔同量级，降低每帧 Catch-up Tick 次数
        private const long TickMs = 10;

        // 一级轮：100 槽 × 10ms = 1s
        private const int MsSlotCount = 100;

        private const long MsSpan = MsSlotCount * TickMs;

        // 二级轮：60 槽 × 1s
        private const int SecSlotCount = 60;

        private const long SecSpan = 60_000;

        // 三级轮：60 槽 × 1min；总跨度 1 小时
        private const int MinSlotCount = 60;

        private const long MinSpan = 3_600_000;

        // 超长定时器固定挂载的分钟槽下标
        private const int OverflowMinSlot = MinSlotCount - 1;

        // 节点对象池，避免 AddTimer 热路径 new
        private readonly Stack<TimerNode> _pool = new Stack<TimerNode>(64);

        // ID → 节点，供 O(1) 惰性删除查找
        private readonly Dictionary<int, TimerNode> _nodes = new Dictionary<int, TimerNode>(64);

        // 下一枚可用定时器 ID（从 1 起，0 表示无效）
        private int _nextId = 1;

        // 跟随 timeScale 的时间轮
        private readonly TimingWheel _scaled;

        // 无视 timeScale 的时间轮
        private readonly TimingWheel _unscaled;

        // true：每帧每节点最多一次回调；false：catch-up 按刻度补齐 Fire
        private bool _maxOneFirePerUpdate;

        public TimerManager() : this(true)
        {
        }

        /// <summary>
        /// 创建定时器服务。
        /// </summary>
        /// <param name="maxOneFirePerUpdate">true=同一 Tick 内每节点最多 Fire 一次（默认）；false=按刻度 catch-up 补齐回调</param>
        public TimerManager(bool maxOneFirePerUpdate = true)
        {
            _maxOneFirePerUpdate = maxOneFirePerUpdate;
            _scaled = new TimingWheel(this);
            _unscaled = new TimingWheel(this);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            Tick(realElapseSeconds, elapseSeconds);
        }

        /// <summary>
        /// 清空双轮与 ID 表，回收全部节点并重置计数。
        /// </summary>
        internal override void Shutdown()
        {
            _scaled.Clear();
            _unscaled.Clear();

            foreach (var kv in _nodes)
            {
                RecycleNode(kv.Value);
            }

            _nodes.Clear();
            _pool.Clear();
            _nextId = 1;
        }

        //////////////////////////////////////////////////////////////////////////
        // ITimerService:

        /// <summary>
        /// 添加定时器；intervalSec 转毫秒后按 TickMs 向下对齐，最小钳制为 TickMs。
        /// 默认每帧每节点最多回调一次；有限 repeatCount 在卡顿后摊到后续帧，不会一次打完。
        /// </summary>
        /// <param name="callback">回调：elapsedSec 为距上次计划触发的实际间隔，第二个参数为已执行次数（从 1 起）</param>
        /// <param name="intervalSec">间隔（秒）</param>
        /// <param name="repeatCount">1=单次，-1=无限，N=次数；0 或小于 -1 非法</param>
        /// <param name="useTimeScale">true=跟随 timeScale（scaled）；false=无视 timeScale（unscaled）</param>
        /// <returns>定时器 ID；失败返回 -1</returns>
        public int AddTimer(Action<float, int> callback, float intervalSec = 1f, int repeatCount = 1, bool useTimeScale = false)
        {
            if (callback == null)
            {
                KLogger.LogError($"[TimerService] callback should not be null");
                return -1;
            }

            // 0 次无意义；仅允许 -1（无限）或正次数
            if (repeatCount == 0 || repeatCount < -1)
            {
                KLogger.LogError($"[TimerService] 无意义的 repeatCount:{repeatCount}");
                return -1;
            }

            long intervalMs = AlignIntervalMs((long)(intervalSec * 1000f));

            var node = RentNode();
            int id = _nextId++;
            if (_nextId <= 0)
            {
                _nextId = 1;
            }

            node.Id = id;
            node.IntervalMs = intervalMs;
            node.RepeatCount = repeatCount;
            node.ExecutedCount = 0;
            node.Callback = callback;
            node.UseTimeScale = useTimeScale;
            node.IsCanceled = false;
            node.Cycle = 0;
            node.Next = null;

            _nodes[id] = node;

            var wheel = useTimeScale ? _scaled : _unscaled;
            node.ExpireTime = wheel.NowMs + intervalMs;
            wheel.Schedule(node);
            return id;
        }

        /// <summary>
        /// 惰性删除：仅标记取消并从 ID 表移除，槽链表中由 Tick 回收。
        /// </summary>
        public bool RemoveTimer(int timerId)
        {
            if (timerId <= 0)
            {
                return false;
            }

            if (!_nodes.TryGetValue(timerId, out var node))
            {
                return false;
            }

            node.IsCanceled = true;
            node.Callback = null;
            _nodes.Remove(timerId);
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        // This:

        /// <summary>
        /// 为 true 时，同一 Tick 内每个节点最多 Fire 一次；错过的 tick 不补回调。
        /// 时间轮时钟仍全量 catch-up。运行时修改只影响后续 Fire 的重挂基准。
        /// 有限 repeatCount 在卡顿后不会在同一帧打完，剩余次数摊到后续帧。
        /// </summary>
        public bool MaxOneFirePerUpdate
        {
            get { return _maxOneFirePerUpdate; }
            set { _maxOneFirePerUpdate = value; }
        }

        /// <summary>
        /// 每帧分别推进 unscaled / scaled 时间轮（时钟 catch-up；回调是否合并由 MaxOneFirePerUpdate 决定）。
        /// </summary>
        /// <param name="fixedDeltaTime">无视 timeScale 的增量（秒）</param>
        /// <param name="flexDeltaTime">跟随 timeScale 的增量（秒）</param>
        public void Tick(float fixedDeltaTime, float flexDeltaTime)
        {
            _unscaled.Advance(fixedDeltaTime);
            _scaled.Advance(flexDeltaTime);
        }

        // 向下对齐到 TickMs，且不小于一个刻度
        private static long AlignIntervalMs(long intervalMs)
        {
            if (intervalMs < TickMs)
            {
                return TickMs;
            }

            return intervalMs - (intervalMs % TickMs);
        }

        // 从池取节点，池空则新建
        private TimerNode RentNode()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            return new TimerNode();
        }

        // 清空节点字段后归还对象池，供后续 AddTimer 复用
        private void RecycleNode(TimerNode node)
        {
            if (node == null)
            {
                return;
            }

            node.Id = 0;
            node.IntervalMs = 0;
            node.RepeatCount = 0;
            node.ExecutedCount = 0;
            node.ExpireTime = 0;
            node.Cycle = 0;
            node.Callback = null;
            node.UseTimeScale = false;
            node.IsCanceled = true;
            node.Next = null;
            _pool.Push(node);
        }

        // 触发回调；支持回调内 Remove 自身；按需重新挂载或回收。
        // MaxOneFirePerUpdate 时到期点跳到本帧终点+interval，避免 catch-up 连射；关闭则仍按 NowMs 补齐。
        private void Fire(TimerNode node)
        {
            if (node.IsCanceled)
            {
                RecycleNode(node);
                return;
            }

            node.ExecutedCount++;
            int execCount = node.ExecutedCount;
            var callback = node.Callback;
            var wheel = node.UseTimeScale ? _scaled : _unscaled;
            long nowMs = _maxOneFirePerUpdate ? wheel.FrameEndMs : wheel.NowMs;
            // 对齐 GameTimerManager：interval + now - expire
            float elapsedSec = (nowMs - node.ExpireTime + node.IntervalMs) / 1000f;

            try
            {
                callback?.Invoke(elapsedSec, execCount);
            }
            catch (Exception ex)
            {
                KLogger.LogError($"[TimerService] Timer ID = {node.Id} callback exception: {ex}");
            }

            if (node.IsCanceled)
            {
                RecycleNode(node);
                return;
            }

            bool again = node.RepeatCount < 0 || node.ExecutedCount < node.RepeatCount;
            if (!again)
            {
                _nodes.Remove(node.Id);
                RecycleNode(node);
                return;
            }

            long baseMs = _maxOneFirePerUpdate ? wheel.FrameEndMs : wheel.NowMs;
            node.ExpireTime = baseMs + node.IntervalMs;
            node.Cycle = 0;
            wheel.Schedule(node);
        }

        // 从槽中取出的已取消节点：若仍在 ID 表则摘掉后回收
        private void Discard(TimerNode node)
        {
            if (node.Id != 0 && _nodes.TryGetValue(node.Id, out var mapped) && ReferenceEquals(mapped, node))
            {
                _nodes.Remove(node.Id);
            }

            RecycleNode(node);
        }

        //////////////////////////////////////////////////////////////////////////
        // TimerNode:

        private sealed class TimerNode
        {
            // 对外唯一 ID
            public int Id;

            // 重复间隔（毫秒）
            public long IntervalMs;

            // 1=单次，-1=无限，N=次数
            public int RepeatCount;

            // 已执行次数（回调参数）
            public int ExecutedCount;

            // 绝对到期时间（所属时间轮逻辑时钟）
            public long ExpireTime;

            // 超长定时器剩余整小时轮次
            public int Cycle;

            // 到期回调：elapsedSec + 已执行次数
            public Action<float, int> Callback;

            // true=挂 scaled 轮；false=挂 unscaled 轮
            public bool UseTimeScale;

            // 惰性删除标记
            public bool IsCanceled;

            // 槽内单向链表
            public TimerNode Next;
        }

        //////////////////////////////////////////////////////////////////////////
        // TimingWheel:

        private sealed class TimingWheel
        {
            private readonly TimerManager _owner;

            // 逻辑当前时间（整数毫秒）
            private long _nowMs;

            // 本帧 Advance 结束后的逻辑时钟；Fire 合并时用作重挂基准
            private long _frameEndMs;

            // 亚毫秒残差，避免 float dt 截断漂移
            private double _residue;
            private readonly TimerNode[] _msSlots = new TimerNode[MsSlotCount];
            private readonly TimerNode[] _secSlots = new TimerNode[SecSlotCount];
            private readonly TimerNode[] _minSlots = new TimerNode[MinSlotCount];
            private int _msCursor;
            private int _secCursor;
            private int _minCursor;

            // 绑定所属 TimerManager
            public TimingWheel(TimerManager owner)
            {
                _owner = owner;
            }

            // 当前逻辑时钟（毫秒）
            public long NowMs => _nowMs;

            // 本帧 Advance 终点（毫秒）；与 TickOne 次数一致
            public long FrameEndMs => _frameEndMs;

            // 累加流逝时间并 catch-up 按 TickMs 推进；预先记下本帧逻辑终点供 Fire 合并重挂
            public void Advance(float deltaSec)
            {
                if (deltaSec <= 0f)
                {
                    return;
                }

                _residue += deltaSec * 1000.0;
                // 与下方 while 次数一致：floor(residue/TickMs)；Fire 时 _nowMs 尚未跑完
                long tickCount = (long)(_residue / TickMs);
                _frameEndMs = _nowMs + tickCount * TickMs;
                while (_residue >= TickMs)
                {
                    _residue -= TickMs;
                    TickOne();
                }
            }

            // 按剩余 delay 挂入 MS/SEC/MIN 槽，或末槽超长（Cycle）路径
            public void Schedule(TimerNode node)
            {
                if (node == null || node.IsCanceled)
                {
                    _owner.Discard(node);
                    return;
                }

                long delay = node.ExpireTime - _nowMs;
                if (delay <= 0)
                {
                    // 已到期：挂到下一刻度槽，保证本帧 catch-up 内可触发
                    delay = TickMs;
                    node.ExpireTime = _nowMs + TickMs;
                }

                if (delay < MsSpan)
                {
                    // 槽距按 TickMs 换算（interval 已对齐，delay 应为刻度整数倍）
                    long ticks = delay / TickMs;
                    if (ticks < 1)
                    {
                        ticks = 1;
                    }

                    int slot = (int)((_msCursor + ticks) % MsSlotCount);
                    Push(_msSlots, slot, node);
                    return;
                }

                if (delay < SecSpan)
                {
                    // 秒轮槽距：至少 1，避免与当前正在级联的槽重叠丢失
                    long steps = delay / MsSpan;
                    int slot = (int)((_secCursor + steps) % SecSlotCount);
                    Push(_secSlots, slot, node);
                    return;
                }

                if (delay < MinSpan)
                {
                    long steps = delay / SecSpan;
                    int slot = (int)((_minCursor + steps) % MinSlotCount);
                    Push(_minSlots, slot, node);
                    return;
                }

                // 超过 1 小时：挂分钟轮末槽，用 Cycle 消化整小时
                node.Cycle = (int)(delay / MinSpan);
                Push(_minSlots, OverflowMinSlot, node);
            }

            // 清空三层槽并重置游标与逻辑时钟
            public void Clear()
            {
                ClearSlots(_msSlots);
                ClearSlots(_secSlots);
                ClearSlots(_minSlots);
                _msCursor = 0;
                _secCursor = 0;
                _minCursor = 0;
                _nowMs = 0;
                _frameEndMs = 0;
                _residue = 0;
            }

            // 推进一个 TickMs：步进毫秒游标，必要时级联秒轮后处理当前槽
            private void TickOne()
            {
                _nowMs += TickMs;
                _msCursor++;
                if (_msCursor >= MsSlotCount)
                {
                    _msCursor = 0;
                    AdvanceSec();
                }

                ProcessMsSlot(_msCursor);
            }

            // 秒轮步进一格（必要时级联分钟轮），并级联当前秒槽
            private void AdvanceSec()
            {
                _secCursor++;
                if (_secCursor >= SecSlotCount)
                {
                    _secCursor = 0;
                    AdvanceMin();
                }

                CascadeSlot(_secSlots, _secCursor);
            }

            // 分钟轮步进一格（绕回），并级联当前分钟槽
            private void AdvanceMin()
            {
                _minCursor++;
                if (_minCursor >= MinSlotCount)
                {
                    _minCursor = 0;
                }

                CascadeMinSlot(_minCursor);
            }

            // 处理毫秒槽：取消则回收，未到期则重挂，到期则 Fire
            private void ProcessMsSlot(int slot)
            {
                TimerNode head = _msSlots[slot];
                _msSlots[slot] = null;
                while (head != null)
                {
                    TimerNode next = head.Next;
                    head.Next = null;

                    if (head.IsCanceled)
                    {
                        _owner.Discard(head);
                        head = next;
                        continue;
                    }

                    // 级联精度：尚未真正到期则重新挂载
                    if (head.ExpireTime > _nowMs)
                    {
                        Schedule(head);
                        head = next;
                        continue;
                    }

                    _owner.Fire(head);
                    head = next;
                }
            }

            // 上级轮槽级联：取出整槽节点，取消则回收，否则重新 Schedule 到更细轮
            private void CascadeSlot(TimerNode[] slots, int slot)
            {
                TimerNode head = slots[slot];
                slots[slot] = null;
                while (head != null)
                {
                    TimerNode next = head.Next;
                    head.Next = null;

                    if (head.IsCanceled)
                    {
                        _owner.Discard(head);
                        head = next;
                        continue;
                    }

                    Schedule(head);
                    head = next;
                }
            }

            // 分钟槽级联：末槽超长定时器先扣 Cycle，归零后再 Schedule
            private void CascadeMinSlot(int slot)
            {
                TimerNode head = _minSlots[slot];
                _minSlots[slot] = null;
                while (head != null)
                {
                    TimerNode next = head.Next;
                    head.Next = null;

                    if (head.IsCanceled)
                    {
                        _owner.Discard(head);
                        head = next;
                        continue;
                    }

                    // 末槽超长定时器：先扣减整小时轮次；归零后必须立即 Schedule，不可再泊回末槽（否则会多等一整轮）
                    if (slot == OverflowMinSlot && head.Cycle > 0)
                    {
                        head.Cycle--;
                        if (head.Cycle > 0)
                        {
                            Push(_minSlots, OverflowMinSlot, head);
                            head = next;
                            continue;
                        }
                    }

                    Schedule(head);
                    head = next;
                }
            }

            // 头插法将节点挂入指定槽链表
            private static void Push(TimerNode[] slots, int slot, TimerNode node)
            {
                node.Next = slots[slot];
                slots[slot] = node;
            }

            // 遍历清空槽数组，槽内节点一律 Discard
            private void ClearSlots(TimerNode[] slots)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    TimerNode head = slots[i];
                    slots[i] = null;
                    while (head != null)
                    {
                        TimerNode next = head.Next;
                        head.Next = null;
                        _owner.Discard(head);
                        head = next;
                    }
                }
            }
        }
    }
}
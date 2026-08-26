using System;

namespace LccHotfix
{
    public abstract class HitMaker
    {
        internal int _hitCount;
        public RawHit[] RawHits { get; protected set; }

        /// <summary>
        /// 设置命中缓冲容量。已有数组且长度足够则复用，不会缩小。
        /// </summary>
        public virtual void SetCapacity(int capacity)
        {
            // 容量不足才分配，避免池化取出后打掉已有缓冲。
            if (RawHits == null || RawHits.Length < capacity)
                RawHits = new RawHit[capacity];
        }

        public bool IsFull()
        {
            return _hitCount >= RawHits.Length;
        }

        public void Add(RawHit hit)
        {
            RawHits[_hitCount] = hit;
            _hitCount++;
        }

        public bool ContainsHit()
        {
            return _hitCount > 0;
        }

        public virtual void Cleanup()
        {
            _hitCount = 0;
            Array.Clear(RawHits, 0, RawHits.Length);
        }

        public virtual void OnRecycle()
        {
            Cleanup();
        }

        public abstract bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity item, out RawHit hit);
    }
}
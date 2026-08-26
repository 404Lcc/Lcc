using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 怪物死亡事件
    /// </summary>
    public struct EvtMonsterDead : IValueEvent
    {
        public uint Tid;
        public Vector3 Position;
        public int SpawnRuleIndex;
        public long HolderEntityId;
        public bool IsSummon;
    }
}

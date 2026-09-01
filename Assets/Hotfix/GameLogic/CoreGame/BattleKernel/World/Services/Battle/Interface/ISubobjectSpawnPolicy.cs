namespace LccHotfix
{
    /// <summary>
    /// 子物体生成前的玩法校验（如小队英雄战死禁止再开火生子物体）。由上层注入。
    /// </summary>
    public interface ISubobjectSpawnPolicy
    {
        /// <summary>返回 false 时禁止从该 owner 生成子物体。</summary>
        bool CanSpawnFromOwner(LogicEntity owner);
    }
}

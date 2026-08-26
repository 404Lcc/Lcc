namespace LccHotfix
{
    /// <summary>
    /// 命中派发伤害前用于调整伤害事件的回调。
    /// </summary>
    public delegate void DamageAdjustFunc(CustomNode node, ref EvtDamage dmg);

    /// <summary>
    /// 命中效果执行前用于调整命中信息的回调。
    /// </summary>
    public delegate void PreHitFunc(CustomNode node, ref HitInfo hitInfo);

    /// <summary>
    /// 写入 Buff GenInfo.PreEnv 的回调。
    /// </summary>
    public delegate void BuffPreEnvFunc(CustomNode node, BuffGenInfo genInfo, in HitInfo hitInfo);

    /// <summary>
    /// AOE 或命中流程中对单个目标执行命中效果的回调。
    /// </summary>
    public delegate void NodeHitEffectFunc(CustomNode node, LogicEntity entity, LogicEntity target, HitInfo hitInfo);
}

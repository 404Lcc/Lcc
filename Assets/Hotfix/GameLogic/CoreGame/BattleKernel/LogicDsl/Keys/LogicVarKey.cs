namespace LccHotfix
{
    public partial class CvKey
    {
        #region 通用上下文

        /// <summary>
        /// 战斗单位 TFighter 静态配置；实体死亡后仍需读取配置时从黑板获取。
        /// </summary>
        public const string CV_FigherCfg = "CV_FigherCfg";

        /// <summary>
        /// 当前战斗补给配置的匿名逻辑参数。
        /// </summary>
        public const string CV_SupplyLogicParams = "CV_SupplyLogicParams";

        /// <summary>
        /// 当前补给卡 TID。
        /// </summary>
        public const string CV_BattleSupplyId = "CV_BattleSupplyId";

        #endregion

        #region 战斗单位与属性

        /// <summary>
        /// 战斗单位配置 ID。
        /// </summary>
        public const string CV_BattleUnitTid = "CV_BattleUnitTid";

        /// <summary>
        /// Whether the enemy FSM should switch to MST_Hit after taking damage.
        /// </summary>
        public const string CV_EnableHurtHitState = "CV_EnableHurtHitState";

        /// <summary>
        /// 最近一次受击的碰撞击中点，用于受击/冒血特效位置。
        /// </summary>
        public const string CV_LastHurtHitPos = "CV_LastHurtHitPos";

        /// <summary>
        /// 属性修改或读取时使用的浮点属性值。
        /// </summary>
        public const string CV_AttributeValuef = "CV_AttributeValuef";

        /// <summary>
        /// 属性修改或读取时使用的属性 Key。
        /// </summary>
        public const string CV_AttributeKey = "CV_AttributeKey";

        #endregion

        #region 技能

        /// <summary>
        /// 当前技能配置 ID。
        /// </summary>
        public const string CV_SkillTid = "CV_SkillTid";

        /// <summary>
        /// 当前技能伤害倍率。
        /// </summary>
        public const string CV_SkillDmageRate = "CV_SkillDmageRate";

        /// <summary>
        /// 技能配置中默认生成的子物体 ID。
        /// </summary>
        public const string CV_BaseSpawnSbjTid = "CV_BaseSpawnSbjTid";

        /// <summary>
        /// 当前技能或命中流程使用的伤害类型。
        /// </summary>
        public const string CV_DamageType = "CV_DamageType";

        /// <summary>
        /// 当前技能等级。
        /// </summary>
        public const string CV_SkillLevel = "CV_SkillLevel";

        /// <summary>
        /// 子物体配置 ID。
        /// </summary>
        public const string CV_SbjTid = "CV_SbjTid";

        /// <summary>
        /// 实际生成子物体时使用的子物体 ID。
        /// </summary>
        public const string CV_SpawnSbjTid = "CV_SpawnSbjTid";

        /// <summary>
        /// 技能搜索目标范围。
        /// </summary>
        public const string CV_SearchRange = "CV_SearchRange";

        #endregion

        #region 目标与瞄准

        /// <summary>
        /// 当前目标实体 ID。
        /// </summary>
        public const string CV_TargetEid = "CV_TargetEid";

        /// <summary>
        /// 当前目标位置。
        /// </summary>
        public const string CV_TargetPos = "CV_TargetPos";

        /// <summary>
        /// 当前目标半径
        /// </summary>
        public const string CV_TargetRadius = "CV_TargetRadius";

        /// <summary>
        /// 当前目标随机半径
        /// </summary>
        public const string CV_TargetRandomRange = "CV_TargetRandomRange";

        /// <summary>
        /// 命中白名单实体集合。
        /// </summary>
        public const string CV_HitWhiteList = "CV_HitWhiteList";

        /// <summary>
        /// 延迟攻击时间，通常来自动画事件时间或技能前摇。
        /// </summary>
        public const string CV_DelayAttackTime = "CV_DelayAttackTime";

        /// <summary>
        /// 当前连续攻击次数。
        /// </summary>
        public const string CV_CurAttackTimes = "CV_CurAttackTimes";

        /// <summary>当前普通攻击从本次起手到下次起手的完整周期。</summary>
        public const string CV_AttackInterval = "CV_AttackInterval";

        /// <summary>当前普通攻击实际使用的前摇时间。</summary>
        public const string CV_AttackBeforeDelay = "CV_AttackBeforeDelay";

        /// <summary>当前普通攻击 Attack 动画的播放倍率。</summary>
        public const string CV_AttackAnimationSpeed = "CV_AttackAnimationSpeed";


        public const string CV_AmmoGambleCritAdd = "CV_AmmoGambleCritAdd";
        public const string CV_AmmoGambleCritDamageAdd = "CV_AmmoGambleCritDamageAdd";
        public const string CV_AmmoGambleDamageMultiplier = "CV_AmmoGambleDamageMultiplier";
        public const string CV_AmmoGambleForceCritical = "CV_AmmoGambleForceCritical";


        #endregion

        #region 子物体

        /// <summary>
        /// 子物体移动速度。
        /// </summary>
        public const string CV_SbjMoveSpeed = "CV_SbjMoveSpeed";

        /// <summary>
        /// 子物体生成时的目标位置。
        /// </summary>
        public const string CV_SbjTargetPos = "CV_SbjTargetPos";

        /// <summary>
        /// 子物体碰撞特效资源 ID 或路径变量。
        /// </summary>
        public const string CV_SbjHitFxRes = "CV_SbjHitFxRes";

        /// <summary>
        /// 子物体命中特效覆盖资源。
        /// </summary>
        public const string CV_SbjHitFxResOverride = "CV_SbjHitFxResOverride";

        /// <summary>
        /// 子物体直线运动模式方向。
        /// </summary>
        public const string CV_ModeStraightDir = "CV_ModeStraightDir";

        /// <summary>
        /// 子物体剩余障碍反弹次数。
        /// </summary>
        public const string CV_SubobjectReflectLeft = "CV_SubobjectReflectLeft";

        /// <summary>
        /// 子物体每次障碍反弹后的伤害倍率。
        /// </summary>
        public const string CV_SubobjectReflectDamageRate = "CV_SubobjectReflectDamageRate";

        /// <summary>
        /// 子物体销毁位置。
        /// </summary>
        public const string CV_SubObjDestroyPos = "CV_SubObjDestroyPos";

        #endregion

        #region 命中派生

        /// <summary>
        /// 子物体命中后派生生成的子物体列表。
        /// </summary>
        public const string CV_SpawnSobjListOnHit = "CV_SpawnSobjListOnHit";

        /// <summary>
        /// 子物体命中后给被命中者附加的 Buff 列表。
        /// </summary>
        public const string CV_BuffListOnHit = "CV_BuffListOnHit";

        #endregion

        #region Buff

        /// <summary>
        /// Buff 持续时间。
        /// </summary>
        public const string CV_BuffDuration = "CV_BuffDuration";

        /// <summary>
        /// 当前正在处理的 Buff 实例。
        /// </summary>
        public const string CV_CurBuff = "CV_CurBuff";

        /// <summary>
        /// Buff 最大等级。
        /// </summary>
        public const string CV_BuffMaxLevel = "CV_BuffMaxLevel";

        /// <summary>
        /// 把子物体命中时的HitInfo传给buff
        /// </summary>
        public const string CV_SubobjHitInfo = "CV_SubobjHitInfo";

        #endregion
    }
}

namespace LccHotfix
{
    /// <summary>
    /// CustomLogic 黑板变量 Key 集合，用于在 GameMode、FSM、Skill、Subobject、Buff 逻辑之间传递上下文。
    /// </summary>
    public partial class CvKey
    {
        #region 通用上下文

        /// <summary>
        /// 战斗单位 TFighter 静态配置；实体死亡后仍需读取配置时从黑板获取。
        /// </summary>
        public const string CV_FigherCfg = "CV_FigherCfg";

        #endregion

        #region 战斗单位与属性

        /// <summary>
        /// 战斗单位配置 ID。
        /// </summary>
        public const string CV_BattleUnitTid = "CV_BattleUnitTid";

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
        /// 技能生成子物体数量。
        /// </summary>
        public const string CV_SpawnSbjCount = "CV_SpawnSbjCount";

        /// <summary>
        /// 并排齐射额外子物体数量。
        /// </summary>
        public const string CV_AbreastSbjCount = "CV_AbreastSbjCount";

        /// <summary>
        /// 散射子物体数量。
        /// </summary>
        public const string CV_ScatterSbjCount = "CV_ScatterSbjCount";

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
        /// 当前目标方向。
        /// </summary>
        public const string CV_TargetDir = "CV_TargetDir";

        /// <summary>
        /// 固定移动方向。
        /// </summary>
        public const string CV_FixedMoveDir = "CV_FixedMoveDir";

        /// <summary>
        /// 命中白名单实体集合。
        /// </summary>
        public const string CV_HitWhiteList = "CV_HitWhiteList";

        /// <summary>
        /// 手动瞄准或锁定释放时的技能方向。
        /// </summary>
        public const string CV_LockedSkillDir = "CV_LockedSkillDir";

        /// <summary>
        /// 延迟攻击时间，通常来自动画事件时间或技能前摇。
        /// </summary>
        public const string CV_DelayAttackTime = "CV_DelayAttackTime";

        /// <summary>
        /// 当前连续攻击次数。
        /// </summary>
        public const string CV_CurAttackTimes = "CV_CurAttackTimes";

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
        /// 子物体生成时的初始位置。
        /// </summary>
        public const string CV_SbjInitPos = "CV_SbjInitPos";

        /// <summary>
        /// 子物体碰撞特效资源 ID 或路径变量。
        /// </summary>
        public const string CV_SbjHitFxRes = "CV_SbjHitFxRes";

        /// <summary>
        /// 子物体命中特效覆盖资源。
        /// </summary>
        public const string CV_SbjHitFxResOverride = "CV_SbjHitFxResOverride";

        /// <summary>
        /// 子物体边界反弹次数。
        /// </summary>
        public const string CV_SbjReboundCount = "CV_SbjReboundCount";

        /// <summary>
        /// 子物体边界反弹音效。
        /// </summary>
        public const string CV_SbjReboundAudio = "CV_SbjReboundAudio";

        /// <summary>
        /// 子物体连锁弹射次数。
        /// </summary>
        public const string CV_ChainBounceCount = "CV_ChainBounceCountAdd";

        /// <summary>
        /// 子物体持续时间。
        /// </summary>
        public const string CV_SbjDuration = "CV_SbjDuration";

        /// <summary>
        /// 跟随目标实体 ID，用于无人机携带旋转翼等跟随型子物体。
        /// </summary>
        public const string CV_FollowEntityId = "CV_FollowEntityId";

        /// <summary>
        /// 子物体直线运动模式方向。
        /// </summary>
        public const string CV_ModeStraightDir = "CV_ModeStraightDir";

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

        #region 爆炸与分裂

        /// <summary>
        /// 爆炸范围。
        /// </summary>
        public const string CV_BombRange = "CV_BombRange";

        /// <summary>
        /// 爆炸次数或爆炸生成数量。
        /// </summary>
        public const string CV_BombCount = "CV_BombCount";

        /// <summary>
        /// 子弹分裂数量。
        /// </summary>
        public const string CV_BulletSplitCount = "CV_BulletSplitCount";

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
        /// 驱散 Buff 列表。
        /// </summary>
        public const string CV_DisperseBuffList = "CV_DisperseBuffList";

        #endregion
    }
}

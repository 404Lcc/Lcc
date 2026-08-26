using System;

namespace LccHotfix
{
    //全特性标记
    [Serializable]
    public partial class PlayerFeaturesContext
    {
        // 全局增伤
        public float PointHighHpMonsterDmgRateAdd; // 对血量高于70%的怪物增伤X%
        public float PointDebuffMonsterDmg; // 对负面状态怪物伤害增加
        public float PointDebuffSourceDamageReduction; // 攻击者带负面状态时受到伤害降低（万分比）
        public float KillRewardDamageAmplifyRate; // 击杀鼓舞：击杀者临时伤害加成
        public float KillRewardMoveSpeedRate; // 击杀疾行：全队临时移速加成
        public float KillRewardCritRate; // 击杀锐目：击杀者临时暴击率加成
        public bool KillRewardProcessStarted; // 击杀奖励监听进程是否已启动
        public float PointCloseMonsterDmg; // 点数修正近距离怪物伤害
        public float PointBeginDmg; // 点数修正开局伤害
        public float PointCriticalExtraDmg; // 点数修正暴击额外万分比伤害
        public float PointDmgExtraDmg; // 点数修正伤害时额外万分比伤害
        public float PointNewSkillDmg; // 点数修正新增技能伤害
        public float PointRandomDmgLimit; // 点数修正随机伤害上限
        public float PointBeginTimeDmg; // 点数修正额外攻击次数
        public float PointKillEliteTreatRatio; // 击杀精英/Boss治疗小队：万分比（100=1%）
        public float PointMedkitDropRatio; // 击杀怪物额外掉药品：万分比（100=1%）
        public bool PassiveMedkitDamageEnabled; // 被动 4300004：使用医疗箱后全队伤害+10%
        public bool PassiveMedkitMoveSpeedEnabled; // 被动 4300005：使用医疗箱后全队移速+10%
        public float PointCoinDropRatio; // 击杀怪物额外掉金币：万分比（100=1%）；局内消费暂未接
        public float PointChooseSupplyTreat; // 选择三选一治疗小队：万分比（100=1%）

        // 三选一系统
        public int PointRefreshSupplyTimes; // 额外广告刷新三选一次数（本局底数 1 之外）

        // 全局技能CD减少
        public float PointSkillCD; // 点数修正技能冷却时间减少

        // 通用三选一
        public float GlobalSkillSearchRangeAddPercent; // 全局寻敌范围增加百分比
        public float GlobalMoveSpeedAddPercent; // 全局移动速度增加百分比
        public float GlobalAmmoReloadSpeedAddPercent; // 全局换弹速度增加百分比
        public float GlobalAmmoMaxAmmoAddPercent; // 全局弹夹容量增加百分比
        public float GlobalAmmoLastShotInGameAmplify; // 全局弹夹最后一发伤害提高，万分比
        public float ReloadDamageReductionAmmoRate; // 弹药英雄换弹期间承伤减免
        public float ReloadDamageReductionMechanismRate; // 机关英雄换弹期间承伤减免
        public float ReloadDamageReductionEnergyRate; // 能量英雄换弹期间承伤减免
        public float EnergyHeroMaxAmmoAddPercent; // 能量专属：能量上限增加百分比
        public float EnergyHeroRecoverySpeedAddPercent; // 能量专属：能量回复速度增加百分比
        public float EnergyHeroFullAmmoDamageAmplify; // 能量专属：满能状态伤害增加，万分比
        public bool BloodSustainProcessStarted; // 嗜血续航监听进程是否已启动
        public float BloodSustainAmmoRestoreRate; // 嗜血弹药：受伤后恢复最大弹夹比例
        public float BloodSustainMechanismReloadSpeedAddPercent; // 嗜血机关：受伤后短时换弹速度加成
        public float BloodSustainEnergyAttackSpeedAddPercent; // 嗜血能量：受伤后短时攻击速度加成

        // 击杀奖励：普通/精良/优秀独立概率判定、独立叠层与封顶。
        public bool ProbabilisticKillRewardProcessStarted;
        public bool ProbabilisticKillRewardNormalHealEnabled;
        public bool ProbabilisticKillRewardMediumHealEnabled;
        public bool ProbabilisticKillRewardExcellentHealEnabled;
        public bool ProbabilisticKillRewardNormalMoveSpeedEnabled;
        public bool ProbabilisticKillRewardExcellentMoveSpeedEnabled;
        public bool ProbabilisticKillRewardNormalAttackSpeedEnabled;
        public bool ProbabilisticKillRewardExcellentAttackSpeedEnabled;
        public bool ProbabilisticKillRewardNormalDamageEnabled;
        public bool ProbabilisticKillRewardExcellentDamageEnabled;

        // 弹药博弈：同名优秀效果覆盖普通效果。
        public bool AmmoGambleFirstShotNormalEnabled;
        public bool AmmoGambleFirstShotExcellentEnabled;
        public bool AmmoGambleLastShotNormalEnabled;
        public bool AmmoGambleLastShotExcellentEnabled;
        public bool AmmoGambleDoubleAmmoNormalEnabled;
        public bool AmmoGambleDoubleAmmoExcellentEnabled;
        public bool AmmoGambleLastShotNoCostNormalEnabled;
        public bool AmmoGambleLastShotNoCostExcellentEnabled;

        // 不屈：普通与优秀可同时获得，实时按当前生命档位叠加。
        public bool GritDamageNormalEnabled;
        public bool GritDamageExcellentEnabled;
        public bool GritAttackSpeedNormalEnabled;
        public bool GritAttackSpeedExcellentEnabled;
        public bool GritReloadSpeedNormalEnabled;
        public bool GritReloadSpeedExcellentEnabled;

        // 战术挂件
        // 类别挂件：Count 为「获得次数」；每次获得按槽位顺序分给同类下一个英雄（可叠加类优先升级已持有者）
        public int TacticalCharmA1LaserSightCount; // A1 激光指示器：弹药类，逐个英雄
        public int TacticalCharmHighScope2XCount; // A4 高倍镜 2 倍：弹药类，逐个英雄
        public int TacticalCharmHighScope4XCount; // A4 高倍镜 4 倍：弹药类，逐个英雄
        public int TacticalCharmHighScope8XCount; // A4 高倍镜 8 倍：弹药类，逐个英雄
        public int TacticalCharmFlashlightCount; // A3 战术手电：弹药类，逐个英雄
        public int TacticalCharmFastChargeRingCount; // E5 快速充能环：能量类，逐个英雄
        public float TacticalCharmEnergyReloadSpeedAddPercent; // E5 换弹速度加成（分给持有者时写入实体）
        public int TacticalCharmCapacitorModuleCount; // E1 电容模块：能量类，逐个英雄
        public float TacticalCharmEnergyAutoRecoverySpeedAddPercent; // E1 自动回复速度加成（分给持有者时写入实体）
        public bool TacticalCharmHelmetEnabled; // G1 头盔：全队英雄挂载头盔并获得受击减伤
        public int TacticalCharmMineLauncherCount; // M3 地雷发射器：机关类，总层数逐个英雄分配，单英雄最大 5
        public int TacticalCharmAttackDroneCount; // G3 攻击无人机：跟随队长，最大 3
        public int TacticalCharmPortableTurretCount; // M5 便携炮台：机关类，逐个英雄
        public bool TacticalCharmClownMaskEnabled; // B-P1 小丑面具：全队英雄各自 CD 免疫一次魅惑
        public bool TacticalCharmAntiTrapBootsEnabled; // B-T1 防滑战靴：全队英雄各自 CD 免疫一次捕兽夹
        public bool TacticalCharmBullfighterCapeEnabled; // B-F1 斗牛披风：全队获得，Buff 实例独立 CD 免疫一次牛/猪冲撞

        // 负面状态持续时间增加
        public float PointDebuffDuration; // 点数修正负面状态持续时间增加

        // 士兵76：新版 1-5 星普攻成长
        public int Soldier76ParallelBulletCount; // 每次射击总弹数（0=仅主弹1发；平行射击=3，平行射击Ⅱ=5；不额外耗弹）
        public float Soldier76ParallelBulletOffset; // 保留字段（现改为角度扇射，不再使用位移）
        public float Soldier76BulletScale; // 子弹视觉/碰撞缩放（0或1=不放大；平行射击Ⅱ=1.5）
        public int Soldier76KillSplitBulletCount; // 击杀后生成的分裂子弹数
        public float Soldier76BounceChance; // 命中后折返概率
        public int Soldier76BounceCount; // 单发子弹最大折返次数（命中反弹=1，命中反弹Ⅱ=2）
        public float Soldier76HitSplitChance; // 命中分裂：命中时分裂概率
        public int Soldier76HitSplitBulletCount; // 命中分裂：分裂子弹数
        public int Soldier76ReloadRadialBulletCount; // 换弹完成后发射的环形子弹数
        public float Soldier76BulletDamageAmplify; // 换弹环射：子弹伤害加成，万分比（1500=+15%）
        public bool Soldier76EmptyMagazineEnabled; // 空仓砸匣（旧）：最后一发打空时自动抛出空弹匣
        public float Soldier76EmptyMagazineRange; // 空仓砸匣：爆炸半径
        public float Soldier76EmptyMagazineDamageRate; // 空仓砸匣：爆炸伤害系数
        public float Soldier76EmptyMagazineHitBackDistance; // 空仓砸匣：击退距离
        public float Soldier76BlastBulletChance; // 爆破子弹：命中小概率爆炸
        public float Soldier76BlastBulletDamageRate; // 爆破子弹：爆炸伤害系数
        public float Soldier76BlastBulletCooldown; // 爆破子弹：公共 CD
        public bool Soldier76InfiniteFireEnabled; // 无限火力：不耗弹/不换弹
        public float Soldier76InfiniteFireAttackSpeedAdd; // 无限火力：攻速加成（0.25=+25%）
        public float Soldier76DamageAmplifyPerKill; // 战争杀戮：每次击杀增加的伤害加成，万分比（5000=+50%）
        public float Soldier76KillDamageAmplifyCap; // 战争杀戮：击杀叠伤封顶，万分比（25000=+250%）
        public float Soldier76KillDamageAmplifyTotal; // 战争杀戮：本局累计击杀伤害加成，万分比
        public float Soldier76DamageAmplifyPerReload; // 换弹蓄势：每次换弹完成增加的伤害加成，万分比（5000=+50%）
        public float Soldier76ReloadDamageAmplifyCap; // 换弹蓄势：换弹叠伤封顶，万分比（25000=+250%）
        public float Soldier76ReloadDamageAmplifyTotal; // 换弹蓄势：本局累计换弹伤害加成，万分比
        public bool Soldier76StarProcessStarted; // 新版星级技能常驻进程是否已启动
        public float Soldier76ColdSnapSlowChance; // Combo 寒流压制：命中叠减速概率
        public float Soldier76MicroExplodeChance; // Combo 火力重狙：微型爆弹触发概率
        public float Soldier76MicroExplodeDamageRate; // Combo 火力重狙：微型爆弹伤害系数
        public float Soldier76MicroExplodeCooldown; // Combo 火力重狙：微型爆弹独立公共 CD
        public int Soldier76MicroExplodeSubobjectTid; // Combo 火力重狙：微型爆弹子物体 Tid
        public bool Soldier76PowerAuraCreated; // 士兵76脚底光环是否已生成（7101014/7101016/7101021）

        // 狙击手
        public int SniperBulletPierceAdd; // 狙击枪子弹额外穿透次数
        public float SniperBulletHitBackEffectAdd; // 狙击枪子弹击退效果增加
        public bool SniperBulletPierceDamageDecayEnabled; // 狙击枪子弹穿透后伤害衰减
        public bool SniperFirstShotInfinitePierceEnabled; // 弹夹第一发无限穿透
        public bool SniperLastShotInfinitePierceEnabled; // 弹夹最后一发无限穿透
        public bool SniperPhoenixBulletEnabled; // 凤凰子弹：无限穿透、伤害和碰撞范围增强
        public int SniperPhoenixBulletSubobjectTid; // 凤凰子弹使用的子物体 Tid
        public bool SniperExposeWeakPointOnHitEnabled; // 弱点暴露：狙击枪子弹命中后施加必暴窗口
        public bool SniperPierceNoDecayEnabled; // 倾泻穿射：狙击枪子弹穿透无衰减
        public bool SniperBarrageEnabled; // 倾泻穿射：每次射击概率打空剩余弹匣
        public float SniperBarrageChance; // 倾泻触发概率
        public float SniperBarrageInterval; // 倾泻连射间隔
        public bool SniperBarrageProcessStarted; // 倾泻穿射常驻进程是否已启动
        public bool SniperCritBoostFirstShotEnabled; // 暴击强化：弹匣第一发暴击率+40%
        public bool SniperCritBoostLastShotEnabled; // 暴击强化Ⅱ：弹匣最后一发暴击率+40%
        public bool SniperCritBoostAllShotEnabled; // 暴击强化Ⅲ：弹匣所有子弹暴击率+40%（覆盖前两档，不叠加）
        public bool SniperApplyWeakOnHitEnabled; // Combo 虚弱狙镰：子弹命中施加虚弱
        public bool SniperJumpOnFirstHitEnabled; // Combo 跳链狙击：首次命中生成跳弹

        // 死神
        public bool DeathScytheApplyWeak; // 死神挥镰命中施加虚弱
        public int DeathScytheSoulSubobjectTid; // 死神挥镰后生成灵魂子物体 Tid
        public float DeathScytheWeakTargetDamageAmplify; // 死神对虚弱目标额外增伤
        public int DeathScytheShockwaveSubobjectTid; // 死神挥镰向前冲击波子物体 Tid
        public bool DeathScytheFearOnHitEnabled; // 死神恐惧镰刀：挥镰命中概率施加恐惧
        public float DeathScytheFearChance; // 死神恐惧镰刀：命中施加恐惧概率
        public float DeathScytheFearDuration; // 死神恐惧镰刀：恐惧持续时间
        public float DeathScytheFearTargetCooldown; // 死神恐惧镰刀：同目标触发 ICD
        public bool DeathScytheSoulExplodeOnExpireEnabled; // 死神魂尽爆散 / 灵魂挥镰Ⅱ：灵魂到期时原地爆散
        public float DeathScytheSoulExplodeRange; // 死神魂尽爆散：爆散范围
        public float DeathScytheSoulExplodeDamageRate; // 死神魂尽爆散：爆散伤害系数
        public string DeathScytheSoulExplodeFxPath; // 死神魂尽爆散：爆散特效资源名
        public bool DeathScytheKillHealEnabled; // 汲取镰刀：死神击杀小概率回血（仅死神）
        public bool DeathScytheKillHealProcessStarted; // 汲取镰刀进程是否已启动
        public float DeathScytheKillHealChance; // 汲取镰刀触发概率
        public float DeathScytheKillHealPercent; // 汲取镰刀回复最大生命百分比
        public bool DeathScytheHitHealEnabled; // 灵魂挥镰 3 级：镰刀攻击吸血
        public bool DeathScytheHitHealProcessStarted; // 攻击吸血监听是否已启动
        public float DeathScytheHitHealPercent; // 攻击吸血：回复伤害百分比（0.05 = 5%）
        public bool DeathScytheDoubleSlashEnabled; // Combo 毒镰收割：双段挥镰
        public float DeathScytheSlashRangeScale; // Combo 虚弱狙镰 / 灵魂挥镰 2 级：镰刀扇形半径倍率（1.5 = +50%）

        // 小美：寒控 1-2 星
        public float MeiFreezeSlowAmplify; // 深度减速：冰冻减速效果额外增强
        public float MeiFreezeDurationAddSeconds; // 深度减速：冰冻持续时间固定增加秒数
        public int MeiBackSplitBulletCount; // 深度减速临时增强：命中后向分裂子弹数
        public float MeiBackSplitAngleInterval; // 深度减速临时增强：每发分裂子弹角度间隔
        public float MeiBackSplitTargetDistance; // 深度减速临时增强：分裂子弹目标距离
        public float MeiFreezeDurationAddRate; // 永冻延长：冰冻持续时间百分比增加
        public float MeiFrozenTargetDamageAmplify; // 永冻延长：冰锥对满层冰冻目标伤害提高，万分比
        public bool MeiFrozenDotEnabled; // 冰蚀之痛：冰冻目标受到周期伤害
        public float MeiFrozenDotDamageRate; // 冰蚀之痛：每跳伤害 = 攻击力 * 系数
        public float MeiFrozenDotInterval; // 冰蚀之痛：伤害间隔
        public bool MeiFrostControlProcessStarted; // 小美寒控常驻供给进程是否已启动
        public bool MeiFrostExplosionEnabled; // 亡者霜爆：冰冻敌人死亡触发标准霜爆
        public int MeiFrostExplosionSubobjectTid; // 亡者霜爆子物体 Tid，未配表时使用逻辑兜底
        public float MeiFrostExplosionCooldown; // 亡者霜爆死亡事件冷却
        public bool MeiFrostSpreadEnabled; // 霜域扩散：强化标准霜爆，并允许缩小版霜爆
        public int MeiFrostSmallExplosionSubobjectTid; // 缩小版霜爆子物体 Tid，未配表时使用逻辑兜底
        public float MeiFrostSpreadUnfrozenChance; // 霜域扩散：非冰冻死亡触发缩小版霜爆概率
        public bool MeiFrostPatrolEnabled; // 寒域巡游：移动距离触发寒域冰爆
        public int MeiFrostPatrolSubobjectTid; // 寒域冰爆子物体 Tid，未配表时使用逻辑兜底
        public float MeiFrostPatrolDistance; // 寒域巡游触发所需累计移动距离
        public float MeiFrostPatrolCooldown; // 寒域巡游冷却

        // 小美：新 1-5 星普攻成长
        public bool MeiScatterEnabled; // 1 星：额外发射左右两枚冰锥
        public float MeiBulletScale; // 冰锥视觉/碰撞缩放（0或1=不放大；冰刃扩散Ⅰ=1.5）
        public int MeiScatterBulletAdd; // Combo 冰雷合奏：扇形冰锥总弹数额外 +N（3→4）
        public float MeiSplitChance; // 2 星：主冰锥命中后分裂概率
        public int MeiSplitBulletCount; // 2 星：主冰锥命中后分裂数量
        public float MeiFullFreezeChance; // 3 星：冰锥命中后直接满层冰冻概率
        public int MeiPierceAdd; // 4 星：冰锥额外穿透次数
        public bool MeiFrozenHitBackEnabled; // 4 星：命中已冰冻目标时触发击退
        public float MeiFrozenHitBackDistance; // 4 星：命中已冰冻目标的基础击退距离
        public float MeiFrozenHitBackCooldown; // 4 星：同目标击退 ICD
        public float MeiFrozenHitBackEliteScale; // 4 星：精英/Boss 击退距离倍率
        public bool MeiIceTrailEnabled; // 5 星：移动时在身后留下冰轨
        public int MeiIceTrailSubobjectTid; // 5 星：冰轨点子物体 Tid
        public float MeiIceTrailLifetime; // 5 星：冰轨点持续时间
        public float MeiIceTrailRadius; // 5 星：冰轨点判定半径
        public float MeiIceTrailSpawnStep; // 5 星：冰轨点生成间距
        public float MeiIceTrailSlowRatio; // 5 星：冰轨内移速保留比例
        public float MeiIceTrailSideShotAngle; // 5 星：站在冰轨上时额外侧锥角度
        public float MeiIceTrailSideShotDamageRate; // 5 星：站在冰轨上时额外侧锥伤害系数
        public float MeiSecondSplitChance; // 5 星：第一代分裂冰锥再次分裂概率
        public int MeiSecondSplitBulletCount; // 5 星：第一代分裂冰锥再次分裂数量

        // 小美：冰霜护盾
        public bool MeiFrostShieldEnabled; // 是否开启冰霜护盾被动
        public bool MeiFrostShieldProcessStarted; // 冰霜护盾供给进程是否已启动
        public float MeiFrostShieldDurationAddSeconds; // 护盾持续时间增量
        public float MeiFrostShieldDamageBonusRate; // 碰触伤害比例加成
        public int MeiFrostShieldTargetCountBonus; // 每次触发额外友方数
        public float MeiFrostShieldHitCooldown; // 同一敌人重复碰触冷却
        public int MeiFrostShieldSubobjectTid; // 冰盾子物体 Tid，未配表时使用逻辑兜底
        public bool MeiFrostShieldGuardEnabled; // 坚冰守护：有冰盾时减伤，整面盾消失时回血
        public float MeiFrostShieldGuardDamageReductionRate; // 坚冰守护减伤比例（0.20 = 20%）
        public float MeiFrostShieldGuardExpireHealPercent; // 坚冰守护：该英雄最后一面冰盾消失时回复最大生命比例
        public bool MeiFrostShieldBurstOnExpireEnabled; // 爆盾伤敌：整面冰盾消失时在宿主位置爆炸
        public float MeiFrostShieldBurstRadius; // 爆盾爆炸范围
        public float MeiFrostShieldBurstDamageRate; // 爆盾爆炸伤害系数
        public string MeiFrostShieldBurstFxPath; // 爆盾爆炸特效

        // 小美：冰爆新星
        public bool MeiFrostBombEnabled; // 按时间间隔在队长圆周三等分点生成印记并引爆
        public bool MeiFrostBombProcessStarted; // 冰霜炸弹供给进程是否已启动
        public float MeiFrostBombInterval; // 触发间隔（秒）
        public float MeiFrostBombFuseTime; // 引爆延迟
        public float MeiFrostBombChainDelay; // 连环爆炸间隔
        public bool MeiFrostBombChainMiddleEnabled; // 启用中层爆炸7101046
        public bool MeiFrostBombChainOuterEnabled; // 启用外层爆炸7101049
        public float MeiFrostBombRadiusBonusRate; // 爆炸半径比例加成累加
        public float MeiFrostBombDamageBonusRate; // 爆炸伤害比例加成累加

        // 小美：扇形冰爆
        public bool MeiFanFrostBombEnabled; // 小美朝向扇形区域周期性生成多批冰爆
        public bool MeiFanFrostBombProcessStarted; // 扇形冰爆供给进程是否已启动
        public float MeiFanFrostBombInterval; // 触发间隔（秒）

        // 小美：冰霜爆炸Ⅳ（原小队冰爆，升级替代脚下冰爆）
        public bool MeiSquadFrostBombEnabled; // 冰霜爆炸Ⅳ：同进程必冻等终局强化
        public bool MeiSquadFrostBombProcessStarted; // 兼容旧字段（Ⅳ已不再另起进程）
        public float MeiSquadFrostBombInterval; // 兼容旧字段
        public float MeiSquadFrostBombRadius; // 相对队长的圆周半径（Ⅰ起共用）

        // 小美：自动寻敌
        public bool MeiChainHomingEnabled;
        public float MeiChainHomingMinTurnRadius; // 最小转弯半径（米）
        public float MeiChainHomingSeekRange; // 寻下一目标范围
        public float MeiChainHomingLifeTime; // 子弹寿命（秒）

        // 鹰眼：1星箭形
        public bool ArcherSplitArrowEnabled; // 分裂箭
        public int ArcherSplitArrowCount; // 分裂额外箭数量
        public float ArcherSplitArrowDamageRate; // 分裂箭伤害系数
        public float ArcherSplitArrowFanHalfAngle; // 分裂扇形半角（度）；<=0 时技能侧用默认
        public bool ArcherBurstArrowEnabled; // 连射：一次攻击多箭一前一后
        public int ArcherBurstArrowCount; // 连射+N（额外后箭数；+1=共2箭，+2=共3箭）
        public float ArcherBurstArrowDelay; // 前后箭间隔
        public float ArcherBurstArrowDamageRate; // 第 2 箭伤害系数
        public float ArcherBurstArrowThirdDamageRate; // 第 3 箭伤害系数
        public bool ArcherParallelArrowEnabled; // 平行箭
        public int ArcherParallelArrowSideCount; // 每侧平行箭数量
        public float ArcherParallelArrowInnerOffset; // 内侧平行偏移
        public float ArcherParallelArrowOuterOffset; // 外侧平行偏移
        public float ArcherParallelArrowDamageRate; // 内侧平行箭伤害系数
        public float ArcherParallelArrowOuterDamageRate; // 外侧平行箭伤害系数
        public int ArcherComboRightParallelAdd; // Combo 弹雨齐射：额外右侧平行箭数量
        public float ArcherComboRightParallelOffset; // Combo 右侧平行箭偏移
        public float ArcherComboRightParallelDamageRate; // Combo 右侧平行箭伤害系数
        // 鹰眼：2星弹道
        public bool ArcherHomingEnabled; // 跟踪箭
        public bool ArcherHomingSelected; // 是否选过跟踪箭基础版
        public float ArcherHomingRange; // 跟踪范围
        public float ArcherHomingTurnSpeed; // 旧：度/秒（已改用最小转弯半径）
        public float ArcherHomingMinTurnRadius; // 跟踪最小转弯半径（米），对齐小美冰锥
        public float ArcherHomingDuration; // 跟踪时长（秒）；<=0 表示全程跟踪（对齐小美）
        public bool ArcherPierceEnabled; // 穿透箭
        public bool ArcherPierceSelected; // 是否选过穿透箭基础版
        public int ArcherPierceCount; // 额外穿透次数
        public float ArcherPierceDecay; // 每次穿透伤害衰减
        public float ArcherPierceMinDamageRate; // 穿透最低伤害系数
        public bool ArcherBounceEnabled; // 反弹箭
        public bool ArcherBounceSelected; // 是否选过反弹箭基础版
        public int ArcherBounceCount; // 怪物间反弹次数
        public float ArcherBounceRange; // 反弹检索半径
        public float ArcherBounceDamageRate; // 反弹箭伤害系数

        // 鹰眼：4星命中特效
        public bool ArcherExplodeArrowEnabled; // 爆箭
        public float ArcherExplodeArrowChance; // 爆炸触发概率
        public float ArcherExplodeArrowRange; // 爆炸范围
        public float ArcherExplodeArrowDamageRate; // 爆炸伤害系数
        public float ArcherExplodeArrowCooldown; // 全场公共事件冷却
        public float ArcherExplodeArrowFuseDuration; // 爆箭引信延时（秒）
        public bool ArcherVolleyAmplifyEnabled; // 齐射增幅：按本轮直射箭数量放大伤害
        public float ArcherVolleyAmplifyPerArrowRate; // 齐射增幅：每枚直射箭的伤害加成
        public bool ArcherArrowRainEnabled; // 万箭天降：每第 N 次普攻在队长周围降下箭雨
        public bool ArcherArrowRainProcessStarted; // 箭雨供给进程是否已启动
        public int ArcherArrowRainAttackInterval; // 箭雨触发所需普攻次数
        public float ArcherArrowRainDuration; // 箭雨持续时间
        public float ArcherArrowRainTickInterval; // 箭雨落点伤害间隔
        public float ArcherArrowRainDamageRate; // 单次箭雨范围伤害系数
        public float ArcherArrowRainRange; // 单次箭雨伤害半径
        public float ArcherArrowRainOrbitRadius; // 队长圆周落点半径（参考小美冰爆）
        public int ArcherArrowRainOrbitCount; // 队长圆周落点数量
        public float ArcherArrowRainStaggerInterval; // 多落点错开间隔（秒）
        public bool ArcherHuntSplitEnabled; // Combo 火力重狙：击杀分裂箭
        public bool ArcherHuntSplitProcessStarted; // 猎杀分裂监听进程是否已启动
        public int ArcherHuntSplitCount; // 击杀分裂箭数量
        public float ArcherHuntSplitDamageRate; // 击杀分裂箭伤害系数
        public float ArcherHuntSplitCooldown; // 击杀分裂事件 CD

        // 忍者：手里剑 1-2 星
        public bool GenjiSplitShurikenEnabled; // 分裂手里剑
        public int GenjiSplitShurikenCount; // 单次攻击额外手里剑数量
        public float GenjiSplitShurikenAngle; // 分裂手里剑夹角
        public float GenjiShurikenScale; // 手里剑视觉/碰撞缩放（0或1=不放大；分裂手里剑Ⅰ=1.5）。仅普攻 Spawn 写入 env，不全局套 6100015；与巨型手里剑取较大值
        public float GenjiShurikenMoveSpeed; // 手里剑飞行速度
        public float GenjiShurikenMaxRange; // 手里剑最大飞行距离（0=用默认）
        public float GenjiShurikenDamageRate; // 手里剑伤害系数
        public bool GenjiPoisonEnabled; // 手里剑命中附加中毒
        public int GenjiPoisonBuffLogicId; // 中毒 Buff 逻辑 ID
        public float GenjiPoisonDuration; // 中毒持续时间
        public float GenjiPoisonInterval; // 中毒跳伤间隔
        public float GenjiPoisonDamageRate; // 中毒每跳伤害系数
        public string GenjiPoisonFxPath; // 中毒 Buff 特效
        public bool GenjiPoisonDamageUpEnabled; // 中毒伤害提升
        public bool GenjiPoisonMustCritEnabled; // Combo 毒镰收割：攻击中毒目标必暴
        public bool GenjiExplosiveShurikenEnabled; // 爆破手里剑
        public float GenjiExplosiveShurikenRange; // 爆炸范围
        public float GenjiExplosiveShurikenDamageRate; // 爆炸伤害系数
        public string GenjiExplosiveShurikenFxPath; // 爆炸特效
        public bool GenjiReturnBoomEnabled; // 折返爆鸣（单次普攻飞镖消失时在忍者身边触发一次）
        public float GenjiReturnBoomRange; // 折返爆鸣范围
        public float GenjiReturnBoomDamageRate; // 折返爆鸣伤害系数
        public float GenjiReturnBoomChance; // 折返爆鸣触发概率（按次普攻判定）
        public string GenjiReturnBoomFxPath; // 折返爆鸣特效
        public int GenjiReturnBoomAttackSerial; // 当前普攻批次号（发射时自增）
        public int GenjiReturnBoomConsumedAttackSerial; // 已触发刃波的普攻批次号
        public bool GenjiRingShurikenEnabled; // 环形手里剑 / 分裂手里剑Ⅲ
        public int GenjiRingShurikenCount; // 环形手里剑数量
        public float GenjiRingShurikenDamageRate; // 环形手里剑单枚伤害系数
        public bool GenjiBackSplitShurikenEnabled; // 分裂手里剑Ⅱ：后方额外飞镖
        public int GenjiBackSplitShurikenCount; // 后方额外飞镖数量
        public float GenjiBackSplitShurikenAngle; // 后方飞镖夹角
        public bool GenjiGiantShurikenEnabled; // 巨型手里剑：碰撞体积放大
        public float GenjiGiantShurikenScale; // 巨型手里剑缩放
        public bool GenjiOrbitShurikenEnabled; // 旋转手里剑
        public bool GenjiOrbitShurikenProcessStarted; // 旋转手里剑进程是否已启动
        public float GenjiOrbitShurikenInterval; // 旋转手里剑触发间隔
        public float GenjiOrbitShurikenDuration; // 单次环绕持续时间
        public int GenjiOrbitShurikenCount; // 环绕飞镖数量
        public float GenjiOrbitShurikenRadius; // 环绕半径
        public float GenjiOrbitShurikenHitRadius; // 单枚碰撞半径
        public float GenjiOrbitShurikenDamageRate; // 环绕飞镖伤害系数
        public float GenjiOrbitShurikenAngularSpeed; // 环绕角速度（度/秒）
        public float GenjiOrbitShurikenTickInterval; // 环绕伤害判定间隔

        // 磁暴步兵：旋转雷盾
        public bool TeslaShieldEnabled; // 是否开启旋转雷盾被动
        public bool TeslaShieldProcessStarted; // 雷盾供给进程是否已启动，防止重复派发时叠加多个进程
        public float TeslaShieldDurationAddSeconds; // 雷盾持续时间增量（2 星 +3s）
        public float TeslaShieldDamageBonusRate; // 雷盾碰触伤害比例加成累加（2 星 +0.20，4 星 +0.15）
        public int TeslaShieldTargetCountBonus; // 雷盾每次触发额外友方数（3/4 星各 +1，须已开系）
        public float TeslaShieldStunDuration; // 雷盾碰触微晕时长
        public float TeslaShieldHitCooldown; // 同一敌人重复碰触冷却
        public bool TeslaShieldTouchFreezeEnabled; // Combo 冰雷合奏：雷盾碰触必冻
        public float TeslaShieldTouchFreezeCooldown; // Combo 冰雷合奏：同目标必冻 ICD
        public bool TeslaShieldUltEnabled; // 是否开启全域雷幕终局
        public float TeslaShieldUltCooldown; // 全域雷幕独立冷却
        public float TeslaShieldUltDuration; // 全域雷幕持续
        public float TeslaShieldUltTempDamageBonusRate; // 雷幕窗内临时碰触伤加成

        // 磁暴步兵：磁链
        public bool TeslaLinkEnabled; // 是否开启磁链被动
        public bool TeslaLinkProcessStarted; // 磁链供给进程是否已启动，防止重复派发时叠加多个进程
        public float TeslaLinkDurationAddSeconds; // 磁链持续时间增量（2 星 +2s）
        public float TeslaLinkSpeedupRate; // 常规磁链加速累加，0.25 表示 +25%
        public float TeslaLinkDistanceBonusRate; // 磁链最大距离比例加成（4 星 +0.20）
        public int TeslaLinkTargetCountBonus; // 磁链并行目标数增量（3/4 星各 +1，须已开系）
        public bool TeslaLinkUltEnabled; // 是否开启磁网超载终局
        public float TeslaLinkUltCooldown; // 磁网超载独立冷却
        public float TeslaLinkUltDuration; // 磁网超载窗口持续
        public float TeslaLinkUltTempSpeedupBonus; // 超载窗内临时加速加成

        // 磁暴步兵：电磁弹跳
        public bool TeslaBounceEnabled; // 是否开启电磁弹跳
        public int TeslaBounceCountBonus; // 弹跳次数增量（2/3/4 星）
        public float TeslaBounceRange; // 每次弹跳的检索半径
        public float TeslaBounceBaseDamageRate; // 第一跳基础伤害系数
        public float TeslaBounceDamageAddPerJump; // 2 星后每次弹跳额外增加的伤害系数
        public float TeslaBounceDecayRate; // 4 星后每跳保留的伤害比例
        public float TeslaBounceMinDamageRate; // 4 星后伤害保底
        public float TeslaBounceStunDuration; // 弹跳命中的眩晕时长
        public bool TeslaBounceApplyVulnerable; // 是否给被弹跳目标施加易伤
        public float TeslaBounceVulnerableDuration; // 易伤持续时间
        public bool TeslaBounceFirstJumpForkEnabled; // 4 星跳链分叉：首次弹跳额外生成一条独立链路
        public bool TeslaBounceTerminalExplosionEnabled; // 5 星链末爆圈：每条链末端结算一次范围电伤
        public float TeslaTerminalJudgmentChance; // 终末审判：链末打雷圈触发概率
        public float TeslaMultiLightningChance; // 多重雷击：普攻起手额外链触发概率（0.5 / 1.0）
        public int TeslaMultiLightningExtraCount; // 多重雷击：额外链数量
        public float TeslaLightningStunChance; // 眩晕强化：闪电链伤害触发眩晕概率
        public float TeslaLightningStunDuration; // 眩晕强化：眩晕时长（秒）

        // 磁暴步兵：应急屏障
        public bool TeslaEmergencyBarrierEnabled; // 是否开启应急屏障
        public bool TeslaEmergencyBarrierProcessStarted; // 应急屏障供给进程是否已启动
        public float TeslaEmergencyBarrierHpThreshold; // 生命阈值，低于该比例触发
        public float TeslaEmergencyBarrierShieldRatio; // 护盾吸收量 = 最大生命 * 比例
        public float TeslaEmergencyBarrierDamageReduction; // 激活期间减伤比例
        public float TeslaEmergencyBarrierDuration; // 持续时间
        public float TeslaEmergencyBarrierCooldown; // 触发冷却
        public int TeslaEmergencyBarrierSubobjectTid; // 屏障子物体 Tid

        // 恢复续航：战场收割
        public bool RecoveryKillHealEnabled; // 击杀敌人回复生命
        public float RecoveryKillHealPercent; // 全队每人回复百分比（0.02 = 2%）
        public bool RecoveryProcessStarted; // 恢复续航供给进程是否已启动
        // 恢复续航：生命汲取
        public bool RecoveryLifeDrainEnabled; // 造成伤害转化为生命
        public float RecoveryLifeDrainPercent; // 转化百分比（0.03 = 3%）；单次回血上限为最大生命同比例
        // 恢复续航：自然愈合
        public bool RecoveryRegenEnabled; // 周期回复生命
        public float RecoveryRegenPercent; // 每周期回复最大生命百分比（0.03 = 3%）
        public float RecoveryRegenInterval; // 回复周期（秒），默认 5

        // 生存强化：普通/优秀档同语义覆盖，不叠加。
        public float SurvivalFlatDamageReductionRate; // 坚韧外皮：常驻承伤减免
        public float SurvivalLowHpDamageReductionRate; // 残血铁壁：生命低于阈值时额外承伤减免
        public float SurvivalHealAmplifyRate; // 愈合强化：最终治疗量加成

        // 风险博弈：绝境伤害
        public bool RiskDesperationEnabled; // HP低于阈值时伤害提高
        public float RiskDesperationHpThreshold; // 生命阈值（0.3 = 30%）
        public float RiskDesperationDamageAmplify; // 伤害倍率（2.0 = +100%）
        // 风险博弈：受伤反击
        public bool RiskCounterEnabled; // 受击时反击伤害
        public float RiskCounterReflectRate; // 反击比例（1.0 = 100%）
        // 风险博弈：濒死无敌
        public bool RiskNearDeathEnabled; // 濒死时免疫死亡并回血
        public float RiskNearDeathInvincibilityDuration; // 无敌持续时间（秒；Buff 5101026 时长需同步）
        public float RiskNearDeathHealPercent; // 濒死回血比例（0.5 = 50%）
        public float RiskNearDeathCooldown; // 触发冷却时间（秒）
        // 敌人特攻：对特定类型怪物增伤
        public bool RiskSpecialDmgEnabled; // 对特定类型怪物增伤
        public float RiskSpecialDmgAmplify; // 伤害倍率（1.3 = 130%）
        public int RiskSpecialDmgTargetStrength; // 目标EnermyStrength值（EestNormal=1, EestElite=2, EestBoss=4）

        // 通用技能：剧毒蛇杖（队长累计移动距离触发，与通用流星雨一致）
        public bool PoisonousSnakeWandEnabled; // 生成剧毒蛇杖
        public bool PoisonousSnakeWandProcessStarted; // 进程单例标记
        public float PoisonousSnakeWandMoveDistance; // 触发所需队长累计移动距离
        public float PoisonousSnakeWandModelScale; // 剧毒蛇杖模型尺寸
        public float PoisonousSnakeWandDamageRatio; // 剧毒蛇杖伤害比例
        public float PoisonousSnakeWandSearchRange; // 剧毒蛇杖索敌半径

        // 通用技能：寒冰月牙
        public bool IceCrescentEnabled; // 是否开启寒冰月牙
        public bool IceCrescentProcessStarted; // 寒冰月牙供给进程是否已启动
        public int IceCrescentLevel; // 当前等级 1~5，数量/穿透/生命周期由等级派生

        // 通用技能：寒冰冰爆
        public bool IceFrostBombEnabled; // 是否开启寒冰冰爆
        public bool IceFrostBombProcessStarted; // 寒冰冰爆供给进程是否已启动
        public int IceFrostBombLevel; // 当前等级 1~5，层数/冻结由等级派生
        public float IceFrostBombInterval; // 触发间隔（秒）

        // 通用技能：冰锥术
        public bool IceConeEnabled; // 是否开启冰锥术
        public bool IceConeProcessStarted; // 冰锥术供给进程是否已启动
        public int IceConeLevel; // 当前等级 1~5，控场/冰爆由等级派生
        public float IceConeSpawnInterval; // 背后生成间隔（秒）
        public float IceConeSeekInterval; // 待机自身圆形索敌间隔（秒）

        // 通用技能：火球术
        public bool FireballEnabled; // 是否开启火球术
        public bool FireballProcessStarted; // 火球术供给进程是否已启动
        public int FireballLevel; // 当前等级 1~5，暴击/火圈由等级派生
        public float FireballSpawnInterval; // 环绕生成间隔（秒）
        public float FireballSeekInterval; // 待机自身圆形索敌间隔（秒）

        // 通用技能：地刺
        public bool GroundSpikeEnabled; // 是否开启地刺
        public bool GroundSpikeProcessStarted; // 地刺供给进程是否已启动
        public int GroundSpikeLevel; // 当前等级 1~5，层数/吸血由等级派生
        public float GroundSpikeInterval; // 触发间隔（秒）

        // 通用技能：雷云
        public bool ThunderCloudEnabled; // 生成雷云
        public int ThunderCloudLightningFragmentCount; // 雷云闪电链分裂数量
        public int ThunderCloudLightningFragmentBounceCount; // 雷云闪电链弹射数量
        public bool ThunderCloudLightningFragmentStunEnabled; // 雷云弹射闪电链造成眩晕
        public bool ThunderCloudLastForever; // 雷云是否永久持续
		
	   // 通用流星雨：队长累计移动距离触发，周围随机敌人单体落星
        public bool MeteorShowerEnabled;
        public bool MeteorShowerProcessStarted;
        public float MeteorShowerMoveDistance; // 触发所需累计移动距离
        public float MeteorShowerSearchRange; // 索敌半径
        public int MeteorShowerTargetCountMin; // 每轮流星最少数量
        public int MeteorShowerTargetCount; // 每轮流星最多数量
        public float MeteorShowerDamageRate; // 单体伤害系数
        public float MeteorShowerFxDuration; // 流星特效寿命
        public float MeteorShowerStunDuration; // 主目标眩晕时长（V）
        public bool MeteorShowerSplashEnabled; // 主落点溅射
        public int MeteorShowerSplashCount; // 溅射目标数
        public float MeteorShowerSplashRange; // 溅射半径
        public float MeteorShowerSplashDamageRate; // 溅射伤害系数
        // 通用技能：恶灵召唤
        public bool EvilSpiritEnabled; // 生成恶灵
        public float EvilSpiritSpawnProbility; // 生成恶灵概率
        public float EvilSpiritDoubleProbility; // 生成两个恶灵概率
        public float EvilSpiritLifeTime; // 恶灵持续时间
        public float EvilSpiritDamageRate; // 恶灵伤害倍率

        // 通用技能：回旋镖（法球类）
        public bool BoomerangOrbEnabled;
        public bool BoomerangOrbProcessStarted;
        public int BoomerangOrbLevel; // 当前等级 1~5，数量/距离/缩放由等级派生
        public float BoomerangOrbTriggerChance; // 旧：每次普攻触发概率。现已改为换弹触发，保留字段避免存档兼容问题

        // 通用技能：反击雷盾（旋转类）
        public bool CounterThunderShieldEnabled;
        public bool CounterThunderShieldProcessStarted;
        public int CounterThunderShieldLevel; // 当前等级 1~5

        // 通用技能：锯齿盘（旋转类）
        public bool SawDiscEnabled;
        public bool SawDiscProcessStarted;
        public int SawDiscLevel; // 当前等级 1~5

        // 通用技能：球状闪电
        public bool BallLightningEnabled; // 是否开启球状闪电
        public bool BallLightningProcessStarted; // 球状闪电供给进程是否已启动
        public int BallLightningLevel; // 当前等级 1~5，数量/分裂由等级派生

        // 通用技能：腐朽领域
        public bool CorruptionAreaEnabled; // 是否开启腐朽领域
        public bool CorruptionAreaSlowDownEnabled; // 是否开启腐朽领域减速
        public float CorruptionAreaModelScale; // 腐朽领域模型比例

        // 通用技能：高爆手雷
        public bool HighExplosiveGrenadeEnabled; // 是否开启高爆手雷
        public bool HighExplosiveGrenadeProcessStarted; // 高爆手雷供给进程是否已启动
        public int HighExplosiveGrenadeLevel; // 当前等级 1~5，枚数/爆炸特效由等级派生

        // 新 combo（飞书 combo技能页）
        public int IceFireDragonCount; // 冰火双龙：旧字段，条数改由技能等级决定，不再用 combo 加条
        public int IceFireDragonPierceAdd; // 冰火双龙：额外穿透次数
        public float IceFireDragonDamageRate; // 冰火双龙：火龙/冰龙伤害系数，0=用技能默认
        public bool ThunderLightningBounceOnce; // 雷霆之怒：所有闪电额外链式跳 1 次
        public bool IceCrescentFreezeEnabled; // 急速冷却：寒冰月牙命中也叠冰冻
        public float GroundSpikeLifeStealRate; // 额外吸血：地刺吸血比例，0=用等级默认（Ⅰ 5% / Ⅴ 10%）
        public bool InstantReloadComboEnabled; // 无限弹药：换弹时长压到击发间隔，仍派换弹完成
    }

    // 策划可直接在此调整不屈的生命档位与普通/优秀档收益。
    public static class GritSupplyConfig
    {
        public const float HighHpThreshold = 0.70f;
        public const float MidHpThreshold = 0.40f;
        public const float LowHpThreshold = 0.20f;

        public const float NormalMidBonus = 0.20f;
        public const float NormalLowBonus = 0.60f;
        public const float NormalCriticalBonus = 1.50f;
        public const float ExcellentMidBonus = 0.30f;
        public const float ExcellentLowBonus = 0.90f;
        public const float ExcellentCriticalBonus = 2.25f;

        public static float GetBonus(float hpRatio, bool normalEnabled, bool excellentEnabled)
        {
            if ((!normalEnabled && !excellentEnabled) || hpRatio >= HighHpThreshold)
                return 0f;

            if (hpRatio >= MidHpThreshold)
                return (normalEnabled ? NormalMidBonus : 0f) + (excellentEnabled ? ExcellentMidBonus : 0f);
            if (hpRatio >= LowHpThreshold)
                return (normalEnabled ? NormalLowBonus : 0f) + (excellentEnabled ? ExcellentLowBonus : 0f);
            return (normalEnabled ? NormalCriticalBonus : 0f) + (excellentEnabled ? ExcellentCriticalBonus : 0f);
        }
    }
}


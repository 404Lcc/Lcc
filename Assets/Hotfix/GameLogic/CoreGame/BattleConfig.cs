using System.Collections.Generic;

namespace PBConfig
{
    public class TBase
    {
        public uint Id { get; set; }
    }

    public class TVector2
    {
        public float X { get; set; }
        public bool HasX { get; set; }
        public float Y { get; set; }
        public bool HasY { get; set; }
    }

    public class TVector
    {
        public float X { get; set; }
        public bool HasX { get; set; }
        public float Y { get; set; }
        public bool HasY { get; set; }
        public float Z { get; set; }
        public bool HasZ { get; set; }
    }

    public enum TCampType
    {
        EctAll = 0,
        EctEarthFederation = 1,
        EctFireEagleLegion = 2,
        EctEmploymentBase = 3,
    }

    public enum TElementType
    {
        EetAll = 0,
        EetAmmunition = 1,
        EetPhysics = 2,
        EetEnergy = 3,
        EetElectricity = 4,
        EetForce = 5,
        EetDark = 6,
    }

    public enum TEnemyStrengthType
    {
        EestAll = 0,
        EestNormal = 1,
        EestElite = 2,
        EestBoss = 4,
    }

    public enum TEnemyAttackType
    {
        EeatAll = 0,
        EeatMelee = 1,
        EeatRanged = 2,
    }

    public enum TEnemyRaceType
    {
        EertAll = 0,
        EertStellar = 1,
        EertVoid = 2,
        EertZerg = 4,
        EertMech = 8,
        EertPathogen = 16,
        EertAbyss = 32,
    }

    public enum TBattleUnitType
    {
        ButNone = 0,
        ButEnemy = 1,
        ButDefender = 2,
        ButWall = 3,
        ButSummon = 4,
        ButDefenseTower = 5,
        ButPet = 6,
    }

    public enum CollisionType
    {
        EhtNone = 0,
        EhtCollider2D = 1,
        EhtRaycast2D = 2,
        EhtBoxCollider = 3,
        EhtRaycast = 4,
        EhtAabb = 5,
    }

    public enum TSkillBuffBenefit
    {
        InvalidBuffBenefit = 0,
        Good = 1,
        Bad = 2,
        Neutral = 3,
    }

    public enum TSkillBuffType
    {
        InvalidBuffType = 0,
        AdBuff = 1,
        DeBuff = 2,
        Locked = 3,
    }

    public enum TSkillMaskBuffType
    {
        InvalidBuffMask = 0,
        UnMovable = 1,
        UnSkillable = 2,
        UnHitable = 4,
        InVisible = 8,
        UnBlockable = 16,
        UnDamageable = 32,
        UnTargetable = 64,
        UnHealable = 128,
        UnMagicHittable = 256,
    }

    public enum TSkillMaskBuffImmuneType
    {
        InvalidBuffImmuneMask = 0,
        InstantKill = 1,
        SlowDown = 2,
        Stun = 4,
        Pull = 8,
        Teleport = 16,
    }

    public enum TSkillValueBuffType
    {
        InvalidBuffValue = 0,
        MoveSpeedValue = 1,
        MoveSpeedPct = 2,
        PhyDmgValue = 3,
        PhyDmgPct = 4,
        PhyDefValue = 5,
        PhyDefPct = 6,
        MaxHpValue = 7,
        MaxHpPct = 8,
        CritValue = 9,
        CritDamageValue = 10,
        CritDamagePct = 11,
        HurtRatio = 12,
        HurtRatioPct = 13,
        ElementDamageRatioFire = 14,
        ElementDamageRatioIce = 15,
        ElementDamageRatioThunder = 16,
        ElementDamageRatioPoison = 17,
        ElementDamageRatioMachine = 18,
        ElementDamageRatioNature = 19,
        ElementDamageRatioDark = 20,
        ElementDamageRatioVoid = 21,
        ElementDamageRatioLight = 22,
        MaxBulletCount = 23,
        BulletRecoverSpeed = 24,
        HpRecoverFixedPhaseBegin = 25,
        UltimateSkillCdratio = 26,
        DodgeRatePct = 27,
    }

    public class TAssetGameObjectModel
    {
        public TBase Base { get; set; } = new TBase();
        public string PathName { get; set; } = "";
    }

    public class TBaseAddonProp
    {
        public TBase Base { get; set; } = new TBase();
        public string Name { get; set; } = "";
        public bool HasName { get; set; }
        public int BaseCritDmgRatio { get; set; }
        public int BaseCritDmgRatioRes { get; set; }
        public int BaseCritRatio { get; set; }
        public int BaseCritRatioRes { get; set; }
        public int BaseDmgRatio { get; set; }
        public int BaseDmgRatioRes { get; set; }
    }

    public class TBasePropGroup
    {
        public long UnitHp { get; set; }
        public bool HasUnitHp { get; set; }
        public int Atk { get; set; }
        public bool HasAtk { get; set; }
        public int Def { get; set; }
        public bool HasDef { get; set; }
        public int Hit { get; set; }
        public bool HasHit { get; set; }
        public int Miss { get; set; }
        public bool HasMiss { get; set; }
    }

    public class TLevelBaseProp
    {
        public TBase Base { get; set; } = new TBase();
        public List<TBasePropGroup> Props { get; } = new List<TBasePropGroup>();
    }

    public class TBattleUnit
    {
        public TBase Base { get; set; } = new TBase();
        public string Name { get; set; } = "";
        public string Desc { get; set; } = "";
        public uint Icon { get; set; }
        public TBattleUnitType BattleUnitType { get; set; }
        public uint BaseAddonPropId { get; set; }
        public uint LevelBaseProp { get; set; }
        public uint LevelIncAddonPropId { get; set; }
        public string AtkDesc { get; set; } = "";
    }

    public class TBuffValue
    {
        public TSkillValueBuffType BuffType { get; set; }
        public float Value { get; set; }
    }

    public class TBuff
    {
        public TBase Base { get; set; } = new TBase();
        public string Name { get; set; } = "";
        public string Desc { get; set; } = "";
        public float During { get; set; }
        public bool MultiDuring { get; set; }
        public uint Icon { get; set; }
        public int BuffGroup { get; set; }
        public TSkillBuffType BuffType { get; set; }
        public int MaxNum { get; set; }
        public int LogicID { get; set; }
        public int LogicParams { get; set; }
        public TSkillBuffBenefit Benefit { get; set; }
        public TSkillMaskBuffImmuneType MaskBuffImmuneType { get; set; }
        public TSkillMaskBuffType MaskBuffType { get; set; }
        public List<TBuffValue> ValueBuffType { get; } = new List<TBuffValue>();
        public uint ImmuneLogic { get; set; }
        public TSkillMaskBuffType DisperseMaskBuff { get; set; }
        public List<TSkillValueBuffType> DisperseBuffTypeList { get; } = new List<TSkillValueBuffType>();
        public List<int> DisperseBuffIdList { get; } = new List<int>();
        public bool ShowIcon { get; set; }
        public bool IsDisperse { get; set; }
        public bool Refreshable { get; set; }
    }

    public class TBuffImmune
    {
        public uint BuffId { get; set; }
        public int ImmuneRatio { get; set; }
    }

    public class TImmuneLogic
    {
        public TBase Base { get; set; } = new TBase();
        public bool ImmuneInstantKill { get; set; }
        public bool HasImmuneInstantKill { get; set; }
        public bool ImmuneSlowDown { get; set; }
        public bool HasImmuneSlowDown { get; set; }
        public bool ImmuneStun { get; set; }
        public bool HasImmuneStun { get; set; }
        public bool ImmunePull { get; set; }
        public bool HasImmunePull { get; set; }
        public bool ImmuneTeleport { get; set; }
        public bool HasImmuneTeleport { get; set; }
        public int ImmuneHitBack { get; set; }
        public bool HasImmuneHitBack { get; set; }
        public List<TBuffImmune> ImmuneBuff { get; } = new List<TBuffImmune>();
    }

    public class TSkillLogic
    {
        public TBase Base { get; set; } = new TBase();
        public string Name { get; set; } = "";
        public string Desc { get; set; } = "";
        public int LogicID { get; set; }
        public int LogicParams { get; set; }
        public float Cd { get; set; }
        public float Range { get; set; }
        public float DamageRate { get; set; }
        public int SkillWeight { get; set; }
        public bool Interruptible { get; set; }
        public bool CloakTargeting { get; set; }
        public string UiName { get; set; } = "";
        public string UiDes { get; set; } = "";
        public int UiIcon { get; set; }
        public List<uint> SubobjIds { get; } = new List<uint>();
    }

    public class TSubobject
    {
        public TBase Base { get; set; } = new TBase();
        public string Name { get; set; } = "";
        public uint Model { get; set; }
        public TVector ModelSize { get; set; } = new TVector();
        public uint LogicID { get; set; }
        public uint LogicParams { get; set; }
        public float During { get; set; }
        public float DamageRate { get; set; }
        public uint HitFx { get; set; }
        public uint HitGroundFx { get; set; }
        public uint DisappearFx { get; set; }
        public CollisionType CollisionType { get; set; }
        public bool HitWithLife { get; set; }
        public int MaxHitCount { get; set; }
        public float HitInterval { get; set; }
        public int SingleHitCount { get; set; }
        public float Radius { get; set; }
        public bool HitCloak { get; set; }
    }

    public class TElementImmune
    {
        public int ImmuneAmmunition { get; set; }
        public bool HasImmuneAmmunition { get; set; }
        public int ImmunePhysics { get; set; }
        public bool HasImmunePhysics { get; set; }
        public int ImmuneEnergy { get; set; }
        public bool HasImmuneEnergy { get; set; }
        public int ImmuneElectricity { get; set; }
        public bool HasImmuneElectricity { get; set; }
        public int ImmuneForce { get; set; }
        public bool HasImmuneForce { get; set; }
        public int ImmuneDark { get; set; }
        public bool HasImmuneDark { get; set; }
    }

    public class TFighter
    {
        public TBase Base { get; set; } = new TBase();
        public string Name { get; set; } = "";
        public uint Model { get; set; }
        public uint AiLogic { get; set; }
        public uint FsmLogic { get; set; }
        public uint Icon { get; set; }
        public uint BornLogic { get; set; }
        public List<uint> BornBuffs { get; } = new List<uint>();
        public List<uint> Skills { get; } = new List<uint>();
        public List<uint> DieSkills { get; } = new List<uint>();
        public TElementImmune ElementImmune { get; set; } = new TElementImmune();
        public uint ImmuneLogic { get; set; }
        public float PropHpMult { get; set; }
        public float PropAtkMult { get; set; }
        public float PropDefMult { get; set; }
        public float PropSpeedMult { get; set; }
        public float Radius { get; set; }
        public float Scale { get; set; }
        public List<uint> DieFXs { get; } = new List<uint>();
        public List<uint> HurtFXs { get; } = new List<uint>();
        public uint RadarModel { get; set; }
        public string DieSound { get; set; } = "";
    }
}

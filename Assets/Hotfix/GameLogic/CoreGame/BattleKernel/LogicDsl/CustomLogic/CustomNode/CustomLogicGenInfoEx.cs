using PBConfig;

namespace LccHotfix
{

    public interface IHasLogicWorld
    {
        LogicWorld LogicWorld { get; }
    }
    
    public interface IHasMetaWorld
    {
        MetaWorld MetaWorld { get; }
    }
    
    public interface IHasOwnerFighterEntityID
    {
        long OwnerFighterEntityID { get; }
    }

    public interface IHasSumUnitSource
    {
        ref UnitSource SumUnitSource { get; }
    }

    public interface IHasFighter
    {
        TFighter FighterCfg { get; }
        int BattleUnitTid { get; }
    }

    public interface IHasOwnerPlayerInfo
    {
        InGamePlayerInfo OwnerPlayerInfo { get; }
    }

    public interface IHasSubobjectSource
    {
        ref SubobjectSource? SubobjSource { get; }
    }

    public interface IHasDamageType
    {
        ref TElementType DamageType { get; }
    }
    




}
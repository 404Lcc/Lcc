namespace LccHotfix
{
    public struct UnitSource
    {
        public IBattlePlayerInfo PlayerInfo;    // 归属方信息
        public long FighterEnityId;             //归属战斗单位Entity
        public int FighterTid;                  // 归属战斗单位配置ID
        public PropertySnapshot Properties;     // 属性切片

        public UnitSource(LogicEntity e)
        {
            PlayerInfo = e.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.UnitOwnerInfoProvider?.GetOwnerInfo(e);
            FighterEnityId = e.ID;
            Properties = new PropertySnapshot();
            Properties.FillFromEntity(e);
            FighterTid = e.hasComBattleUnitTag ? e.comBattleUnitTag.Tag.FighterId ?? 0 : 0;
        }
    }
}

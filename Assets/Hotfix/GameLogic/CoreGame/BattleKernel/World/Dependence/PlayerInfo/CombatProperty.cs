namespace LccHotfix
{
    public sealed class GameplayCombatPropertyVolumeProvider : ICombatPropertyVolumeProvider
    {
        public void AddCategoryVolumes(LogicEntity entity, ref PropertySnapshot snapshot)
        {
            if (entity == null)
                return;

            if (!entity.GetBattleUnitTag(out var unitTag))
            {
                BattleLogger.LogError("叠加 玩家身上记录的集体生效的分类属性 unitTag == null");
                return;
            }

            var playerInfo = entity.GetPlayerInfo();
            if (playerInfo == null)
                return;

            snapshot.Add(playerInfo.VolumePlayer.Property);

            var volumeInfoBattleUnit = playerInfo.GetVolume_BattleUnit(unitTag.BattleUnitTid ?? 0);
            if (volumeInfoBattleUnit != null)
                snapshot.Add(volumeInfoBattleUnit.Property);

            var volumeInfoCamp = playerInfo.GetVolume_Camp(unitTag.Camp ?? 0);
            if (volumeInfoCamp != null)
                snapshot.Add(volumeInfoCamp.Property);

            var volumeInfoUnitType = playerInfo.GetVolume_UnitType(unitTag.BattleUnitType ?? 0);
            if (volumeInfoUnitType != null)
                snapshot.Add(volumeInfoUnitType.Property);

            var volumeInfoAttackType = playerInfo.GetVolume_AttackType(unitTag.AttackType ?? 0);
            if (volumeInfoAttackType != null)
                snapshot.Add(volumeInfoAttackType.Property);
            // 元素词条按本次伤害 TElementType 在 DamagePropertyModifier 结算时叠加，不绑 BattleUnitTag。
        }

        public void AddSubobjectVolume(IBattlePlayerInfo playerInfo, uint subobjectTid, ref PropertySnapshot snapshot)
        {
            if (playerInfo is not InGamePlayerInfo inGamePlayerInfo)
            {
                BattleLogger.LogError("叠加 玩家身上记录的集体生效的分类属性 playerInfo == null");
                return;
            }

            var volumeSubobjectTid = inGamePlayerInfo.GetVolume_SubobjectTid((int)subobjectTid);
            if (volumeSubobjectTid != null)
                snapshot.Add(volumeSubobjectTid.Property);
        }
    }

    public partial class InGamePlayerInfo : ICombatPropertyVolumeInfo
    {
        public void AddSubobjectVolume(uint subobjectTid, ref PropertySnapshot snapshot)
        {
            var volumeSubobjectTid = GetVolume_SubobjectTid((int)subobjectTid);
            if (volumeSubobjectTid != null)
                snapshot.Add(volumeSubobjectTid.Property);
        }
    }
}

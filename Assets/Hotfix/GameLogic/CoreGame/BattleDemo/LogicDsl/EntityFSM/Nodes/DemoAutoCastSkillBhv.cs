namespace LccHotfix
{
    public class DemoAutoCastSkillBhv : BehaviorNodeBase, INeedStopCheck
    {
        protected override float OnUpdate(float dt)
        {
            var entity = GetOwnerEntity();
            if (entity == null || entity.IsDead() || entity.hasComSkillProcess || !entity.hasComSkillSlot)
            {
                return dt;
            }

            foreach (var skillSlot in entity.comSkillSlot.skillSlots)
            {
                if (skillSlot?.Cfg == null || skillSlot.CdTimer > 0f)
                {
                    continue;
                }

                var target = GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.TargetQueryService?.SearchByDistanceY(entity, skillSlot.Cfg.Range, skillSlot.Cfg.CloakTargeting);
                if (target == null || target.IsDead())
                {
                    continue;
                }

                CreateSkillProcess(entity, (int)skillSlot.Tid, target, EDamageType.EdtAmmunition, 0);
                BattleLogger.LogDebug($"DemoAutoCastSkillBhv cast skill={skillSlot.Tid}, caster={entity.ID}, target={target.ID}");
                break;
            }

            return dt;
        }

        public bool CanStop()
        {
            return false;
        }
    }
}

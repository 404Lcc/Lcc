namespace LccModel
{
    /// <summary>
    /// 伤害吸血组件
    /// </summary>
    public class DamageBloodSuckComponent : Component
    {
        public override void Awake()
        {
            var combatEntity = Parent.GetParent<CombatEntity>();
            combatEntity.ListenActionPoint(ActionPointType.PostCauseDamage, OnCauseDamage);
        }

        private void OnCauseDamage(Entity action)
        {
            var damageAction = action as DamageAction;
            var value = damageAction.DamageValue * 0.2f;
            var combatEntity = Parent.GetParent<CombatEntity>();
            if (combatEntity.CureAbility.TryMakeAction(out var cureAction))
            {
                cureAction.Creator = combatEntity;
                cureAction.Target = combatEntity;
                cureAction.CureValue = (int)value;
                cureAction.SourceAssignAction = null;
                cureAction.ApplyCure();
            }
        }
    }
}
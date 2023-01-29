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
        public override void OnDestroy()
        {
            base.OnDestroy();

            var combatEntity = Parent.GetParent<CombatEntity>();
            combatEntity.UnListenActionPoint(ActionPointType.PostCauseDamage, OnCauseDamage);
        }
        private void OnCauseDamage(Entity action)
        {
            var damageAction = action as DamageAction;
            var value = damageAction.damageValue * 0.2f;
            var combatEntity = Parent.GetParent<CombatEntity>();
            if (combatEntity.cureActionAbility.TryMakeAction(out var cureAction))
            {
                cureAction.Creator = combatEntity;
                cureAction.Target = combatEntity;
                cureAction.cureValue = (int)value;
                cureAction.SourceAssignAction = null;
                cureAction.ApplyCure();
            }
        }
    }
}
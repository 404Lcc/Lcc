namespace LccModel
{
    public interface IAbilityEntity
    {
        public CombatEntity OwnerEntity
        {
            get;
            set;
        }
        public CombatEntity ParentEntity
        {
            get;
        }
        public bool Enable
        {
            get; set;
        }


        // 激活能力
        public void ActivateAbility();


        // 结束能力
        public void EndAbility();

        // 创建能力执行体
        public Entity CreateExecution();
    }
}
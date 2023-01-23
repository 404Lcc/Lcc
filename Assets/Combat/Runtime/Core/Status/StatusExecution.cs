using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 状态能力执行体
    /// </summary>
    public abstract class StatusExecution : Entity, IAbilityExecution
    {
        public Entity AbilityEntity { get; set; }
        public CombatEntity OwnerEntity { get; set; }


        public void BeginExecute()
        {
        }

        public void EndExecute()
        {
        }
    }
}
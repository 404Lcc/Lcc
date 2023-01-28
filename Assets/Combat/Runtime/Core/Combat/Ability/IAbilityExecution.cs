﻿namespace LccModel
{
    /// <summary>
    /// 能力执行体，能力执行体是实际创建、执行能力表现，触发能力效果应用的地方 这里可以存一些表现执行相关的临时的状态数据
    /// </summary>
    public interface IAbilityExecution
    {
        public Entity AbilityEntity { get; set; }
        public CombatEntity OwnerEntity { get; set; }


        // 开始执行
        public void BeginExecute();

        // 结束执行
        public void EndExecute();
    }
}
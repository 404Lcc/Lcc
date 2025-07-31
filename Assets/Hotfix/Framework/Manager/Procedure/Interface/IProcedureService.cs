using System;
using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 流程管理器
    /// </summary>
    public interface IProcedureService : IService
    {
        /// <summary>
        /// 当前流程
        /// </summary>
        ProcedureBase CurrentProcedure { get; }

        /// <summary>
        /// 所有流程
        /// </summary>
        Dictionary<Type, ProcedureBase> Procedures { get; }

        /// <summary>
        /// 所有流程的类型
        /// </summary>
        List<Type> ProcedureTypes { get; }

        /// <summary>
        /// 任意流程切换事件（上一个离开的流程、下一个进入的流程）
        /// </summary>
        event Action<ProcedureBase, ProcedureBase> AnyProcedureSwitchEvent;

        /// <summary>
        /// 获取流程
        /// </summary>
        /// <param name="type">流程类</param>
        /// <returns>流程对象</returns>
        ProcedureBase GetProcedure(Type type);

        /// <summary>
        /// 是否存在指定类型的流程
        /// </summary>
        /// <param name="type">流程类</param>
        /// <returns>是否存在</returns>
        bool IsExistProcedure(Type type);

        /// <summary>
        /// 切换流程
        /// </summary>
        /// <param name="type">目标流程</param>
        public void SwitchProcedure<T>() where T : ProcedureBase;

        /// <summary>
        /// 切换流程
        /// </summary>
        /// <param name="type">目标流程</param>
        void SwitchProcedure(Type type);
    }
}
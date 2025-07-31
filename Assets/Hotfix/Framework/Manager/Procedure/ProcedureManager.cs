using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 流程管理器
    /// </summary>
    internal class ProcedureManager : Module, IProcedureService
    {
        private float _timer = 0;

        /// <summary>
        /// 当前流程
        /// </summary>
        public ProcedureBase CurrentProcedure { get; private set; }

        /// <summary>
        /// 所有流程
        /// </summary>
        public Dictionary<Type, ProcedureBase> Procedures { get; private set; } = new Dictionary<Type, ProcedureBase>();

        /// <summary>
        /// 所有流程的类型
        /// </summary>
        public List<Type> ProcedureTypes { get; private set; } = new List<Type>();

        /// <summary>
        /// 任意流程切换事件（上一个离开的流程、下一个进入的流程）
        /// </summary>
        public event Action<ProcedureBase, ProcedureBase> AnyProcedureSwitchEvent;

        public ProcedureManager()
        {
            //创建所有已激活的流程对象
            foreach (Type item in Main.CodeTypesService.GetTypes(typeof(ProcedureAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(ProcedureAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    Procedures.Add(item, Activator.CreateInstance(item) as ProcedureBase);
                    ProcedureTypes.Add(item);
                }
            }

            //流程初始化
            foreach (var procedure in Procedures)
            {
                procedure.Value.OnInit();
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (CurrentProcedure != null)
            {
                CurrentProcedure.OnUpdate();

                if (_timer < 1)
                {
                    _timer += Time.deltaTime;
                }
                else
                {
                    _timer -= 1;
                    CurrentProcedure.OnUpdateSecond();
                }
            }
        }

        internal override void Shutdown()
        {
            Procedures.Clear();
            ProcedureTypes.Clear();
        }

        /// <summary>
        /// 获取流程
        /// </summary>
        /// <param name="type">流程类</param>
        /// <returns>流程对象</returns>
        public ProcedureBase GetProcedure(Type type)
        {
            if (Procedures.ContainsKey(type))
            {
                return Procedures[type];
            }
            else
            {
                throw new Exception($"获取流程失败：不存在流程 {type.Name} 或者流程未激活！");
            }
        }

        /// <summary>
        /// 是否存在指定类型的流程
        /// </summary>
        /// <param name="type">流程类</param>
        /// <returns>是否存在</returns>
        public bool IsExistProcedure(Type type)
        {
            return Procedures.ContainsKey(type);
        }

        /// <summary>
        /// 切换流程
        /// </summary>
        /// <typeparam name="T">目标流程</typeparam>
        public void SwitchProcedure<T>() where T : ProcedureBase
        {
            SwitchProcedure(typeof(T));
        }

        /// <summary>
        /// 切换流程
        /// </summary>
        /// <param name="type">目标流程</param>
        public void SwitchProcedure(Type type)
        {
            if (Procedures.ContainsKey(type))
            {
                if (CurrentProcedure == Procedures[type])
                    return;

                ProcedureBase lastProcedure = CurrentProcedure;
                ProcedureBase nextProcedure = Procedures[type];
                if (lastProcedure != null)
                {
                    lastProcedure.OnLeave(nextProcedure);
                }

                nextProcedure.OnEnter(lastProcedure);
                CurrentProcedure = nextProcedure;

                AnyProcedureSwitchEvent?.Invoke(lastProcedure, nextProcedure);
            }
            else
            {
                throw new Exception($"切换流程失败：不存在流程 {type.Name} 或者流程未激活！");
            }
        }
    }
}
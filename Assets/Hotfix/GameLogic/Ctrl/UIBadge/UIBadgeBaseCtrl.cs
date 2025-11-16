using System;
using System.Collections.Generic;
using LccModel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public abstract class UIBadgeBaseCtrl<THandler> : UIBehaviour, IBadgeComponent where THandler : BadgeComponentHandler, new()
    {
        private THandler _handler;

        [Tooltip("配置名")] [SerializeField] protected string _badgeName;
        protected GameObject _badgeObject;

        /// <summary>
        /// 当前数量
        /// </summary>
        public int Count => _handler.Count;

        public BadgeConfig GetConfig(string badgeName)
        {
            return Main.ClientConfigService.GetConfig<BadgeClientConfig>().GetConfig(badgeName);
        }

        public abstract void RefreshDisplay();

        protected override void Awake()
        {
            _handler = new THandler();
            _handler.InitComponent(this);
            if (!string.IsNullOrEmpty(_badgeName))
            {
                SetBadge(_badgeName);
            }
        }

        protected override void OnDestroy()
        {
            _handler.Dispose();
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="badgeName">配置名</param>
        /// <param name="refresh">是否立刻刷新</param>
        public void SetBadge(string badgeName, bool refresh = true)
        {
            if (!string.IsNullOrEmpty(badgeName))
            {
                return;
            }

            _badgeName = badgeName;
            _handler.SetBadge(badgeName, refresh);
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        /// <param name="listeners">监听事件列表</param>
        /// <param name="listenerCheckDict">监听事件过滤器</param>
        public void SetListeners(IEnumerable<Type> listeners, Dictionary<Type, Func<IEventMessage, bool>> listenerCheckDict = null)
        {
            _handler.SetListeners(listeners, listenerCheckDict);
        }

        /// <summary>
        /// 设置当前参数
        /// </summary>
        /// <param name="args"></param>
        public void SetArgs(params object[] args)
        {
            _handler.SetArgs(args);
        }

        /// <summary>
        /// 设置数量
        /// </summary>
        /// <param name="count"></param>
        public void SetCount(int count)
        {
            _handler.SetCount(count);
        }
    }
}
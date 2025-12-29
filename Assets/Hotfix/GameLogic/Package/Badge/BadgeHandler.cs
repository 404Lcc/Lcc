using System;
using System.Collections.Generic;
using LccModel;
using UnityEngine;
using Event = LccModel.Event;

namespace LccHotfix
{
    public abstract class BadgeHandler
    {
        protected string _badgeName;
        protected BadgeConfig _config;

        protected Delegate _resolve;
        protected Type[] _resolveParamTypes = Array.Empty<Type>();
        protected object[] _params = Array.Empty<object>();
        protected List<Type> _listenerList = new List<Type>();
        protected Dictionary<Type, Func<IEventMessage, bool>> _listenerCheckDict = new Dictionary<Type, Func<IEventMessage, bool>>();

        protected Func<int> _getCount;

        protected bool _dirty;

        /// <summary>
        /// 当前数量
        /// </summary>
        public int Count { get; protected set; } = -1;

        public BadgeHandler()
        {
            _getCount = () => _resolve != null ? Convert.ToInt32(_resolve.DynamicInvoke(_params)) : 0;
            Main.BadgeService.RegisterHandler(this);
            OnInit();
        }

        public void LateUpdate()
        {
            // 标记时刷新
            if (!_dirty)
                return;

            Refresh();
            _dirty = false;
        }

        public void Dispose()
        {
            OnDispose();

            Main.BadgeService.UnRegisterHandler(this);
            SetResolveDelegate(null);
            _params = Array.Empty<object>();
            SetListeners(null);
            _getCount = null;
            _dirty = false;
            Count = 0;
        }

        /// <summary>
        /// 根据配置名获取配置
        /// </summary>
        /// <param name="badgeName">配置名</param>
        /// <returns>配置名对应的配置</returns>
        protected abstract BadgeConfig GetConfig(string badgeName);

        /// <summary>
        /// 刷新显示
        /// </summary>
        protected abstract void RefreshDisplay();

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="badgeName">配置名</param>
        /// <param name="refresh">是否立刻刷新</param>
        public void SetBadge(string badgeName, bool refresh = true)
        {
            if (_badgeName == badgeName)
                return;

            _badgeName = badgeName;
            _config = GetConfig(badgeName);
            if (_config != null)
            {
                SetResolveDelegate(_config.Resolve);
                SetListeners(_config.Listeners, _config.ListenerCheckDict);
            }
            else
            {
                SetResolveDelegate(null);
                SetListeners(null);
            }

            if (refresh)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (_getCount == null)
            {
                SetCount(0);
                return;
            }

            SetCount(_getCount());
        }

        /// <summary>
        /// 设置获取数量的委托
        /// </summary>
        /// <param name="resolve"></param>
        private void SetResolveDelegate(Delegate resolve)
        {
            if (resolve == _resolve)
                return;

            _resolve = resolve;

            if (resolve == null)
            {
                _resolveParamTypes = Array.Empty<Type>();
                _params = Array.Empty<object>();
                return;
            }

            var parameters = resolve.Method.GetParameters();

            //缓存参数类型
            if (parameters.Length > 0)
            {
                _resolveParamTypes = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    _resolveParamTypes[i] = parameters[i].ParameterType;
                }
            }
            else
            {
                _resolveParamTypes = Array.Empty<Type>();
            }

            FillDefaultParams();
        }

        /// <summary>
        /// 填充默认参数
        /// </summary>
        private void FillDefaultParams()
        {
            //根据参数类型填充默认值
            if (_resolveParamTypes.Length > 0)
            {
                _params = new object[_resolveParamTypes.Length];
                for (int i = 0; i < _resolveParamTypes.Length; ++i)
                {
                    var type = _resolveParamTypes[i];
                    _params[i] = type.IsValueType ? Activator.CreateInstance(type) : null;
                }
            }
            else
            {
                _params = Array.Empty<object>();
            }
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        /// <param name="listeners">监听事件列表</param>
        /// <param name="listenerCheckDict">监听事件过滤器</param>
        public void SetListeners(IEnumerable<Type> listeners, Dictionary<Type, Func<IEventMessage, bool>> listenerCheckDict = null)
        {
            ClearListeners();

            if (listeners == null)
                return;

            foreach (var item in listeners)
            {
                if (!typeof(IEventMessage).IsAssignableFrom(item))
                {
                    Debug.LogError($"[Badge] 监听器类型 {item} 未实现 IEventMessage 接口，BadgeName = {_badgeName}");
                    continue;
                }

                Event.AddListener(item, OnEventTriggered);
                _listenerList.Add(item);

                if (listenerCheckDict != null && listenerCheckDict.TryGetValue(item, out var check))
                {
                    _listenerCheckDict.Add(item, check);
                }
            }
        }

        /// <summary>
        /// 清空当前Listener
        /// </summary>
        private void ClearListeners()
        {
            //将当前监听的类型全部移除
            for (int i = 0; i < _listenerList.Count; ++i)
            {
                Event.RemoveListener(_listenerList[i], OnEventTriggered);
            }

            _listenerList.Clear();
            _listenerCheckDict.Clear();
        }

        private void OnEventTriggered(IEventMessage message)
        {
            //有检查则先检查
            if (_listenerCheckDict.TryGetValue(message.GetType(), out var check) && !check(message))
                return;

            //触发事件时标记
            _dirty = true;
        }

        /// <summary>
        /// 设置当前参数
        /// </summary>
        /// <param name="args"></param>
        public void SetArgs(params object[] args)
        {
            if (args == null)
            {
                args = Array.Empty<object>();
            }

            if (args.Length != _resolveParamTypes.Length)
            {
                Debug.LogError($"[Badge] 参数长度不匹配：需要{_resolveParamTypes.Length}个参数，但实际传入{args.Length}个，BadgeName = {_badgeName}");
                return;
            }

            for (int i = 0; i < _resolveParamTypes.Length; ++i)
            {
                var type = _resolveParamTypes[i];
                var arg = args[i];

                if (arg == null)
                {
                    if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
                    {
                        Debug.LogError($"[Badge] 第{i}个参数不能为null，期望类型为{type.Name}，BadgeName = {_badgeName}");
                        return;
                    }
                }
                else
                {
                    if (!type.IsInstanceOfType(arg))
                    {
                        Debug.LogError($"[Badge] 第{i}个参数类型不匹配：期望为{type.Name}类型，但实际为{arg.GetType().Name}类型，BadgeName = {_badgeName}");
                        return;
                    }
                }
            }

            _params = args;

            Refresh();
        }

        /// <summary>
        /// 设置数量
        /// </summary>
        /// <param name="count"></param>
        public void SetCount(int count)
        {
            if (count == Count)
                return;

            Count = count;
            RefreshDisplay();
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnDispose()
        {
        }
    }
}
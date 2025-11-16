using System;
using System.Collections.Generic;
using System.Linq;
using LccHotfix;
using LccModel;
using UnityEngine;

[ClientConfig]
public class BadgeClientConfig : IClientConfig
{
    private Dictionary<string, BadgeConfig> _badgeConfig = new Dictionary<string, BadgeConfig>();

    public void LoadConfig()
    {
    }

    public void UnloadConfig()
    {
        _badgeConfig.Clear();
    }

    /// <summary>
    /// 根据配置名获取配置
    /// </summary>
    /// <param name="badgeName"></param>
    /// <returns></returns>
    public BadgeConfig GetConfig(string badgeName)
    {
        if (_badgeConfig.TryGetValue(badgeName, out var config))
        {
            return config;
        }

        return null;
    }

    /// <summary>
    /// 创建配置
    /// </summary>
    /// <param name="badgeName">配置名</param>
    /// <param name="resolve">获取数量的委托（需返回int）</param>
    /// <param name="listeners">监听事件</param>
    /// <param name="listenerCheckDict">监听事件过滤器</param>
    private void CreateConfig(string badgeName, Delegate resolve, Type[] listeners = null, Dictionary<Type, Func<IEventMessage, bool>> listenerCheckDict = null)
    {
        if (_badgeConfig.ContainsKey(badgeName))
        {
            Debug.LogError($"[Badge] 重复配置，BadgeName = {badgeName}");
            return;
        }

        BadgeConfig config = new BadgeConfig();
        config.Resolve = resolve;
        config.Listeners = listeners;
        config.ListenerCheckDict = listenerCheckDict;
        _badgeConfig.Add(badgeName, config);
    }

    /// <summary>
    /// 创建配置（支持通过已注册的配置名获取监听事件）
    /// </summary>
    /// <param name="badgeName">配置名</param>
    /// <param name="resolve">获取数量的委托（需返回int）</param>
    /// <param name="listeners">监听事件</param>
    /// <param name="listenerCheckDict">监听事件过滤器</param>
    /// <param name="badgeNames">已注册的配置名</param>
    private void CreateConfig(string badgeName, Delegate resolve, Type[] listeners = null, Dictionary<Type, Func<IEventMessage, bool>> listenerCheckDict = null, params string[] badgeNames)
    {
        var otherListeners = GetListeners(badgeNames);
        var mergedListeners = listeners != null ? listeners.Concat(otherListeners).ToArray() : otherListeners;
        CreateConfig(badgeName, resolve, mergedListeners, listenerCheckDict);
    }

    /// <summary>
    /// 获取配置的listener
    /// </summary>
    /// <param name="badgeNames">已注册的配置名</param>
    /// <returns></returns>
    private Type[] GetListeners(params string[] badgeNames)
    {
        var listeners = new HashSet<Type>();
        for (int i = 0; i < badgeNames.Length; i++)
        {
            if (!_badgeConfig.TryGetValue(badgeNames[i], out var config))
                continue;

            foreach (var item in config.Listeners)
            {
                listeners.Add(item);
            }
        }

        return listeners.ToArray();
    }

    /// <summary>
    /// 获取配置的数量
    /// </summary>
    /// <param name="badgeName">配置名</param>
    /// <param name="args">参数</param>
    /// <returns></returns>
    private int GetBadgeCount(string badgeName, params object[] args)
    {
        if (!_badgeConfig.TryGetValue(badgeName, out var config))
        {
            return TraceBadgeCount(badgeName, 0);
        }

        try
        {
            var parameters = config.Resolve.Method.GetParameters();

            if (parameters.Length > 0)
            {
                var resolveParamTypes = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    var type = parameters[i].ParameterType;
                    resolveParamTypes[i] = type;
                }

                if (args == null)
                {
                    args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        var type = parameters[i].ParameterType;
                        args[i] = type.IsValueType ? Activator.CreateInstance(type) : null;
                    }
                }
                else
                {
                    if (args.Length != resolveParamTypes.Length)
                    {
                        Debug.LogError($"[Badge] 参数长度不匹配：需要{resolveParamTypes.Length}个参数，但实际传入{args.Length}个，BadgeName = {badgeName}");
                        return TraceBadgeCount(badgeName, 0);
                    }

                    for (int i = 0; i < resolveParamTypes.Length; ++i)
                    {
                        var type = resolveParamTypes[i];
                        var arg = args[i];

                        if (arg == null)
                        {
                            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
                            {
                                Debug.LogError($"[Badge] 第{i}个参数不能为null，期望类型为{type.Name}，BadgeName = {badgeName}");
                                return TraceBadgeCount(badgeName, 0);
                            }
                        }
                        else
                        {
                            if (!type.IsInstanceOfType(arg))
                            {
                                Debug.LogError($"[Badge] 第{i}个参数类型不匹配：期望为{type.Name}类型，但实际为{arg.GetType().Name}类型，BadgeName = {badgeName}");
                                return TraceBadgeCount(badgeName, 0);
                            }
                        }
                    }
                }
            }

            return TraceBadgeCount(badgeName, Convert.ToInt32(config.Resolve.DynamicInvoke(args)));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Badge] 解析时发生异常，BadgeName = {badgeName}: {ex.Message}, \n{ex.StackTrace}");
        }

        return TraceBadgeCount(badgeName, 0);
    }

    private int TraceBadgeCount(string badgeName, int count)
    {
        Debug.LogWarning($"[Badge] Badge 数量 {badgeName}: {count.ToString()}");
        return count;
    }
}
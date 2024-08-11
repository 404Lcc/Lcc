using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
	public class EventGroup
	{
		private readonly Dictionary<Type, List<Action<IEventMessage>>> _cachedListener = new Dictionary<Type, List<Action<IEventMessage>>>();

		/// <summary>
		/// 添加一个监听
		/// </summary>
		public void AddListener<TEvent>(Action<IEventMessage> listener) where TEvent : IEventMessage
		{
			Type eventType = typeof(TEvent);
			if (_cachedListener.ContainsKey(eventType) == false)
				_cachedListener.Add(eventType, new List<Action<IEventMessage>>());

			if (_cachedListener[eventType].Contains(listener) == false)
			{
				_cachedListener[eventType].Add(listener);
				Event.AddListener(eventType, listener);
			}
			else
			{
				Debug.LogWarning($"Event listener is exist : {eventType}");
			}
		}

		/// <summary>
		/// 移除所有缓存的监听
		/// </summary>
		public void RemoveAllListener()
		{
			foreach (var pair in _cachedListener)
			{
				Type eventType = pair.Key;
				for (int i = 0; i < pair.Value.Count; i++)
				{
					Event.RemoveListener(eventType, pair.Value[i]);
				}
				pair.Value.Clear();
			}
			_cachedListener.Clear();
		}
	}
}
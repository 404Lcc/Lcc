using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class DictionaryList<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
        private List<TKey> _keyList = new List<TKey>();
        private List<TValue> _valueList = new List<TValue>();

        public int Count => dict.Count;
        public List<TKey> KeyList => _keyList;
        public List<TValue> ValueList => _valueList;

        public void Add(TKey key, TValue val)
        {
            dict.Add(key, val);
            _keyList.Add(key);
            _valueList.Add(val);
        }

        public void Remove(TKey key)
        {
            if (dict.ContainsKey(key))
            {
                var value = dict[key];
                dict.Remove(key);
                if (_valueList.Contains(value))
                {
                    _valueList.Remove(value);
                }
            }

            if (_keyList.Contains(key))
            {
                _keyList.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (dict.TryGetValue(key, out value))
            {
                return true;
            }

            return false;
        }

        public void Clear()
        {
            dict.Clear();
            _keyList.Clear();
            _valueList.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return dict.ContainsKey(key);
        }

        public TValue this[TKey key]
        {
            set
            {
                int index = GetIndex(key);
                if (index == -1)
                {
                    Debug.LogError($"key = {key} not found!");
                    return;
                }

                dict[key] = value;
                _valueList[index] = value;
            }
            get => dict[key];
        }
        
        private int GetIndex(TKey key)
        {
            for (int i = 0; i < _keyList.Count; i++)
            {
                if (_keyList[i].Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
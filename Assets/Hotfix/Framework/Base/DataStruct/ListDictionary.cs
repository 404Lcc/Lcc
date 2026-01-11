using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class ListDictionary<TKey, TValue> : IEnumerable<TValue>
    {
        private readonly Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        private readonly List<TValue> _list = new List<TValue>();
        private readonly List<TKey> _keyList = new List<TKey>();

        public int Count => _dict.Count;

        public List<TKey> KeyList => _keyList;

        public List<TValue> List => _list;

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue obj)
        {
            if (_dict.ContainsKey(key))
            {
                Debug.LogError($"Add Repeated key: {key}");
                return;
            }

            _dict.Add(key, obj);
            _list.Add(obj);
            _keyList.Add(key);
        }

        public void Set(TKey key, TValue obj)
        {
            Remove(key);
            Add(key, obj);
        }

        public void Remove(TKey key)
        {
            if (!_dict.TryGetValue(key, out var obj))
            {
                return;
            }

            _dict.Remove(key);
            _list.Remove(obj);
            _keyList.Remove(key);
        }

        public void Clear()
        {
            _dict.Clear();
            _list.Clear();
            _keyList.Clear();
        }

        class ListMapEnumerable : IEnumerator<TValue>
        {
            private List<TValue> _list;
            private int _index;
            private TValue _curr;

            public void SetList(List<TValue> list)
            {
                _list = list;
                _index = -1;
                _curr = default;
            }

            public bool MoveNext()
            {
                _index++;
                if (_index >= _list.Count)
                {
                    return false;
                }

                _curr = _list[_index];
                return true;
            }

            public void Reset()
            {
                _list = null;
                _index = -1;
                _curr = default;
            }

            public TValue Current => _curr;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                Reset();
            }
        }

        private readonly ListMapEnumerable _customEnumerable = new ListMapEnumerable();

        public IEnumerator<TValue> GetEnumerator()
        {
            _customEnumerable.SetList(_list);
            return _customEnumerable;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int GetIdx(TKey key)
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

        public TValue this[TKey key]
        {
            set
            {
                _dict[key] = value;
                int idx = GetIdx(key);
                if (idx == -1)
                {
                    Debug.LogError($"key = {key} not found!");
                    return;
                }

                _list[idx] = value;
            }
            get => _dict[key];
        }
    }
}
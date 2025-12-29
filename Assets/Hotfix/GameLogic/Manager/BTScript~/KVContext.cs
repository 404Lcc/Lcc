using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class KVContext
    {
        protected Dictionary<int, int> _intDict = new Dictionary<int, int>();
        protected Dictionary<int, float> _floatDict = new Dictionary<int, float>();

        protected Dictionary<int, string> _stringDict = new Dictionary<int, string>();
        protected Dictionary<int, bool> _boolDict = new Dictionary<int, bool>();
        protected Dictionary<int, object> _objectDict = new Dictionary<int, object>();

        protected Dictionary<int, Vector3> _vectorDict = new Dictionary<int, Vector3>();

        public void Clear()
        {
            _intDict.Clear();
            _floatDict.Clear();
            _stringDict.Clear();
            _boolDict.Clear();
            _objectDict.Clear();
            _vectorDict.Clear();
        }

        public int GetInt(int key, int defaultValue = 0)
        {
            if (_intDict.TryGetValue(key, out var value))
            {
                return value;
            }

            value = defaultValue;
            return value;
        }

        public void AddInt(int key, int value, int defaultValue = 0)
        {
            var oldValue = GetInt(key, defaultValue);
            _intDict[key] = oldValue + value;
        }

        public void MinusInt(int key, int value)
        {
            if (_intDict.TryGetValue(key, out var oldValue))
            {
                _intDict[key] = oldValue - value;
            }
        }





        public float GetFloat(int key, float defaultValue = 0)
        {
            if (_floatDict.TryGetValue(key, out var value))
            {
                return value;
            }

            value = defaultValue;
            return value;
        }

        public void AddFloat(int key, float value, float defaultValue = 0)
        {
            var oldValue = GetFloat(key, defaultValue);
            _floatDict[key] = oldValue + value;
        }

        public void MinusFloat(int key, float value)
        {
            if (_floatDict.TryGetValue(key, out var oldValue))
            {
                _floatDict[key] = oldValue - value;
            }
        }

        public string GetString(int key, string defaultValue = "")
        {
            if (_stringDict.TryGetValue(key, out var value))
            {
                return value;
            }

            value = defaultValue;
            return value;
        }

        public string SetString(int key, string value)
        {
            _stringDict[key] = value;
            return value;
        }

        public bool GetBool(int key, bool defaultValue = false)
        {
            if (_boolDict.TryGetValue(key, out var value))
            {
                return value;
            }

            value = defaultValue;
            return value;
        }

        public void SetBool(int key, bool value)
        {
            _boolDict[key] = value;

        }



        public T GetObject<T>(int key, T defaultValue = null) where T : class
        {
            if (_objectDict.TryGetValue(key, out var value))
            {
                return value as T;
            }

            return defaultValue;
        }

        public void SetObject(int key, object value)
        {
            _objectDict[key] = value;

        }


        public Vector3 GetVector(int key)
        {
            if (_vectorDict.TryGetValue(key, out var value))
            {
                return value;
            }

            value = Vector3.zero;
            return value;
        }

        public void SetVector(int key, Vector3 value)
        {
            _vectorDict[key] = value;

        }


        public bool HasKey(int key)
        {
            if (_intDict.ContainsKey(key))
                return true;
            if (_floatDict.ContainsKey(key))
                return true;
            if (_stringDict.ContainsKey(key))
                return true;
            if (_boolDict.ContainsKey(key))
                return true;
            if (_objectDict.ContainsKey(key))
                return true;
            if (_vectorDict.ContainsKey(key))
                return true;

            return false;
        }

        public void RemoveKey(int key)
        {
            if (_intDict.ContainsKey(key))
            {
                _intDict.Remove(key);
            }

            if (_floatDict.ContainsKey(key))
            {
                _floatDict.Remove(key);
            }

            if (_stringDict.ContainsKey(key))
            {
                _stringDict.Remove(key);
            }

            if (_boolDict.ContainsKey(key))
            {
                _boolDict.Remove(key);
            }

            if (_objectDict.ContainsKey(key))
            {
                _objectDict.Remove(key);
            }

            if (_vectorDict.ContainsKey(key))
            {
                _vectorDict.Remove(key);
            }
        }

        public void Copy(KVContext target, bool overlap = false)
        {
            foreach (var item in target._intDict)
            {
                if (overlap)
                {
                    _intDict[item.Key] = item.Value;
                }
                else
                {
                    _intDict.TryAdd(item.Key, item.Value);
                }
            }

            foreach (var item in target._floatDict)
            {
                if (overlap)
                {
                    _floatDict[item.Key] = item.Value;
                }
                else
                {
                    _floatDict.TryAdd(item.Key, item.Value);
                }
            }

            foreach (var item in target._stringDict)
            {
                if (overlap)
                {
                    _stringDict[item.Key] = item.Value;
                }
                else
                {
                    _stringDict.TryAdd(item.Key, item.Value);
                }
            }

            foreach (var item in target._boolDict)
            {
                if (overlap)
                {
                    _boolDict[item.Key] = item.Value;
                }
                else
                {
                    _boolDict.TryAdd(item.Key, item.Value);
                }
            }

            foreach (var item in target._objectDict)
            {
                if (overlap)
                {
                    _objectDict[item.Key] = item.Value;
                }
                else
                {
                    _objectDict.TryAdd(item.Key, item.Value);
                }
            }

            foreach (var item in target._vectorDict)
            {
                if (overlap)
                {
                    _vectorDict[item.Key] = item.Value;
                }
                else
                {
                    _vectorDict.TryAdd(item.Key, item.Value);
                }
            }
        }


        public void Copy(KVContext target, int key, bool overlap = false)
        {
            if (target._intDict.ContainsKey(key))
            {
                if (overlap)
                {
                    _intDict[key] = target._intDict[key];
                }
                else
                {
                    _intDict.TryAdd(key, target._intDict[key]);
                }
            }

            if (target._floatDict.ContainsKey(key))
            {
                if (overlap)
                {
                    _floatDict[key] = target._floatDict[key];
                }
                else
                {
                    _floatDict.TryAdd(key, target._floatDict[key]);
                }
            }

            if (target._stringDict.ContainsKey(key))
            {
                if (overlap)
                {
                    _stringDict[key] = target._stringDict[key];
                }
                else
                {
                    _stringDict.TryAdd(key, target._stringDict[key]);
                }
            }

            if (target._boolDict.ContainsKey(key))
            {
                if (overlap)
                {
                    _boolDict[key] = target._boolDict[key];
                }
                else
                {
                    _boolDict.TryAdd(key, target._boolDict[key]);
                }
            }

            if (target._objectDict.ContainsKey(key))
            {
                if (overlap)
                {
                    _objectDict[key] = target._objectDict[key];
                }
                else
                {
                    _objectDict.TryAdd(key, target._objectDict[key]);
                }
            }

            if (target._vectorDict.ContainsKey(key))
            {
                if (overlap)
                {
                    _vectorDict[key] = target._vectorDict[key];
                }
                else
                {
                    _vectorDict.TryAdd(key, target._vectorDict[key]);
                }
            }
        }
    }
}
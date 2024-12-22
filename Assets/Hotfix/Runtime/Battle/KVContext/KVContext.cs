using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public enum KVType
    {
        RootScript,
    }
    public class KVContext
    {
        protected Dictionary<KVType, int> _intDict = new Dictionary<KVType, int>();
        protected Dictionary<KVType, float> _floatDict = new Dictionary<KVType, float>();

        protected Dictionary<KVType, string> _stringDict = new Dictionary<KVType, string>();
        protected Dictionary<KVType, bool> _boolDict = new Dictionary<KVType, bool>();
        protected Dictionary<KVType, object> _objectDict = new Dictionary<KVType, object>();

        protected Dictionary<KVType, Vector3> _vectorDict = new Dictionary<KVType, Vector3>();

        public void Clear()
        {
            _intDict.Clear();
            _floatDict.Clear();
            _stringDict.Clear();
            _boolDict.Clear();
            _objectDict.Clear();
            _vectorDict.Clear();
        }

        public int GetInt(KVType key, int defaultValue = 0)
        {
            if (_intDict.TryGetValue(key, out var value))
            {
                return value;
            }
            value = defaultValue;
            return value;
        }

        public void AddInt(KVType key, int value, int defaultValue = 0)
        {
            var oldValue = GetInt(key, defaultValue);
            _intDict[key] = oldValue + value;
        }

        public void MinusInt(KVType key, int value)
        {
            if (_intDict.TryGetValue(key, out var oldValue))
            {
                _intDict[key] = oldValue - value;
            }
        }





        public float GetFloat(KVType key, float defaultValue = 0)
        {
            if (_floatDict.TryGetValue(key, out var value))
            {
                return value;
            }
            value = defaultValue;
            return value;
        }

        public void AddFloat(KVType key, float value, float defaultValue = 0)
        {
            var oldValue = GetFloat(key, defaultValue);
            _floatDict[key] = oldValue + value;
        }

        public void MinusFloat(KVType key, float value)
        {
            if (_floatDict.TryGetValue(key, out var oldValue))
            {
                _floatDict[key] = oldValue - value;
            }
        }





        public string GetString(KVType key, string defaultValue = "")
        {
            if (_stringDict.TryGetValue(key, out var value))
            {
                return value;
            }
            value = defaultValue;
            return value;
        }

        public string SetString(KVType key, string value)
        {
            _stringDict[key] = value;
            return value;
        }





        public bool GetBool(KVType key, bool defaultValue = false)
        {
            if (_boolDict.TryGetValue(key, out var value))
            {
                return value;
            }
            value = defaultValue;
            return value;
        }
        public void SetBool(KVType key, bool value)
        {
            _boolDict[key] = value;

        }



        public T GetObject<T>(KVType key, T defaultValue = null) where T : class
        {
            if (_objectDict.TryGetValue(key, out var value))
            {
                return value as T;
            }
            return defaultValue;
        }
        public void SetObject(KVType key, object value)
        {
            _objectDict[key] = value;

        }


        public Vector3 GetVector(KVType key)
        {
            if (_vectorDict.TryGetValue(key, out var value))
            {
                return value;
            }
            value = Vector3.zero;
            return value;
        }

        public void SetVector(KVType key, Vector3 value)
        {
            _vectorDict[key] = value;

        }






        public bool HasKey(KVType key)
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

        public void RemoveKey(KVType key)
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


        public void Copy(KVContext target, KVType key, bool overlap = false)
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
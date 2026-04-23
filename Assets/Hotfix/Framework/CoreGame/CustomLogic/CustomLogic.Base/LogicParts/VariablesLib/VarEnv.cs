using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LccHotfix
{
    public interface IVariables
    {
        bool HasVar(string id);
        bool ClearVar(string key);
        void Clear();
        void CopyTo(VarEnv env, bool skipSameKey = true, bool logSameKey = true);
    }

    public class VariablesImp<T> : IVariables
    {
        protected Dictionary<string, T> _varDic = new Dictionary<string, T>(4);

        public void WriteVar(string id, T value)
        {
            _varDic[id] = value;
        }

        public bool ReadVar(string id, out T getV)
        {
            if (_varDic.ContainsKey(id))
            {
                getV = _varDic[id];
                return true;
            }

            getV = default(T);
            return false;
        }

        public bool HasVar(string id)
        {
            return _varDic.ContainsKey(id);
        }

        public bool ClearVar(string key)
        {
            return _varDic.Remove(key);
        }

        public void Clear()
        {
            _varDic.Clear();
        }

        public void CopyTo(VarEnv env, bool skipSameKey = true, bool logSameKey = true)
        {
            foreach (var kv in _varDic)
            {
                var key = kv.Key;
                var value = kv.Value;
                if (env.HasVar<T>(key))
                {
                    if (logSameKey)
                    {
                        env.ReadVar<T>(key, out var oldV);
                    }

                    if (skipSameKey)
                    {
                        continue;
                    }
                }

                env.WriteVar(key, value);
            }
        }
    }

    //Variables Env
    public class VarEnv : ICanRecycle
    {
        private Dictionary<System.Type, IVariables> _varTypeDic;

        private static int _index = 0;
        private int _createIdx = 0;

        public bool IsInPool { get; private set; } = false;

        public VarEnv()
        {
            _createIdx = ++_index;
            _varTypeDic = new Dictionary<System.Type, IVariables>(5);
        }
        

        public void Construct()
        {
            IsInPool = false;
        }

        public void Destroy()
        {
            IsInPool = true;
            Clear();
        }
        

        public void AddVarType<T>(System.Type type)
        {
            if (!_varTypeDic.ContainsKey(type))
                _varTypeDic.Add(type, new VariablesImp<T>());
        }

        private VariablesImp<T> GetVariables<T>(bool autoAdd = false)
        {
            var type = typeof(T);
            if (_varTypeDic.ContainsKey(type))
            {
                return _varTypeDic[type] as VariablesImp<T>;
            }

            if (autoAdd)
            {
                var variables = new VariablesImp<T>();
                _varTypeDic.Add(typeof(T), variables);
                return variables;
            }

            return null;
        }

        private IVariables GetIVariables<T>()
        {
            var typeT = typeof(T);
            if (typeT.IsClass || typeT.IsInterface)
            {
                return GetVariables<object>();
            }

            if (typeT == typeof(uint))
            {
                LogWrapper.LogError($"GetIVariables case uint");
                return GetVariables<int>();
            }
            else
            {
                return GetVariables<T>();
            }
        }


        public bool ReadVar<T>(string key, out T value)
        {
            var typeT = typeof(T);
            if (typeT.IsClass || typeT.IsInterface)
            {
                var variables = GetVariables<object>();
                if (variables != null && variables.ReadVar(key, out var exist))
                {
                    if (exist is T v)
                    {
                        value = v;
                        return true;
                    }
                }
            }
            else
            {
                value = default;
                if (typeT == typeof(uint))
                {
                    LogWrapper.LogError($"ReadVar case uint  key={key}, value={value}");
                    var variables = GetVariables<int>();
                    if (variables != null && variables.ReadVar(key, out var exist))
                    {
                        value = Unsafe.As<int, T>(ref exist);
                        return true;
                    }
                }
                else
                {
                    var variables = GetVariables<T>();
                    if (variables != null && variables.ReadVar(key, out value))
                    {
                        return true;
                    }
                }
            }

            value = default(T);
            return false;
        }

        public void WriteVar<T>(string key, T value)
        {
            var typeT = typeof(T);
            if (typeT.IsClass || typeT.IsInterface)
            {
                var variables = GetVariables<object>(true);
                variables.WriteVar(key, value);
            }
            else
            {
                if (typeT == typeof(uint))
                {
                    var intV = Unsafe.As<T, int>(ref value);
                    LogWrapper.LogError($"WriteVar case uint  key={key}, value={value}, intV={intV}");
                    var variables = GetVariables<int>(true);
                    variables.WriteVar(key, intV);
                }
                else
                {
                    VariablesImp<T> variables = GetVariables<T>(true);
                    variables.WriteVar(key, value);
                }
            }
        }

        public bool HasVar<T>(string key)
        {
            var variables = GetIVariables<T>();
            return variables?.HasVar(key) ?? false;
        }

        public bool ClearVar<T>(string key)
        {
            var variables = GetIVariables<T>();
            return variables?.ClearVar(key) ?? false;
        }

        public void Clear()
        {
            foreach (var kv in _varTypeDic)
            {
                kv.Value.Clear();
            }
        }

        public void CopyTo(in VarEnv env)
        {
            foreach (var item in _varTypeDic)
            {
                var variables = item.Value;
                variables.CopyTo(env);
            }
        }

        public bool CopyTo<T>(in VarEnv env, string key, bool logError = true)
        {
            if (env.HasVar<T>(key))
                return false;
            if (ReadVar<T>(key, out var value))
            {
                env.WriteVar<T>(key, value);
                return true;
            }
            if (logError)
            {
                LogWrapper.LogError($"复制黑板时，没有找到变量! key={key}, valueType={typeof(T)}");
            }
            return false;
        }
        
        public bool CopyTo<T>(in VarEnv env, string key, string newKey)
        {
            if (env.HasVar<T>(newKey))
                return false;
            if (ReadVar<T>(key, out var value))
            {
                env.WriteVar<T>(newKey, value);
                return true;
            }
            LogWrapper.LogError($"复制黑板时，没有找到变量! key={key}, newKey={newKey}, valueType={typeof(T)}");
            return false;
        }
    }
}

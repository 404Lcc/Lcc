using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class ValueBlackboard
    {
        protected Dictionary<string, AnyValve> mVarDic = new Dictionary<string, AnyValve>();

        public void SetValve<T>(string ID, T value)
        {
            mVarDic[ID] = value switch
            {
                int i => i,
                long l => l,
                float f => f,
                bool b => b,
                Vector3 vec3 => vec3,
                Vector2 vec2 => vec2,
                Quaternion q => q,
                string str => str,
                _ => AnyValve.FromObject(value)
            };
        }

        public bool GetInt(string ID, out int getV, int defaultV = 0)
        {
            if (mVarDic.TryGetValue(ID, out var anyV))
            {
                getV = anyV.AsInt;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetLong(string ID, out long getV, long defaultV = 0)
        {
            if (mVarDic.TryGetValue(ID, out var anyV))
            {
                getV = anyV.AsLong;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetFloat(string ID, out float getV, float defaultV = 0f)
        {
            if (mVarDic.TryGetValue(ID, out var anyV))
            {
                getV = anyV.AsFloat;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetVector3(string ID, out Vector3 getV, Vector3 defaultV = default(Vector3))
        {
            if (mVarDic.TryGetValue(ID, out var anyV))
            {
                getV = anyV.AsVector3;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetVector2(string ID, out Vector2 getV, Vector2 defaultV = default(Vector2))
        {
            if (mVarDic.TryGetValue(ID, out var anyV))
            {
                getV = anyV.AsVector2;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetQuaternion(string ID, out Quaternion getV, Quaternion defaultV = default(Quaternion))
        {
            if (mVarDic.TryGetValue(ID, out var anyV))
            {
                getV = anyV.AsQuaternion;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetString(string ID, out string getV, string defaultV = null)
        {
            if (mVarDic.TryGetValue(ID, out var anyV))
            {
                getV = anyV.AsString;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool HasVar(string ID)
        {
            return mVarDic.ContainsKey(ID);
        }

        public void Clear()
        {
            mVarDic.Clear();
        }
    }
}
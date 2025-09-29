using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public readonly struct AnyValve
    {
        readonly long _long0;
        readonly float _float0;
        readonly float _float1;
        readonly float _float2;
        readonly float _float3;
        readonly object _object0;
        readonly int _index;

        AnyValve(int index, long i0 = 0, float f0 = 0, float f1 = 0, float f2 = 0, float f3 = 0, object obj0 = null)
        {
            _index = index;
            _long0 = i0;
            _float0 = f0;
            _float1 = f1;
            _float2 = f2;
            _float3 = f3;
            _object0 = obj0;
        }


        public int Index => _index;

        public const int IndexInt = 0;
        public const int IndexLong = 1;
        public const int IndexBool = 2;
        public const int IndexFloat = 3;
        public const int IndexVec3 = 4;
        public const int IndexVec2 = 5;
        public const int IndexQuaternion = 6;
        public const int IndexString = 7;
        public const int IndexObject = 8;

        public bool IsInt => _index == IndexInt;
        public bool IsLong => _index == IndexLong;
        public bool IsBool => _index == IndexBool;
        public bool IsFloat => _index == IndexFloat;
        public bool IsVector3 => _index == IndexVec3;
        public bool IsVector2 => _index == IndexVec2;
        public bool IsQuaternion => _index == IndexQuaternion;
        public bool IsString => _index == IndexString;
        public bool IsObject => _index == IndexObject;

        public int AsInt => IsInt ? (int)_long0 : throw new InvalidOperationException($"Cannot return as int _index={_index}");

        public long AsLong => IsLong ? _long0 : throw new InvalidOperationException($"Cannot return as long _index={_index}");

        public bool AsBool => IsBool ? _long0 != 0 : throw new InvalidOperationException($"Cannot return as bool _index={_index}");

        public float AsFloat => IsFloat ? _float0 : throw new InvalidOperationException($"Cannot return as float _index={_index}");

        public Vector3 AsVector3 => IsVector3 ? new Vector3(_float0, _float1, _float2) : throw new InvalidOperationException($"Cannot return as Vec3 _index={_index}");

        public Vector2 AsVector2 => IsVector2 ? new Vector2(_float0, _float1) : throw new InvalidOperationException($"Cannot return as Vec2 _index={_index}");

        public Quaternion AsQuaternion => IsQuaternion ? new Quaternion(_float0, _float1, _float2, _float3) : throw new InvalidOperationException($"Cannot return as Quaternion _index={_index}");

        public string AsString => IsString ? _object0 as string : throw new InvalidOperationException($"Cannot return as String _index={_index}");

        public object AsObject => IsObject ? _object0 : throw new InvalidOperationException($"Cannot return as Object _index={_index}");

        public static implicit operator AnyValve(int i) => new AnyValve(IndexInt, i0: i);
        public static implicit operator AnyValve(long l) => new AnyValve(IndexLong, i0: l);
        public static implicit operator AnyValve(bool b) => new AnyValve(IndexBool, i0: b ? 1 : 0);
        public static implicit operator AnyValve(float f) => new AnyValve(IndexFloat, f0: f);
        public static implicit operator AnyValve(Vector3 v) => new AnyValve(IndexVec3, f0: v.x, f1: v.y, f2: v.z);
        public static implicit operator AnyValve(Vector2 v) => new AnyValve(IndexVec2, f0: v.x, f1: v.y);
        public static implicit operator AnyValve(Quaternion q) => new AnyValve(IndexQuaternion, f0: q.x, f1: q.y, f2: q.z, f3: q.w);
        public static implicit operator AnyValve(string s) => new AnyValve(IndexString, obj0: s);

        public static AnyValve FromObject(object o)
        {
            return new AnyValve(IndexObject, obj0: o);
        }

        private const double TOLERANCE = 0.000001;

        bool Equals(AnyValve other) =>
            _index == other._index &&
            _index switch
            {
                IndexInt => _long0 == other._long0,
                IndexLong => _long0 == other._long0,
                IndexBool => _long0 == other._long0,
                IndexFloat => Math.Abs((double)_float0 - (double)other._float0) < TOLERANCE,
                IndexVec3 => (Math.Abs((double)_float0 - (double)other._float0) < TOLERANCE) && (Math.Abs((double)_float1 - (double)other._float1) < TOLERANCE) && (Math.Abs((double)_float2 - (double)other._float2) < TOLERANCE),
                IndexVec2 => (Math.Abs((double)_float0 - (double)other._float0) < TOLERANCE) && (Math.Abs((double)_float1 - (double)other._float1) < TOLERANCE),
                IndexQuaternion => (Math.Abs((double)_float0 - (double)other._float0) < TOLERANCE) && (Math.Abs((double)_float1 - (double)other._float1) < TOLERANCE) && (Math.Abs((double)_float2 - (double)other._float2) < TOLERANCE) && (Math.Abs((double)_float3 - (double)other._float3) < TOLERANCE),
                IndexString => Equals(_object0, other._object0),
                IndexObject => Equals(_object0, other._object0),
                _ => false
            };



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is AnyValve o && Equals(o);
        }

    }

    public class ValueBlackboard
    {
        protected Dictionary<string, AnyValve> _varDict = new Dictionary<string, AnyValve>();

        public void SetValve<T>(string ID, T value)
        {
            _varDict[ID] = value switch
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

        public bool GetInt(string id, out int getV, int defaultV = 0)
        {
            if (_varDict.TryGetValue(id, out var anyV))
            {
                getV = anyV.AsInt;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetLong(string id, out long getV, long defaultV = 0)
        {
            if (_varDict.TryGetValue(id, out var anyV))
            {
                getV = anyV.AsLong;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetFloat(string id, out float getV, float defaultV = 0f)
        {
            if (_varDict.TryGetValue(id, out var anyV))
            {
                getV = anyV.AsFloat;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetVector3(string id, out Vector3 getV, Vector3 defaultV = default(Vector3))
        {
            if (_varDict.TryGetValue(id, out var anyV))
            {
                getV = anyV.AsVector3;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetVector2(string id, out Vector2 getV, Vector2 defaultV = default(Vector2))
        {
            if (_varDict.TryGetValue(id, out var anyV))
            {
                getV = anyV.AsVector2;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetQuaternion(string id, out Quaternion getV, Quaternion defaultV = default(Quaternion))
        {
            if (_varDict.TryGetValue(id, out var anyV))
            {
                getV = anyV.AsQuaternion;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetString(string id, out string getV, string defaultV = null)
        {
            if (_varDict.TryGetValue(id, out var anyV))
            {
                getV = anyV.AsString;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool GetObject(string id, out object getV, object defaultV = null)
        {
            if (_varDict.TryGetValue(id, out var anyV))
            {
                getV = anyV.AsObject;
                return true;
            }

            getV = defaultV;
            return false;
        }

        public bool HasVar(string id)
        {
            return _varDict.ContainsKey(id);
        }

        public void Clear()
        {
            _varDict.Clear();
        }
    }
}
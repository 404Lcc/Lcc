using System.Runtime.InteropServices;
using UnityEngine;

namespace LccHotfix
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FloatIntUnion
    {
        [FieldOffset(0)]
        public float floatValue;
    
        [FieldOffset(0)]
        public uint intValue;
    }

    public struct SecureFloat
    {
        private uint _encryptedValue;
        private uint _key;

        public SecureFloat(float value, int key = 0)
        {
            if (key == 0)
            {
                key = UnityEngine.Random.Range(100000000, 999999999);
            }
            _key = (uint)key;
            _encryptedValue = Encrypt(value, _key);
        }

        public float Value => GetValue();

        public string DebugStr => $"Value={Value}, _encryptedValue={_encryptedValue}, _key={_key}";

        public bool IsApproximate(float v, float delta = 1f)
        {
            return Mathf.Abs(GetValue() - v) < delta;
        }
        
        public float GetValue()
        {
            uint decryptedInt = _encryptedValue ^ _key;
            FloatIntUnion union = new FloatIntUnion { intValue = decryptedInt };
            return union.floatValue;
        }

        public void SetValue(float value)
        {
            _encryptedValue = Encrypt(value, _key);
        }
        
        public void ChangeValue(float changeV)
        {
            SetValue(GetValue() + changeV);
        }

        private static uint Encrypt(float value, uint key)
        {
            FloatIntUnion union = new FloatIntUnion { floatValue = value };
            return union.intValue ^ key;
        }
        
        public static implicit operator float(SecureFloat secureFloat) => secureFloat.GetValue();
        //public static implicit operator SecureFloat(float value) => new SecureFloat(value);
    }
}
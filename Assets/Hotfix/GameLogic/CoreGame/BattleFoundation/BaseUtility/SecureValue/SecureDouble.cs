using System.Runtime.InteropServices;
using UnityEngine;

namespace LccHotfix
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DoubleLongUnion
    {
        [FieldOffset(0)]
        public double doubleValue;
    
        [FieldOffset(0)]
        public ulong longValue;
    }

    public struct SecureDouble
    {
        private ulong _encryptedValue;
        private ulong _key;

        public SecureDouble(double value, int key = 0)
        {
            if (key == 0)
            {
                key = UnityEngine.Random.Range(100000000, 999999999);
            }
            _key = (ulong)key;
            DoubleLongUnion union = new DoubleLongUnion { doubleValue = value };
            _encryptedValue =  union.longValue ^ _key;
        }

        public double Value => GetValue();

        public string DebugStr => $"Value={Value}, _encryptedValue={_encryptedValue}, _key={_key}";
        
        public double GetValue()
        {
            ulong decryptedInt = _encryptedValue ^ _key;
            DoubleLongUnion union = new DoubleLongUnion { longValue = decryptedInt };
            return union.doubleValue;
        }

        public void SetValue(double value)
        {
            DoubleLongUnion union = new DoubleLongUnion { doubleValue = value };
            _encryptedValue =  union.longValue ^ _key;
        }
        
        public void ChangeValue(double changeV)
        {
            SetValue(GetValue() + changeV);
        }
        
        public static implicit operator double(SecureDouble secureDouble) => secureDouble.GetValue();
        //public static implicit operator SecureDouble(double value) => new SecureDouble(value);
    }
}
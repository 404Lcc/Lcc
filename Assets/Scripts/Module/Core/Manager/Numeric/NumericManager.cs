using System.Collections.Generic;

namespace LccModel
{
    public class NumericManager : Singleton<NumericManager>
    {
        public Dictionary<int, long> numericDict = new Dictionary<int, long>();
        public long this[NumericType type]
        {
            get
            {
                return GetKey((int)type);
            }
            set
            {
                long oldValue = GetKey((int)type);
                if (oldValue == value)
                {
                    return;
                }
                numericDict[(int)type] = value;
                Update(type);
            }
        }
        private long GetKey(int key)
        {
            numericDict.TryGetValue(key, out long value);
            return value;
        }
        public int GetInt(NumericType type)
        {
            return (int)GetKey((int)type);
        }
        public float GetFloat(NumericType type)
        {
            return (float)GetKey((int)type) / 10000;
        }
        public long GetLong(NumericType type)
        {
            return GetKey((int)type);
        }
        public int GetInt(int type)
        {
            return (int)GetKey(type);
        }
        public float GetFloat(int type)
        {
            return (float)GetKey(type) / 10000;
        }
        public long GetLong(int type)
        {
            return GetKey(type);
        }
        public void Set(NumericType type, int value)
        {
            this[type] = value;
        }
        public void Set(NumericType type, float value)
        {
            this[type] = (int)(value * 10000);
        }
        public void Set(NumericType type, long value)
        {
            this[type] = value;
        }
        public void Update(NumericType type)
        {
            if (type < NumericType.Max)
            {
                return;
            }
            int final = (int)type / 10;
            int bas = final * 10 + 1;
            int add = final * 10 + 2;
            int pct = final * 10 + 3;
            int finalAdd = final * 10 + 4;
            int finalPct = final * 10 + 5;
            long oldValue = numericDict[final];
            long newValue = (long)(((GetKey(bas) + GetKey(add)) * (100 + GetFloat(pct)) / 100f + GetKey(finalAdd)) * (100 + GetFloat(finalPct)) / 100f * 10000);
            numericDict[final] = newValue;
            EventManager.Instance.Publish(new Numeric(type, oldValue, newValue));
        }
    }
}
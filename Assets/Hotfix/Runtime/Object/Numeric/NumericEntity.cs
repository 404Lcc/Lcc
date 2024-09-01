using System.Collections.Generic;

namespace LccHotfix
{
    public class NumericEntity : AObjectBase
    {
        public Dictionary<int, long> numericDict = new Dictionary<int, long>();
        public long this[int type]
        {
            get
            {
                return GetKey(type);
            }
            set
            {
                long oldValue = GetKey(type);
                if (oldValue == value)
                {
                    return;
                }
                numericDict[type] = value;
                Update(type);
            }
        }
        private long GetKey(int key)
        {
            numericDict.TryGetValue(key, out long value);
            return value;
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
        public void Set(int type, int value)
        {
            this[type] = value;
        }
        public void Set(int type, float value)
        {
            this[type] = (int)(value * 10000);
        }
        public void Set(int type, long value)
        {
            this[type] = value;
        }
        public void Update(int type)
        {
            if (type < NumericType.Max)
            {
                return;
            }
            int final = type / 10;
            int bas = final * 10 + 1;
            int add = final * 10 + 2;
            int pct = final * 10 + 3;
            int finalAdd = final * 10 + 4;
            int finalPct = final * 10 + 5;

            long newValue = (long)(((GetKey(bas) + GetKey(add)) * (100 + GetFloat(pct)) / 100f + GetKey(finalAdd)) * (100 + GetFloat(finalPct)) / 100f);
            this[final] = newValue;
        }
    }
}
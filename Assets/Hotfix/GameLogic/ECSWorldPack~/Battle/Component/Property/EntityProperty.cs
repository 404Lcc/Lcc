using System.Collections.Generic;

namespace LccHotfix
{
    //public enum PropertyType
    //{
    //    Invalid = 0,

    //    Stunable,//能不能被眩晕
    //    Moveable,//能不能移动
    //    Skillable,//能不能放技能
    //    Hitbackable,//能不能受击
    //    Visible,//能不能显示
    //    Blockable,//能不能被被碰撞
    //    Damageable,//能不能被伤害
    //    Targetable,//能不能主动碰撞
    //    Healable,//能不能回血
    //    Dieable,//能不能死亡

    //    IsAlive, //是否活着
    //    IsStuning,//是否眩晕

    //    BoolPropertyTypeMax,//end

    //    ValuePropertyType = 32,

    //    Attack,
    //    Defence,
    //    MaxHp,
    //    MoveSpeed,//移动速度
    //    CriticalRate,//暴击率
    //    CriticalDamage,//暴击伤害
    //    Cure,//治疗


    //    ValuePropertyTypeMax,//end
    //}

    public abstract class EntityProperty<T>
    {
        protected T _sum = default;
        protected Dictionary<int, T> _multiValue = new Dictionary<int, T>();

        public void Clear(int key)
        {
            OnClear(key);
        }

        public void Add(int key, T value)
        {
            OnAdd(key, value);
        }

        public void Minus(int key, T value)
        {
            OnMinus(key, value);
        }

        public void Set(int key, T value)
        {
            OnSet(key, value);
        }

        public T Get(int key)
        {
            var value = default(T);
            _multiValue.TryGetValue(key, out value);
            return value;
        }

        public void Set(T value)
        {
            _multiValue[0] = value;
            _sum = value;
        }

        public T Get()
        {
            return _sum;
        }

        protected virtual void OnSet(int key, T value)
        {
        }

        protected virtual void OnClear(int key)
        {
        }

        protected virtual void OnAdd(int key, T value)
        {
        }

        protected virtual void OnMinus(int key, T value)
        {
        }


    }

    public class FloatProperty : EntityProperty<float>
    {
        protected override void OnAdd(int key, float value)
        {
            if (!_multiValue.ContainsKey(key))
            {
                _multiValue.Add(key, 0);
            }

            _multiValue[key] += value;
            _sum += value;
        }

        protected override void OnMinus(int key, float value)
        {
            if (!_multiValue.ContainsKey(key))
            {
                _multiValue.Add(key, 0);
            }

            _multiValue[key] -= value;
            _sum -= value;
        }

        protected override void OnSet(int key, float value)
        {
            Clear(key);
            Add(key, value);
        }

        protected override void OnClear(int key)
        {
            var value = Get(key);
            _multiValue.Remove(key);
            _sum -= value;
        }

        public FloatProperty Clone()
        {
            var newProperty = new FloatProperty();

            foreach (var item in _multiValue)
            {
                newProperty._multiValue.Add(item.Key, item.Value);
            }

            newProperty._sum = _sum;

            return newProperty;
        }
    }

    public class BoolProperty : EntityProperty<bool>
    {
        protected override void OnSet(int key, bool value)
        {
            _multiValue[key] = value;
            if (key != 0)
            {
                bool ret = true;
                foreach (var v in _multiValue)
                {
                    ret &= v.Value;
                }

                _sum = ret;
            }
        }

        protected override void OnClear(int key)
        {
            _multiValue.Remove(key);
            if (key != 0)
            {
                bool ret = true;
                foreach (var v in _multiValue)
                {
                    ret &= v.Value;
                }

                _sum = ret;
            }
        }

        public BoolProperty Clone()
        {
            var newProperty = new BoolProperty();

            foreach (var item in _multiValue)
            {
                newProperty._multiValue.Add(item.Key, item.Value);
            }

            newProperty._sum = _sum;

            return newProperty;
        }
    }
}
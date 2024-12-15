using System.Collections.Generic;

namespace LccHotfix
{
    public class ComProperty : LogicComponent
    {
        private Dictionary<int, FloatProperty> _floatDict = new Dictionary<int, FloatProperty>();

        private Dictionary<int, BoolProperty> _boolDict = new Dictionary<int, BoolProperty>();

        public override void Dispose()
        {
            base.Dispose();
            _floatDict.Clear();
            _boolDict.Clear();
        }

        public void Init(float initHp, float initAttack, float initMoveSpeed)
        {
            _floatDict.Clear();
            _boolDict.Clear();

            SetDefaultFloat((int)PropertyType.MaxHp, initHp);
            SetDefaultFloat((int)PropertyType.Attack, initAttack);
            SetDefaultFloat((int)PropertyType.MoveSpeed, initMoveSpeed);
            SetDefaultBool((int)PropertyType.IsAlive, true);

        }

        public ComProperty Clone()
        {
            var newComProperty = new ComProperty();

            foreach (var item in _floatDict)
            {
                newComProperty._floatDict.Add(item.Key, item.Value.Clone());
            }

            foreach (var item in _boolDict)
            {
                newComProperty._boolDict.Add(item.Key, item.Value.Clone());
            }

            return newComProperty;
        }

        public void SetDefaultFloat(int key, float value)
        {
            if (!_floatDict.ContainsKey(key))
            {
                _floatDict.Add(key, new FloatProperty());
            }

            _floatDict[key].Set(value);
        }

        public void AddDefaultFloat(PropertyType key, float value)
        {
            if (_floatDict.TryGetValue((int)key, out var oldProp))
            {
                _floatDict[(int)key].Set(oldProp.Get(0) + value);
            }
        }

        public void MinusDefaultFloat(PropertyType key, float value)
        {
            if (_floatDict.TryGetValue((int)key, out var oldProp))
            {
                _floatDict[(int)key].Set(oldProp.Get(0) - value);
            }
        }

        public float GetFloat(PropertyType key, float defaultV = 0)
        {
            if (_floatDict.TryGetValue((int)key, out var prop))
            {
                return prop.Get();
            }
            return defaultV;
        }

        public float GetDefaultFloat(PropertyType key, float defaultV = 0)
        {
            if (_floatDict.TryGetValue((int)key, out var prop))
            {
                return _floatDict[(int)key].Get(0);
            }
            return defaultV;
        }

        public void SetFloat(PropertyType key, int subKey, float value)
        {
            SetFloat((int)key, subKey, value);
        }

        public void SetFloat(int key, int reasonKey, float value)
        {
            if (!_floatDict.ContainsKey(key))
            {
                _floatDict.Add(key, new FloatProperty());
            }

            _floatDict[key].Set(reasonKey, value);
        }

        public void AddFloat(int key, int reasonKey, float value)
        {
            if (!_floatDict.ContainsKey(key))
            {
                _floatDict.Add(key, new FloatProperty());
            }

            _floatDict[key].Add(reasonKey, value);
        }
        public void ClearFloat(int key, int reasonKey)
        {
            if (_floatDict.TryGetValue((int)key, out var prop))
            {
                prop.Clear(reasonKey);
            }
        }

        public void ClearFloat(PropertyType key, int reasonKey)
        {
            if (_floatDict.TryGetValue((int)key, out var prop))
            {
                prop.Clear(reasonKey);
            }
        }


        public void SetDefaultBool(int key, bool value)
        {
            if (!_boolDict.ContainsKey(key))
            {
                _boolDict.Add(key, new BoolProperty());
            }
            _boolDict[key].Set(value);
        }

        public bool GetBool(PropertyType key, bool defaultV = false)
        {
            if (_boolDict.TryGetValue((int)key, out var prop))
            {
                return prop.Get();
            }
            return defaultV;
        }

        public bool GetDefaultBool(PropertyType key, bool defaultV = false)
        {
            if (_boolDict.TryGetValue((int)key, out var prop))
            {
                return _boolDict[(int)key].Get(0);
            }
            return defaultV;
        }

        public void SetBool(int key, int reasonKey, bool value)
        {
            if (!_boolDict.ContainsKey(key))
            {
                _boolDict.Add(key, new BoolProperty());
            }

            _boolDict[key].Set(reasonKey, value);
        }
        public void ClearBool(int key, int reasonKey)
        {
            if (_boolDict.TryGetValue((int)key, out var prop))
            {
                prop.Clear(reasonKey);
            }
        }

        public float GetBaseAttack()
        {
            return _floatDict[(int)PropertyType.Attack].Get(0);
        }


        // bool values
        public bool isStunable => GetBool(PropertyType.Stunable, true);
        public bool isMovable => GetBool(PropertyType.Moveable, true);
        public bool isSkillable => GetBool(PropertyType.Skillable, true);
        public bool isHitbackable => GetBool(PropertyType.Hitbackable, true);
        public bool isBlockable => GetBool(PropertyType.Blockable, true);
        public bool isDamageable => GetBool(PropertyType.Damageable, true);
        public bool isTargetable => GetBool(PropertyType.Targetable, true);
        public bool isHealable => GetBool(PropertyType.Healable, true);
        public bool isDieable => GetBool(PropertyType.Dieable, true);
        public bool isAlive => GetBool(PropertyType.IsAlive, true);
        public bool isStuning => GetBool(PropertyType.IsStuning, false);


        // float values
        public float moveSpeed => GetFloat(PropertyType.MoveSpeed, 0);
        public float maxHP => GetFloat(PropertyType.MaxHp, 0);
        public float attack => GetFloat(PropertyType.Attack, 0);
        public float criticalRate => GetFloat(PropertyType.CriticalRate, 0);
        public float criticalDamage => GetFloat(PropertyType.CriticalDamage, 0);


    }




    public partial class LogicEntity
    {

        public ComProperty comProperty { get { return (ComProperty)GetComponent(LogicComponentsLookup.ComProperty); } }
        public bool hasComProperty { get { return HasComponent(LogicComponentsLookup.ComProperty); } }

        public ComProperty AddComProperty()
        {
            var index = LogicComponentsLookup.ComProperty;
            var component = (ComProperty)CreateComponent(index, typeof(ComProperty));
            AddComponent(index, component);
            return component;
        }
        public void ReplaceComProperty()
        {
            var index = LogicComponentsLookup.ComProperty;
            var component = (ComProperty)CreateComponent(index, typeof(ComProperty));
            ReplaceComponent(index, component);
        }

        public void SetProperty(PropertyType key, bool newV, int reasonKey = 0)
        {
            if (!hasComProperty)
            {
                AddComProperty();
            }
            comProperty.SetBool((int)key, reasonKey, newV);
        }

        public void SetProperty(PropertyType key, float newV, int reasonKey = 0)
        {
            if (!hasComProperty)
            {
                AddComProperty();
            }
            comProperty.SetFloat((int)key, reasonKey, newV);
        }
    }


    public sealed partial class LogicMatcher
    {

        private static Entitas.IMatcher<LogicEntity> _matcherComProperty;

        public static Entitas.IMatcher<LogicEntity> ComProperty
        {
            get
            {
                if (_matcherComProperty == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComProperty);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComProperty = matcher;
                }

                return _matcherComProperty;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComProperty;
    }
}
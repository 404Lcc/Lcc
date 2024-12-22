using System.Collections.Generic;

namespace LccHotfix
{
    public class ComProperty : LogicComponent
    {
        private Dictionary<PropertyType, FloatProperty> _floatDict = new Dictionary<PropertyType, FloatProperty>();

        private Dictionary<PropertyType, BoolProperty> _boolDict = new Dictionary<PropertyType, BoolProperty>();

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

            SetBaseFloat(PropertyType.MaxHp, initHp);
            SetBaseFloat(PropertyType.Attack, initAttack);
            SetBaseFloat(PropertyType.MoveSpeed, initMoveSpeed);



            SetBaseBool(PropertyType.Stunable, true);
            SetBaseBool(PropertyType.Moveable, true);
            SetBaseBool(PropertyType.Skillable, true);
            SetBaseBool(PropertyType.Hitbackable, true);
            SetBaseBool(PropertyType.Blockable, true);
            SetBaseBool(PropertyType.Damageable, true);
            SetBaseBool(PropertyType.Targetable, true);
            SetBaseBool(PropertyType.Healable, true);
            SetBaseBool(PropertyType.Dieable, true);
            SetBaseBool(PropertyType.IsAlive, true);
            SetBaseBool(PropertyType.IsStuning, false);

        }

        //Float
        public float GetBaseFloat(PropertyType key)
        {
            if (_floatDict.TryGetValue(key, out var prop))
            {
                return prop.Get(0);
            }
            return 0;
        }

        public void SetBaseFloat(PropertyType key, float value)
        {
            if (!_floatDict.ContainsKey(key))
            {
                _floatDict.Add(key, new FloatProperty());
            }

            _floatDict[key].Set(value);
        }

        public void AddBaseFloat(PropertyType key, float value)
        {
            if (_floatDict.TryGetValue(key, out var oldProp))
            {
                _floatDict[key].Set(oldProp.Get(0) + value);
            }
        }

        public void MinusBaseFloat(PropertyType key, float value)
        {
            if (_floatDict.TryGetValue(key, out var oldProp))
            {
                _floatDict[key].Set(oldProp.Get(0) - value);
            }
        }

        public float GetFloat(PropertyType key)
        {
            if (_floatDict.TryGetValue(key, out var prop))
            {
                return prop.Get();
            }
            return 0;
        }

        public void SetSubFloat(PropertyType key, int subKey, float value)
        {
            if (!_floatDict.ContainsKey(key))
            {
                _floatDict.Add(key, new FloatProperty());
            }

            _floatDict[key].Add(subKey, value);
        }
        public void ClearSubFloat(PropertyType key, int subKey)
        {
            if (_floatDict.TryGetValue(key, out var prop))
            {
                prop.Clear(subKey);
            }
        }




        //Bool
        public bool GetBaseBool(PropertyType key)
        {
            if (_boolDict.TryGetValue(key, out var prop))
            {
                return prop.Get(0);
            }
            return false;
        }

        public void SetBaseBool(PropertyType key, bool value)
        {
            if (!_boolDict.ContainsKey(key))
            {
                _boolDict.Add(key, new BoolProperty());
            }
            _boolDict[key].Set(value);
        }

        public bool GetBool(PropertyType key)
        {
            if (_boolDict.TryGetValue(key, out var prop))
            {
                return prop.Get();
            }
            return false;
        }



        public void SetSubBool(PropertyType key, int subKey, bool value)
        {
            if (!_boolDict.ContainsKey(key))
            {
                _boolDict.Add(key, new BoolProperty());
            }

            _boolDict[key].Set(subKey, value);
        }
        public void ClearBool(PropertyType key, int subKey)
        {
            if (_boolDict.TryGetValue(key, out var prop))
            {
                prop.Clear(subKey);
            }
        }

        public float GetBaseAttack()
        {
            return _floatDict[PropertyType.Attack].Get(0);
        }


        // bool values
        public bool isStunable => GetBool(PropertyType.Stunable);
        public bool isMovable => GetBool(PropertyType.Moveable);
        public bool isSkillable => GetBool(PropertyType.Skillable);
        public bool isHitbackable => GetBool(PropertyType.Hitbackable);
        public bool isBlockable => GetBool(PropertyType.Blockable);
        public bool isDamageable => GetBool(PropertyType.Damageable);
        public bool isTargetable => GetBool(PropertyType.Targetable);
        public bool isHealable => GetBool(PropertyType.Healable);
        public bool isDieable => GetBool(PropertyType.Dieable);
        public bool isAlive => GetBool(PropertyType.IsAlive);
        public bool isStuning => GetBool(PropertyType.IsStuning);


        // float values
        public float moveSpeed => GetFloat(PropertyType.MoveSpeed);
        public float maxHP => GetFloat(PropertyType.MaxHp);
        public float attack => GetFloat(PropertyType.Attack);
        public float criticalRate => GetFloat(PropertyType.CriticalRate);
        public float criticalDamage => GetFloat(PropertyType.CriticalDamage);


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

        public void SetProperty(PropertyType key, float newValue, int subKey = 0)
        {
            if (!hasComProperty)
            {
                AddComProperty();
            }
            comProperty.SetSubFloat(key, subKey, newValue);
        }

        public void SetProperty(PropertyType key, bool newValue, int subKey = 0)
        {
            if (!hasComProperty)
            {
                AddComProperty();
            }
            comProperty.SetSubBool(key, subKey, newValue);
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
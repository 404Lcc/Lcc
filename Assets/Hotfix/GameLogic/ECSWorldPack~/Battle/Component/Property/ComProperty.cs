using cfg;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ComProperty : LogicComponent
    {
        private Dictionary<ValuePropertyType, FloatProperty> _floatDict = new Dictionary<ValuePropertyType, FloatProperty>();

        private Dictionary<BoolPropertyType, BoolProperty> _boolDict = new Dictionary<BoolPropertyType, BoolProperty>();

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

            SetBaseFloat(ValuePropertyType.MaxHp, initHp);
            SetBaseFloat(ValuePropertyType.Attack, initAttack);
            SetBaseFloat(ValuePropertyType.MoveSpeed, initMoveSpeed);



            SetBaseBool(BoolPropertyType.Stunable, true);
            SetBaseBool(BoolPropertyType.Moveable, true);
            SetBaseBool(BoolPropertyType.Skillable, true);
            SetBaseBool(BoolPropertyType.Hitbackable, true);
            SetBaseBool(BoolPropertyType.Blockable, true);
            SetBaseBool(BoolPropertyType.Damageable, true);
            SetBaseBool(BoolPropertyType.Targetable, true);
            SetBaseBool(BoolPropertyType.Healable, true);
            SetBaseBool(BoolPropertyType.Dieable, true);
            SetBaseBool(BoolPropertyType.IsAlive, true);
            SetBaseBool(BoolPropertyType.IsStuning, false);

        }

        //Float
        public float GetBaseFloat(ValuePropertyType key)
        {
            if (_floatDict.TryGetValue(key, out var prop))
            {
                return prop.Get(0);
            }

            return 0;
        }

        public void SetBaseFloat(ValuePropertyType key, float value)
        {
            if (!_floatDict.ContainsKey(key))
            {
                _floatDict.Add(key, new FloatProperty());
            }

            _floatDict[key].Set(value);
        }

        public void AddBaseFloat(ValuePropertyType key, float value)
        {
            if (_floatDict.TryGetValue(key, out var oldProp))
            {
                _floatDict[key].Set(oldProp.Get(0) + value);
            }
        }

        public void MinusBaseFloat(ValuePropertyType key, float value)
        {
            if (_floatDict.TryGetValue(key, out var oldProp))
            {
                _floatDict[key].Set(oldProp.Get(0) - value);
            }
        }

        public float GetFloat(ValuePropertyType key)
        {
            if (_floatDict.TryGetValue(key, out var prop))
            {
                return prop.Get();
            }

            return 0;
        }

        public void SetSubFloat(ValuePropertyType key, int subKey, float value)
        {
            if (!_floatDict.ContainsKey(key))
            {
                _floatDict.Add(key, new FloatProperty());
            }

            _floatDict[key].Add(subKey, value);
        }

        public void ClearSubFloat(ValuePropertyType key, int subKey)
        {
            if (_floatDict.TryGetValue(key, out var prop))
            {
                prop.Clear(subKey);
            }
        }




        //Bool
        public bool GetBaseBool(BoolPropertyType key)
        {
            if (_boolDict.TryGetValue(key, out var prop))
            {
                return prop.Get(0);
            }

            return false;
        }

        public void SetBaseBool(BoolPropertyType key, bool value)
        {
            if (!_boolDict.ContainsKey(key))
            {
                _boolDict.Add(key, new BoolProperty());
            }

            _boolDict[key].Set(value);
        }

        public bool GetBool(BoolPropertyType key)
        {
            if (_boolDict.TryGetValue(key, out var prop))
            {
                return prop.Get();
            }

            return false;
        }



        public void SetSubBool(BoolPropertyType key, int subKey, bool value)
        {
            if (!_boolDict.ContainsKey(key))
            {
                _boolDict.Add(key, new BoolProperty());
            }

            _boolDict[key].Set(subKey, value);
        }

        public void ClearBool(BoolPropertyType key, int subKey)
        {
            if (_boolDict.TryGetValue(key, out var prop))
            {
                prop.Clear(subKey);
            }
        }

        public float GetBaseAttack()
        {
            return _floatDict[ValuePropertyType.Attack].Get(0);
        }


        // bool values
        public bool isStunable => GetBool(BoolPropertyType.Stunable);
        public bool isMovable => GetBool(BoolPropertyType.Moveable);
        public bool isSkillable => GetBool(BoolPropertyType.Skillable);
        public bool isHitbackable => GetBool(BoolPropertyType.Hitbackable);
        public bool isBlockable => GetBool(BoolPropertyType.Blockable);
        public bool isDamageable => GetBool(BoolPropertyType.Damageable);
        public bool isTargetable => GetBool(BoolPropertyType.Targetable);
        public bool isHealable => GetBool(BoolPropertyType.Healable);
        public bool isDieable => GetBool(BoolPropertyType.Dieable);
        public bool isAlive => GetBool(BoolPropertyType.IsAlive);
        public bool isStuning => GetBool(BoolPropertyType.IsStuning);


        // float values
        public float moveSpeed => GetFloat(ValuePropertyType.MoveSpeed);
        public float maxHP => GetFloat(ValuePropertyType.MaxHp);
        public float attack => GetFloat(ValuePropertyType.Attack);
        public float criticalRate => GetFloat(ValuePropertyType.CriticalRate);
        public float criticalDamage => GetFloat(ValuePropertyType.CriticalDamage);


    }




    public partial class LogicEntity
    {

        public ComProperty comProperty
        {
            get { return (ComProperty)GetComponent(LogicComponentsLookup.ComProperty); }
        }

        public bool hasComProperty
        {
            get { return HasComponent(LogicComponentsLookup.ComProperty); }
        }

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

        public void SetProperty(ValuePropertyType key, float newValue, int subKey = 0)
        {
            if (!hasComProperty)
            {
                AddComProperty();
            }

            comProperty.SetSubFloat(key, subKey, newValue);
        }

        public void SetProperty(BoolPropertyType key, bool newValue, int subKey = 0)
        {
            if (!hasComProperty)
            {
                AddComProperty();
            }

            comProperty.SetSubBool(key, subKey, newValue);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComPropertyIndex = new ComponentTypeIndex(typeof(ComProperty));
        public static int ComProperty => ComPropertyIndex.index;
    }
}
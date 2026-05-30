using UnityEngine;

namespace LccHotfix
{
    public class HpComponent : LogicComponent
    {
        private SecureDouble _hp;
        public double Hp { get { return _hp; } }
        
        private SecureDouble _maxHp;
        public double MaxHp { get { return _maxHp; } }

        public float HpPercent
        {
            get
            {
                if (MaxHp < 0)
                    return 1f;
                float percent = (float)(Hp / MaxHp);
                return Mathf.Clamp(percent, 0f, 1f);
            }
        }
        
        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
        }

        public void Init(double hp, double maxHp)
        {
            _hp = new SecureDouble(hp);
            _maxHp = new SecureDouble(maxHp);
        }
        
        public void SetHP(double newHp)
        {
            _hp.SetValue(newHp);
            _owner.ReplaceComponent(LogicComponentsLookup.ComHp, this);
        }

        public void SetMaxHP(double newMaxHp)
        {
            _maxHp.SetValue(newMaxHp);
            _owner.ReplaceComponent(LogicComponentsLookup.ComHp, this);
        }

        public void ChangeHP(double changeHp)
        {
            _hp.ChangeValue(changeHp);
            _owner.ReplaceComponent(LogicComponentsLookup.ComHp, this);
        }
    }

    public partial class LogicEntity
    {
        public HpComponent comHp
        {
            get { return (HpComponent)GetComponent(LogicComponentsLookup.ComHp); }
        }

        public bool hasComHp
        {
            get { return HasComponent(LogicComponentsLookup.ComHp); }
        }

        public void SetComHp(double hp, double maxHp)
        {
            var index = LogicComponentsLookup.ComHp;
            if (hasComHp)
            {
                comHp.SetHP(hp);
                comHp.SetMaxHP(maxHp);
            }
            else
            {
                var component = (HpComponent)CreateComponent(index, typeof(HpComponent));
                component.Init(hp, maxHp);
                AddComponent(index, component);
            }
        }
        
        public void RemoveComHp()
        {
            if (hasComHp)
            {
                RemoveComponent(LogicComponentsLookup.ComHp);
            }
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComHpIndex = new(typeof(HpComponent));
        public static int ComHp => _ComHpIndex.Index;
    }
}

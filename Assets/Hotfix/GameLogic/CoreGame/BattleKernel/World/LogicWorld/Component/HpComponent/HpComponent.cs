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
            var oldHp = Hp;
            _hp.SetValue(newHp);
            _owner.ReplaceComponent(LogicComponentsLookup.ComHp, this);
            NotifyHealthChanged();
            BattleDeathTransition.Handle(_owner, oldHp, newHp);
        }

        public void SetMaxHP(double newMaxHp)
        {
            _maxHp.SetValue(newMaxHp);
            _owner.ReplaceComponent(LogicComponentsLookup.ComHp, this);
            NotifyHealthChanged();
        }

        public void ChangeHP(double changeHp)
        {
            var oldHp = Hp;
            _hp.ChangeValue(changeHp);
            _owner.ReplaceComponent(LogicComponentsLookup.ComHp, this);
            NotifyHealthChanged();
            BattleDeathTransition.Handle(_owner, oldHp, Hp);
        }

        private void NotifyHealthChanged()
        {
            _owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.HealthTraceSink?.OnHealthChanged(_owner, Hp, MaxHp);
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

    /// <summary>
    /// 标记「本轮死亡流程已触发」，防止 Nt_Death / 死亡事件重入。
    /// 自爆等可在 HP&gt;0 时预先打上；濒死拦截回血或复活后清除。
    /// </summary>
    public class BattleDeathTriggeredComponent : LogicComponent
    {
    }

    public partial class LogicEntity
    {
        public BattleDeathTriggeredComponent comBattleDeathTriggered
        {
            get { return (BattleDeathTriggeredComponent)GetComponent(LogicComponentsLookup.ComBattleDeathTriggered); }
        }

        public bool hasComBattleDeathTriggered
        {
            get { return HasComponent(LogicComponentsLookup.ComBattleDeathTriggered); }
        }

        public void AddComBattleDeathTriggered()
        {
            var index = LogicComponentsLookup.ComBattleDeathTriggered;
            var component = (BattleDeathTriggeredComponent)CreateComponent(index, typeof(BattleDeathTriggeredComponent));
            AddComponent(index, component);
        }

        public void RemoveComBattleDeathTriggered()
        {
            if (hasComBattleDeathTriggered)
            {
                RemoveComponent(LogicComponentsLookup.ComBattleDeathTriggered);
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComBattleDeathTriggeredIndex = new(typeof(BattleDeathTriggeredComponent));
        public static int ComBattleDeathTriggered => _ComBattleDeathTriggeredIndex.Index;
    }
}

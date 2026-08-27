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
            HandleBattleDeathTransition(oldHp, newHp);
        }

        public void SetMaxHP(double newMaxHp)
        {
            _maxHp.SetValue(newMaxHp);
            _owner.ReplaceComponent(LogicComponentsLookup.ComHp, this);
        }

        public void ChangeHP(double changeHp)
        {
            var oldHp = Hp;
            _hp.ChangeValue(changeHp);
            _owner.ReplaceComponent(LogicComponentsLookup.ComHp, this);
            HandleBattleDeathTransition(oldHp, Hp);
        }

        //这是什么动机产生的？ 为什么Hp组件内要有这个
        private void HandleBattleDeathTransition(double oldHp, double newHp)
        {
            if (_owner == null)
            {
                return;
            }

            if (oldHp > 0 && newHp <= 0)
            {
                if (_owner.hasComBattleDeathTriggered)
                {
                    return;
                }

                _owner.AddComBattleDeathTriggered();
                _owner.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.DamagePolicyService?.DispatchTriggerDeath(_owner);
                var isIntercepted = new StandaloneEntityCmdPreHandler().PreHandleCommand(_owner, new EntityCommand
                {
                    CmdType = EntityCmdType.Nt_Death,
                });
                if (isIntercepted && _owner.hasComHp && _owner.comHp.Hp > 0 && _owner.hasComBattleDeathTriggered)
                {
                    _owner.RemoveComBattleDeathTriggered();
                }
                return;
            }

            if (oldHp <= 0 && newHp > 0 && _owner.hasComBattleDeathTriggered)
            {
                _owner.RemoveComBattleDeathTriggered();
            }
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

    //这个组件，又是什么思路？
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

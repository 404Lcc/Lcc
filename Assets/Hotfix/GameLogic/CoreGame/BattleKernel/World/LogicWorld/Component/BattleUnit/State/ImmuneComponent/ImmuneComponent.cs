using HotUpdate.Framework.PbCfg;
using PBConfig;
using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 免疫组件
    /// 其实是特化的属性组件，但是其中的免疫属性功能较为统一、数量可能会膨胀很多，而且具有较强的语义性，故单独拆出来
    /// </summary>
    public class ImmuneComponent : LogicComponent
    {
        protected readonly Dictionary<int, MultChangeBool_OR> _boolImmunes = new();
        protected readonly Dictionary<int, MultChangeInt_ADD> _permyriadImmunes = new();
        protected readonly Dictionary<int, MultChangeInt_ADD> _buffImmunes = new();

        // 布尔类型免疫效果
        public bool InstantKill => GetBoolImmune(BoolImmune.InstantKill); // 秒杀
        public bool SlowDown => GetBoolImmune(BoolImmune.SlowDown); // 减速
        public bool Stun => GetBoolImmune(BoolImmune.Stun); // 无法行动（眩晕等）
        public bool Pull => GetBoolImmune(BoolImmune.Pull); // 牵引
        public bool Teleport => GetBoolImmune(BoolImmune.Teleport); // 传送

        // 万分比类型免疫效果
        public int HitBack => GetPermyriadImmune(PermyriadImmune.HitBack); // 击退抗性

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
            _boolImmunes.Clear();
            _permyriadImmunes.Clear();
            _buffImmunes.Clear();
        }

        public void Init(TImmuneLogic immuneLogic)
        {
            AddLogic(immuneLogic, 0);
        }

        public void AddLogic(TImmuneLogic immuneLogic, int flag)
        {
            AddBoolImmune(BoolImmune.InstantKill, immuneLogic.ImmuneInstantKill, flag);
            AddBoolImmune(BoolImmune.SlowDown, immuneLogic.ImmuneSlowDown, flag);
            AddBoolImmune(BoolImmune.Stun, immuneLogic.ImmuneStun, flag);
            AddBoolImmune(BoolImmune.Pull, immuneLogic.ImmunePull, flag);
            AddBoolImmune(BoolImmune.Teleport, immuneLogic.ImmuneTeleport, flag);

            AddPermyriadImmune(PermyriadImmune.HitBack, immuneLogic.ImmuneHitBack, flag);

            foreach (var immuneBuff in immuneLogic.ImmuneBuff)
            {
                var buffCfg = PbCfg.GetData<TBuff>(immuneBuff.BuffId);
                if (buffCfg == null)
                {
                    continue;
                }
                AddBuffImmune(buffCfg.LogicID, immuneBuff.ImmuneRatio, flag);
            }
        }

        public void RemoveLogic(int flag)
        {
            RemoveBoolImmune(BoolImmune.InstantKill, flag);
            RemoveBoolImmune(BoolImmune.SlowDown, flag);
            RemoveBoolImmune(BoolImmune.Stun, flag);
            RemoveBoolImmune(BoolImmune.Pull, flag);
            RemoveBoolImmune(BoolImmune.Teleport, flag);

            RemovePermyriadImmune(PermyriadImmune.HitBack, flag);

            foreach (var buffId in _buffImmunes.Keys)
            {
                RemoveBuffImmune(buffId, flag);
            }
        }

        public bool GetBoolImmune(int boolImmuneType)
        {
            if (_boolImmunes.TryGetValue(boolImmuneType, out var multChange))
            {
                return multChange.Value;
            }
            return false;
        }

        public void AddBoolImmune(int boolImmuneType, bool value, int flag = 0)
        {
            if (!_boolImmunes.TryGetValue(boolImmuneType, out var multChange))
            {
                if (value == false)
                {
                    return;
                }
                multChange = new MultChangeBool_OR(false);
                _boolImmunes.Add(boolImmuneType, multChange);
            }
            multChange.AddChange(value, flag);
        }

        public void RemoveBoolImmune(int boolImmuneType, int flag = 0)
        {
            if (_boolImmunes.TryGetValue(boolImmuneType, out var multChange))
            {
                _boolImmunes.Remove(boolImmuneType);
            }
        }

        public int GetPermyriadImmune(int permyriadImmueType)
        {
            if (_permyriadImmunes.TryGetValue(permyriadImmueType, out var multChange))
            {
                return multChange.Value;
            }
            return 0;
        }

        public void AddPermyriadImmune(int permyriadImmueType, int value, int flag = 0)
        {
            if (!_permyriadImmunes.TryGetValue(permyriadImmueType, out var multChange))
            {
                if (value == 0)
                {
                    return;
                }
                multChange = new MultChangeInt_ADD(0);
                _permyriadImmunes.Add(permyriadImmueType, multChange);
            }
            multChange.AddChange(value, flag);
        }

        public void RemovePermyriadImmune(int permyriadImmueType, int flag = 0)
        {
            if (_permyriadImmunes.TryGetValue(permyriadImmueType, out var multChange))
            {
                _permyriadImmunes.Remove(permyriadImmueType);
            }
        }

        public int GetBuffImmune(int buffId)
        {
            if (_buffImmunes.TryGetValue(buffId, out var multChange))
            {
                return multChange.Value;
            }
            return 0;
        }

        public void AddBuffImmune(int buffId, int value, int flag = 0)
        {
            if (!_buffImmunes.TryGetValue(buffId, out var multChange))
            {
                if (value == 0)
                {
                    return;
                }
                multChange = new MultChangeInt_ADD(0);
                _buffImmunes.Add(buffId, multChange);
            }
            multChange.AddChange(value, flag);
        }

        public void RemoveBuffImmune(int buffId, int flag = 0)
        {
            if (_buffImmunes.TryGetValue(buffId, out var multChange))
            {
                _buffImmunes.Remove(buffId);
            }
        }

    }

    public partial class LogicEntity
    {
        public ImmuneComponent comImmune
        {
            get { return (ImmuneComponent)GetComponent(LogicComponentsLookup.ComImmune); }
        }

        public bool hasComImmune
        {
            get { return HasComponent(LogicComponentsLookup.ComImmune); }
        }

        public void AddComImmune(TImmuneLogic immuneLogic, int flag = 0)
        {
            var index = LogicComponentsLookup.ComImmune;
            if (hasComImmune)
            {
                comImmune.AddLogic(immuneLogic, flag);
            }
            else
            {
                var component = (ImmuneComponent)CreateComponent(index, typeof(ImmuneComponent));
                component.Init(immuneLogic);
                AddComponent(index, component);
            }
        }

        public void RemoveComImmune()
        {
            if (hasComImmune)
            {
                RemoveComponent(LogicComponentsLookup.ComHp);
            }
        }

        public bool IsImmuneToInstantKill => hasComImmune ? comImmune.InstantKill : false;
        public bool IsImmuneToSlowDown => hasComImmune ? comImmune.SlowDown : false;
        public bool IsImmuneToStun => hasComImmune ? comImmune.Stun : false;
        public bool IsImmuneToPull => hasComImmune ? comImmune.Pull : false;
        public bool IsImmuneToTeleport => hasComImmune ? comImmune.Teleport : false;

        public int ImmuneToHitBack => hasComImmune ? comImmune.HitBack : 0;

        public int GetBuffImmune(int buffId)
        {
            if (hasComImmune)
            {
                return comImmune.GetBuffImmune(buffId);
            }
            return 0;
        }

    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComImmuneIndex = new(typeof(ImmuneComponent));
        public static int ComImmune => _ComImmuneIndex.Index;
    }
}

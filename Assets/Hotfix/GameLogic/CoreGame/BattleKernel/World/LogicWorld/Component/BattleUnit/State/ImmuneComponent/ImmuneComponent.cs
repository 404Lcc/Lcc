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
        protected readonly Dictionary<int, MultChangeBool_OR> _boolImmunes = new(); // 布尔类型免疫效果，使用 OR 关系叠加
        protected readonly Dictionary<int, MultChangeInt_ADD> _permyriadImmunes = new(); // 万分比类型免疫效果，使用加法关系叠加
        protected readonly Dictionary<int, MultChangeInt_ADD> _buffImmunes = new(); // Buff免疫效果，使用加法关系叠加
        protected readonly Dictionary<int, MultChangeInt_ADD> _buffTagImmunes = new(); // Buff标签免疫效果，使用加法关系叠加

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
            _buffTagImmunes.Clear();
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
                multChange.RemoveChange(flag);
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
                multChange.RemoveChange(flag);
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
                multChange.RemoveChange(flag);
            }
        }

        public int GetBuffTagImmune(int buffTag)
        {
            var value = 0;
            foreach (var kv in _buffTagImmunes)
            {
                if ((kv.Key & buffTag) == kv.Key)
                {
                    value += kv.Value.Value;
                }
            }
            return value;
        }

        public void AddBuffTagImmune(int buffTag, int value, int flag = 0)
        {
            if (!_buffTagImmunes.TryGetValue(buffTag, out var multChange))
            {
                if (value == 0)
                {
                    return;
                }
                multChange = new MultChangeInt_ADD(0);
                _buffTagImmunes.Add(buffTag, multChange);
            }
            multChange.AddChange(value, flag);
        }

        public void RemoveBuffTagImmune(int buffTag, int flag = 0)
        {
            if (_buffTagImmunes.TryGetValue(buffTag, out var multChange))
            {
                multChange.RemoveChange(flag);
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

        public void AddComImmune()
        {
            var index = LogicComponentsLookup.ComImmune;
            if (!hasComImmune)
            {
                var component = (ImmuneComponent)CreateComponent(index, typeof(ImmuneComponent));
                AddComponent(index, component);
            }
        }

        public void RemoveComImmune()
        {
            if (hasComImmune)
            {
                RemoveComponent(LogicComponentsLookup.ComImmune);
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

        public int GetBuffTagImmune(int buffTag)
        {
            if (hasComImmune)
            {
                return comImmune.GetBuffTagImmune(buffTag);
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

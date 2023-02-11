using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Reflection;

namespace LccModel
{
    [LabelText("触发类型")]
    public enum EffectTriggerType
    {
        [LabelText("（空）")]
        None = 0,
        [LabelText("立即触发")]
        Instant = 1,
        [LabelText("条件触发")]
        Condition = 2,
        [LabelText("行动点触发")]
        Action = 3,
        [LabelText("间隔触发")]
        Interval = 4,
        [LabelText("在行动点且满足条件")]
        ActionCondition = 5,
    }
    [LabelText("触发条件")]
    public enum ConditionType
    {
        [LabelText("自定义条件")]
        CustomCondition = 0,
        [LabelText("当生命值低于x")]
        WhenHPLower = 1,
        [LabelText("当生命值低于百分比x")]
        WhenHPPctLower = 2,
        [LabelText("当x秒内没有受伤")]
        WhenInTimeNoDamage = 3,
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class EffectAttribute : Attribute
    {
        private readonly string _effectType;
        private readonly int _order;

        public EffectAttribute(string effectType, int order)
        {
            _effectType = effectType;
            _order = order;
        }

        public string EffectType => _effectType;
        public int Order => _order;
    }

    [Serializable]
    public abstract class Effect
    {
        private const string EffectTypeNameDefine = "(添加效果修饰)";

        [HideInInspector]
        public bool IsSkillEffect;

        [HideInInspector]
        public bool IsExecutionEffect;

        [HideInInspector]
        public bool IsItemEffect;

        [HideInInspector]
        public virtual string Label => "Effect";

        [ToggleGroup("Enabled", "$Label")]
        public bool Enabled;

        [HorizontalGroup("Enabled/Hor")]
        [ToggleGroup("Enabled"), HideIf("HideEffectTriggerType", true), HideLabel]
        public EffectTriggerType EffectTriggerType;

        [HorizontalGroup("Enabled/Hor")]
        [ToggleGroup("Enabled"), HideIf("IsSkillEffect", true), ShowIf("EffectTriggerType", EffectTriggerType.Condition), HideLabel]
        public ConditionType ConditionType;

        [HorizontalGroup("Enabled/Hor")]
        [ToggleGroup("Enabled"), HideIf("IsSkillEffect", true), ShowIf("EffectTriggerType", EffectTriggerType.Action), HideLabel]
        public ActionPointType ActionPointType;

        [HorizontalGroup("Enabled/Hor")]
        [ToggleGroup("Enabled"), HideIf("IsSkillEffect", true), ShowIf("EffectTriggerType", EffectTriggerType.Interval), SuffixLabel("毫秒", true), HideLabel]
        public string Interval;

        [ToggleGroup("Enabled"), HideIf("IsSkillEffect", true), LabelText("条件参数 x="), ShowIf("EffectTriggerType", EffectTriggerType.Condition)]
        public string ConditionParams;


        [ShowIf("@this.DecoratorList != null && this.DecoratorList.Count > 0")]
        [ToggleGroup("Enabled"), LabelText("效果修饰"), PropertyOrder(100)]
        [HideReferenceObjectPicker, ListDrawerSettings(DraggableItems = false)]
        public List<EffectDecorator> DecoratorList = new List<EffectDecorator>();

        [ToggleGroup("Enabled")]
        [HorizontalGroup("Enabled/Hor2", PaddingLeft = 20, PaddingRight = 20)]
        [HideLabel, OnValueChanged("AddEffect"), ValueDropdown("EffectTypeSelect"), PropertyOrder(101)]
        public string EffectTypeName = EffectTypeNameDefine;

        public bool HideEffectTriggerType
        {
            get
            {
                if (this is ActionControlEffect) return true;
                if (this is AttributeModifyEffect) return true;
                if (this is DamageBloodSuckEffect) return true;
                if (this is CustomEffect) return true;
                return IsSkillEffect || IsItemEffect;
            }
        }

        public IEnumerable<string> EffectTypeSelect()
        {
            var types = typeof(EffectDecorator).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(EffectDecorator).IsAssignableFrom(x))
                .Where(x => x.GetCustomAttribute<EffectDecorateAttribute>() != null)
                .OrderBy(x => x.GetCustomAttribute<EffectDecorateAttribute>().Order)
                .Select(x => x.GetCustomAttribute<EffectDecorateAttribute>().Label);
            var results = types.ToList();
            results.Insert(0, EffectTypeNameDefine);
            return results;
        }

        private void AddEffect()
        {
            if (EffectTypeName != EffectTypeNameDefine)
            {
                Type effectType = typeof(EffectDecorator).Assembly.GetTypes()
                    .Where(x => !x.IsAbstract)
                    .Where(x => typeof(EffectDecorator).IsAssignableFrom(x))
                    .Where(x => x.GetCustomAttribute<EffectDecorateAttribute>() != null)
                    .Where(x => x.GetCustomAttribute<EffectDecorateAttribute>().Label == EffectTypeName)
                    .FirstOrDefault();

                if (effectType != null)
                {
                    var effect = Activator.CreateInstance(effectType) as EffectDecorator;
                    effect.Enabled = true;
                    if (DecoratorList == null)
                    {
                        DecoratorList = new List<EffectDecorator>();
                    }
                    DecoratorList.Add(effect);
                }

                EffectTypeName = EffectTypeNameDefine;
            }
        }
    }
}
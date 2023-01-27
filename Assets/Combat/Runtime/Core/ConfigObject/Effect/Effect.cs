using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Reflection;

namespace LccModel
{
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

        [ToggleGroup("Enabled"), ShowIf("IsSkillEffect", true)]
        public AddSkillEffetTargetType AddSkillEffectTargetType;

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
        public string ConditionParam;

        //[ToggleGroup("Enabled"), LabelText("触发概率")]
        //public string TriggerProbability = "100%";

        //Effect是直接效果，效果修饰是基于直接效果的辅助效果
        [ShowIf("@this.Decorators != null && this.Decorators.Count > 0")]
        [ToggleGroup("Enabled"), LabelText("效果修饰"), PropertyOrder(100)]
        [HideReferenceObjectPicker, ListDrawerSettings(DraggableItems = false)]
        public List<EffectDecorator> Decorators = new List<EffectDecorator>();

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
                var effectType = typeof(EffectDecorator).Assembly.GetTypes()
                    .Where(x => !x.IsAbstract)
                    .Where(x => typeof(EffectDecorator).IsAssignableFrom(x))
                    .Where(x => x.GetCustomAttribute<EffectDecorateAttribute>() != null)
                    .Where(x => x.GetCustomAttribute<EffectDecorateAttribute>().Label == EffectTypeName)
                    .FirstOrDefault();

                if (effectType != null)
                {
                    var effect = Activator.CreateInstance(effectType) as EffectDecorator;
                    effect.Enabled = true;
                    if (Decorators == null) Decorators = new List<EffectDecorator>();
                    Decorators.Add(effect);
                }

                EffectTypeName = EffectTypeNameDefine;
            }
        }
    }
}
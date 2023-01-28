using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace LccModel
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class EffectDecorateAttribute : Attribute
    {
        private string _label;
        private int _order;
        public EffectDecorateAttribute(string label, int order)
        {
            _label = label;
            _order = order;
        }

        public string Label => _label;

        public int Order => _order;
    }

    /// <summary>
    /// Ğ§¹ûĞŞÊÎ
    /// </summary>
    [Serializable]
    public abstract class EffectDecorator
    {
        [HideInInspector]
        public virtual string Label => "Effect";

        [ToggleGroup("Enabled", "$Label")]
        public bool Enabled;
    }
}
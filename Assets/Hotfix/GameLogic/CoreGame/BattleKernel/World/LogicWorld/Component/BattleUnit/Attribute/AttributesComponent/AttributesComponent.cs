using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace LccHotfix
{
    public static class AttributeBool
    {
        public const int Moveable = 1; // 是否可以移动
        public const int CanCollision = 2; // 是否可以发生碰撞
        public const int Visible = 3;
        public const int Stunned = 4; // 是否处于不能行动状态（眩晕等）
        public const int CanBeTargeted = 5; // 是否可以被索敌
        public const int CanBeHurted = 6; // 是否可以被伤害
        public const int Cloaked = 7; // 是否隐身
        public const int CloakRevealed = 8; // 隐身是否被揭露
        public const int Flying = 9; // 是否处于飞行中
    }

    /// <summary>
    /// 实体属性组件：int/bool/double/float 走定长热桶，其余类型用稠密 TypeKey 字典。
    /// </summary>
    public sealed partial class AttributesComponent : LogicComponent
    {
        // Profiler 采样点：统计 GetAttribute / SetAttribute 开销
        private static readonly ProfilerMarker s_ComAttributesMarker = new("ComAttributes");
        
        private static readonly System.Type[] s_fastTypes =
        {
            typeof(int),
            typeof(bool),
            typeof(double),
            typeof(float),
        };
        protected readonly IAttributes[] _fastBuckets = new IAttributes[s_fastTypes.Length];
        // 非热类型分类器 <TypeKey, 属性表>；首次写入懒创建
        protected Dictionary<int, IAttributes> _classifier;

        // 进程内仅非热类型单调分配稠密 TypeKey
        private static int s_nextTypeKey = s_fastTypes.Length;
        private static class TypeKeyOf<T>
        {
            // -1 = 非热类型，走 _classifier
            public static readonly int FastSlot = ResolveFastSlot();
            // 仅 FastSlot < 0 时分配；热类型 Id 无意义
            public static readonly int Id = FastSlot >= 0
                ? -1
                : System.Threading.Interlocked.Increment(ref s_nextTypeKey) - 1;
            private static int ResolveFastSlot()
            {
                var type = typeof(T);
                var fastTypes = s_fastTypes;
                for (int i = 0; i < fastTypes.Length; i++)
                {
                    if (type == fastTypes[i]) return i;
                }
                return -1;
            }
        }


        /// <summary>从池租用 TMod 并注册属性，defaultValue 写入 DefaultValue。</summary>
        public bool SetAttribute<TValue, TMod>(int attrName, TValue defaultValue)
            where TMod : MultChangeValue<TValue>, new()
        {
            var mod = MultChangeValue<TValue>.Acquire<TMod>(defaultValue);
            return SetAttribute(attrName, mod);
        }

        // 注册已租用的修改器；同 key 已存在时归还池
        private bool SetAttribute<TValue>(int attrName, IModifyValue<TValue> multValue)
        {
            using (s_ComAttributesMarker.Auto())
            {
                var modifiers = getAttributesByType<TValue>();
                if (modifiers == null)
                {
                    modifiers = new AttributeModifiers<TValue>();
                    var slot = TypeKeyOf<TValue>.FastSlot;
                    if (slot >= 0)
                    {
                        _fastBuckets[slot] = modifiers;
                    }
                    else
                    {
                        _classifier ??= new Dictionary<int, IAttributes>();
                        _classifier.Add(TypeKeyOf<TValue>.Id, modifiers);
                    }
                }
                else if (modifiers.ContainsKey(attrName))
                {
                    if (multValue is IReference pooled)
                    {
                        ReferencePool.Release(pooled);
                    }
                    return false;
                }

                modifiers.Add(attrName, multValue);
                return true;
            }
        }


        public IModifyValue<TValue> GetAttribute<TValue>(int attrName, bool logError = true)
        {
            using (s_ComAttributesMarker.Auto())
            {
                var modifiers = getAttributesByType<TValue>(logError);
                if (modifiers != null && modifiers.TryGetValue(attrName, out var modifier))
                {
                    return modifier;
                }
                return null;
            }
        }

        /// <summary>读取属性当前值；缺失时返回 defaultValue。</summary>
        public TValue GetValue<TValue>(int attrName, TValue defaultValue = default, bool logError = true)
        {
            var modifiers = GetAttribute<TValue>(attrName, logError);
            if (modifiers != null)
            {
                return modifiers.Value;
            }

            return defaultValue;
        }

        /// <summary>读取属性默认值；缺失时返回 defaultValue。</summary>
        public TValue GetDefaultValue<TValue>(int attrName, TValue defaultValue = default, bool logError = true)
        {
            var modifiers = GetAttribute<TValue>(attrName, logError);
            if (modifiers != null)
            {
                return modifiers.DefaultValue;
            }

            return defaultValue;
        }

        /// <summary>尝试读取属性当前值；失败时写出 defaulValue。</summary>
        public bool TryGetValue<TValue>(int attrName, out TValue getValue, TValue defaulValue)
        {
            var modifier = GetAttribute<TValue>(attrName);
            if (modifier != null)
            {
                getValue = modifier.Value;
                return true;
            }

            getValue = defaulValue;
            return false;
        }

        /// <summary>尝试读取属性当前值；成功时写回 getValue。</summary>
        public bool TryGetValue<TValue>(int attrName, ref TValue getValue)
        {
            var modifier = GetAttribute<TValue>(attrName);
            if (modifier != null)
            {
                getValue = modifier.Value;
                return true;
            }

            return false;
        }

        /// <summary>对指定属性叠加一次修改（flag 标识来源）。</summary>
        public bool Modify<TValue>(int attrName, TValue getValue, int flag)
        {
            var modifier = GetAttribute<TValue>(attrName);
            if (modifier != null)
            {
                modifier.AddChange(getValue, flag);
                return true;
            }

            return false;
        }

        /// <summary>是否存在指定属性修改器。</summary>
        public bool Has<TValue>(int attrName)
        {
            var modifier = GetAttribute<TValue>(attrName, false);
            return modifier != null;
        }

        /// <summary>移除指定 flag 的修改项。</summary>
        public bool RemoveModify<TValue>(int attrName, int flag)
        {
            var modifier = GetAttribute<TValue>(attrName);
            if (modifier != null)
            {
                modifier.RemoveChange(flag);
                return true;
            }

            return false;
        }

        /// <summary>清空指定属性上的全部修改项。</summary>
        public void ClearModify<TValue>(int attrName)
        {
            var modifier = GetAttribute<TValue>(attrName);
            if (modifier != null)
            {
                modifier.Clear();
            }
        }

        /// <summary>清空各属性表内容并保留容器引用，供组件池复用。</summary>
        public void Clear()
        {
            var buckets = _fastBuckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i]?.Clear();
            }

            var classifier = _classifier;
            if (classifier == null)
            {
                return;
            }

            foreach (var pair in classifier)
            {
                pair.Value.Clear();
            }
        }

        private AttributeModifiers<TValue> getAttributesByType<TValue>(bool logError = false)
        {
            var slot = TypeKeyOf<TValue>.FastSlot;
            IAttributes attrs;
            if (slot >= 0)
            {
                attrs = _fastBuckets[slot];
            }
            else if (_classifier == null || !_classifier.TryGetValue(TypeKeyOf<TValue>.Id, out attrs))
            {
                attrs = null;
            }

            if (attrs == null)
            {
                if (logError)
                {
                    KLogger.LogError("getAttributesByType == null  " + typeof(TValue));
                }
                return null;
            }

            return attrs as AttributeModifiers<TValue>;
        }

        public interface IAttributes
        {
            System.Type AttributeType { get; }
            /// <summary>清空表内修改器，保留热槽数组与冷字典实例。</summary>
            void Clear();
        }

        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 单值类型属性修改器表：key 在 [0, FastKeyCapacity) 走定长数组，其余走冷字典；与将来 int 脏标记位宽对齐。
        /// </summary>
        public class AttributeModifiers<TValue> : IAttributes
        {
            // 热 key 容量；与 int 脏标记 32 位对齐，改容量需同步脏标记方案
            public const int FastKeyCapacity = 32;

            // 热槽懒分配；下标即 attrName，null 表示未占用
            private IModifyValue<TValue>[] _fastSlots = new IModifyValue<TValue>[FastKeyCapacity];
            // 冷 key 字典；仅 key 越界或未进热区时使用
            private Dictionary<int, IModifyValue<TValue>> _coldDict = new ();

            //////////////////////////////////////////////////////////////////////////
            // IAttributes:
            public System.Type AttributeType
            {
                get { return typeof(TValue); }
            }

            /// <summary>清空热槽与冷字典条目，将 IModifyValue 还回 ReferencePool，不释放底层容器。</summary>
            public void Clear()
            {
                var fastSlots = _fastSlots;
                if (fastSlots != null)
                {
                    for (int i = 0; i < fastSlots.Length; i++)
                    {
                        var value = fastSlots[i];
                        if (value == null)
                        {
                            continue;
                        }

                        // 池化修改器归还；非 IReference 仅摘引用
                        if (value is IReference pooled)
                        {
                            ReferencePool.Release(pooled);
                        }
                        fastSlots[i] = null;
                    }
                }

                var coldDict = _coldDict;
                if (coldDict == null)
                {
                    return;
                }

                foreach (var pair in coldDict)
                {
                    if (pair.Value is IReference pooled)
                    {
                        ReferencePool.Release(pooled);
                    }
                }
                coldDict.Clear();
            }

            //////////////////////////////////////////////////////////////////////////
            // This:
            /// <summary>是否已注册指定 attrName 的修改器。</summary>
            public bool ContainsKey(int key)
            {
                if ((uint)key < FastKeyCapacity)
                {
                    return _fastSlots != null && _fastSlots[key] != null;
                }

                return _coldDict != null && _coldDict.ContainsKey(key);
            }

            /// <summary>按 attrName 取修改器；未注册时返回 false。</summary>
            public bool TryGetValue(int key, out IModifyValue<TValue> value)
            {
                if ((uint)key < FastKeyCapacity)
                {
                    if (_fastSlots != null)
                    {
                        value = _fastSlots[key];
                        return value != null;
                    }

                    value = null;
                    return false;
                }

                if (_coldDict != null)
                {
                    return _coldDict.TryGetValue(key, out value);
                }

                value = null;
                return false;
            }

            /// <summary>注册修改器；同 key 已存在时抛 ArgumentException（对齐 Dictionary.Add）。</summary>
            public void Add(int key, IModifyValue<TValue> value)
            {
                if ((uint)key < FastKeyCapacity)
                {
                    _fastSlots ??= new IModifyValue<TValue>[FastKeyCapacity];
                    if (_fastSlots[key] != null)
                    {
                        throw new ArgumentException("An item with the same key has already been added.");
                    }

                    _fastSlots[key] = value;
                    return;
                }

                _coldDict ??= new Dictionary<int, IModifyValue<TValue>>();
                _coldDict.Add(key, value);
            }
        }
    }


    //////////////////////////////////////////////////////////////////////////
    public partial class LogicEntity
    {
        public AttributesComponent comAttributes
        {
            get { return (AttributesComponent)GetComponent(LogicComponentsLookup.ComAttributes); }
        }

        public bool hasComAttributes
        {
            get { return HasComponent(LogicComponentsLookup.ComAttributes); }
        }

        public AttributesComponent AddComAttributes()
        {
            var index = LogicComponentsLookup.ComAttributes;
            var component = (AttributesComponent)CreateComponent(index, typeof(AttributesComponent));
            component.Clear();

            // 默认的布尔状态需要在这里定义
            component.SetAttribute<bool, MultChangeBool_AND>(AttributeBool.Moveable, true);
            component.SetAttribute<bool, MultChangeBool_AND>(AttributeBool.CanCollision, true);
            component.SetAttribute<bool, MultChangeBool_AND>(AttributeBool.Visible, true);
            component.SetAttribute<bool, MultChangeBool_OR>(AttributeBool.Stunned, false);
            component.SetAttribute<bool, MultChangeBool_AND>(AttributeBool.CanBeHurted, true);
            component.SetAttribute<bool, MultChangeBool_AND>(AttributeBool.CanBeTargeted, true);
            component.SetAttribute<bool, MultChangeBool_OR>(AttributeBool.Cloaked, false);
            component.SetAttribute<bool, MultChangeBool_OR>(AttributeBool.CloakRevealed, false);
            component.SetAttribute<bool, MultChangeBool_OR>(AttributeBool.Flying, false);

            AddComponent(index, component);
            return component;
        }

        public void GetTotalPropertySnapshot(out PropertySnapshot snapshot)
        {
            snapshot = new PropertySnapshot();
            snapshot.FillFromEntity(this);
        }
    }
    

    //////////////////////////////////////////////////////////////////////////
    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComAttributesIndex = new(typeof(AttributesComponent));
        public static int ComAttributes => _ComAttributesIndex.Index;
    }
}

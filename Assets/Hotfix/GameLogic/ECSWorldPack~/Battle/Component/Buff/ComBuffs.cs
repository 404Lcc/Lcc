using cfg;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ComBuffs : LogicComponent
    {
        private Dictionary<int, BuffState> _buffDict = new Dictionary<int, BuffState>();

        public Dictionary<int, BuffState> BuffDict => _buffDict;

        public override void PostInitialize(LogicEntity owner)
        {
            base.PostInitialize(owner);

            _buffDict.Clear();
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var item in _buffDict.Values)
            {
                item.IsActive = false;
                //手动走一下，以防sys也删了
                item.LeaveState();
            }

            _buffDict.Clear();

        }

        public Buff GetBuffConfig(int buffId)
        {
            var config = Main.ConfigService.Tables.TBBuff.Get(buffId);
            return config;
        }

        public float GetBuffDuring(int buffId)
        {
            var config = GetBuffConfig(buffId);
            if (config == null)
                return -1;
            return config.During;
        }

        public bool HasBuff(int buffId)
        {
            if (_buffDict.TryGetValue(buffId, out var buffState))
            {
                return buffState.IsActive;
            }

            return false;
        }

        public BuffState GetBuff(int buffId)
        {
            if (_buffDict.TryGetValue(buffId, out var buffState))
            {
                if (buffState.IsActive)
                {
                    return buffState;
                }
            }

            return null;
        }

        public void AddBuff(int buffId, int fromLogicID, long fromEntityID, long toEntityID, float during = -1, KVContext context = null)
        {
            var entity = EntityUtility.GetEntity(toEntityID);
            if (entity == null)
                return;

            if (!entity.hasComBuffs)
                return;

            var config = GetBuffConfig(buffId);
            if (config == null)
                return;

            var comBuffs = entity.comBuffs;

            // Buff被其它buff免疫，此次加buff失败

            foreach (var item in comBuffs.BuffDict.Values)
            {
                var cfg = item.BuffConfig;

                // 免疫buff id组
                if (cfg.ImmuneBuffGroupIdList.Contains(config.BuffGroupId))
                {
                    return;
                }

                // 免疫buff 布尔效果
                if ((cfg.ImmuneBoolBuffType & config.BoolBuffType) != 0)
                {
                    return;
                }

                // 免疫buff 数值效果
                foreach (var immuneValueBuffType in cfg.ImmuneValueBuffTypeList)
                {
                    foreach (var valueBuff in config.ValueBuffType)
                    {
                        if (valueBuff.Type == immuneValueBuffType)
                        {
                            return;
                        }
                    }
                }
            }


            var level = 1;
            var maxLevel = config.MaxLevel;
            BuffState buffState = null;
            if (_buffDict.ContainsKey(buffId))
            {
                buffState = _buffDict[buffId];
            }

            if (buffState != null)
            {
                level = buffState.Level;
                if (level < maxLevel)
                {
                    level++;
                }
            }

            if (during == -1)
            {
                during = config.During;
            }

            var buffGenInfo = new BuffInfo()
            {
                buffId = buffId,
                fromEntityId = fromEntityID,
                entityId = entity.comID.id,
                fromLogicId = fromLogicID,
                during = during,
                level = level,
                maxLevel = maxLevel,
                btScript = config.BtScript,
                context = context,
            };

            if (config.StackType == StackType.AddSubBuff)
            {
            }
            else if (config.StackType == StackType.AddLevel)
            {
                if (buffState != null)
                {
                    buffState.UpdateLevel(buffGenInfo);
                }
                else
                {
                    buffState = new BuffState();
                    buffState.Init(buffGenInfo);
                    buffState.EnterState();
                    _buffDict.Add(buffId, buffState);
                }
            }

            // 驱散已经存在的Buff
            foreach (var item in comBuffs.BuffDict.Values)
            {
                var cfg = item.BuffConfig;

                // 不可被驱散
                if (!cfg.IsDisperse)
                    continue;

                // 驱散buff buff组
                if (config.DisperseBuffGroupIdList.Contains(cfg.BuffGroupId))
                {
                    comBuffs.RemoveBuff(item.BuffId);
                    continue;
                }

                // 驱散buff 布尔效果
                if ((config.DisperseBoolBuffType & cfg.BoolBuffType) != 0)
                {
                    comBuffs.RemoveBuff(item.BuffId);
                    continue;
                }

                // 驱散buff 数值效果

                foreach (var valueBuff in config.ValueBuffType)
                {
                    foreach (var disperseValueBuffType in cfg.DisperseValueBuffTypeList)
                    {
                        if (valueBuff.Type == disperseValueBuffType)
                        {
                            comBuffs.RemoveBuff(item.BuffId);
                            continue;
                        }
                    }
                }
            }
        }

        public void RemoveBuff(int buffId)
        {
            if (_buffDict.TryGetValue(buffId, out var buffState))
            {
                _buffDict[buffId].IsActive = false;
            }
        }
    }


    public partial class LogicEntity
    {
        public ComBuffs comBuffs
        {
            get { return (ComBuffs)GetComponent(LogicComponentsLookup.ComBuffs); }
        }

        public bool hasComBuffs
        {
            get { return HasComponent(LogicComponentsLookup.ComBuffs); }
        }

        public void AddComBuffs()
        {
            var index = LogicComponentsLookup.ComBuffs;
            var component = (ComBuffs)CreateComponent(index, typeof(ComBuffs));
            AddComponent(index, component);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComBuffsIndex = new ComponentTypeIndex(typeof(ComBuffs));
        public static int ComBuffs => ComBuffsIndex.index;
    }
}
namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 战斗状态、有效性和目标资格相关扩展。
    /// </summary>
    public static class LogicEntityStateExtensions
    {
        /// <summary>
        /// 获取实体日志标识，优先输出子物体或战斗单位配置 ID。
        /// </summary>
        public static string GetLogStr(this LogicEntity entity)
        {
            if (entity == null)
            {
                return "null";
            }

            if (entity.hasComSubobject)
            {
                return $"(ID:{entity.ID},Subobj:{entity.comSubobject.ConfigId})";
            }

            if (entity.hasComBattleUnitTag)
            {
                return $"(ID:{entity.ID},BattleUnit:{entity.comBattleUnitTag.Tag.BattleUnitTid})";
            }

            return $"{entity.ID}";
        }

        /// <summary>
        /// 判断实体是否可参与常规战斗流程，要求未死亡并具备属性和视图组件。
        /// </summary>
        public static bool IsValid(this LogicEntity entity)
        {
            if (entity.IsDead())
            {
                return false;
            }

            if (!entity.hasComAttributes)
            {
                return false;
            }

            if (!entity.hasComView)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断实体是否可作为索敌或技能目标。
        /// </summary>
        public static bool IsValidTarget(this LogicEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (!entity.hasComHp)
            {
                return false;
            }

            if (entity.hasComDeath)
            {
                return false;
            }

            if (!entity.CanBeTarget())
            {
                return false;
            }

            if (!entity.hasComView)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试获取战斗单位标签信息。
        /// </summary>
        public static bool GetBattleUnitTag(this LogicEntity entity, out BattleUnitTag unitTag)
        {
            if (entity.hasComBattleUnitTag)
            {
                unitTag = entity.comBattleUnitTag.Tag;
                return true;
            }

            BattleLogger.LogError("GetBattleUnitTag !entity.hasComBattleUnitTag");
            unitTag = default;
            return false;
        }

        /// <summary>
        /// 判断实体是否处于隐身且未被反隐揭示状态。
        /// </summary>
        public static bool IsCloaked(this LogicEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (!entity.hasComAttributes)
            {
                return false;
            }

            return entity.GetAttributeBool(AttributeBool.Cloaked, false) && !entity.GetAttributeBool(AttributeBool.CloakRevealed, false);
        }

        /// <summary>
        /// 判断实体是否已死亡、被回收或血量归零。
        /// </summary>
        public static bool IsDead(this LogicEntity entity)
        {
            if (entity == null)
            {
                return true;
            }

            if (!entity.isEnabled)
            {
                BattleLogger.LogError($"IsDead found recycled entity, id={(entity.hasComID ? entity.ID : 0)}, creationIndex={entity.creationIndex}");
                return true;
            }

            if (entity.hasComDeath)
            {
                return true;
            }

            if (entity.hasComHp && entity.comHp.Hp <= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断实体对象是否存在且仍处于启用状态。
        /// </summary>
        public static bool IsEnabled(this LogicEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return entity.isEnabled;
        }

        /// <summary>
        /// 判断实体是否仍是指定创建序号对应的有效实体，避免误用已回收实体。
        /// </summary>
        public static bool IsEnabled(this LogicEntity entity, int creationIndex)
        {
            if (entity == null)
            {
                return false;
            }

            if (!entity.isEnabled)
            {
                return false;
            }

            if (entity.creationIndex != creationIndex)
            {
                BattleLogger.LogError($"entity.IsEnabled found entity.creationIndex({entity.creationIndex}) != creationIndex({creationIndex})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断实体是否允许参与碰撞检测。
        /// </summary>
        public static bool CanCollider(this LogicEntity entity)
        {
            if (entity.hasComAttributes)
            {
                var comAttributes = entity.comAttributes;
                if (!comAttributes.GetValue<bool>(AttributeBool.CanCollision, false))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断实体是否允许被选为目标。
        /// </summary>
        public static bool CanBeTarget(this LogicEntity entity)
        {
            if (entity.hasComAttributes)
            {
                var comAttributes = entity.comAttributes;
                if (!comAttributes.GetValue<bool>(AttributeBool.CanBeTargeted, false))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断实体是否允许受到伤害。
        /// </summary>
        public static bool CanBeHurt(this LogicEntity entity)
        {
            if (entity.hasComAttributes)
            {
                var comAttributes = entity.comAttributes;
                if (!comAttributes.GetValue<bool>(AttributeBool.CanBeHurted, false))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

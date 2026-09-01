namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 生命/有效性相关扩展。
    /// </summary>
    public static class LogicEntityLifeExtensions
    {
        /// <summary>
        /// 判断实体是否可参与常规战斗流程，要求未死亡并具备属性和视图组件。
        /// </summary>
        public static bool IsValid(this LogicEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (!entity.isEnabled)
            {
                return false;
            }

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
        public static bool IsValidTarget(this LogicEntity entity, bool AA = false)
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

            if (!entity.CanBeTarget(AA))
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
    }
}

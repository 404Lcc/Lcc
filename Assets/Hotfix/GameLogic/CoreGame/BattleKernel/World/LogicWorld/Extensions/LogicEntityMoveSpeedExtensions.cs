using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 有效移速相关扩展（Kernel，仅属性公式）。
    /// </summary>
    public static class LogicEntityMoveSpeedExtensions
    {
        /// <summary>
        /// 与 BattleGameplayConfig.PLAYER_BASE_SPEED 同值；Kernel 不引用 Gameplay 配置。
        /// </summary>
        private const float DefaultMoveSpeed = 2.2f;

        /// <summary>
        /// 有效移速：MoveSpeed × MoveSpeedRatio × (1 + GlobalMoveSpeedAdd)。
        /// 小队队员同源由 SquadFollowBhv 等 Gameplay 逻辑负责，本方法不含小队语义。
        /// </summary>
        public static float GetEffectiveMoveSpeed(this LogicEntity entity)
        {
            if (entity == null || !entity.hasComAttributes)
            {
                return DefaultMoveSpeed;
            }

            var moveSpeed = entity.GetAttributeFloat(PropertyFloat.MoveSpeed, DefaultMoveSpeed);
            var moveSpeedRatio = entity.GetAttributeFloat(PropertyFloat.MoveSpeedRatio, 1f);
            var globalMoveSpeedAdd = entity.GetPlayerInfo()?.FeaturesContext?.GlobalMoveSpeedAddPercent ?? 0f;
            return moveSpeed * moveSpeedRatio * (1f + Mathf.Max(0f, globalMoveSpeedAdd));
        }
    }
}

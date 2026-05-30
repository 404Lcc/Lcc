using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 逻辑 Transform 与主视图 Transform 同步相关扩展。
    /// </summary>
    public static class LogicEntityTransformExtensions
    {
        /// <summary>
        /// 设置实体逻辑位置，并同步主视图世界坐标。
        /// </summary>
        public static void SetPosition(this LogicEntity entity, Vector3 pos)
        {
            if (entity.hasComTransform)
            {
                entity.comTransform.SetPosition(pos);
            }

            if (entity.hasComView)
            {
                var actorView = entity.comView.MainActorView<MainGameObjectView>();
                if (actorView != null)
                {
                    actorView.Transform.position = pos;
                }
            }
        }

        /// <summary>
        /// 按方向和距离对目标应用击退，并受目标击退免疫属性影响。
        /// </summary>
        public static void ApplyHitBack(this LogicEntity target, Vector3 dir, float dist, float effectAdd = 0f)
        {
            if (target == null || !target.hasComTransform)
            {
                return;
            }

            const int PermyriadFull = 10000;
            if (target.ImmuneToHitBack >= PermyriadFull)
            {
                return;
            }

            float pushDist = dist * (1f - target.ImmuneToHitBack / (float)PermyriadFull);
            if (pushDist <= 0)
            {
                return;
            }

            pushDist *= 1f + effectAdd;

            if (dir.sqrMagnitude > 0.001f)
            {
                dir = dir.normalized;
            }

            target.SetPosition(target.position + dir * pushDist);
        }

        /// <summary>
        /// 设置实体逻辑缩放和朝向，并同步主视图本地缩放。
        /// </summary>
        public static void SetScale(this LogicEntity entity, Vector3 scale, int dirX)
        {
            if (entity.hasComTransform)
            {
                entity.comTransform.SetScale(scale);
                entity.comTransform.SetDirX(dirX);
            }

            if (entity.hasComView)
            {
                var actorView = entity.comView.MainActorView<MainGameObjectView>();
                if (actorView != null)
                {
                    actorView.Transform.localScale = new Vector3(scale.x * dirX, scale.y, scale.z);
                }
            }
        }

        /// <summary>
        /// 设置实体水平朝向，1 表示右，-1 表示左。
        /// </summary>
        public static void SetDir(this LogicEntity entity, int dirX)
        {
            Vector3 scale = new Vector3(Mathf.Abs(entity.scale.x), entity.scale.y, entity.scale.z);
            if (entity.hasComTransform)
            {
                entity.comTransform.SetDirX(dirX);
                entity.comTransform.SetScale(new Vector3(scale.x * dirX, scale.y, scale.z));
            }

            if (entity.hasComView)
            {
                var actorView = entity.comView.MainActorView<MainGameObjectView>();
                if (actorView != null)
                {
                    actorView.Transform.localScale = new Vector3(scale.x * dirX, scale.y, scale.z);
                }
            }
        }

        /// <summary>
        /// 获取实体当前水平朝向，缺省返回 1。
        /// </summary>
        public static int GetDir(this LogicEntity entity)
        {
            if (entity.hasComTransform)
            {
                return entity.comTransform.GetDir();
            }

            return 1;
        }

        /// <summary>
        /// 按是否朝左设置实体水平朝向。
        /// </summary>
        public static void SetDir(this LogicEntity entity, bool isLeft)
        {
            if (entity.hasComTransform)
            {
                entity.SetDir(isLeft ? -1 : 1);
            }
        }

        /// <summary>
        /// 根据目标位置调整实体朝向。
        /// </summary>
        public static void LookAt(this LogicEntity entity, Vector3 target)
        {
            if (entity.hasComTransform)
            {
                bool isLeft = entity.comTransform.position.x >= target.x;
                entity.SetDir(isLeft ? -1 : 1);
            }
        }

        /// <summary>
        /// 设置实体逻辑旋转，并同步主视图旋转。
        /// </summary>
        public static void SetQuaternion(this LogicEntity entity, Quaternion quaternion)
        {
            if (entity.hasComTransform)
            {
                entity.comTransform.SetRotation(quaternion);
            }

            if (entity.hasComView)
            {
                var actorView = entity.comView.MainActorView<MainGameObjectView>();
                if (actorView != null)
                {
                    actorView.Transform.rotation = quaternion;
                }
            }
        }
    }
}

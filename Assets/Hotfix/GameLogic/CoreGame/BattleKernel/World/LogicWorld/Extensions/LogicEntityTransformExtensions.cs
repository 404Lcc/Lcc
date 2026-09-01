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

        /// <summary>
        /// 设置实体逻辑缩放和朝向，并同步主视图本地缩放。
        /// </summary>
        public static void SetScale(this LogicEntity entity, Vector3 scale)
        {
            if (entity.hasComTransform)
            {
                entity.comTransform.SetScale(scale);
            }

            if (entity.hasComView)
            {
                var actorView = entity.comView.MainActorView<MainGameObjectView>();
                if (actorView != null)
                {
                    actorView.Transform.localScale = scale;
                }
            }
        }

        /// <summary>
        /// 设置实体逻辑缩放和朝向，并同步主视图本地缩放。
        /// </summary>
        public static void SetScale2D(this LogicEntity entity, Vector3 scale, int dir2D)
        {
            if (entity.hasComTransform)
            {
                entity.comTransform.SetScale(scale);
                entity.comTransform.SetDir2D(dir2D);
            }

            if (entity.hasComView)
            {
                var actorView = entity.comView.MainActorView<MainGameObjectView>();
                if (actorView != null)
                {
                    actorView.Transform.localScale = new Vector3(scale.x * dir2D, scale.y, scale.z);
                }
            }
        }

        /// <summary>
        /// 设置实体水平朝向，1 表示右，-1 表示左。
        /// </summary>
        public static void SetDir2D(this LogicEntity entity, int dir2D)
        {
            Vector3 scale = new Vector3(Mathf.Abs(entity.scale.x), entity.scale.y, entity.scale.z);
            if (entity.hasComTransform)
            {
                entity.comTransform.SetDir2D(dir2D);
                entity.comTransform.SetScale(new Vector3(scale.x * dir2D, scale.y, scale.z));
            }

            if (entity.hasComView)
            {
                var actorView = entity.comView.MainActorView<MainGameObjectView>();
                if (actorView != null)
                {
                    actorView.Transform.localScale = new Vector3(scale.x * dir2D, scale.y, scale.z);
                }
            }
        }

        /// <summary>
        /// 获取实体当前水平朝向，缺省返回 1。
        /// </summary>
        public static int GetDir2D(this LogicEntity entity)
        {
            if (entity.hasComTransform)
            {
                return entity.comTransform.GetDir2D();
            }

            return 1;
        }
        
        /// <summary>
        /// 根据目标位置调整实体朝向。
        /// </summary>
        public static void LookAt2D(this LogicEntity entity, Vector3 target)
        {
            if (entity.hasComTransform)
            {
                bool isLeft = entity.comTransform.position.x >= target.x;
                entity.SetDir2D(isLeft ? -1 : 1);
            }
        }
    }
}

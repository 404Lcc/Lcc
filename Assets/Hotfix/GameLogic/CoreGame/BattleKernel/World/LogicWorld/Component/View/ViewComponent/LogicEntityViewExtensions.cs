using HotUpdate.Framework.PbCfg;
using PBConfig;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 主视图、挂点和表现 Transform 查询相关扩展。
    /// </summary>
    public static class LogicEntityViewExtensions
    {
        /// <summary>
        /// 获取实体主 GameObject 视图包装器。
        /// </summary>
        public static MainGameObjectView GetMainGameObjectView(this LogicEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            if (entity.hasComView)
            {
                var objView = entity.comView.MainActorView<MainGameObjectView>();
                if (objView == null)
                {
                    BattleLogger.LogError($"entity.GetMainGameObjectView objView == null, e={entity.GetLogStr()}");
                    return null;
                }

                return objView;
            }

            return null;
        }

        /// <summary>
        /// 获取实体主视图 Transform。
        /// </summary>
        public static Transform GetEntityTf(this LogicEntity entity)
        {
            var objView = entity.GetMainView(null, false);
            if (objView == null)
            {
                return null;
            }

            return objView.Transform;
        }

        /// <summary>
        /// 获取实体主视图指定挂点 Transform。
        /// </summary>
        public static Transform GetMainViewBindTransform(this LogicEntity entity, string bindPoint, bool logError = true)
        {
            var mainView = entity.GetMainView(bindPoint, logError);
            var bpTrans = mainView?.GetBindPoint(bindPoint);
            if (bpTrans == null)
            {
                if (logError)
                {
                    BattleLogger.LogError($"entity.GetMainViewBindTransform bpTrans == null, e={GetEntityLogId(entity)},bindPoint={bindPoint}");
                }

                return null;
            }

            return bpTrans;
        }

        /// <summary>
        /// 获取实体主 GameObject 视图，可选择校验挂点名并打印错误。
        /// </summary>
        public static MainGameObjectView GetMainView(this LogicEntity entity, string bindPoint, bool logError)
        {
            if (entity == null)
            {
                return null;
            }

            if (bindPoint != null && string.IsNullOrEmpty(bindPoint))
            {
                if (logError)
                {
                    BattleLogger.LogError("entity.GetMainView bindPoint.IsNullOrEmpty");
                }

                return null;
            }

            if (!entity.hasComView)
            {
                if (logError)
                {
                    BattleLogger.LogError($"entity.GetMainView !entity.hasComView, e={GetEntityLogId(entity)}");
                }

                return null;
            }

            var objView = entity.comView.MainActorView<MainGameObjectView>();
            if (objView == null && logError)
            {
                BattleLogger.LogError($"entity.GetMainView objView == null, e={GetEntityLogId(entity)}, bindPoint={bindPoint}");
            }

            return objView;
        }

        /// <summary>
        /// 判断实体主 GameObject 视图是否已加载完成。
        /// </summary>
        public static bool IsMainGameObjectViewReady(this LogicEntity entity)
        {
            if (entity == null || !entity.hasComView)
                return false;

            var objView = entity.comView.MainActorView<MainGameObjectView>();
            return objView?.GameObject != null;
        }

        /// <summary>
        /// 判断实体主视图是否存在指定挂点。
        /// </summary>
        public static bool HasMainViewBindPoint(this LogicEntity entity, string bindPoint, bool logError)
        {
            var mainView = entity.GetMainView(bindPoint, logError);
            return mainView?.HasBindPoint(bindPoint) ?? false;
        }

        /// <summary>
        /// 获取实体主视图指定挂点世界坐标，缺失时回退到实体逻辑坐标。
        /// </summary>
        public static Vector3 GetMainViewBindPos(this LogicEntity entity, string bindPoint)
        {
            var bpTrans = entity.GetMainViewBindTransform(bindPoint);
            if (bpTrans == null)
            {
                return entity.position;
            }

            return bpTrans.position;
        }

        /// <summary>
        /// 获取从起始点指向目标表现碰撞体最近点的向量；找不到碰撞体时回退到目标逻辑位置。
        /// </summary>
        public static Vector3 ClosestDistance(this Vector3 startPoint, LogicEntity targetEntity, float maxDistance = 10f)
        {
            if (targetEntity is null || !targetEntity.hasComTransform)
            {
                return Vector3.zero;
            }

            var diff = targetEntity.comTransform.position - startPoint;
            if (!targetEntity.hasComView)
            {
                return diff;
            }

            var collider = targetEntity.comView.MainActorView<MainGameObjectView>()?.GameObject?.GetComponentInChildren<Collider2D>();
            if (collider is null || !collider.gameObject.activeInHierarchy)
            {
                BattleLogger.LogWarning($"ClosestDistance 找不到碰撞体，EntityId={targetEntity.ID}");
                return diff;
            }

            var distance = diff.magnitude;
            if (distance > maxDistance)
            {
                return diff;
            }

            var closestPoint = collider.ClosestPoint(startPoint);
            return (Vector3)closestPoint - startPoint;
        }

        /// <summary>
        /// 获取实体 ID 日志文本。
        /// </summary>
        private static string GetEntityLogId(LogicEntity entity)
        {
            return entity == null ? "null" : $"{entity.ID}";
        }
    }
}

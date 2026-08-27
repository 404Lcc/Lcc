using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 跟随目标实体移动：每帧将本实体位置同步到目标位置；目标无效时本实体立即结束生命并结束 Locomotion。
    /// </summary>
    public class LocomotionFollow : LocomotionBase
    {
        public long FollowEntityId { get; set; }

        public override void Update(float dt, LogicEntity entity, MetaWorld metaWorld)
        {
            var target = entity.OwnerWorld?.GetEntityWithComID(FollowEntityId);
            if (target == null || !target.hasComTransform)
            {
                if (entity.hasComLife)
                    entity.ReplaceComLife(0f);
                IsRuning = false;
                return;
            }

            var targetPos = target.comTransform.position;
            var curPos = entity.comTransform.position;
            DeltaPosition = targetPos - curPos;
        }
    }
}

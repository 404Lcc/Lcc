using UnityEngine;
using Random = UnityEngine.Random;

namespace LccHotfix
{
    public class HandleSubobjColliderTransferBhvCfg : ICustomNodeCfg
    {
        public FloatCfg Prob;

        public virtual System.Type NodeType() { return typeof(HandleSubobjColliderTransferBhv); }

        public HandleSubobjColliderTransferBhvCfg(string prob)
        {
            Prob = new FloatCfg(prob);
        }
    }

    public class HandleSubobjColliderTransferBhv : BehaviorNode<HandleSubobjColliderTransferBhvCfg>, IEntityCommandHandler
    {
        private void Transfer(LogicEntity target, Vector3 transferPos)
        {
            target.SetPosition(transferPos);
        }

        public virtual bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            if (cmd.CmdType != EntityCmdType.Nt_ColliderHit)
                return false;

            var rand = Random.Range(0, 100);
            if (rand >= _cfg.Prob.GetValue(this))
                return true;

            var hitEntityId = cmd.HitInfo.hitEntityID;
            var hitEntity = entity.OwnerWorld.GetEntityWithComID(hitEntityId);
            if (hitEntity == null)
                return false;
            if (!entity.hasComTransform)
                return false;
            if (!hitEntity.hasComTransform)
                return false;
            if (!entity.hasComBounds)
                return false;
            if (hitEntity.IsImmuneToTeleport)
                return false;

            var hitEntityPos = hitEntity.comTransform.position;
            var bounds = entity.comBounds.GetBounds();
            var height = bounds.Height();
            var transferPos = hitEntityPos + new Vector3(0, height, 0);
            if (hitEntity.hasComLocomotion && hitEntity.comLocomotion.Locomotion is LocomotionStraightDir dirLocomotion)
            {
                float angleRad = Mathf.Atan2(dirLocomotion.Dir.y, dirLocomotion.Dir.x);
                var newLength = height / Mathf.Sin(angleRad);
                transferPos = hitEntityPos + dirLocomotion.Dir * newLength;
            }

            Transfer(hitEntity, transferPos);
            this.GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.SubobjectTransferEffectService?.OnTransferred(this, hitEntity);
            return true;
        }
    }
}

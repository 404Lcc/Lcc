using ET;

namespace LccModel
{
    [EventHandler]
    public class SyncTransformEventHandler : AEvent<SyncTransform>
    {
        public override async ETTask Publish(SyncTransform data)
        {
            CombatView entity = null;
            if (entity != null)
            {
                entity.TransformViewComponent.SyncTransform(data.position, data.rotation, data.localScale);
            }
            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncAnimationEventHandler : AEvent<SyncAnimation>
    {
        public override async ETTask Publish(SyncAnimation data)
        {
            CombatView entity = null;
            if (entity != null)
            {
                entity.AnimationViewComponent.PlayAnimation(data.type, data.speed, data.isLoop);
            }
            await ETTask.CompletedTask;
        }
    }

    [EventHandler]
    public class SyncDeadEventHandler : AEvent<SyncDead>
    {
        public override async ETTask Publish(SyncDead data)
        {

            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncDamageEventHandler : AEvent<SyncDamage>
    {
        public override async ETTask Publish(SyncDamage data)
        {

            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncCureEventHandler : AEvent<SyncCure>
    {
        public override async ETTask Publish(SyncCure data)
        {

            await ETTask.CompletedTask;
        }
    }
}
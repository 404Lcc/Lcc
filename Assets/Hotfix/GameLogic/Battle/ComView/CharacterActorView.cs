using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class CharacterActorView : ActorView
    {
        public Vector2 size;
        public AABB bounds;

        public override void Init(GameObjectPoolObject poolObject)
        {
            base.Init(poolObject);

            Main.GizmoService.AddGizmo(OnGizmos);
        }

        public override void Dispose()
        {
            base.Dispose();
            Main.GizmoService.RemoveGizmo(OnGizmos);
        }

        public override void SyncTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            base.SyncTransform(position, rotation, scale);

            UpdateBounds();
        }

        public virtual void OnGizmos()
        {
            if (bounds == null)
                return;

            // 绘制障碍物
            Gizmos.color = Color.red;
            var center2D = (bounds.minPoint + bounds.maxPoint) * 0.5f;
            var center = new Vector3(center2D.x, 0, center2D.y);
            var size = new Vector3(bounds.Width(), 0, bounds.Height());
            Gizmos.DrawCube(center, size);
        }

        public virtual void SetSize(Vector2 size)
        {
            this.size = size;
            this.bounds = new AABB(-size / 2, size / 2);
            UpdateBounds();
        }
        
        public virtual void UpdateBounds()
        {
            var selfPos = new Vector2(GameObject.transform.position.x, GameObject.transform.position.z);
            bounds.minPoint = selfPos - size / 2;
            bounds.maxPoint = bounds.minPoint + size;
        }
        
        public virtual void ApplyDamage()
        {
            
        }

        public float PlayAnim(string animName, bool isForce = false)
        {
            if (!isForce)
            {
                //相同动画不播放
                var normalizedTime = Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (normalizedTime <= 1)
                {
                    return GameUtility.GetAnimationClipLengthByNameMatched(Animator, animName);
                }
            }
            else
            {
                // 强制停止当前动画
                Animator.StopPlayback();
            }

            Animator.Play(animName, 0, 0);

            return GameUtility.GetAnimationClipLengthByNameMatched(Animator, animName);
        }
    }
}
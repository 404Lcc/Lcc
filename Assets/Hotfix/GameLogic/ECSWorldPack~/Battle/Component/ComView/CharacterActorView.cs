using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public enum CharacterPlane
    {
        XY,
        XZ,
    }

    public class CharacterActorView : ActorView
    {
        public CharacterPlane plane;
        public Vector2 size;
        public AABB bounds;

        public override void Init(GameObjectPoolAsyncOperation poolObject)
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
            var center = Vector3.zero;
            var size = Vector3.zero;
            switch (plane)
            {
                case CharacterPlane.XY:
                    center = new Vector3(center2D.x, center2D.y, 0);
                    size = new Vector3(bounds.Width(), bounds.Height(), 0);
                    break;
                case CharacterPlane.XZ:
                    center = new Vector3(center2D.x, 0, center2D.y);
                    size = new Vector3(bounds.Width(), 0, bounds.Height());
                    break;
            }

            Gizmos.DrawCube(center, size);
        }

        public virtual void SetPlane(CharacterPlane plane)
        {
            this.plane = plane;
        }

        public virtual void SetSize(Vector2 size)
        {
            this.size = size;
            this.bounds = new AABB(-size / 2, size / 2);
            UpdateBounds();
        }

        public virtual void UpdateBounds()
        {
            var selfPos = Vector2.zero;
            switch (plane)
            {
                case CharacterPlane.XY:
                    selfPos = new Vector2(GameObject.transform.position.x, GameObject.transform.position.y);
                    break;
                case CharacterPlane.XZ:
                    selfPos = new Vector2(GameObject.transform.position.x, GameObject.transform.position.z);
                    break;
            }

            bounds.minPoint = selfPos - size / 2;
            bounds.maxPoint = bounds.minPoint + size;
        }

        public virtual void ApplyDamage()
        {
        }

        public float PlayAnim(string animName, bool isForce = false, int layer = 0)
        {
            if (!isForce)
            {
                //相同动画不播放
                var normalizedTime = Animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
                if (Animator.GetCurrentAnimatorStateInfo(layer).IsName(animName) && normalizedTime <= 1)
                {
                    return GameUtility.GetAnimationClipLengthByNameMatched(Animator, animName);
                }
            }
            else
            {
                // 强制停止当前动画
                Animator.StopPlayback();
            }

            Animator.Play(animName, layer, 0);

            return GameUtility.GetAnimationClipLengthByNameMatched(Animator, animName);
        }
    }
}
using UnityEngine;

namespace LccModel
{
    public class SyncTransform
    {
        public long id;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
        public int sortingOrder;
        public SyncTransform()
        {
        }
        public SyncTransform(long id, Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            this.id = id;
            this.position = position;
            this.rotation = rotation;
            this.localScale = localScale;
        }
    }
    public class SyncAnimation
    {
        public long id;
        public AnimationType type;
        public float speed;
        public bool isLoop;
        public SyncAnimation()
        {
        }
        public SyncAnimation(long id, AnimationType type, float speed, bool isLoop)
        {
            this.id = id;
            this.type = type;
            this.speed = speed;
            this.isLoop = isLoop;
        }
    }
    public class SyncDead
    {
        public long id;
        public SyncDead()
        {
        }
        public SyncDead(long id)
        {
            this.id = id;
        }
    }
    public class SyncDamage
    {
        public long id;
        public int damage;
        public SyncDamage()
        {
        }
        public SyncDamage(long id, int damage)
        {
            this.id = id;
            this.damage = damage;
        }
    }
    public class SyncCure
    {
        public long id;
        public int cure;
        public SyncCure()
        {
        }
        public SyncCure(long id, int cure)
        {
            this.id = id;
            this.cure = cure;
        }
    }
}
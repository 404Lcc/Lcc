using UnityEngine;

namespace LccModel
{
    public class SyncCreateCombat
    {
        public long id;
        public SyncCreateCombat()
        {
        }
        public SyncCreateCombat(long id)
        {
            this.id = id;
        }
    }
    public class SyncDeleteCombat
    {
        public long id;
        public SyncDeleteCombat()
        {
        }
        public SyncDeleteCombat(long id)
        {
            this.id = id;
        }
    }
    public class SyncCreateAbilityItem
    {
        public long id;
        public SyncCreateAbilityItem()
        {
        }
        public SyncCreateAbilityItem(long id)
        {
            this.id = id;
        }
    }
    public class SyncDeleteAbilityItem
    {
        public long id;
        public SyncDeleteAbilityItem()
        {
        }
        public SyncDeleteAbilityItem(long id)
        {
            this.id = id;
        }
    }
    public class SyncTransform
    {
        public long id;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
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
    public class SyncParticleEffect
    {
        public long id;
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public SyncParticleEffect()
        {
        }
        public SyncParticleEffect(long id, string name, Vector3 position, Quaternion rotation)
        {
            this.id = id;
            this.name = name;
            this.position = position;
            this.rotation = rotation;
        }
    }
    public class SyncDeleteParticleEffect
    {
        public long id;
        public string name;
        public SyncDeleteParticleEffect()
        {
        }
        public SyncDeleteParticleEffect(long id, string name)
        {
            this.id = id;
            this.name = name;
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
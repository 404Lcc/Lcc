using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public static class EntityExtension
    {
        public static void SetPosition(this LogicEntity entity, Vector3 pos)
        {
            if (entity.hasComTransform)
            {
                entity.comTransform.SetPosition(pos);
            }

            var actorView = entity.comView.ActorView;
            if (actorView != null)
            {
                actorView.Transform.position = pos;
            }
        }

        public static void SetQuaternion(this LogicEntity entity, Quaternion quaternion)
        {
            if (entity.hasComTransform)
            {
                entity.comTransform.SetRotation(quaternion);
            }

            var actorView = entity.comView.ActorView;
            if (actorView != null)
            {
                actorView.Transform.rotation = quaternion;
            }
        }

        public static void SetScale(this LogicEntity entity, Vector3 scale, int dirX)
        {
            if (entity.hasComTransform)
            {
                entity.comTransform.SetScale(scale);
                entity.comTransform.SetDirX(dirX);
            }

            var actorView = entity.comView.ActorView;
            if (actorView != null)
            {
                actorView.Transform.localScale = new Vector3(scale.x * dirX, scale.y, scale.z);
            }
        }

        public static void SetDir(this LogicEntity entity, int dirX)
        {
            Vector3 scale = Vector3.one;
            if (entity.hasComTransform)
            {
                scale = entity.comTransform.scale;
                entity.comTransform.SetDirX(dirX);
            }

            var actorView = entity.comView.ActorView;
            if (actorView != null)
            {
                actorView.Transform.localScale = new Vector3(scale.x * dirX, scale.y, scale.z);
            }
        }

        public static void SetDir(this LogicEntity entity, bool isLeft)
        {
            if (entity.hasComTransform)
            {
                entity.SetDir(isLeft ? -1 : 1);
            }
        }

        public static void LookAt(this LogicEntity entity, Vector3 target)
        {
            if (entity.hasComTransform)
            {
                bool isLeft = entity.comTransform.position.x >= target.x;
                entity.SetDir(isLeft ? -1 : 1);
            }
        }


        public static bool HasTag(this LogicEntity entity, TagType tag)
        {
            if (!entity.hasComTag)
                return false;
            return entity.comTag.HasTag(tag);
        }

        public static int GetHeroConfigID(this LogicEntity entity)
        {
            if (entity.hasComHero)
            {
                return entity.comHero.ConfigID;
            }

            return -1;
        }

        public static int GetMonsterConfigID(this LogicEntity entity)
        {
            if (entity.hasComMonster)
            {
                return entity.comMonster.ConfigID;
            }

            return -1;
        }
    }
}
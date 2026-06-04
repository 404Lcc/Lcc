using HotUpdate.Framework.PbCfg;
using PBConfig;

namespace LccHotfix
{
    public static class DemoBattleConfigIds
    {
        public const uint FriendFighter = 1001;
        public const uint EnemyFighter = 2001;

        public const uint FriendSkill = 3001;
        public const uint EnemySkill = 3002;
        public const int ProjectileSkillLogic = 30001;

        public const uint FriendProjectile = 4001;
        public const uint EnemyProjectile = 4002;
        public const int ProjectileSubobjectLogic = 40001;

        public const int UnitFsmLogic = 10001;
    }

    public static class DemoBattleConfigRegister
    {
        public static void Register()
        {
            RegisterFighters();
            RegisterSkills();
            RegisterSubobjects();
        }

        private static void RegisterFighters()
        {
            var friend = new TFighter
            {
                Name = "Demo Friend",
                FsmLogic = DemoBattleConfigIds.UnitFsmLogic,
                Radius = 0.5f,
                Scale = 1f,
            };
            friend.Base.Id = DemoBattleConfigIds.FriendFighter;
            friend.Skills.Add(DemoBattleConfigIds.FriendSkill);
            PbCfg.RegisterData(DemoBattleConfigIds.FriendFighter, friend);

            var enemy = new TFighter
            {
                Name = "Demo Enemy",
                FsmLogic = DemoBattleConfigIds.UnitFsmLogic,
                Radius = 0.5f,
                Scale = 1f,
            };
            enemy.Base.Id = DemoBattleConfigIds.EnemyFighter;
            enemy.Skills.Add(DemoBattleConfigIds.EnemySkill);
            PbCfg.RegisterData(DemoBattleConfigIds.EnemyFighter, enemy);
        }

        private static void RegisterSkills()
        {
            RegisterSkill(DemoBattleConfigIds.FriendSkill, "Demo Friend Shot", DemoBattleConfigIds.FriendProjectile);
            RegisterSkill(DemoBattleConfigIds.EnemySkill, "Demo Enemy Shot", DemoBattleConfigIds.EnemyProjectile);
        }

        private static void RegisterSkill(uint tid, string name, uint subobjectTid)
        {
            var skill = new TSkillLogic
            {
                Name = name,
                LogicID = DemoBattleConfigIds.ProjectileSkillLogic,
                Cd = 1.2f,
                Range = 8f,
                DamageRate = 1f,
            };
            skill.Base.Id = tid;
            skill.SubobjIds.Add(subobjectTid);
            PbCfg.RegisterData(tid, skill);
        }

        private static void RegisterSubobjects()
        {
            RegisterProjectile(DemoBattleConfigIds.FriendProjectile, "Demo Friend Projectile");
            RegisterProjectile(DemoBattleConfigIds.EnemyProjectile, "Demo Enemy Projectile");
        }

        private static void RegisterProjectile(uint tid, string name)
        {
            var subobject = new TSubobject
            {
                Name = name,
                LogicID = DemoBattleConfigIds.ProjectileSubobjectLogic,
                During = 3f,
                DamageRate = 1f,
                CollisionType = CollisionType.EhtAabb,
                HitWithLife = true,
                MaxHitCount = 1,
                SingleHitCount = 1,
                HitInterval = 0.1f,
                Radius = 0.2f,
            };
            subobject.Base.Id = tid;
            PbCfg.RegisterData(tid, subobject);
        }
    }
}
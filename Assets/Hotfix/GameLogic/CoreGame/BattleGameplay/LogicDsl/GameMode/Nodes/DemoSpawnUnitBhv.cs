using UnityEngine;
using HotUpdate.Framework.PbCfg;
using PBConfig;

namespace LccHotfix
{
    public class DemoSpawnUnitBhvCfg : ICustomNodeCfg
    {
        public DemoBattlePlayerSide PlayerSide;
        public EFaction Faction;
        public Vector3 Position;
        public int FighterId;
        public double Hp = 100;
        public float BoundsRadius = 0.5f;
        public string EntityIdVar;

        public System.Type NodeType()
        {
            return typeof(DemoSpawnUnitBhv);
        }
    }

    public class DemoSpawnUnitBhv : BehaviorNode<DemoSpawnUnitBhvCfg>
    {
        protected override void OnBegin()
        {
            var creationInfo = GetVar<ECGameWorldCreationInfo>(CvKey.CV_WorldInfo);
            var logicWorld = this.GetLogicWorld();
            if (creationInfo == null || logicWorld == null)
            {
                BattleLog.Error("DemoSpawnUnitBhv creationInfo == null || logicWorld == null");
                return;
            }

            var playerInfo = _cfg.PlayerSide == DemoBattlePlayerSide.Friend ? creationInfo.FriendPlayer : creationInfo.EnemyPlayer;
            var fighterCfg = PbCfg.GetData<TFighter>((uint)_cfg.FighterId);
            var entity = logicWorld.AddEntity(null);
            entity.AddComTransform(_cfg.Position, Quaternion.identity, Vector3.one);
            entity.ReplaceComFaction(_cfg.Faction);
            entity.AddComOwnerPlayer(playerInfo);
            entity.AddComBattleUnitTag(new BattleUnitTag
            {
                PlayerIndex = playerInfo.PlayerIndex,
                FighterId = _cfg.FighterId,
                BattleUnitTid = _cfg.FighterId
            });
            entity.SetComHp(_cfg.Hp, _cfg.Hp);
            entity.ReplaceComBounds(_cfg.Position, _cfg.BoundsRadius);
            entity.AddComCommandSender();
            InitDemoAttributes(entity, _cfg.Hp);
            InitSkillSlot(entity, fighterCfg);
            InitMainFsm(entity, playerInfo, fighterCfg);

            if (!string.IsNullOrEmpty(_cfg.EntityIdVar))
            {
                SetVar(_cfg.EntityIdVar, entity.ID);
            }

            BattleLog.Debug($"DemoSpawnUnitBhv spawned side={_cfg.PlayerSide}, entityId={entity.ID}, fighterId={_cfg.FighterId}");
        }

        private void InitDemoAttributes(LogicEntity entity, double hp)
        {
            var attributes = entity.AddComAttributes();
            attributes.SetAttribute<double>(PropertyFloat.Health, new MultChangeDouble_ADD((float)hp));
            attributes.SetAttribute<double>(PropertyFloat.Attack, new MultChangeDouble_ADD(35));
            attributes.SetAttribute<double>(PropertyFloat.Defense, new MultChangeDouble_ADD(5));
            attributes.SetAttribute<double>(PropertyFloat.Hit, new MultChangeDouble_ADD(10000));
            attributes.SetAttribute<double>(PropertyFloat.Dodge, new MultChangeDouble_ADD(0));
        }

        private void InitSkillSlot(LogicEntity entity, TFighter fighterCfg)
        {
            if (fighterCfg == null || fighterCfg.Skills.Count <= 0)
            {
                BattleLog.Warning($"DemoSpawnUnitBhv fighter skill config missing, fighterId={_cfg.FighterId}");
                return;
            }

            entity.AddComSkillSlot(fighterCfg.Skills.ToArray());
        }

        private void InitMainFsm(LogicEntity entity, IBattlePlayerInfo playerInfo, TFighter fighterCfg)
        {
            if (fighterCfg == null || fighterCfg.FsmLogic <= 0)
            {
                BattleLog.Warning($"DemoSpawnUnitBhv fighter fsm config missing, fighterId={_cfg.FighterId}");
                return;
            }

            var service = GetLogicWorld()?.CustomLogicService;
            if (service == null)
            {
                BattleLog.Error("DemoSpawnUnitBhv CustomLogicService == null");
                return;
            }

            var varEnv = service.NewVarEnv();
            FillMainFsmVarEnv(varEnv, entity, playerInfo, _cfg.FighterId, fighterCfg);
            var genInfo = service.NewGenInfo<UnitLogicGenInfo>();
            genInfo.LogicConfigID = (int)fighterCfg.FsmLogic;
            genInfo.ConfigContainerName = LogicContainerKey.LogicConfigs_EntityFSM;
            genInfo.PreEnv = varEnv;
            genInfo.SumUnitSource = new UnitSource(entity);
            var fsm = service.CreateLogic<BattleFSM>(genInfo);
            entity.AddComFSM(fsm);
        }
    }
}

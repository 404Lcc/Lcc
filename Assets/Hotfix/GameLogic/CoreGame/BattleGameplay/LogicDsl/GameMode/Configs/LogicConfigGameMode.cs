using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public partial class LogicConfigs_GameMode : LogicConfigBase
    {
        public LogicConfigs_GameMode(string name) : base(name, 8)
        {
            DefaultLogicType = typeof(BattleModeLogic);
            InitConfigs();
        }

        private void InitConfigs()
        {
            AddConfig(ECGameWorldCreationInfo.DemoModeLogicId, new List<ICustomNodeCfg>
            {
                FSM("GST_Start", new List<ICustomNodeCfg>
                {
                    CustomState("GST_Start", "GST_Playing", Seq(new List<ICustomNodeCfg>
                    {
                        Bhv<DemoGameModeStartBhv>(),
                        new DemoSpawnUnitBhvCfg
                        {
                            PlayerSide = DemoBattlePlayerSide.Friend,
                            Faction = EFaction.Friend,
                            Position = new Vector3(-2f, 0f, 0f),
                            FighterId = 1001,
                            Hp = 100,
                            BoundsRadius = 0.5f,
                            EntityIdVar = CvKey.CV_DemoFriendEntityId,
                        },
                        new DemoSpawnUnitBhvCfg
                        {
                            PlayerSide = DemoBattlePlayerSide.Enemy,
                            Faction = EFaction.Enemy,
                            Position = new Vector3(2f, 0f, 0f),
                            FighterId = 2001,
                            Hp = 100,
                            BoundsRadius = 0.5f,
                            EntityIdVar = CvKey.CV_DemoEnemyEntityId,
                        },
                    })),

                    CustomState("GST_Playing", Bhv<DemoGameModeTickBhv>()),

                    CustomState("GST_Over", Seq(new List<ICustomNodeCfg>
                    {
                        Bhv<DemoGameModeFinishBhv>(),
                    })),
                }, new List<StateTransitionNodeCfg>
                {
                    new StateTransitionNodeCfg(DemoGameModeHasResult(), "GST_Over"),
                }).WithRecordCurStateIDTo(CvKey.CV_DemoGameModeCurState),
            });
        }

        private DelegateConditionCfg DemoGameModeHasResult()
        {
            return new DelegateConditionCfg(node => node.GetVar(CvKey.CV_DemoGameResult, DemoGameResult.None) != DemoGameResult.None);
        }
    }
}

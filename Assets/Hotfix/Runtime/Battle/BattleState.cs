using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class BattleStartState : IStateNode
    {
        public BattleGameMode battleGameMode;

        public void OnCreate(StateMachine machine)
        {
            battleGameMode = machine.GetBlackboardValue("GameMode") as BattleGameMode;
        }

        public void OnEnter()
        {
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
    }
}
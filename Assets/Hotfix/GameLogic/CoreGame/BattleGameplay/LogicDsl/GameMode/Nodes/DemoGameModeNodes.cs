using UnityEngine;

namespace LccHotfix
{
    public class DemoGameModeStartBhv : BehaviorNodeBase
    {
        protected override void OnBegin()
        {
            var logicWorld = this.GetLogicWorld();
            if (logicWorld != null)
            {
                logicWorld.GameOver = false;
            }

            SetVar(CvKey.CV_DemoGameModeState, DemoGameModeState.Running);
            SetVar(CvKey.CV_DemoGameResult, DemoGameResult.None);
            SetVar(CvKey.CV_DemoLastLogSecond, -1);
            BattleLog.Debug("Demo game mode started by LogicDsl");
        }
    }

    public class DemoGameModeTickBhv : BehaviorNodeBase, INeedStopCheck
    {
        protected override float OnUpdate(float dt)
        {
            if (GetVar(CvKey.CV_DemoGameResult, DemoGameResult.None) != DemoGameResult.None)
            {
                return dt;
            }

            LogTick();
            CheckFinish();
            return dt;
        }

        public bool CanStop()
        {
            return false;
        }

        private void LogTick()
        {
            var duration = GetDuration();
            var second = Mathf.FloorToInt(duration);
            var lastSecond = GetVar(CvKey.CV_DemoLastLogSecond, -1);
            if (second == lastSecond)
            {
                return;
            }

            SetVar(CvKey.CV_DemoLastLogSecond, second);
            BattleLog.Debug($"Demo battle running by LogicDsl, duration={second}s, friend={CountAlive(EFaction.Friend)}, enemy={CountAlive(EFaction.Enemy)}");
        }

        private void CheckFinish()
        {
            var creationInfo = GetVar<ECGameWorldCreationInfo>(CvKey.CV_WorldInfo);
            if (creationInfo != null && creationInfo.DemoMaxDurationSeconds > 0f && GetDuration() >= creationInfo.DemoMaxDurationSeconds)
            {
                SetResult(DemoGameResult.Timeout);
                return;
            }

            if (creationInfo != null && !creationInfo.DemoAutoFinishWhenSideDead)
            {
                return;
            }

            var friendAlive = CountAlive(EFaction.Friend);
            var enemyAlive = CountAlive(EFaction.Enemy);
            if (friendAlive <= 0 && enemyAlive <= 0)
            {
                SetResult(DemoGameResult.Draw);
            }
            else if (friendAlive <= 0)
            {
                SetResult(DemoGameResult.Lose);
            }
            else if (enemyAlive <= 0)
            {
                SetResult(DemoGameResult.Win);
            }
        }

        private void SetResult(DemoGameResult result)
        {
            SetVar(CvKey.CV_DemoGameResult, result);
            BattleLog.Debug($"Demo game mode result decided by LogicDsl, result={result}");
        }

        private float GetDuration()
        {
            return RootLogic is IGameDuration duration ? duration.GetGameDuration() : 0f;
        }

        private int CountAlive(EFaction faction)
        {
            var logicWorld = this.GetLogicWorld();
            if (logicWorld == null)
            {
                return 0;
            }

            int count = 0;
            var group = logicWorld.GetLogicGroup_Faction_HP_Transform();
            foreach (var entity in group.GetEntities())
            {
                if (entity.Faction == faction && entity.comHp.Hp > 0 && !entity.hasComDeath)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public class DemoGameModeFinishBhv : BehaviorNodeBase
    {
        protected override void OnBegin()
        {
            SetVar(CvKey.CV_DemoGameModeState, DemoGameModeState.Finished);
            var logicWorld = this.GetLogicWorld();
            if (logicWorld != null)
            {
                logicWorld.GameOver = true;
            }

            var result = GetVar(CvKey.CV_DemoGameResult, DemoGameResult.None);
            var duration = RootLogic is IGameDuration gameDuration ? gameDuration.GetGameDuration() : 0f;
            BattleLog.Debug($"Demo game mode finished by LogicDsl, result={result}, duration={duration:0.00}s");
        }
    }
}

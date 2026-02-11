using UnityEngine;

namespace LccHotfix
{
    public class BattleWorldCreationInfo : IWorldCreationInfo
    {

    }

    [Procedure]
    public class BattleProcedure : LoadProcedureHandler, ICoroutine
    {
        private ECGameWorld _world;

        public BattleProcedure()
        {
            procedureType = ProcedureType.Battle.ToInt();
            loadType = LoadingType.Normal;
        }

        public override void ProcedureStartHandler()
        {
            base.ProcedureStartHandler();

            //进入
            Log.Debug("进入Battle");

            _world = ECGameWorld.CreateWorld(new BattleWorldCreationInfo());
            ProcedureLoadEndHandler();
        }

        public override void Tick()
        {
            base.Tick();

            if (IsLoading)
            {
                return;
            }

            _world?.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (IsLoading)
            {
                return;
            }

            _world?.LateUpdate();
        }

        public override void ProcedureExitHandler()
        {
            base.ProcedureExitHandler();

            _world.DestroyWorlds();
            Log.Debug("退出Battle");
        }
    }
}
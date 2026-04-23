using UnityEngine;

namespace LccHotfix
{
    [Procedure]
    public class BattleProcedure : LoadProcedureHandler, ICoroutine
    {
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

            ProcedureLoadEndHandler();
        }

        public override void Tick()
        {
            base.Tick();

            if (IsLoading)
            {
                return;
            }

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (IsLoading)
            {
                return;
            }

        }

        public override void ProcedureExitHandler()
        {
            base.ProcedureExitHandler();

            Log.Debug("退出Battle");
        }
    }
}
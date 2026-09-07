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
            KLogger.Log("进入Battle");

            Main.UIService.ShowDomain(UIRootDefine.UIRootBattle, UIPanelDefine.UIBattlePanel);
            
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

        public override void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.FixedUpdate(elapseSeconds, realElapseSeconds);

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

            KLogger.Log("退出Battle");
        }
    }
}
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using LitJson;


namespace LccModel
{
    public class FsmGetNotice : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage(Launcher.Instance.GameLanguage.GetLanguage("msg_load"));

            UILoadingPanel.Instance.UpdateLoadingPercent(51, 70);
            Launcher.Instance.StartCoroutine(GetNoticeInfo());
        }


        //这里提前拿一下停服公告
        public IEnumerator GetNoticeInfo()
        {
            yield return Launcher.Instance.GameNotice.GetNoticeBoard();

            NextState();
        }

        public void NextState()
        {
            _machine.ChangeState<FsmInitializePackage>();

        }

        public void OnUpdate()
        {
        }
        public void OnExit()
        {


        }

    }
}
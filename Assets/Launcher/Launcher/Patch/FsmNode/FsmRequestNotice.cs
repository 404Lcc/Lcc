using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using LitJson;


namespace LccModel
{
    public class FsmRequestNotice : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage(Launcher.Instance.GameLanguage.GetLanguage("msg_request_notice"));

            UILoadingPanel.Instance.UpdateLoadingPercent(51, 70);
            Launcher.Instance.StartCoroutine(GetNoticeInfo());
        }


        //这里提前拿一下停服公告
        public IEnumerator GetNoticeInfo()
        {
#if !Offline
            yield return Launcher.Instance.GameNotice.GetNoticeBoard();
#endif
            yield return null;

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
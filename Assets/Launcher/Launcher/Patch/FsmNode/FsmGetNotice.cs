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
            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_load"));

            UILoadingPanel.Instance.UpdateLoadingPercent(51, 70);
            Launcher.Instance.StartCoroutine(GetNoticeInfo());
        }


        //这里提前拿一下停服公告
        public IEnumerator GetNoticeInfo()
        {
            yield return Launcher.Instance.GetNoticeBoard();

            NextState();
        }

        public void NextState()
        {
            _machine.ChangeState<FsmInitialize>();

        }

        public void OnUpdate()
        {
        }
        public void OnExit()
        {


        }

    }
}
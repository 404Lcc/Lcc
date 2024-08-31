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



        public IEnumerator GetNoticeInfo()
        {
            yield return Launcher.Instance.GetNoticeBoard();
            yield return Launcher.Instance.GetNotice();

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
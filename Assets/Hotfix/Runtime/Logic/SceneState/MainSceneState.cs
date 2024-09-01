using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Main)]
    public class MainSceneState : SceneState
    {
        public override void OnEnter(object[] args)
        {
            base.OnEnter(args);
            Debug.Log("Main" + "����");

            //��������main�ĵ�������
            //����Ѿ����ڵĵ������ݣ��жϲ���
            PanelManager.Instance.ShowPanel(PanelType.UIMain, new ShowPanelData(false, true, null, true, false, true));

        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Main" + "�˳�");
        }
    }
}
using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Main)]
    public class MainSceneState : SceneState
    {
        public override async ETTask OnEnter()
        {
            await base.OnEnter();
            Debug.Log("Main" + "����");

            await SceneLoadManager.Instance.LoadSceneAsync(SceneName.Main, AssetType.Scene);

            //��������main�ĵ�������
            //����Ѿ����ڵĵ������ݣ��жϲ���
            PanelManager.Instance.ShowPanel(PanelType.UIMain, new ShowPanelData(false, true, null, true, false, true));

        }
        public override async ETTask OnExit()
        {
            await base.OnExit();
            Debug.Log("Main" + "�˳�");
        }
    }
}
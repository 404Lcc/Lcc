using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Game)]
    public class GameSceneState : SceneState
    {
        public override async ETTask OnEnter()
        {
            await base.OnEnter();
            Debug.Log("Game" + "����");

            PanelManager.Instance.HidePanel(PanelType.Top);

            await SceneLoadManager.Instance.LoadSceneAsync(SceneName.Game, AssetType.Scene);

            var combat1 = CombatContext.Instance.AddCombat(1);
            var combat2 = CombatContext.Instance.AddCombat(2, TagType.Enemy);
            combat2.TransformComponent.position = new Vector3(0, 10, 0);
            combat2.AttachSkill(1);
            combat2.AddComponent<AIComponent>(Vector3.zero).SetState(new IdleState());

            //��������main�ĵ�������
            //����Ѿ����ڵĵ������ݣ��жϲ���
            PanelManager.Instance.ShowPanel(PanelType.Game, new ShowPanelData(false, true, null, true, false, true));


            //�ͷ��չ���ִ���壬ִ���������һִ֡��������Ҫ�ȴ���һ֡ʹ�õ���
            await Timer.Instance.WaitAsync(1000);



         

        }
        public override async ETTask OnExit()
        {
            await base.OnExit();
            Debug.Log("Game" + "�˳�");
        }
    }
}
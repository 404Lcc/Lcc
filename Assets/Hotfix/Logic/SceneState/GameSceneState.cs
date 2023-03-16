using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateName.Game)]
    public class GameSceneState : SceneState
    {
        public override async ETTask OnEnter()
        {
            await base.OnEnter();
            Debug.Log("Game" + "����");

            PanelManager.Instance.HidePanel(PanelType.Top);

            await SceneLoadManager.Instance.LoadSceneAsync(SceneName.Game, AssetType.Scene);

            //��������main�ĵ�������
            //����Ѿ����ڵĵ������ݣ��жϲ���
            PanelManager.Instance.ShowPanel(PanelType.Game, new ShowPanelData(false, true, null, true, false, true));



            var combat1 = CombatContext.Instance.AddCombat();
            var combat2 = CombatContext.Instance.AddCombat();


            //�ͷ��չ�
            combat1.GetComponent<SpellAttackComponent>().SpellAttackWithTarget(combat2);




            var item = combat1.AttachItem(UnityEngine.Resources.Load<ItemConfigObject>("Item_1"));
            //�ͷ��չ���ִ���壬ִ���������һִ֡��������Ҫ�ȴ���һ֡ʹ�õ���
            await Timer.Instance.WaitAsync(1000);



            //ʹ����Ʒ
            combat1.GetComponent<SpellItemComponent>().SpellItemWithTarget(item, combat2);

        }
        public override async ETTask OnExit()
        {
            await base.OnExit();
            Debug.Log("Game" + "�˳�");
        }
    }
}
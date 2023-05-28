using LccModel;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LccHotfix
{
    public class UIMainModel : ViewModelBase
    {
    }
    public class UIMainPanel : UIPanel<UIMainModel>
    {
        public Button testBtn;
        public Button test1Btn;
        public Button test2Btn;
        public Button test3Btn;
        public override void OnInitComponent(Panel panel)
        {
            base.OnInitComponent(panel);

        }
        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);

            panel.data.type = UIType.Normal;
            panel.data.showMode = UIShowMode.HideOther;
            panel.data.navigationMode = UINavigationMode.IgnoreNavigation;
        }
        public override void OnShow(Panel panel, AObjectBase contextData = null)
        {
            base.OnShow(panel, contextData);
            ShowTopPanel(TopType.CloseBtn | TopType.Gold, "Main");
        }
        public override void OnRegisterUIEvent(Panel panel)
        {
            testBtn.onClick.AddListener(OnTest);
            test1Btn.onClick.AddListener(OnTest1);
            test2Btn.onClick.AddListener(OnTest2);
            test3Btn.onClick.AddListener(OnTest3);
        }
        //AssetOperationHandle handle;
        public void OnTest()
        {
            AssetManager.Instance.LoadSceneAsync("Login", UnityEngine.SceneManagement.LoadSceneMode.Single, true, AssetType.Scene);
        }
        public void OnTest1()
        {
            AssetManager.Instance.LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Single, true, AssetType.Scene);
        }
        public void OnTest2()
        {
            AssetManager.Instance.LoadAsset<GameObject>(out var handle, "Tips", AssetSuffix.Prefab, AssetType.Tool);
        }
        public void OnTest3()
        {
            //AssetManager.Instance.UnLoadAsset(handle);
            //ModelManager.Instance.GetModel<MainModel>().EnterGame();
        }
    }
}
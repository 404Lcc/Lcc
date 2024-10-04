using UnityEngine.UI;

namespace LccHotfix
{
    public class UILoginPanel : UILogicBase
    {
        public Button startBtn;
        public override void OnStart()
        {
            base.OnStart();
            startBtn.onClick.AddListener(OnStartBtn);
        }
        public override void OnPause()
        {
            base.OnPause();


        }
        public void OnStartBtn()
        {
            SceneManager.Instance.ChangeScene(SceneType.Main);
        }
    }
}
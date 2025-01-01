using LccModel;
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

            //如果不在提审状态
            if (!Launcher.Instance.IsAuditServer())
            {
                var isNoticeBoard = Launcher.Instance.CheckNoticeBoard();
                if (isNoticeBoard)
                {
                    //屏蔽登录
                    return;
                }
            }
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
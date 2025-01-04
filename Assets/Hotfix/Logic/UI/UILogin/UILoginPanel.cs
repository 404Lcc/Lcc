using LccModel;
using System.Collections;
using UnityEngine.UI;

namespace LccHotfix
{
    public class UILoginPanel : UILogicBase, ICoroutine
    {
        public Button startBtn;
        public override void OnStart()
        {
            base.OnStart();
            startBtn.onClick.AddListener(OnStartBtn);

            this.StartCoroutine(Open());
        }

        public IEnumerator Open()
        {
            yield return RequestMaintainTips();

            //如果不在提审状态
            if (!Launcher.Instance.IsAuditServer())
            {
                Launcher.Instance.OpenGameNoticeBoard(() => { /*WindowManager.Instance.OpenWindow<UINoticeBoardPanel>(UIWindowDefine.UINoticeBoardPanel);*/ });

                var isNoticeBoard = Launcher.Instance.CheckNoticeBoard();
                if (isNoticeBoard)
                {
                    Log.Debug("打开维护公告");
                    //屏蔽登录
                    yield break;
                }
            }
        }

        //请求维护公告
        public IEnumerator RequestMaintainTips()
        {
            yield return Launcher.Instance.GetNoticeBoard();
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
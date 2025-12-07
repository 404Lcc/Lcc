using LccModel;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class UILoginPanel : UIElementBase, ICoroutine
    {
        public Button startBtn;

        public override void OnInit()
        {
            base.OnInit();
            
            var e = Node as ElementNode;
            e.IsFullScreen = true;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            startBtn.onClick.AddListener(OnStartBtn);

            //this.StartCoroutine(Open());


        }

        public override void OnShow(object[] paramsList)
        {
            base.OnShow(paramsList);
            Main.WindowService.OpenWindow(UIPanelDefine.UIMainPanel, null);
            // Main.WindowService.OpenRoot(UIRootDefine.UIRootBattle, null);
        }



        public IEnumerator Open()
        {
            yield return RequestMaintainTips();

            //如果不在提审状态
            if (!Launcher.Instance.IsAuditServer())
            {
                Launcher.Instance.GameNotice.OpenGameNoticeBoard(() =>
                {
                    /*WindowManager.Instance.OpenWindow<UINoticeBoardPanel>(UIWindowDefine.UINoticeBoardPanel);*/
                });

                var isNoticeBoard = Launcher.Instance.GameNotice.CheckNoticeBoard();
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
            yield return Launcher.Instance.GameNotice.GetNoticeBoard();
        }


        public void OnStartBtn()
        {
            Main.WindowService.OpenWindow(UIPanelDefine.UIBattlePanel, null);
            // Main.ProcedureService.ChangeProcedure(ProcedureType.Main.ToInt());
        }
    }
}
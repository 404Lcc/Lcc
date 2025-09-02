using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class FsmLoadLanguage : IStateNode
    {
        private StateMachine _machine;
        
        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            Launcher.Instance.StartCoroutine(InitLanguage());
        }
        
        //游戏启动时第一次初始化语言
        private IEnumerator InitLanguage()
        {
            Launcher.Instance.GameLanguage.curLanguage = Launcher.Instance.GameLanguage.GetSelectedLanguage();

            // 加载多语言文本
            var txtRes = Resources.LoadAsync<TextAsset>("LanguageConfig_" + Launcher.Instance.GameLanguage.curLanguage);
            while (!txtRes.isDone)
                yield return null;
            TextAsset txtAsset = txtRes.asset as TextAsset;
            string languageTxt = null;
            if (txtAsset != null)
            {
                languageTxt = txtAsset.text;
                Resources.UnloadAsset(txtAsset);
            }

            yield return null;
            // 加载字体
            Launcher.Instance.GameLanguage.InitFontAsset();
            yield return null;
            Launcher.Instance.GameLanguage.OnLanguageAssetLoad(languageTxt);
            Launcher.Instance.GameLanguage.Set(Launcher.Instance.GameLanguage.curLanguage);

            Debug.Log("完成多语言初始化加载");

            UIForeGroundPanel.Instance.FadeOut(0.5f, null, false);

            UILoadingPanel.Instance.SetStartLoadingBg();
            
            _machine.ChangeState<FsmRequestServer>();
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
    }
}
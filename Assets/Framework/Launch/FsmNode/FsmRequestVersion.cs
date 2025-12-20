using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public class FsmRequestVersion : FsmLaunchStateNode
    {

        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(4);
            if (!GameConfig.IsEnablePatcher)
            {
                ChangeToNextState();
                return;
            }

            StartCoroutine(Request());
        }
        protected override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmInitializePackage>();
        }

        public IEnumerator Request()
        {
            yield return RequestVersion();
            yield return RequestVersionConfig();
            ChangeToNextState();
        }

        public IEnumerator RequestVersion()
        {
            string url = $"{GameConfig.CenterServerAddress}/{(GameConfig.IsRelease ? "Release" : "Dev")}/{ResPath.PlatformDirectory}/version.txt";

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
            web.timeout = 20;
            yield return web.SendWebRequest();

            if (!string.IsNullOrEmpty(web.error))
            {
                Debug.LogError($"RequestVersion 失败 url = {url}");
                LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                {
                    Content = StringTable.Get("Hint.RequestVersionFailed"),
                    btnOptionList = new List<UIPanelLaunch.MessageBoxOption> {
                        new ()
                        {
                            name = StringTable.Get("Op.Retry"),
                            action = OnRetry,
                        }
                    },
                });
            }
            else
            {
                string content = web.downloadHandler.text;
                PatchConfig.version = JsonUtility.ToObject<Version>(content);
            }
        }
        
        public IEnumerator RequestVersionConfig()
        {
            string url = $"{GameConfig.CenterServerAddress}/{(GameConfig.IsRelease ? "Release" : "Dev")}/{ResPath.PlatformDirectory}/versionConfig.txt";

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
            web.timeout = 20;
            yield return web.SendWebRequest();

            if (!string.IsNullOrEmpty(web.error))
            {
                Debug.LogError($"RequestVersionConfig 失败 url = {url}");
                LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                {
                    Content = StringTable.Get("Hint.RequestVersionFailed"),
                    btnOptionList = new List<UIPanelLaunch.MessageBoxOption> {
                        new ()
                        {
                            name = StringTable.Get("Op.Retry"),
                            action = OnRetry,
                        }
                    },
                });
            }
            else
            {
                string content = web.downloadHandler.text;
                PatchConfig.versionConfig = JsonUtility.ToObject<VersionConfig>(content);
            }
        }
        
        private void OnRetry()
        {
            _machine.ChangeState<FsmRequestVersion>();
        }
    }
}
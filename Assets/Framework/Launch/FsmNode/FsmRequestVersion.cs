using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public enum RequestState
    {
        None = 0,
        RequestVersionError,
        RequestVersionSuccess,
        RequestVersionConfigError,
        RequestVersionConfigSuccess,
    }

    public class FsmRequestVersion : FsmLaunchStateNode
    {
        private RequestState _state = RequestState.None;

        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(4);
            if (!GameConfig.IsEnablePatcher)
            {
                ChangeToNextState();
                return;
            }

            _state = RequestState.None;
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
            if (_state == RequestState.RequestVersionError)
            {
                yield break;
            }

            yield return RequestVersionConfig();
            if (_state == RequestState.RequestVersionConfigError)
            {
                yield break;
            }

            if (int.Parse(GameConfig.AppVersion) < PatchConfig.version.MinVersion)
            {
                LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                {
                    Content = StringTable.Get("Hint.BaseVersionTooLow"),
                    btnOptionList = new List<UIPanelLaunch.MessageBoxOption>
                    {
                        new()
                        {
                            name = StringTable.Get("Op.Quit"),
                            action = OnQuit,
                        }
                    },
                });
            }

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
                _state = RequestState.RequestVersionError;
                Debug.LogError($"RequestVersion 失败 url = {url}");
                LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                {
                    Content = StringTable.Get("Hint.RequestVersionFailed"),
                    btnOptionList = new List<UIPanelLaunch.MessageBoxOption>
                    {
                        new()
                        {
                            name = StringTable.Get("Op.Retry"),
                            action = OnRetry,
                        }
                    },
                });
            }
            else
            {
                _state = RequestState.RequestVersionSuccess;
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
                _state = RequestState.RequestVersionConfigError;
                Debug.LogError($"RequestVersionConfig 失败 url = {url}");
                LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                {
                    Content = StringTable.Get("Hint.RequestVersionFailed"),
                    btnOptionList = new List<UIPanelLaunch.MessageBoxOption>
                    {
                        new()
                        {
                            name = StringTable.Get("Op.Retry"),
                            action = OnRetry,
                        }
                    },
                });
            }
            else
            {
                _state = RequestState.RequestVersionConfigSuccess;
                string content = web.downloadHandler.text;
                PatchConfig.versionConfig = JsonUtility.ToObject<VersionConfig>(content);
            }
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnRetry()
        {
            _machine.ChangeState<FsmRequestVersion>();
        }
    }
}
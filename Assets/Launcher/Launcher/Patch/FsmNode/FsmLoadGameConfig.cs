using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public class FsmLoadGameConfig : IStateNode
    {
        public const string ConfigBuild = "GameConfig_Build.txt";
        public const string ConfigVersion = "GameConfig_Version.txt";
        public const string ConfigLanguage = "GameConfig_Language.txt";
        
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            Launcher.Instance.StartCoroutine(InitGameConfig());
        }
        
        private IEnumerator InitGameConfig()
        {
            yield return LoadBuildConfig();
            yield return LoadVersionConfig();
            yield return LoadLanguageConfig();
            
            if (Launcher.Instance.GameConfig.isReleaseCenterServer && Launcher.Instance.GameConfig.isRelease)
            {
                //走sdk渠道号
                Launcher.Instance.GameConfig.AddConfig("channel", Launcher.Instance.GameConfig.channel);//todo 重新设置渠道 通过sdk获取
            }

            Debug.Log("Local GameConfig.appVersion:" + Launcher.Instance.GameConfig.appVersion);
            Debug.Log("Local GameConfig.channel:" + Launcher.Instance.GameConfig.channel);
            Debug.Log("Local GameConfig.resVersion:" + Launcher.Instance.GameConfig.resVersion);
            Debug.Log("Local GameConfig.centerServerAddress:" + Launcher.Instance.GameConfig.centerServerAddress);
            Debug.Log("Local GameConfig.isReleaseCenterServer:" + Launcher.Instance.GameConfig.isReleaseCenterServer);
            Debug.Log("Local GameConfig.isRelease:" + Launcher.Instance.GameConfig.isRelease);
            Debug.Log("Local GameConfig.chargeDirect:" + Launcher.Instance.GameConfig.chargeDirect);
            Debug.Log("Local GameConfig.selectServer:" + Launcher.Instance.GameConfig.selectServer);
            Debug.Log("Local GameConfig.checkResUpdate:" + Launcher.Instance.GameConfig.checkResUpdate);
            Debug.Log("Local GameConfig.useSDK:" + Launcher.Instance.GameConfig.useSDK);
            
            //本地版本
            var showApp = int.Parse(Application.version.Split('.')[0]);
            UILoadingPanel.Instance.SetVersion("version " + showApp + "." + Launcher.Instance.GameConfig.appVersion + "." + Launcher.Instance.GameConfig.channel + "." + Launcher.Instance.GameConfig.resVersion);

            // _machine.ChangeState<FsmLoadLanguage>();
        }
        private IEnumerator LoadBuildConfig()
        {
            string configUrl = $"{ResPath.ResStreamingPathWeb}/{ConfigBuild}";
            UnityWebRequest req = UnityWebRequest.Get(configUrl);
            yield return req.SendWebRequest();
            if (!string.IsNullOrEmpty(req.error))
            {
                Debug.LogError("load build error:" + req.error);
                yield break;
            }

            try
            {
                byte[] bytes = req.downloadHandler.data;
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                Launcher.Instance.GameConfig.ReadBuild(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }
        private IEnumerator LoadVersionConfig()
        {
            string configUrl = $"{ResPath.ResStreamingPathWeb}/{ConfigVersion}";
            UnityWebRequest req = UnityWebRequest.Get(configUrl);
            yield return req.SendWebRequest();
            if (!string.IsNullOrEmpty(req.error))
            {
                Debug.LogError("load version error:" + req.error);
                yield break;
            }

            try
            {
                byte[] bytes = req.downloadHandler.data;
                bytes = bytes.ByteXOR(BitConverter.GetBytes(Launcher.Instance.GameConfig.buildTime));
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                Launcher.Instance.GameConfig.ReadVersion(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }


        public IEnumerator LoadLanguageConfig()
        {
            string configUrl = $"{ResPath.ResStreamingPathWeb}/{ConfigLanguage}";
            UnityWebRequest req = UnityWebRequest.Get(configUrl);
            yield return req.SendWebRequest();
            if (!string.IsNullOrEmpty(req.error))
            {
                Debug.LogError("load language error:" + req.error);
                yield break;
            }

            try
            {
                byte[] bytes = req.downloadHandler.data;
                bytes = bytes.ByteXOR(BitConverter.GetBytes(Launcher.Instance.GameConfig.buildTime));
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                Launcher.Instance.GameConfig.ReadLanguage(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }
        
        public void OnUpdate()
        {
            
        }
        public void OnExit()
        {

        }
    }
}
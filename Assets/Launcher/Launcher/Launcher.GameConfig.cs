using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public partial class Launcher
    {
        private GameConfig mGameConfig;
        public static GameConfig GameConfig
        {
            get => Instance.mGameConfig;
        }

        private const string configBuild = "GameConfig_Build.txt";
        private const string configVersion = "GameConfig_Version.txt";
        private const string configLanguage = "GameConfig_Language.txt";

        private IEnumerator InitGameConfig()
        {
            mGameConfig = new GameConfig();
            yield return StartCoroutine(LoadBuildConfig());
            yield return StartCoroutine(LoadVersionConfig());
            yield return StartCoroutine(LoadLanguageConfig());
        }
        private IEnumerator LoadBuildConfig()
        {
            string configUrl = PathUtility.GetStreamingAssetsPathWeb(configBuild);
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
                mGameConfig.ReadBuild(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }
        private IEnumerator LoadVersionConfig()
        {
            string configUrl = PathUtility.GetStreamingAssetsPathWeb(configVersion);
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
                bytes = bytes.ByteXOR(BitConverter.GetBytes(mGameConfig.buildTime));
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                mGameConfig.ReadVersion(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }


        public IEnumerator LoadLanguageConfig()
        {
            string configUrl = PathUtility.GetStreamingAssetsPathWeb(configLanguage);
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
                bytes = bytes.ByteXOR(BitConverter.GetBytes(mGameConfig.buildTime));
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                mGameConfig.ReadLanguage(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }
    }
}
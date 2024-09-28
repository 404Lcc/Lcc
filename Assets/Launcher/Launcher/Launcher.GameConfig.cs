using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public partial class Launcher
    {
        private GameConfig _gameConfig;
        public static GameConfig GameConfig => Instance._gameConfig;

        public const string ConfigBuild = "GameConfig_Build.txt";
        public const string ConfigVersion = "GameConfig_Version.txt";
        public const string ConfigLanguage = "GameConfig_Language.txt";

        private IEnumerator InitGameConfig()
        {
            _gameConfig = new GameConfig();
            yield return StartCoroutine(LoadBuildConfig());
            yield return StartCoroutine(LoadVersionConfig());
            yield return StartCoroutine(LoadLanguageConfig());
        }
        private IEnumerator LoadBuildConfig()
        {
            string configUrl = PathUtility.GetStreamingAssetsPathWeb(ConfigBuild);
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
                _gameConfig.ReadBuild(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }
        private IEnumerator LoadVersionConfig()
        {
            string configUrl = PathUtility.GetStreamingAssetsPathWeb(ConfigVersion);
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
                bytes = bytes.ByteXOR(BitConverter.GetBytes(_gameConfig.buildTime));
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                _gameConfig.ReadVersion(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }


        public IEnumerator LoadLanguageConfig()
        {
            string configUrl = PathUtility.GetStreamingAssetsPathWeb(ConfigLanguage);
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
                bytes = bytes.ByteXOR(BitConverter.GetBytes(_gameConfig.buildTime));
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                _gameConfig.ReadLanguage(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }
    }
}
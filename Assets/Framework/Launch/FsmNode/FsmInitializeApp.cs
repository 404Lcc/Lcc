using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmInitializeApp : FsmLaunchStateNode
    {
        public const string ConfigBuild = "GameConfig_Build";
        public const string ConfigVersion = "GameConfig_Version";
        public const string ConfigLanguage = "GameConfig_Language";

        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(1);

            LoadBuildConfig();
            LoadVersionConfig();
            LoadLanguageConfig();

            if (Application.isEditor)
                AssetConfig.PlayMode = GameConfig.IsEnablePatcher ? EPlayMode.HostPlayMode : EPlayMode.EditorSimulateMode;
            else
                AssetConfig.PlayMode = GameConfig.IsEnablePatcher ? EPlayMode.HostPlayMode : EPlayMode.OfflinePlayMode;

            TextAsset jStr = Resources.Load<TextAsset>($"Launch/Strings/launch_strings_{GameConfig.AppLanguage}");
            StringTable.Strings = JsonUtility.ToObject<Dictionary<string, string>>(jStr.text);

            ChangeToNextState();
        }

        private void LoadBuildConfig()
        {
            try
            {
                byte[] bytes = Resources.Load<TextAsset>(ConfigBuild).bytes;
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                GameConfig.ReadBuild(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private void LoadVersionConfig()
        {
            try
            {
                byte[] bytes = Resources.Load<TextAsset>(ConfigVersion).bytes;
                bytes = bytes.ByteXOR(BitConverter.GetBytes(GameConfig.BuildTime));
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                GameConfig.ReadVersion(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public void LoadLanguageConfig()
        {
            try
            {
                byte[] bytes = Resources.Load<TextAsset>(ConfigLanguage).bytes;
                bytes = bytes.ByteXOR(BitConverter.GetBytes(GameConfig.BuildTime));
                string text = System.Text.UnicodeEncoding.UTF8.GetString(bytes);
                GameConfig.ReadLanguage(text);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        protected override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmStartSplash>();
        }
    }
}
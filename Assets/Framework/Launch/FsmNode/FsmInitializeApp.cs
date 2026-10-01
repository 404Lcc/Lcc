using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmInitializeApp : FsmLaunchStateNode
    {
        public const string ConfigBuild = "GameConfig_Build";
        public const string ConfigVersion = "GameConfig_Version";

        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(1);

            LoadBuildConfig();
            LoadVersionConfig();

            // 对齐 trunk：Settings > 系统语言 > 默认语言
            {
                var languageName = FileHelper.LoadString(AppConfig.LocalizationSettingsNamePath);
                if (string.IsNullOrEmpty(languageName))
                {
                    if (AppConfig.LanguageDict.TryGetValue(Application.systemLanguage, out languageName))
                        AppConfig.AppLanguage = languageName;
                    else
                        AppConfig.AppLanguage = AppConfig.DefaultLanguage;
                }
                else
                {
                    AppConfig.AppLanguage = languageName;
                }
            }

            InitFontAsset();

            if (Application.isEditor)
                AssetConfig.PlayMode = GameConfig.IsEnablePatcher ? EPlayMode.HostPlayMode : EPlayMode.EditorSimulateMode;
            else
                AssetConfig.PlayMode = GameConfig.IsEnablePatcher ? EPlayMode.HostPlayMode : EPlayMode.OfflinePlayMode;

            TextAsset jStr = Resources.Load<TextAsset>($"Launch/Strings/launch_strings_{AppConfig.AppLanguage}");
            if (jStr != null)
                StringTable.Strings = JsonUtility.ToObject<Dictionary<string, string>>(jStr.text);
            else
                Debug.LogError($"[Launch] Missing launch_strings_{AppConfig.AppLanguage}");

            ChangeToNextState();
        }

        public void InitFontAsset()
        {
            TMP_FontAsset defaultFontAsset = Resources.Load<TMP_FontAsset>("Fonts/Font SDF");
            if (defaultFontAsset == null)
            {
                Debug.LogError("[Launch] Missing Fonts/Font SDF");
                return;
            }

            Font fontAssetCur = Resources.Load<Font>("Fonts/" + AppConfig.AppLanguage);
            if (fontAssetCur == null)
                fontAssetCur = Resources.Load<Font>("Fonts/font_default");
            if (fontAssetCur == null)
            {
                Debug.LogError($"[Launch] Missing Fonts/{AppConfig.AppLanguage} and Fonts/font_default");
                return;
            }

            defaultFontAsset.ClearFontAssetData(true);
            defaultFontAsset.characterLookupTable.Clear();
            defaultFontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                fontAssetCur, 50, 8,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                2048, 2048, AtlasPopulationMode.Dynamic);
            defaultFontAsset.fallbackFontAssetTable.Clear();
            defaultFontAsset.fallbackFontAssetTable.Add(fontAsset);
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

        protected override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmStartSplash>();
        }
    }
}

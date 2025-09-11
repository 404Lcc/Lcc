using TMPro;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace LccEditor
{
    public class EditorStartup
    {
        public const string DefaultFont = "Font SDF"; // 默认字体资源

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [DidReloadScripts]
        public static void AllScriptsCompiled()
        {
            if (Application.isBatchMode == false)
            {
                System.Func<System.Threading.Tasks.Task> func = async () =>
                {
                    await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(0.1));

                    InitFontAsset();
                };
                func();
            }
        }

        public static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                InitFontAsset();
            }
            else if (state == PlayModeStateChange.ExitingEditMode)
            {

            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {

            }
        }

        public static void InitFontAsset()
        {
            TMP_FontAsset defaultFontAsset = Resources.Load<TMP_FontAsset>("Fonts/" + DefaultFont);
            Font fontAssetCur = Resources.Load<Font>("Fonts/font_default");

            //清除之前用的字体资源数据和字符，使fallback略过该字体
            //删除texture至一张， 且将尺寸置为0
            defaultFontAsset.ClearFontAssetData(true);
            defaultFontAsset.characterLookupTable.Clear();

            //设置目标字体为静态生成，阻止新的字符生成
            defaultFontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(fontAssetCur, 50, 8, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
            defaultFontAsset.fallbackFontAssetTable.Clear();
            defaultFontAsset.fallbackFontAssetTable.Add(fontAsset);
        }
    }
}
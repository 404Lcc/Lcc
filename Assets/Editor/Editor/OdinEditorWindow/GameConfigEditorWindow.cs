using LccModel;
using Sirenix.OdinInspector;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [MenuTree("框架配置", 7)]
    public class GameConfigEditorWindow : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "框架配置";
        public GameConfigEditorWindow()
        {
        }
        public GameConfigEditorWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }

        [PropertySpace(10)]
        [LabelText("加密GameConfig"), Button(ButtonSizes.Gigantic, Name = "加密GameConfig")]
        public void GameConfig()
        {
            var dtNow = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            FileUtility.SaveAsset("Assets/Resources/GameConfig_Build.txt", dtNow.ToString());

            var time = long.Parse(Encoding.UTF8.GetString(FileUtility.GetAsset("Assets/Resources/GameConfig_Build.txt")));
            var bytes = BitConverter.GetBytes(time);

            TextAsset version = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameConfig/GameConfig_Version.txt");
            FileUtility.SaveAsset("Assets/Resources/GameConfig_Version.txt", version.bytes.ByteXOR(bytes));

            TextAsset language = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameConfig/GameConfig_Language.txt");
            FileUtility.SaveAsset("Assets/Resources/GameConfig_Language.txt", language.bytes.ByteXOR(bytes));
            AssetDatabase.SaveAssets();
        }

        [PropertySpace(10)]
        [LabelText("解密GameConfig"), Button(ButtonSizes.Gigantic, Name = "解密GameConfig")]
        public void DecryptGameConfig()
        {
            var time = long.Parse(Encoding.UTF8.GetString(FileUtility.GetAsset("Assets/Resources/GameConfig_Build.txt")));
            var bytes = BitConverter.GetBytes(time);

            var version = FileUtility.GetAsset("Assets/Resources/GameConfig_Version.txt").ByteXOR(bytes).Utf8ToStr();
            Debug.Log("version=" + version);
            var language = FileUtility.GetAsset("Assets/Resources/GameConfig_Language.txt").ByteXOR(bytes).Utf8ToStr();
            Debug.Log("language=" + language);

        }
    }
}
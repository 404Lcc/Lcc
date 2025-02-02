using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class UIEditorWindow : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "UI工具";

        public UIEditorWindow()
        {
        }
        public UIEditorWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }



        [PropertySpace(10)]
        [LabelText("UIDev"), Button(ButtonSizes.Gigantic, Name = "UIDev")]
        public void UIDev()
        {
            var defaultFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Fonts/Font SDF.asset");

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



        [PropertySpace(10)]
        [LabelText("UI/清理UI内容"), Button(ButtonSizes.Gigantic, Name = "清理UI内容")]
        public void ReplaceAllTxt()
        {
            try
            {
                List<GameObject> prefabList = GetAllUIPrefab();

                EditorUtility.DisplayProgressBar("替换全部UI内容", "替换中...", 0);

                for (int i = 0; i < prefabList.Count; i++)
                {
                    ClearText(prefabList[i].transform);

                    EditorUtility.DisplayProgressBar("替换全部UI内容", "替换中...", ((float)i / prefabList.Count));

                    PrefabUtility.SavePrefabAsset(prefabList[i]);
                }

                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"Error: {e.StackTrace}");
            }
        }


        [PropertySpace(10)]
        [LabelText("UI/替换全部UI字体"), Button(ButtonSizes.Gigantic, Name = "替换全部UI字体")]
        public void ReplaceALLFont()
        {
            List<GameObject> prefabList = GetAllUIPrefab();

            EditorUtility.DisplayProgressBar("替换字体", "替换中...", 0);

            for (int i = 0; i < prefabList.Count; i++)
            {
                ReplaceFont(prefabList[i].transform);

                EditorUtility.DisplayProgressBar("替换字体", "替换中...", ((float)i / prefabList.Count));

                PrefabUtility.SavePrefabAsset(prefabList[i]);
            }

            EditorUtility.ClearProgressBar();
        }

        private List<GameObject> GetAllUIPrefab()
        {
            List<GameObject> list = new List<GameObject>();
            foreach (var item in AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Res/Prefab/Panel" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(item);
                var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (prefab != null)
                {
                    list.Add(prefab);
                }
            }

            return list;
        }
        private void ReplaceFont(Transform transform)
        {
            var tmp = transform.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Fonts/Font SDF.asset");
                tmp.font = font;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                ReplaceFont(transform.GetChild(i));
            }
        }



        private void ClearText(Transform transform)
        {
            var tmp = transform.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = string.Empty;
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                ClearText(transform.GetChild(i));
            }
        }
    }
}
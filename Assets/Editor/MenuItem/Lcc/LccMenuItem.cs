using LccHotfix;
using LccModel;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;

namespace LccEditor
{
    public class LccMenuItem
    {
        [MenuItem("工具箱/重启游戏 _F5")]
        private static void ReturnToStart()
        {
            HotfixFunc.CallPublicStaticMethod("LccHotfix", "Init", "ReturnToStart");
        }

        [MenuItem("工具箱/退出账号 _F6")]
        private static void ReturnToLogin()
        {
            HotfixFunc.CallPublicStaticMethod("LccHotfix", "Init", "ReturnToLogin");
        }

        [MenuItem("GameObject/UI/垂直滑动列表")]
        public static void CreateLoopVerticalScroll()
        {
            if (Selection.activeTransform)
            {
                GameObject obj = new GameObject("loopScroll");

                var bg = obj.AddComponent<Image>();
                obj.AddComponent(typeof(RectMask2D));

                var ex = obj.AddComponent<ScrollerProEx>();
                var pro = obj.AddComponent<ScrollerPro>();
                ex.horizontal = false;
                ex.vertical = true;
                pro.needScroller = true;

                bg.color = new Color(0, 0, 0, 0);
                bg.raycastTarget = true;

                GameObject item = new GameObject("item");
                item.AddComponent<RectTransform>();
                item.transform.SetParent(obj.transform, false);

                obj.transform.SetParent(Selection.activeTransform, false);
                Selection.activeTransform = obj.transform;
            }
        }

        [MenuItem("GameObject/UI/水平滑动列表")]
        public static void CreateLoopHorizontalScroll()
        {
            if (Selection.activeTransform)
            {
                GameObject obj = new GameObject("loopScroll");

                var bg = obj.AddComponent<Image>();
                obj.AddComponent(typeof(RectMask2D));

                var ex = obj.AddComponent<ScrollerProEx>();
                var pro = obj.AddComponent<ScrollerPro>();
                ex.horizontal = true;
                ex.vertical = false;
                pro.needScroller = true;

                bg.color = new Color(0, 0, 0, 0);
                bg.raycastTarget = true;

                GameObject item = new GameObject("item");
                item.AddComponent<RectTransform>();
                item.transform.SetParent(obj.transform, false);

                obj.transform.SetParent(Selection.activeTransform, false);
                Selection.activeTransform = obj.transform;
            }
        }

        [MenuItem("GameObject/UI/固定滑动列表")]
        public static void CreateNoneLoopScroll()
        {
            if (Selection.activeTransform)
            {
                GameObject obj = new GameObject("loopScroll");

                var bg = obj.AddComponent<Image>();
                obj.AddComponent(typeof(RectMask2D));

                var ex = obj.AddComponent<ScrollerProEx>();
                var pro = obj.AddComponent<ScrollerPro>();
                ex.horizontal = false;
                ex.vertical = false;
                pro.needScroller = false;

                bg.color = new Color(0, 0, 0, 0);
                bg.raycastTarget = true;

                GameObject item = new GameObject("item");
                item.AddComponent<RectTransform>();
                item.transform.SetParent(obj.transform, false);

                obj.transform.SetParent(Selection.activeTransform, false);
                Selection.activeTransform = obj.transform;
            }
        }

        //[MenuItem("Assets/工具箱/热更Panel", false, 31)]
        //public static void CreateHotfixPanel()
        //{
        //    string pathName = $"{CreateScriptUtility.GetSelectedPath()}/NewHotfixPanel.cs";
        //    Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
        //    ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtility>(), pathName, icon, "Assets/Editor/Utility/CreateScript/Template/HotfixPanelTemplate.txt");
        //}
        //[MenuItem("Assets/工具箱/热更ViewModel", false, 32)]
        //public static void CreateHotfixViewModel()
        //{
        //    string pathName = $"{CreateScriptUtility.GetSelectedPath()}/NewHotfixViewModel.cs";
        //    Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
        //    ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtility>(), pathName, icon, "Assets/Editor/Utility/CreateScript/Template/HotfixViewModelTemplate.txt");
        //}
        //[MenuItem("Assets/工具箱/主工程Panel", false, 33)]
        //public static void CreateModelPanel()
        //{
        //    string pathName = $"{CreateScriptUtility.GetSelectedPath()}/NewModelPanel.cs";
        //    Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
        //    ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtility>(), pathName, icon, "Assets/Editor/Utility/CreateScript/Template/ModelPanelTemplate.txt");
        //}
        //[MenuItem("Assets/工具箱/主工程ViewModel", false, 34)]
        //public static void CreateModelViewModel()
        //{
        //    string pathName = $"{CreateScriptUtility.GetSelectedPath()}/NewModelViewModel.cs";
        //    Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
        //    ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtility>(), pathName, icon, "Assets/Editor/Utility/CreateScript/Template/ModelViewModelTemplate.txt");
        //}
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ModelBindPointSettings", menuName = "ModelBindPoint/Create Model Bind Point Settings")]
public class ModelBindPointSettings : ScriptableObject
{
    [Header("挂点名称列表")]
    public string[] BindPointNameList;

    [Header("模型根目录")]
    [Tooltip("直接拖拽文件夹到这里")]
    public DefaultAsset ModelRootPath;

    [Header("文件夹路径（只读）")]
    [SerializeField, TextArea(1, 2)]
    private string folderPath;

    [Header("代码生成路径")]
    [SerializeField, TextArea(1, 2)]
    public string CodeGeneratePath = "/Hotfix/GameLogic/Utility/Model/";

    public string[] PrefabPaths;


    // 公共属性，用于获取路径
    public string FolderPath
    {
        get
        {
#if UNITY_EDITOR
            if (ModelRootPath != null)
            {
                return AssetDatabase.GetAssetPath(ModelRootPath);
            }
#endif
            return folderPath;
        }
    }

    // 在 Inspector 修改时自动更新路径
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (ModelRootPath != null)
        {
            string path = AssetDatabase.GetAssetPath(ModelRootPath);
            if (AssetDatabase.IsValidFolder(path))
            {
                folderPath = path;
            }
            else
            {
                Debug.LogWarning("请选择文件夹而不是文件！");
                ModelRootPath = null;
            }
        }
        else
        {
            folderPath = string.Empty;
        }
#endif
    }

    public void Refresh()
    {
        if (ModelRootPath != null)
        {
            string path = AssetDatabase.GetAssetPath(ModelRootPath);
            var newObjs = GetAllPrefabs(path);
            PrefabPaths = new string[newObjs.Length];
            for (int i = 0; i < newObjs.Length; i++)
            {
                PrefabPaths[i] = AssetDatabase.GetAssetPath(newObjs[i]);
            }
        }
    }
    
    private GameObject[] GetAllPrefabs(string folderPath)
    {
        List<GameObject> objs = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null && !objs.Contains(prefab))
            {
                objs.Add(prefab);
            }
        }

        return objs.ToArray();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ModelBindPointSettings))]
public class ModelBindPointSettingsEditor : UnityEditor.Editor
{
    private SerializedProperty nameListProperty;
    private SerializedProperty codeGeneratePathProperty;
    private Vector2 nameListScroll;

    private Vector2 prefabNameScroll;
    
    private void OnEnable()
    {
        nameListProperty = serializedObject.FindProperty("BindPointNameList");
        codeGeneratePathProperty = serializedObject.FindProperty("CodeGeneratePath");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ModelBindPointSettings config = (ModelBindPointSettings)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("文件夹选择", EditorStyles.boldLabel);

        // 使用 PropertyField 确保序列化
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ModelRootPath"), 
            new GUIContent("目标文件夹"));

        // 显示路径（只读）
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("文件夹路径", config.FolderPath);
        EditorGUI.EndDisabledGroup();

        // 辅助按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("在资源管理器中显示"))
        {
            if (!string.IsNullOrEmpty(config.FolderPath))
            {
                string absolutePath = System.IO.Path.GetFullPath(config.FolderPath);
                System.Diagnostics.Process.Start("explorer.exe", absolutePath);
            }
        }

        if (GUILayout.Button("清空引用"))
        {
            serializedObject.FindProperty("ModelRootPath").objectReferenceValue = null;
            serializedObject.FindProperty("folderPath").stringValue = string.Empty;
        }

        
        EditorGUILayout.EndHorizontal();

        // 验证当前引用
        if (config.ModelRootPath != null)
        {
            string path = AssetDatabase.GetAssetPath(config.ModelRootPath);
            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorGUILayout.HelpBox("当前引用不是有效的文件夹！", MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新"))
            {
                var newObjs = GetAllPrefabs(path);
                config.PrefabPaths = new string[newObjs.Length];
            }
            // 获取文件夹下所有的prefab
            var objs = GetAllPrefabs(path);

            EditorGUILayout.LabelField($"检测到的Prefab数量: {objs.Length}");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            prefabNameScroll = EditorGUILayout.BeginScrollView(prefabNameScroll, GUILayout.Height(200));
            config.PrefabPaths = new string[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                config.PrefabPaths[i] = AssetDatabase.GetAssetPath(objs[i]);
                EditorGUILayout.BeginHorizontal();

                // 索引标签
                EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(30));

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(objs[i].name);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }
        
        
        
        if (nameListProperty != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"挂点数量: {nameListProperty.arraySize}");
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                nameListProperty.arraySize++;
            }
            if (GUILayout.Button("清空", GUILayout.Width(60)))
            {
                nameListProperty.arraySize = 0;
            }
            EditorGUILayout.EndHorizontal();
            
            nameListScroll = EditorGUILayout.BeginScrollView(nameListScroll, GUILayout.Height(200));
            
            for (int i = 0; i < nameListProperty.arraySize; i++)
            {
                SerializedProperty element = nameListProperty.GetArrayElementAtIndex(i);
                
                EditorGUILayout.BeginHorizontal();
                
                // 索引标签
                EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(30));
                
                // 名称输入框
                EditorGUILayout.PropertyField(element, GUIContent.none);
                
                // 删除按钮
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    nameListProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(codeGeneratePathProperty, new GUIContent("代码生成路径: "));
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }

    private GameObject[] GetAllPrefabs(string folderPath)
    {
        List<GameObject> objs = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null && !objs.Contains(prefab))
            {
                objs.Add(prefab);
            }
        }

        return objs.ToArray();
    }
}
#endif

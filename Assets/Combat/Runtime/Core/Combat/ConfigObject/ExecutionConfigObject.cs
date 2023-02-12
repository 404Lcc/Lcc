using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LccModel
{
    [CreateAssetMenu(fileName = "Execution", menuName = "技能|状态/Execution")]
    public class ExecutionConfigObject : ScriptableObject
    {
        public string Id;
        public float TotalTime;
        public ExecutionTargetInputType TargetInputType;

        [Space(10)]
        [ListDrawerSettings(DraggableItems = false, ShowItemCount = false, CustomAddFunction = "AddExecuteClipData")]
        public List<ExecuteClipData> ExecuteClipDataList = new List<ExecuteClipData>();
        private void AddExecuteClipData()
        {
            var obj = CreateInstance<ExecuteClipData>();
            obj.name = "ExecuteClipData";
            obj.ExecuteClipType = ExecuteClipType.CollisionExecute;
            obj.CollisionExecuteData = new CollisionExecuteData();
            obj.GetClipTime().EndTime = 0.1f;
            ExecuteClipDataList.Add(obj);
            AssetDatabase.AddObjectToAsset(obj, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
#if UNITY_EDITOR
        #region 自动命名
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (UnityEditor.Selection.assetGUIDs.Length == 1)
            {
                string guid = UnityEditor.Selection.assetGUIDs[0];
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var config = UnityEditor.AssetDatabase.LoadAssetAtPath<ExecutionConfigObject>(assetPath);
                if (config != this)
                {
                    return;
                }
                var oldName = Path.GetFileNameWithoutExtension(assetPath);
                var newName = $"Execution_{this.Id}";
                if (oldName != newName)
                {
                    UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
                }
            }
        }
        #endregion
#endif
    }
}
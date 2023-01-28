using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LccModel
{
    [CreateAssetMenu(fileName = "Execution", menuName = "技能|状态/Execution")]
    public class ExecutionObject : ScriptableObject
    {
        [DelayedProperty]
        public string Id;
        public float TotalTime;

        [ReadOnly, Space(10)]
        public List<ExecuteClipData> ExecuteClips = new List<ExecuteClipData>();

#if UNITY_EDITOR


        #region 自动命名
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (UnityEditor.Selection.assetGUIDs.Length == 1)
            {
                string guid = UnityEditor.Selection.assetGUIDs[0];
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var config = UnityEditor.AssetDatabase.LoadAssetAtPath<ExecutionObject>(assetPath);
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
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LccModel
{
    [CreateAssetMenu(fileName = "Execution", menuName = "����|״̬/Execution")]
    public class ExecutionObject : ScriptableObject
    {
        [DelayedProperty]
        public string Id;
        public float TotalTime;
        //public string ObjAssetName;
        //[OnValueChanged("OnValueChanged")]
        [DelayedProperty]
        public GameObject ObjAsset;
        public ExecutionTargetInputType TargetInputType;
        [ShowIf("TargetInputType", ExecutionTargetInputType.Point)]
        [LabelText("��Χָʾ��")]
        public GameObject RangeIndicatorObjAsset;
        [ShowIf("TargetInputType", ExecutionTargetInputType.Point)]
        [LabelText("Ŀ���ָʾ��")]
        public GameObject PointIndicatorObjAsset;
        [ShowIf("TargetInputType", ExecutionTargetInputType.Point)]
        [LabelText("����ָʾ��")]
        public GameObject DirectionIndicatorObjAsset;
        //public string BindSkillName;
        //public SkillConfigObject BindSkill;
        //public ExecutionItem ExecutionItem;
        //[PreviouslySerializedAs("ExecutionClips")]
        [ReadOnly, Space(10)]
        public List<ExecuteClipData> ExecuteClips = new List<ExecuteClipData>();

#if UNITY_EDITOR
        //private void OnValueChanged()
        //{
        //    if (ObjAsset != null)
        //    {
        //        ObjAssetName = ObjAsset.name;
        //    }
        //}

        //[Button("Save Clips")]
        private void SaveClips()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            //Log.Debug("SaveObject");
        }

        private void BeginBox()
        {
            GUILayout.Space(20);
            if (GUILayout.Button("Save Clips"))
            {
                SaveClips();
            }
            GUILayout.Space(10);
            SirenixEditorGUI.DrawThickHorizontalSeparator();
            GUILayout.Space(10);
        }

        [OnInspectorGUI("BeginBox", append: false)]
        [SerializeField, LabelText("�Զ�������")]
        public bool AutoRename { get { return StatusConfigObject.AutoRenameStatic; } set { StatusConfigObject.AutoRenameStatic = value; } }

        private void OnEnable()
        {
            StatusConfigObject.AutoRenameStatic = UnityEditor.EditorPrefs.GetBool("AutoRename", true);
        }

        private void OnDisable()
        {
            UnityEditor.EditorPrefs.SetBool("AutoRename", StatusConfigObject.AutoRenameStatic);
        }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (!AutoRename)
            {
                return;
            }

            RenameFile();
        }

        [Button("�����������ļ�"), HideIf("AutoRename")]
        private void RenameFile()
        {
            string[] guids = UnityEditor.Selection.assetGUIDs;
            int i = guids.Length;
            if (i == 1)
            {
                string guid = guids[0];
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var so = UnityEditor.AssetDatabase.LoadAssetAtPath<ExecutionObject>(assetPath);
                if (so != this)
                {
                    return;
                }
                var fileName = System.IO.Path.GetFileName(assetPath);
                var newName = $"Execution_{this.Id}";
                if (!fileName.StartsWith(newName))
                {
                    UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
                }
            }
        }
#endif
    }
}

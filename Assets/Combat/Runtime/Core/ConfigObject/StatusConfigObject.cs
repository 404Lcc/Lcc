﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LccModel
{
    public class ChildStatus
    {
        [LabelText("状态效果")]
        public StatusConfigObject StatusConfigObject;

        //public ET.StatusConfig StatusConfig { get; set; }

        [LabelText("参数列表"), HideReferenceObjectPicker]
        public Dictionary<string, string> Params = new Dictionary<string, string>();
    }

    public enum StatusType
    {
        [LabelText("Buff(增益)")]
        Buff,
        [LabelText("Debuff(减益)")]
        Debuff,
        [LabelText("其他")]
        Other,
    }


    [CreateAssetMenu(fileName = "状态配置", menuName = "技能|状态/状态配置")]
    public class StatusConfigObject
#if !NOT_UNITY
        : SerializedScriptableObject
#endif
    {
        [LabelText(StatusIdLabel), DelayedProperty]
        public string ID = "Status1";
        [LabelText(StatusNameLabel), DelayedProperty]
        public string Name = "状态1";
        [LabelText(StatusTypeLabel)]
        public StatusType StatusType;
        [HideInInspector]
        public uint Duration;
        [LabelText("是否在状态栏显示"), UnityEngine.Serialization.FormerlySerializedAs("ShowInStatusIconList")]
        public bool ShowInStatusSlots;
        [LabelText("能否叠加")]
        public bool CanStack;
        [LabelText("最高叠加层数"), ShowIf("CanStack"), Range(0, 99)]
        public int MaxStack = 0;

        [LabelText("子状态效果")]
        public bool EnableChildrenStatuses;
        [OnInspectorGUI("DrawSpace", append: true)]
        [HideReferenceObjectPicker]
        [LabelText("子状态效果列表"), ShowIf("EnableChildrenStatuses"), ListDrawerSettings(DraggableItems = false, ShowItemCount = false, CustomAddFunction = "AddChildStatus")]
        public List<ChildStatus> ChildrenStatuses = new List<ChildStatus>();

        private void AddChildStatus()
        {
            ChildrenStatuses.Add(new ChildStatus());
        }

        [LabelText("效果列表"), Space(30)]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, ShowItemCount = false, HideAddButton = true)]
        [HideReferenceObjectPicker]
        public List<Effect> Effects = new List<Effect>();


        [HorizontalGroup(PaddingLeft = 40, PaddingRight = 40)]
        [HideLabel, OnValueChanged("AddEffect"), ValueDropdown("EffectTypeSelect")]
        public string EffectTypeName = "(添加效果)";

        public IEnumerable<string> EffectTypeSelect()
        {
            var types = typeof(Effect).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(Effect).IsAssignableFrom(x))
                .Where(x => x.GetCustomAttribute<EffectAttribute>() != null)
                .OrderBy(x => x.GetCustomAttribute<EffectAttribute>().Order)
                .Select(x => x.GetCustomAttribute<EffectAttribute>().EffectType);

            var results = types.ToList();
            results.Insert(0, "(添加效果)");
            return results;
        }

        private void AddEffect()
        {
            if (EffectTypeName != "(添加效果)")
            {
                var effectType = typeof(Effect).Assembly.GetTypes()
                    .Where(x => !x.IsAbstract)
                    .Where(x => typeof(Effect).IsAssignableFrom(x))
                    .Where(x => x.GetCustomAttribute<EffectAttribute>() != null)
                    .Where(x => x.GetCustomAttribute<EffectAttribute>().EffectType == EffectTypeName)
                    .FirstOrDefault();
                var effect = Activator.CreateInstance(effectType) as Effect;
                effect.Enabled = true;

                Effects.Add(effect);
                EffectTypeName = "(添加效果)";
            }
        }

#if !NOT_UNITY
        [LabelText("状态特效")]
        [OnInspectorGUI("BeginBox", append:false)]
        public GameObject ParticleEffect;

        public GameObject GetParticleEffect() => ParticleEffect;

        [LabelText("状态音效")]
        [OnInspectorGUI("EndBox", append:true)]
        public AudioClip Audio;

        [TextArea, LabelText("状态描述")]
        public string StatusDescription;
#endif
        
#if UNITY_EDITOR
        [SerializeField, LabelText("自动重命名")]
        public bool AutoRename { get { return AutoRenameStatic; } set { AutoRenameStatic = value; } }
        public static bool AutoRenameStatic = true;

        private void OnEnable()
        {
            AutoRenameStatic = UnityEditor.EditorPrefs.GetBool("AutoRename", true);
        }

        private void OnDisable()
        {
            UnityEditor.EditorPrefs.SetBool("AutoRename", AutoRenameStatic);
        }

        private void DrawSpace()
        {
            GUILayout.Space(20);
        }

        private void BeginBox()
        {
            GUILayout.Space(30);
            Sirenix.Utilities.Editor.SirenixEditorGUI.DrawThickHorizontalSeparator();
            GUILayout.Space(10);
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox("状态表现");
        }

        private void EndBox()
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
            GUILayout.Space(30);
            Sirenix.Utilities.Editor.SirenixEditorGUI.DrawThickHorizontalSeparator();
            GUILayout.Space(10);
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

        [Button("重命名配置文件"), HideIf("AutoRename")]
        private void RenameFile()
        {
            string[] guids = UnityEditor.Selection.assetGUIDs;
            int i = guids.Length;
            if (i == 1)
            {
                string guid = guids[0];
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var so = UnityEditor.AssetDatabase.LoadAssetAtPath<StatusConfigObject>(assetPath);
                if (so != this)
                {
                    return;
                }
                var fileName = Path.GetFileNameWithoutExtension(assetPath);
                var newName = $"Status_{this.ID}";
                if (fileName != newName)
                {

                    UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
                }
            }
        }
#endif


#if EGamePlay_EN
        private const string StatusIdLabel = "StatusID";
        private const string StatusNameLabel = "Name";
        private const string StatusTypeLabel = "Type";
#else
        private const string StatusIdLabel = "状态ID";
        private const string StatusNameLabel = "状态名称";
        private const string StatusTypeLabel = "状态类型";
#endif
    }
}
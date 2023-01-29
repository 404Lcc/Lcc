using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("状态类型")]
    public enum StatusType
    {
        [LabelText("Buff(增益)")]
        Buff,
        [LabelText("Debuff(减益)")]
        Debuff,
        [LabelText("其他")]
        Other,
    }
    public class ChildStatus
    {
        [LabelText("状态效果")]
        public StatusConfigObject StatusConfigObject;

        [LabelText("参数列表"), HideReferenceObjectPicker]
        public Dictionary<string, string> ParamsDict = new Dictionary<string, string>();
    }


    [CreateAssetMenu(fileName = "状态配置", menuName = "技能|状态/状态配置")]
    public class StatusConfigObject : SerializedScriptableObject
    {
        [LabelText("状态Id")]
        public int Id;
        [LabelText("状态名称")]
        public string Name = "状态1";
        public StatusType StatusType;
        [HideInInspector]
        public uint Duration;
        [LabelText("是否在状态栏显示")]
        public bool ShowInStatusSlot;
        [LabelText("能否叠加")]
        public bool CanStack;
        [LabelText("最高叠加层数"), ShowIf("CanStack"), Range(0, 99)]
        public int MaxStack = 0;

        [LabelText("子状态效果")]
        public bool EnableChildStatus;
        [OnInspectorGUI("DrawSpace", append: true)]
        [HideReferenceObjectPicker]
        [LabelText("子状态效果列表"), ShowIf("EnableChildStatus"), ListDrawerSettings(DraggableItems = false, ShowItemCount = false, CustomAddFunction = "AddChildStatus")]
        public List<ChildStatus> StatusList = new List<ChildStatus>();

        private void AddChildStatus()
        {
            StatusList.Add(new ChildStatus());
        }

        [LabelText("效果列表"), Space(30)]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, ShowItemCount = false, HideAddButton = true)]
        [HideReferenceObjectPicker]
        public List<Effect> EffectList = new List<Effect>();


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

                EffectList.Add(effect);
                EffectTypeName = "(添加效果)";
            }
        }


        [LabelText("状态特效")]
        [OnInspectorGUI("BeginBox", append: false)]
        public GameObject ParticleEffect;

        public GameObject GetParticleEffect() => ParticleEffect;

        [LabelText("状态音效")]
        [OnInspectorGUI("EndBox", append: true)]
        public AudioClip Audio;

        [TextArea, LabelText("状态描述")]
        public string StatusDescription;


#if UNITY_EDITOR


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

        #region 自动命名
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (UnityEditor.Selection.assetGUIDs.Length == 1)
            {
                string guid = UnityEditor.Selection.assetGUIDs[0];
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var config = UnityEditor.AssetDatabase.LoadAssetAtPath<StatusConfigObject>(assetPath);
                if (config != this)
                {
                    return;
                }
                var oldName = Path.GetFileNameWithoutExtension(assetPath);
                var newName = $"Status_{this.Id}";
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
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("效果应用目标选择方式")]
    public enum SkillTargetSelectType
    {
        [LabelText("手动指定")]
        PlayerSelect,
        [LabelText("碰撞检测")]
        CollisionSelect,

        [LabelText("条件指定")]
        ConditionSelect,
        [LabelText("自定义")]
        Custom,
    }
    [LabelText("技能类型")]
    public enum SkillSpellType
    {
        [LabelText("主动技能")]
        Initiative,
        [LabelText("被动技能")]
        Passive,
    }
    [LabelText("技能作用对象")]
    public enum SkillAffectTargetType
    {
        [LabelText("自身")]
        Self = 0,
        [LabelText("己方")]
        SelfTeam = 1,
        [LabelText("敌方")]
        EnemyTeam = 2,
    }
    [CreateAssetMenu(fileName = "技能配置", menuName = "技能|状态/技能配置")]
    public class SkillConfigObject : SerializedScriptableObject
    {
        [LabelText("技能ID"), DelayedProperty]
        public int Id;
        [LabelText("技能名称"), DelayedProperty]
        public string Name = "技能1";
        public SkillSpellType SkillSpellType;
        [LabelText("技能作用对象"), ShowIf("SkillSpellType", SkillSpellType.Initiative)]
        public SkillAffectTargetType AffectTargetType;
        [LabelText("技能目标检测方式"), ShowIf("SkillSpellType", SkillSpellType.Initiative)]
        public SkillTargetSelectType TargetSelectType;



        [LabelText("冷却时间"), SuffixLabel("毫秒", true), ShowIf("SkillSpellType", SkillSpellType.Initiative)]
        public uint ColdTime;

        [LabelText("附加状态效果")]
        public bool EnableChildrenStatuses;

        [HideReferenceObjectPicker]
        [LabelText("附加状态效果列表"), ShowIf("EnableChildrenStatuses"), ListDrawerSettings(DraggableItems = false, ShowItemCount = false, CustomAddFunction = "AddChildStatus")]
        public List<ChildStatus> ChildrenStatuses = new List<ChildStatus>();
        private void AddChildStatus()
        {
            ChildrenStatuses.Add(new ChildStatus());
        }


        [TextArea, LabelText("技能描述")]
        public string SkillDescription;


        [OnInspectorGUI("BeginBox", append: false)]
        [LabelText("效果列表"), Space(30)]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, ShowItemCount = false, HideAddButton = true)]
        [HideReferenceObjectPicker]
        public List<Effect> Effects = new List<Effect>();





        [OnInspectorGUI("EndBox", append: true)]
        [HorizontalGroup(PaddingLeft = 40, PaddingRight = 40)]
        [HideLabel, OnValueChanged("AddEffect"), ValueDropdown("EffectTypeSelect")]
        public string EffectTypeName = "(添加效果)";

        public IEnumerable<string> EffectTypeSelect()
        {
            var types = typeof(Effect).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(Effect).IsAssignableFrom(x))
                .Where(x => x.GetCustomAttribute<EffectAttribute>() != null && x != typeof(ActionControlEffect) && x != typeof(AttributeModifyEffect))
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
                effect.IsSkillEffect = true;
                Effects.Add(effect);

                EffectTypeName = "(添加效果)";
            }
        }

#if UNITY_EDITOR

        private void DrawSpace()
        {
            GUILayout.Space(20);
        }

        private void BeginBox()
        {

            Sirenix.Utilities.Editor.SirenixEditorGUI.DrawThickHorizontalSeparator();
            GUILayout.Space(10);

        }

        private void EndBox()
        {

            GUILayout.Space(30);

        }

        #region 自动命名
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (UnityEditor.Selection.assetGUIDs.Length == 1)
            {
                string guid = UnityEditor.Selection.assetGUIDs[0];
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var config = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillConfigObject>(assetPath);
                if (config != this)
                {
                    return;
                }
                var oldName = Path.GetFileNameWithoutExtension(assetPath);
                var newName = $"Skill_{this.Id}";
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
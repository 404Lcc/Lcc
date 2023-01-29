#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using UnityEngine.Timeline;

namespace LccModel
{
    public class ExecutionEventEmitter : SignalEmitter
    {
        public ExecutionEventType ExecutionEventType;
        [LabelText("碰撞体名称")]
        public string ColliderName;
        public CollisionMoveType ColliderType;
        [LabelText("存活时间")]
        public float ExistTime;
        public EffectApplyType EffectApplyType;


        public override void OnInitialize(TrackAsset aPent)
        {
            base.OnInitialize(aPent);
            retroactive = true;
            emitOnce = true;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ExecutionEventEmitter))]
    public class ExecutionEventEmitterInspector : OdinEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            var emitter = target as ExecutionEventEmitter;
            if (emitter.asset == null)
            {
                SignalAsset signalAsset = null;
                var arr = AssetDatabase.FindAssets("t:SignalAsset", new string[] { "Assets" });
                foreach (var item in arr)
                {
                    signalAsset = AssetDatabase.LoadAssetAtPath<SignalAsset>(AssetDatabase.GUIDToAssetPath(item));
                    if (signalAsset != null) break;
                }

                emitter.asset = signalAsset;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();


            var emitter = target as ExecutionEventEmitter;
            emitter.time = EditorGUILayout.FloatField("Time", (float)emitter.time);
            emitter.retroactive = EditorGUILayout.Toggle("Retroactive", emitter.retroactive);
            emitter.emitOnce = EditorGUILayout.Toggle("EmitOnce", emitter.emitOnce);
            EditorGUILayout.Space(20);
            emitter.ExecutionEventType = (ExecutionEventType)SirenixEditorFields.EnumDropdown("事件类型", emitter.ExecutionEventType);

            if (emitter.ExecutionEventType == ExecutionEventType.TriggerSpawnCollider)
            {
                emitter.ColliderName = EditorGUILayout.TextField("碰撞体名称", emitter.ColliderName);
                emitter.ColliderType = (CollisionMoveType)SirenixEditorFields.EnumDropdown("碰撞体类型", emitter.ColliderType);
                if (emitter.ColliderType == CollisionMoveType.SelectedDirection
                    || emitter.ColliderType == CollisionMoveType.SelectedPosition
                    || emitter.ColliderType == CollisionMoveType.ForwardFly
                    )
                {

                    emitter.ExistTime = EditorGUILayout.FloatField("存活时间", emitter.ExistTime);
                }
                emitter.EffectApplyType = (EffectApplyType)EditorGUILayout.EnumPopup("应用效果", emitter.EffectApplyType);
            }

            if (emitter.ExecutionEventType == ExecutionEventType.TriggerApplyEffect)
            {
                emitter.EffectApplyType = (EffectApplyType)EditorGUILayout.EnumPopup("应用效果", emitter.EffectApplyType);
            }

            serializedObject.ApplyModifiedProperties();
            if (!EditorUtility.IsDirty(emitter))
            {
                EditorUtility.SetDirty(emitter);
            }
        }
    }
#endif
}
using System;
using System.Collections.Generic;
using System.Reflection;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {

//will auto register in unity
#if UNITY_5_3_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static private void RegisterBindingAction()
        {
            ILRuntime.Runtime.CLRBinding.CLRBindingUtils.RegisterBindingAction(Initialize);
        }


        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Byte_Binding.Register(app);
            UnityEngine_Input_Binding.Register(app);
            UnityEngine_Vector2_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            UnityEngine_Camera_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            UnityEngine_Screen_Binding.Register(app);
            LccModel_TransformComponent_Binding.Register(app);
            UnityEngine_Mathf_Binding.Register(app);
            UnityEngine_Time_Binding.Register(app);
            UnityEngine_Touch_Binding.Register(app);
            UnityEngine_EventSystems_EventSystem_Binding.Register(app);
            UnityEngine_Quaternion_Binding.Register(app);
            UnityEngine_Ray_Binding.Register(app);
            System_String_Binding.Register(app);
            UnityEngine_LayerMask_Binding.Register(app);
            UnityEngine_Physics_Binding.Register(app);
            UnityEngine_RaycastHit_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_Binding.Register(app);
            LccModel_UpdatePanel_Binding.Register(app);
            ET_ETTask_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            LccModel_CombatContext_Binding.Register(app);
            LccModel_Combat_Binding.Register(app);
            LccModel_OrcaComponent_Binding.Register(app);
            System_Array_Binding.Register(app);
            LccModel_AObjectBase_Binding.Register(app);
            LccModel_Singleton_1_Timer_Binding.Register(app);
            LccModel_Timer_Binding.Register(app);
            LccModel_DragEventTrigger_Binding.Register(app);
            UnityEngine_EventSystems_PointerEventData_Binding.Register(app);
            LccModel_Vector2Expand_Binding.Register(app);
            System_Action_2_Vector2_Single_Binding.Register(app);
            System_Collections_Generic_List_1_Combat_Binding.Register(app);
            System_Object_Binding.Register(app);
            UnityEngine_UI_Slider_Binding.Register(app);
            System_Int32_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
            LccModel_LogUtil_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            UnityEngine_Events_UnityEvent_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Enum_Binding.Register(app);
            TMPro_TMP_Text_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Exception_Binding.Register(app);
            LccModel_PathUtil_Binding.Register(app);
            System_IO_FileInfo_Binding.Register(app);
            System_IO_FileSystemInfo_Binding.Register(app);
            LccModel_FileUtil_Binding.Register(app);
            LccModel_ByteExpand_Binding.Register(app);
            LccModel_RijndaelUtil_Binding.Register(app);
            LccModel_JsonUtil_Binding.Register(app);
            System_IO_DirectoryInfo_Binding.Register(app);
            DG_Tweening_DOTween_Binding.Register(app);
            DG_Tweening_TweenSettingsExtensions_Binding.Register(app);
            System_Action_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_AssetHandle_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_AssetHandle_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_AssetHandle_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            LccModel_AssetManager_Binding.Register(app);
            YooAsset_AssetHandle_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_1_AudioClip_Binding.Register(app);
            UnityEngine_AudioSource_Binding.Register(app);
            LccModel_WebUtil_Binding.Register(app);
            ET_ETTask_1_AudioClip_Binding.Register(app);
            System_Type_Binding.Register(app);
            System_Reflection_FieldInfo_Binding.Register(app);
            System_Collections_Generic_HashSet_1_Type_Binding.Register(app);
            System_Collections_Generic_HashSet_1_Type_Binding_Enumerator_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            UnityEngine_TextAsset_Binding.Register(app);
            LccModel_ProtobufUtil_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding.Register(app);
            UnityEngine_Application_Binding.Register(app);
            LccModel_ResolutionUtil_Binding.Register(app);
            LccModel_DisplayMode_Binding.Register(app);
            UnityEngine_QualitySettings_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_String_Binding.Register(app);
            System_Activator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            UnityEngine_Canvas_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_String_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Int32_Binding.Register(app);
            ET_ETAsyncTaskMethodBuilder_1_ILTypeInstance_Binding.Register(app);
            ScreenAdaptationUtil_Binding.Register(app);
            System_Collections_Generic_List_1_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Stack_1_ILTypeInstance_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            ET_ETTask_1_AssetHandle_Binding.Register(app);
            LccModel_CoroutineLockManager_Binding.Register(app);
            ET_ETTask_1_CoroutineLock_Binding.Register(app);
            LccModel_CoroutineLock_Binding.Register(app);
            ET_ETTask_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_FieldInfo_Binding.Register(app);
            LccModel_LccView_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            UnityEngine_Video_VideoPlayer_Binding.Register(app);
            UnityEngine_RenderTexture_Binding.Register(app);
            UnityEngine_UI_RawImage_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Type_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Type_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_HashSet_1_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Type_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_Type_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding_Enumerator_Binding.Register(app);
            LccModel_UnOrderMultiMapSet_2_Type_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int64_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Queue_1_Int64_Binding.Register(app);
            LccModel_ObjectUtil_Binding.Register(app);
            LccModel_IdUtil_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int64_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int64_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Reflection_MethodBase_Binding.Register(app);
            LccModel_CoroutineObject_Binding.Register(app);
            System_Collections_Generic_Queue_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Queue_1_ETTask_Binding.Register(app);
            UnityEngine_Pool_ObjectPool_1_ILTypeInstance_Binding.Register(app);
            LccModel_TransformExpand_Binding.Register(app);
            LccModel_ScrollerPro_Binding.Register(app);
            UnityEngine_RectTransform_Binding.Register(app);
            LccModel_GroupBase_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Vector2_Binding.Register(app);
            EnhancedUI_EnhancedScroller_EnhancedScroller_Binding.Register(app);
            System_Func_1_Int32_Binding.Register(app);
            System_Action_2_Int32_ILTypeInstance_Binding.Register(app);
            LccModel_Singleton_1_Loader_Binding.Register(app);
            LccModel_Loader_Binding.Register(app);
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
        }
    }
}

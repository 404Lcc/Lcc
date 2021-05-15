using System;
using System.Collections.Generic;
using System.Reflection;

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
            UnityEngine_Camera_Binding.Register(app);
            UnityEngine_Screen_Binding.Register(app);
            UnityEngine_Vector2_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            UnityEngine_Mathf_Binding.Register(app);
            UnityEngine_Time_Binding.Register(app);
            UnityEngine_Input_Binding.Register(app);
            UnityEngine_Touch_Binding.Register(app);
            UnityEngine_EventSystems_EventSystem_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            UnityEngine_Quaternion_Binding.Register(app);
            UnityEngine_Ray_Binding.Register(app);
            System_String_Binding.Register(app);
            UnityEngine_LayerMask_Binding.Register(app);
            UnityEngine_Physics_Binding.Register(app);
            UnityEngine_RaycastHit_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            LccModel_DragEventTrigger_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_EventSystems_PointerEventData_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Type_Binding.Register(app);
            System_Reflection_FieldInfo_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_IO_FileInfo_Binding.Register(app);
            System_IO_FileSystemInfo_Binding.Register(app);
            System_IO_DirectoryInfo_Binding.Register(app);
            LccModel_Singleton_1_AudioManager_Binding.Register(app);
            LccModel_Objects_Binding.Register(app);
            LccModel_AudioManager_Binding.Register(app);
            System_Enum_Binding.Register(app);
            System_Int32_Binding.Register(app);
            LccModel_DisplayMode_Binding.Register(app);
            UnityEngine_QualitySettings_Binding.Register(app);
            UnityEngine_ScreenCapture_Binding.Register(app);
            System_Text_Encoding_Binding.Register(app);
            LccModel_Singleton_1_AssetManager_Binding.Register(app);
            LccModel_AssetManager_Binding.Register(app);
            UnityEngine_UI_Image_Binding.Register(app);
            UnityEngine_SpriteRenderer_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncVoidMethodBuilder_Binding.Register(app);
            System_Threading_Tasks_Task_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_Binding.Register(app);
            UnityEngine_RectTransformUtility_Binding.Register(app);
            UnityEngine_RectTransform_Binding.Register(app);
            UnityEngine_Rect_Binding.Register(app);
            System_Collections_Hashtable_Binding.Register(app);
            System_Collections_IEnumerable_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            System_Activator_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Array_Binding.Register(app);
            LccModel_Singleton_1_MonoManager_Binding.Register(app);
            LccModel_MonoManager_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding_Enumerator_Binding.Register(app);
            LccModel_Singleton_1_ILRuntimeManager_Binding.Register(app);
            LccModel_ILRuntimeManager_Binding.Register(app);
            System_Collections_Generic_List_1_Int32_Binding.Register(app);
            System_Collections_IDictionaryEnumerator_Binding.Register(app);
            LccModel_UIEventHandlerAttribute_Binding.Register(app);
            LccModel_LccView_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_FieldInfo_Binding.Register(app);
            UnityEngine_MonoBehaviour_Binding.Register(app);
            System_Collections_Generic_Queue_1_Int64_Binding.Register(app);
            UnityEngine_Behaviour_Binding.Register(app);
            System_IO_Directory_Binding.Register(app);
            System_Char_Binding.Register(app);
            System_Collections_Generic_List_1_DirectoryInfo_Binding.Register(app);
            System_IO_File_Binding.Register(app);
            System_Collections_Generic_List_1_FileInfo_Binding.Register(app);
            System_IO_Path_Binding.Register(app);
            System_IO_FileStream_Binding.Register(app);
            System_IO_Stream_Binding.Register(app);
            System_Byte_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            UnityEngine_Resolution_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            System_Security_Cryptography_MD5CryptoServiceProvider_Binding.Register(app);
            System_Security_Cryptography_HashAlgorithm_Binding.Register(app);
            System_Text_StringBuilder_Binding.Register(app);
            UnityEngine_Application_Binding.Register(app);
            System_Security_Cryptography_RijndaelManaged_Binding.Register(app);
            System_Security_Cryptography_SymmetricAlgorithm_Binding.Register(app);
            System_Security_Cryptography_ICryptoTransform_Binding.Register(app);
            System_Convert_Binding.Register(app);
            UnityEngine_WWWForm_Binding.Register(app);
            System_Uri_Binding.Register(app);
            UnityEngine_Networking_UnityWebRequest_Binding.Register(app);
            System_Action_1_String_Binding.Register(app);
            System_Action_1_Boolean_Binding.Register(app);
            System_NotSupportedException_Binding.Register(app);
            UnityEngine_Networking_DownloadHandler_Binding.Register(app);
            System_Action_1_Byte_Array_Binding.Register(app);
            UnityEngine_Networking_DownloadHandlerTexture_Binding.Register(app);
            System_Action_1_Texture2D_Binding.Register(app);
            System_Action_2_Texture2D_Byte_Array_Binding.Register(app);
            UnityEngine_Networking_DownloadHandlerAudioClip_Binding.Register(app);
            System_Action_1_AudioClip_Binding.Register(app);
            System_Action_2_AudioClip_Byte_Array_Binding.Register(app);
            UnityEngine_Networking_DownloadHandlerAssetBundle_Binding.Register(app);
            System_Action_1_AssetBundle_Binding.Register(app);
            System_Action_2_AssetBundle_Byte_Array_Binding.Register(app);
            LccModel_Singleton_1_SceneLoadManager_Binding.Register(app);
            LccModel_SceneLoadManager_Binding.Register(app);
            LccModel_Init_Binding.Register(app);
            System_Threading_Monitor_Binding.Register(app);
            UnityEngine_TextAsset_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
        }
    }
}

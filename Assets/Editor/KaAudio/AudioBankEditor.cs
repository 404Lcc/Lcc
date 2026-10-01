
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;
using LccHotfix;

namespace LccHotfix.Editor.KaAudio
{
    public class AudioBankEditor : EditorWindow
    {
        private string audioBankPath;
        private AudioBankConfig audioBankConfig;
        private int selectedEventIndex = -1;
        private AudioManager audioManager;
        
        private Rect eventListRect = new Rect(0, 20, 200, 400);
        private Rect eventPropertyRect = new Rect(200, 20, 600, 150);
        private Vector2 eventListScrollPosition = new Vector2();
        private Vector2 eventPropertiesScrollPosition = new Vector2();
        private Color unselectedButton = new Color(0.8f, 0.8f, 0.8f, 1);
        
        [MenuItem("工具箱/音频库编辑器")]
        private static void OpenAudioBankEditor()
        {
            AudioBankEditor editor = GetWindow<AudioBankEditor>();
            editor.titleContent = new GUIContent("音频库编辑器");
            editor.Show();
        }

        private AudioManager GetAudioManager()
        {
            if (Application.isPlaying)
            {
                // todo 直接使用游戏内的audioManager
            }
            if (audioManager is not null)
            {
                return audioManager;
            }

            audioManager = new AudioManager();
            audioManager.Register(true);
            return audioManager;
        }

        private void Awake()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private void OnDestroy()
        {
            EditorApplication_playModeStateChanged(PlayModeStateChange.EnteredEditMode);
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            audioManager?.Dispose();
            audioManager = null;
        }

        private void Update()
        {
            audioManager?.Update();
            Repaint();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Actions", EditorStyles.toolbarDropDown))
            {
                GenericMenu newNodeMenu = new GenericMenu();
                newNodeMenu.AddItem(new GUIContent("新建音频事件"), false, AddEvent);
                newNodeMenu.AddItem(new GUIContent("删除音频事件"), false, ConfirmDeleteEvent);
                newNodeMenu.AddItem(new GUIContent("音频事件排序"), false, SortEventList);
                newNodeMenu.AddItem(new GUIContent("保存音频库"), false, Save);
                newNodeMenu.AddItem(new GUIContent("保存并关闭音频库"), false, SaveAndClose);
                // newNodeMenu.AddItem(new GUIContent("Preview Event"), false, PreviewEvent);
                // newNodeMenu.AddItem(new GUIContent("Stop Preview"), false, StopPreview);
                newNodeMenu.ShowAsContext();
            }
            GUILayout.EndHorizontal();
            
            if (selectedEventIndex > -1)
            {
                DrawEventProperties(selectedEventIndex);
            }
            DrawEventList();
        }
        
        private void DrawEventList()
        {
            eventListRect.height = position.height;
            GUILayout.BeginArea(eventListRect);
            eventListScrollPosition = EditorGUILayout.BeginScrollView(eventListScrollPosition);
            if (GUILayout.Button("打开"))
            {
                SaveAndClose();
                EditorGUIUtility.ShowObjectPicker<AudioBankConfig>(audioBankConfig, false, "", 0);
            }
            if (GUILayout.Button("保存"))
            {
                Save();
            }
            audioBankConfig ??= EditorGUIUtility.GetObjectPickerObject() as AudioBankConfig;
            if (audioBankConfig == null) {
                audioBankPath = "";
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            audioBankPath = AssetDatabase.GetAssetPath(audioBankConfig);

            audioBankConfig.Name = EditorGUILayout.TextField("库名", audioBankConfig.Name);
            EditorGUILayout.PrefixLabel("事件列表");
            if (audioBankConfig.AudioEvents is not null)
            {
                for (var i = 0; i < audioBankConfig.AudioEvents.Count; i++)
                {
                    if (audioBankConfig.AudioEvents[i] is null)
                    {
                        continue;
                    }
                    GUI.color = selectedEventIndex == i ? Color.white : unselectedButton;
                    if (GUILayout.Button(audioBankConfig.AudioEvents[i].Name))
                    {
                        SelectEvent(i);
                    }
                }
            }
            GUI.color = unselectedButton;
            if (GUILayout.Button("[ + ]"))
            {
                AddEvent();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private void DrawEventProperties(int eventIndex)
        {
            if (eventIndex >= audioBankConfig.AudioEvents.Count)
            {
                return;
            }
            var audioEvent = audioBankConfig.AudioEvents[eventIndex];
            if (audioEvent is null)
            {
                return;
            }
            eventPropertyRect.width = position.width - eventListRect.width;
            eventPropertyRect.height = position.height;
            
            GUILayout.BeginArea(eventPropertyRect);
            eventPropertiesScrollPosition = EditorGUILayout.BeginScrollView(eventPropertiesScrollPosition);
            
            // 名称
            audioEvent.Name = EditorGUILayout.TextField("名称", audioEvent.Name);
            // 分组标签
            audioEvent.TagName = EditorGUILayout.TextField("分组标签", audioEvent.TagName);

            // 注释
            EditorGUILayout.PrefixLabel("注释");
            audioEvent.Desc = EditorGUILayout.TextArea(audioEvent.Desc, GUILayout.Height(60));
            EditorGUILayout.Space(10);

            // 音频剪辑（列表）
            EditorGUILayout.PrefixLabel("音频剪辑（每行1个）");
            audioEvent.Clips = new List<string>(EditorGUILayout.TextArea(String.Join("\n", audioEvent.Clips), GUILayout.Height(80)).Split("\n"));
            EditorGUILayout.Space(10);

            // 事件（下拉框）
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("事件");
            if (EditorGUILayout.DropdownButton(new GUIContent(audioEvent.Event), FocusType.Keyboard))
            {
                var alls = GetAudioManager().GetAllEventTypes();
                var menu = new GenericMenu();
                foreach (var item in alls.Where(item => !string.IsNullOrEmpty(item)))
                {
                    menu.AddItem(new GUIContent(item), audioEvent.Event != null && audioEvent.Event.Equals(item), (object value) =>
                    {
                        audioEvent.Event = value.ToString();
                    }, item);
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            
            // 其余基本属性
            audioEvent.Volume = EditorGUILayout.Slider("相对音量", audioEvent.Volume, 0f, 1.0f);
            audioEvent.MaxStack = EditorGUILayout.IntField("最大堆叠", audioEvent.MaxStack);
            audioEvent.MinInterval = EditorGUILayout.FloatField("最小间隔", audioEvent.MinInterval);
            audioEvent.PitchShift = EditorGUILayout.FloatField("音高浮动", audioEvent.PitchShift);
            audioEvent.CanClean = EditorGUILayout.Toggle("可被清理", audioEvent.CanClean);
            audioEvent.CanPause = EditorGUILayout.Toggle("可被暂停", audioEvent.CanPause);
            
            // 预览按钮
            if (GUILayout.Button("预览播放"))
            {
                try
                {
                    GetAudioManager().CreateEvent(audioEvent)?.Play();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            if (GUILayout.Button("停止播放"))
            {
                audioManager?.DestroyAllEvent();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private void AddEvent()
        {
            var newEvent = new AudioEventConfig
            {
                Name = "New Audio Event",
                Clips = new List<string>(),
            };
            audioBankConfig.AudioEvents.Add(newEvent);
            SelectEvent(audioBankConfig.AudioEvents.Count - 1);
        }

        private void ConfirmDeleteEvent()
        {
            var selectedEvent = audioBankConfig.AudioEvents[selectedEventIndex];
            if (EditorUtility.DisplayDialog("删除确认", "是否删除音频事件 \"" + selectedEvent.Name + "\"?", "是", "否"))
            {
                audioBankConfig.AudioEvents.RemoveAt(selectedEventIndex);
                selectedEventIndex--;
            }
        }
        
        private void Save()
        {
            if (audioBankConfig is null)
            {
                return;
            }
            audioBankConfig.Save(audioBankPath);
        }

        private void SaveAndClose()
        {
            if (audioBankConfig is null)
            {
                return;
            }
            audioBankConfig.Save(audioBankPath);
            audioBankPath = "";
            audioBankConfig = null;
            selectedEventIndex = -1;
        }

        private void SelectEvent(int selection)
        {
            GUI.FocusControl("");
            selectedEventIndex = selection;
        }
        
        private void SortEventList()
        {
            if (audioBankConfig is not null)
            {
                audioBankConfig.AudioEvents.Sort((config1, config2) =>
                    string.Compare(config1.Name, config2.Name, StringComparison.Ordinal));
            }
            
        }
    }
}
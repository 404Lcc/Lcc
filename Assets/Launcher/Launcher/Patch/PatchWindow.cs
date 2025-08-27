using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class PatchWindow
    {
        /// <summary>
        /// 对话框封装类
        /// </summary>
        private class MessageBox
        {
            private GameObject _cloneObject;
            private TextMeshProUGUI _content;
            private Button _confirmBtn;
            private Action _confirm;
            private bool _needHide;

            public bool ActiveSelf => _cloneObject.activeSelf;

            public void Create(GameObject cloneObject)
            {
                _cloneObject = cloneObject;
                _content = cloneObject.transform.Find("BG/ContentText").GetComponent<TextMeshProUGUI>();
                _confirmBtn = cloneObject.transform.Find("BG/ConfirmBtn").GetComponent<Button>();
                _confirmBtn.onClick.AddListener(OnConfirm);
            }

            public void Show(string content, Action confirm, bool needHide)
            {
                _content.text = content;
                _confirm = confirm;
                _needHide = needHide;
                _cloneObject.SetActive(true);
                _cloneObject.transform.SetAsLastSibling();
            }

            public void Hide()
            {
                _content.text = string.Empty;
                _confirm = null;
                _cloneObject.SetActive(false);
            }

            private void OnConfirm()
            {
                _confirm?.Invoke();
                if (_needHide)
                {
                    Hide();
                }
            }
        }

        private readonly EventGroup _eventGroup = new EventGroup();
        private readonly List<MessageBox> _msgBoxList = new List<MessageBox>();

        // UGUI相关
        private Slider _downloadProgress;
        private TextMeshProUGUI _tips;
        private GameObject _messageBoxObj;

        public void Init(Slider downloadProgress, TextMeshProUGUI tips, GameObject messageBoxObj)
        {
            _downloadProgress = downloadProgress;
            _tips = tips;
            _messageBoxObj = messageBoxObj;
            _messageBoxObj.SetActive(false);

            _eventGroup.AddListener<PatchEventDefine.InitializeFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.PatchStepsChange>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.FoundUpdateFiles>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.DownloadUpdate>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.PackageVersionRequestFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.PackageManifestUpdateFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.WebFileDownloadFailed>(OnHandleEventMessage);
        }

        public void Dispose()
        {
            _eventGroup.RemoveAllListener();
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        private void OnHandleEventMessage(IEventMessage message)
        {
            if (message is PatchEventDefine.InitializeFailed)
            {
                System.Action callback = () => { UserEventDefine.UserTryInitialize.SendEventMessage(); };
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_init_failed"), callback);
            }
            else if (message is PatchEventDefine.PatchStepsChange)
            {
                var msg = message as PatchEventDefine.PatchStepsChange;
                _tips.text = msg.Tips;
                UnityEngine.Debug.Log(msg.Tips);
            }
            else if (message is PatchEventDefine.FoundUpdateFiles)
            {
                var msg = message as PatchEventDefine.FoundUpdateFiles;
                System.Action callback = () => { UserEventDefine.UserBeginDownloadWebFiles.SendEventMessage(); };
                float sizeMB = msg.TotalSizeBytes / 1048576f;
                sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
                string totalSizeMB = sizeMB.ToString("f1");
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_found_update", msg.TotalCount.ToString(), totalSizeMB), callback);
            }
            else if (message is PatchEventDefine.DownloadUpdate)
            {
                var msg = message as PatchEventDefine.DownloadUpdate;
                _downloadProgress.value = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
                string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
                string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
                _tips.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
            }
            else if (message is PatchEventDefine.PackageVersionRequestFailed)
            {
                System.Action callback = () => { UserEventDefine.UserTryRequestPackageVersion.SendEventMessage(); };
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_update_failed_static"), callback);
            }
            else if (message is PatchEventDefine.PackageManifestUpdateFailed)
            {
                System.Action callback = () => { UserEventDefine.UserTryUpdatePackageManifest.SendEventMessage(); };
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_update_failed_patch"), callback);
            }
            else if (message is PatchEventDefine.WebFileDownloadFailed)
            {
                var msg = message as PatchEventDefine.WebFileDownloadFailed;
                System.Action callback = () => { UserEventDefine.UserTryDownloadWebFiles.SendEventMessage(); };
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_download_failed", msg.FileName), callback);
            }
            else
            {
                throw new System.NotImplementedException($"{message.GetType()}");
            }
        }

        /// <summary>
        /// 显示对话框
        /// </summary>
        public void ShowMessageBox(string content, Action confirm, bool needHide = true)
        {
            // 尝试获取一个可用的对话框
            MessageBox msgBox = null;
            for (int i = 0; i < _msgBoxList.Count; i++)
            {
                var item = _msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            // 如果没有可用的对话框，则创建一个新的对话框
            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = GameObject.Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
                msgBox.Create(cloneObject);
                _msgBoxList.Add(msgBox);
            }

            // 显示对话框
            msgBox.Show(content, confirm, needHide);
        }
    }
}
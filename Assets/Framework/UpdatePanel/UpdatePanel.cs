using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class MessageBox
    {
        public GameObject cloneObject;
        public Text infoText;
        public Button confirmBtn;
        public Action completed;

        public bool ActiveSelf
        {
            get
            {
                return cloneObject.activeSelf;
            }
        }

        public void Create(GameObject cloneObject)
        {
            this.cloneObject = cloneObject;
            infoText = cloneObject.transform.Find("BG/InfoText").GetComponent<Text>();
            confirmBtn = cloneObject.transform.Find("BG/ConfirmBtn").GetComponent<Button>();
            confirmBtn.onClick.AddListener(OnConfirm);
        }
        public void Show(string info, Action completed)
        {
            infoText.text = info;
            this.completed = completed;
            cloneObject.SetActive(true);
            cloneObject.transform.SetAsLastSibling();
        }
        public void Hide()
        {
            infoText.text = string.Empty;
            completed = null;
            cloneObject.SetActive(false);
        }
        private void OnConfirm()
        {
            completed?.Invoke();
            Hide();
        }
    }
    public class UpdatePanel : MonoBehaviour
    {
        public float currentPercent;
        public float targetPercent;
        public float updateRate;

        public static UpdatePanel Instance;

        public Slider progress;
        public Text progressText;
        public Text tipsText;
        public GameObject messageBox;

        public List<MessageBox> msgBoxList = new List<MessageBox>();

        private readonly EventGroup _eventGroup = new EventGroup();

        public void Start()
        {
            Instance = this;

            _eventGroup.AddListener<PatchEventDefine.InitializeFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.PatchStatesChange>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.FoundUpdateFiles>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.DownloadProgressUpdate>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.PackageVersionUpdateFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.PatchManifestUpdateFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchEventDefine.WebFileDownloadFailed>(OnHandleEventMessage);
        }
        public void OnDestroy()
        {
            _eventGroup.RemoveAllListener();
            Instance = null;
        }

        void Update()
        {
            if (currentPercent < targetPercent)
            {
                currentPercent += updateRate;
                currentPercent = Mathf.Clamp(currentPercent, 0, 100);
                progress.value = currentPercent * 0.01f;
                progressText.text = (int)currentPercent + "%";
            }
        }

        public void UpdateLoadingPercent(int from, int to, float rate = 1)
        {
            ShowAll();


            updateRate = rate;
            targetPercent = to;
            currentPercent = Mathf.Clamp(currentPercent, from, to);

            progress.value = currentPercent * 0.01f;
            progressText.text = (int)currentPercent + "%";
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }


        private void ShowAll()
        {
            gameObject.SetActive(true);
        }



        private void ShowMessageBox(string info, Action completed)
        {
            MessageBox msgBox = null;
            for (int i = 0; i < msgBoxList.Count; i++)
            {
                var item = msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = GameObject.Instantiate(messageBox, messageBox.transform.parent);
                msgBox.Create(cloneObject);
                msgBoxList.Add(msgBox);
            }
            msgBox.Show(info, completed);
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        private void OnHandleEventMessage(IEventMessage message)
        {
            if (message is PatchEventDefine.InitializeFailed)
            {
                Action callback = () =>
                {
                    UserEventDefine.UserTryInitialize.SendEventMessage();
                };
                ShowMessageBox($"Failed to initialize package !", callback);
            }
            else if (message is PatchEventDefine.PatchStatesChange)
            {
                var msg = message as PatchEventDefine.PatchStatesChange;
                tipsText.text = msg.Tips;
            }
            else if (message is PatchEventDefine.FoundUpdateFiles)
            {
                var msg = message as PatchEventDefine.FoundUpdateFiles;
                Action callback = () =>
                {
                    UserEventDefine.UserBeginDownloadWebFiles.SendEventMessage();
                };
                float sizeMB = msg.TotalSizeBytes / 1048576f;
                sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
                string totalSizeMB = sizeMB.ToString("f1");
                ShowMessageBox($"Found update patch files, Total count {msg.TotalCount} Total szie {totalSizeMB}MB", callback);
            }
            else if (message is PatchEventDefine.DownloadProgressUpdate)
            {
                var msg = message as PatchEventDefine.DownloadProgressUpdate;
                progress.value = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
                string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
                string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
                tipsText.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
            }
            else if (message is PatchEventDefine.PackageVersionUpdateFailed)
            {
                Action callback = () =>
                {
                    UserEventDefine.UserTryUpdatePackageVersion.SendEventMessage();
                };
                ShowMessageBox($"Failed to update static version, please check the network status.", callback);
            }
            else if (message is PatchEventDefine.PatchManifestUpdateFailed)
            {
                Action callback = () =>
                {
                    UserEventDefine.UserTryUpdatePatchManifest.SendEventMessage();
                };
                ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
            }
            else if (message is PatchEventDefine.WebFileDownloadFailed)
            {
                var msg = message as PatchEventDefine.WebFileDownloadFailed;
                Action callback = () =>
                {
                    UserEventDefine.UserTryDownloadWebFiles.SendEventMessage();
                };
                ShowMessageBox($"Failed to download file : {msg.FileName}", callback);
            }
            else
            {
                throw new NotImplementedException($"{message.GetType()}");
            }
        }
    }
}
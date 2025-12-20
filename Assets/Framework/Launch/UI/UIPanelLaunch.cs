using UnityEngine;

namespace LccModel
{
    public partial class UIPanelLaunch : MonoBehaviour
    {
        void Awake()
        {
            Init();
            _uniEventGroup.AddListener<LaunchEvent.StateChanged>(OnLaunchStateChanged);
            _uniEventGroup.AddListener<LaunchEvent.ShowMessageBox>(OnShowMessageBox);
            _uniEventGroup.AddListener<LaunchEvent.ShowProgress>(OnShowProgress);
            _uniEventGroup.AddListener<LaunchEvent.ShowVersion>(OnShowVersion);
        }

        private void OnLaunchStateChanged(IEventMessage obj)
        {
            if (obj is LaunchEvent.StateChanged message)
            {
                SetHint(StringTable.Get(message.To));
            }
        }
        
        public static string FormatBytes(long bytes, double min = 0.01f)
        {
            var megabytes = bytes / (1024.0 * 1024.0);
            if (megabytes < min)
                megabytes = min;
            return $"{megabytes:F2}M";
        }
        
        private void OnShowMessageBox(IEventMessage obj)
        {
            if (obj is LaunchEvent.ShowMessageBox message)
            {
                ShowMessageBox(message.Params);
            }
        }

        private void OnShowProgress(IEventMessage obj)
        {
            if (obj is LaunchEvent.ShowProgress message)
            {
                SetProgress(message.Progress, message.ProgressText);
            }
        }

        private void OnShowVersion(IEventMessage obj)
        {
            if (obj is LaunchEvent.ShowVersion message)
            {
                SetVersion(message.VersionStr);
            }
        }

        void OnDestroy()
        {
            _uniEventGroup.RemoveAllListener();
        }
    }
}

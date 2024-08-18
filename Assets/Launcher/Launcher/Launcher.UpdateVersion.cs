using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Diagnostics;


namespace LccModel
{
    public partial class Launcher
    {
        //远程下载包描述
        public string forceUpdateDesc;

        public void SetUpdateInfo(int version, string downloadPackageUrl, string desc)
        {
            forceUpdateDesc = desc;
        }
        private void UpdateNewVersion()
        {
            Application.OpenURL(mSvrAppForceUpdateUrl);
        }

        public void ForceUpdate()
        {
            UILoadingPanel.Instance.ShowMessageBox(Launcher.Instance.GetLanguage("msg_update"), () =>
            {
                UpdateNewVersion();
            }, false);
        }


        /// <summary>
        /// 判断是否要重新安装
        /// </summary>
        /// <returns></returns>
        public bool CheckIfAppShouldUpdate()
        {
            if (string.IsNullOrEmpty(mSvrAppForceUpdateUrl))
                return false;

            return GameConfig.appVersion != mSvrVersion;
        }
    }
}
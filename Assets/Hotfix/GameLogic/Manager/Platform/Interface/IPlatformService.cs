using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using cfg;
using LccModel;
using Luban;
using RVO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace LccHotfix
{
    public interface IPlatformService : IService
    {
        void InitCallbackMessage();


        void InitPlatform();


        #region 回调

        void OnAndroidCallback(string data);

        #endregion


        #region 平台

        List<string> RequestServerList();
        void SetServerCurrent(string server);
        string GetServerCurrent();

        string GetTimeZone();

        string GetUserRegion();

        #endregion
    }
}
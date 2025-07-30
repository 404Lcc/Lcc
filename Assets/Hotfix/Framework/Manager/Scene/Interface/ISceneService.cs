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
    public interface ISceneService : IService
    {
        #region Load Scene

        SceneType curState { get; }
        SceneType preState { get; }

        bool IsLoading { get; }

        LoadSceneHandler GetScene(SceneType type);

        void ChangeScene(LoadSceneHandler handler);

        void ChangeScene(SceneType type);

        IEnumerator ShowSceneLoading(LoadingType loadType);


        void BeginLoad();


        void CleanScene();

        #endregion

        #region 切场景界面

        void OpenChangeScenePanel();

        void CleanChangeSceneParam();

        #endregion
    }
}
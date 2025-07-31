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
using UnityEngine.UI;
using UnityEngine.Video;

namespace LccHotfix
{
    public interface IWorldService : IService
    {
        void CreateWorld<T>(GameModeState gameModeState) where T : ECSWorld;

        ECSWorld GetWorld();

        T GetWorld<T>() where T : ECSWorld;

        void ExitWorld();


        void PauseWorld();

        void ResumeWorld();
    }
}
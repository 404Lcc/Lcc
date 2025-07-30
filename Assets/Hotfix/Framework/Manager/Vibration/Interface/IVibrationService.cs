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
    public interface IVibrationService : IService
    {
        bool IsVibrationEnabled { get; set; }
        void Vibrate(float duration, float intensity = 1.0f);


        void LightVibration();
        void MediumVibration();
        void StrongVibration();
    }
}
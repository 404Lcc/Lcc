using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using cfg;
using LccModel;
using Luban;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace LccHotfix
{
    public interface IHotfixBridgeService : IService
    {
        void Init();

        object CrossDomainCallMethod(ClassInfo clsInfo, BindingFlags flag, object[] param, object instance = null);

        object CrossDomainCallProperty(ClassInfo clsInfo, BindingFlags flag, object instance = null);

        object CrossDomainCallField(ClassInfo clsInfo, BindingFlags flag, object instance = null);
    }
}
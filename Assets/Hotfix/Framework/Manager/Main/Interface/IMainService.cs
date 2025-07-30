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

namespace LccHotfix
{
    public interface IMainService : IService
    {
        /// <summary>
        /// 增加游戏框架模块。
        /// </summary>
        T AddModule<T>() where T : Module, IService;
    }
}
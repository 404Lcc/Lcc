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
    public interface IIconService : IService
    {
        IconBase GetIcon(IconType type, Transform parent, IconSize size = IconSize.Size_100);

        /// <summary>
        /// icon回收接口
        /// </summary>
        void RecycleIcon<T>(T iconBase) where T : IconBase;

        Vector3 GetIconScale(IconSize size);
    }
}
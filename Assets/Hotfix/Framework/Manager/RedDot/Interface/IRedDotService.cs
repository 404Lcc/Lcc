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
    public interface IRedDotService : IService
    {
        /// <summary>
        /// 增加红点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="isNeedShow"></param>
        void AddRedDotNode(string parent, string target, bool isNeedShow);


        /// <summary>
        /// 移除红点
        /// </summary>
        /// <param name="target"></param>
        void RemoveRedDotNode(string target);

        /// <summary>
        /// 增加运行时红点数据
        /// </summary>
        /// <param name="target"></param>
        void AddRuntimeData(string target, int id = 0);


        /// <summary>
        /// 移除运行时红点数据
        /// </summary>
        /// <param name="target"></param>
        void RemoveRuntimeData(string target, int id = 0);


        /// <summary>
        /// 隐藏红点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool HideRedDotNode(string target, int id = 0);

        /// <summary>
        /// 显示红点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool ShowRedDotNode(string target, int id = 0);


        #region 接口

        /// <summary>
        /// 节点是否已经处于显示状态
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool IsLogicAlreadyShow(string target, int id);

        /// <summary>
        /// 获取这个红点数据 
        /// </summary>
        RedDotNode GetData(string key);

        /// <summary>
        /// 添加变化监听
        /// </summary>
        bool AddChanged(string key, int id, Action<string, int, int> action);

        /// <summary>
        /// 移除变化监听
        /// </summary>
        bool RemoveChanged(string key, int id, Action<string, int, int> action);

        #endregion
    }
}
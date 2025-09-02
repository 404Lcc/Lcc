using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public enum RequestServerStatus
    {
        None,
        Succeed,
        Failed
    }
    
    public class GameServerConfig
    {
        //需要重新校验热更数据
        public bool reCheckVersionUpdate = false;

        //远程渠道
        public int svrChannel;
        //远程版本
        public int svrVersion;

        //远程推荐服
        public string svrLoginServer;
        //远程服务器列表
        public List<string> svrLoginServerList;
        //远程资源服
        public string svrResourceServerUrl;
        //远程资源版本号
        public int svrResVersion;
        //远程更包地址
        public string svrAppForceUpdateUrl;

        //公告地址
        public string noticeUrl;

        public RequestServerStatus Status { get; set; } = RequestServerStatus.None;
    }
}
using System.Collections.Generic;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    internal class PlatformManager : Module, IPlatformService
    {

        private BaseCallbackMessage baseCallbackMessage;

        private BasePlatform basePlatform;


        public PlatformManager()
        {
            InitCallbackMessage();
            InitPlatform();

            if (basePlatform.RequestServer() == string.Empty)
            {
                Log.Debug("推荐服务器是空的");

                var serverList = RequestServerList();
                if (serverList.Count > 0)
                {
                    SetServerCurrent(serverList[0]);
                }
                else
                {
                    Log.Debug("服务器列表是空的");
                }

            }
            else
            {
                SetServerCurrent(basePlatform.RequestServer());
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }



        public void InitCallbackMessage()
        {
            if (Application.isEditor)
            {
                baseCallbackMessage = new DevCallbackMessage();
            }
            else
            {
                if (Launcher.GameConfig.useSDK)
                {
                    baseCallbackMessage = new DevCallbackMessage();
                }
                else
                {
                    baseCallbackMessage = new DevCallbackMessage();
                }
            }
        }


        public void InitPlatform()
        {
            if (Application.isEditor)
            {
                basePlatform = new DevPlatform();
            }
            else
            {
                if (Launcher.GameConfig.useSDK)
                {
                    basePlatform = new DevPlatform();
                }
                else
                {
                    basePlatform = new DevPlatform();
                }
            }

        }



        #region 回调
        public void OnAndroidCallback(string data)
        {
            baseCallbackMessage.OnAndroidCallback(data);
        }
        #endregion


        #region 平台
        public List<string> RequestServerList()
        {
            return basePlatform.RequestServerList();
        }
        public void SetServerCurrent(string server)
        {
            basePlatform.SetServerCurrent(server);
        }
        public string GetServerCurrent()
        {
            return basePlatform.GetServerCurrent();
        }

        public string GetTimeZone()
        {
            return basePlatform.GetTimeZone().ToString();
        }

        public string GetUserRegion()
        {
            return basePlatform.GetUserRegion();
        }

        #endregion



    }
}
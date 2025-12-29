using System.Collections.Generic;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    internal class PlatformManager : Module, IPlatformService
    {
        private BaseCallbackMessage baseCallbackMessage;
        private BasePlatform basePlatform;
        private BaseAccount baseAccount;

        public PlatformManager()
        {
            InitCallbackMessage();
            InitPlatform();
            InitAccount();

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
                if (GameConfig.IsEnableSDK)
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
                if (GameConfig.IsEnableSDK)
                {
                    basePlatform = new DevPlatform();
                }
                else
                {
                    basePlatform = new DevPlatform();
                }
            }
        }

        public void InitAccount()
        {
            if (Application.isEditor)
            {
                baseAccount = new DevAccount();
            }
            else
            {
                if (GameConfig.IsEnableSDK)
                {
                    baseAccount = new DevAccount();
                }
                else
                {
                    baseAccount = new DevAccount();
                }
            }
        }


        #region 账号

        public void SetAccount(string account)
        {
            baseAccount.SetAccount(account);
        }

        public void SetPlayerId(string playerId)
        {
            baseAccount.SetPlayerId(playerId);
        }

        public string GetAccount()
        {
            return baseAccount.GetAccount();
        }

        public string GetToken()
        {
            return baseAccount.GetToken();
        }

        #endregion

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

        public string GetChannel()
        {
            return basePlatform.GetChannel();
        }

        public string GetDeviceId()
        {
            return basePlatform.GetDeviceId();
        }

        #endregion
    }
}
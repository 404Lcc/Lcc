using System.Collections.Generic;

namespace LccHotfix
{
    public interface IPlatformService : IService
    {
        void InitCallbackMessage();


        void InitPlatform();


        #region 账号

        void SetAccount(string account);

        void SetPlayerId(string playerId);

        string GetAccount();

        string GetToken();

        #endregion

        #region 回调

        void OnAndroidCallback(string data);

        #endregion


        #region 平台

        List<string> RequestServerList();
        void SetServerCurrent(string server);
        string GetServerCurrent();

        string GetTimeZone();

        string GetUserRegion();

        string GetChannel();

        string GetDeviceId();

        #endregion
    }
}
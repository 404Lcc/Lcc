using System.Collections.Generic;

namespace LccHotfix
{
    public interface IPlatformService : IService
    {
        void InitCallbackMessage();


        void InitPlatform();


        #region 回调

        void OnAndroidCallback(string data);

        #endregion


        #region 平台

        List<string> RequestServerList();
        void SetServerCurrent(string server);
        string GetServerCurrent();

        string GetTimeZone();

        string GetUserRegion();

        #endregion
    }
}
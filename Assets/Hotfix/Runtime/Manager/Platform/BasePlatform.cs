using System.Collections.Generic;

namespace LccHotfix
{
    public interface BasePlatform
    {
        string RequestServer();
        List<string> RequestServerList();

        void SetServerCurrent(string server);
        string GetServerCurrent();
        string GetUserRegion();
        double GetTimeZone();
        string GetChannel();
        string GetDeviceId();
    }
}
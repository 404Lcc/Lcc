using System;
using System.Collections.Generic;
using LccModel;

namespace LccHotfix
{
    public class DevPlatform : BasePlatform
    {
        public string curServer;

        public string RequestServer()
        {
            return Launcher.Instance.svrLoginServer;
        }
        public List<string> RequestServerList()
        {
            if (Launcher.Instance.svrLoginServerList == null)
            {
                return new List<string>();
            }
            return Launcher.Instance.svrLoginServerList;
        }
        public void SetServerCurrent(string server)
        {
            curServer = server;
        }
        public string GetServerCurrent()
        {
            return curServer;
        }
        public string GetUserRegion()
        {
            return "";
        }

        public double GetTimeZone()
        {
            return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
        }

        public string GetChannel()
        {
            return string.Empty;
        }

        public string GetDeviceId()
        {
            return string.Empty;
        }
    }
}
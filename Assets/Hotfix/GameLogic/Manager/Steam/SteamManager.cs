using System;
using System.Text;
using AOT;
using UnityEngine;
using Steamworks;

namespace LccHotfix
{
    public class SteamManager : Module, ISteamService
    {
        private bool _init;
    
        private SteamAPIWarningMessageHook_t _steamAPIWarningMessageHook;
    
        [MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }
    
        public SteamManager()
        {
            try
            {
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(480)))
                {
                    Application.Quit();
                    return;
                }
            }
            catch (DllNotFoundException e)
            {
                Application.Quit();
                return;
            }
    
            _init = SteamAPI.Init();
    
            if (!_init)
            {
                Application.Quit();
                return;
            }
    
            if (_steamAPIWarningMessageHook == null)
            {
                _steamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(_steamAPIWarningMessageHook);
            }
        }
    
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!_init)
            {
                return;
            }
    
            SteamAPI.RunCallbacks();
        }
    
        internal override void Shutdown()
        {
            if (!_init)
            {
                return;
            }
    
            _steamAPIWarningMessageHook = null;
            SteamAPI.Shutdown();
        }
    }
}
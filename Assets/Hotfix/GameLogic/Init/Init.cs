using LccModel;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections;

namespace LccHotfix
{
    public partial class Init
    {
        public static bool HotfixGameStarted { set; get; } = false;
        public static void Start()
        {
            HotfixGameStarted = false;
            Log.SetLogHelper(new DefaultLogHelper());
            Main.SetMain(new DefaultMain());
            
            try
            {
                Launcher.Instance.actionFixedUpdate += FixedUpdate;
                Launcher.Instance.actionUpdate += Update;
                Launcher.Instance.actionLateUpdate += LateUpdate;
                Launcher.Instance.actionClose += Close;
                Launcher.Instance.actionOnDrawGizmos += DrawGizmos;

                Launcher.Instance.LoadFinish();
                HotfixGameStarted = true;
            }
            catch (System.Exception e)
            {
                Log.Error(e);
            }
        }
        private static void FixedUpdate()
        {
            if (!Launcher.Instance.GameStarted)
                return;
        }
        private static void Update()
        {
            if (!Launcher.Instance.GameStarted)
                return;
            Main.Current.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
        private static void LateUpdate()
        {
            if (!Launcher.Instance.GameStarted)
                return;
            Main.Current.LateUpdate();
        }
        private static void DrawGizmos()
        {
        }
        private static void Close()
        {
            Launcher.Instance.actionFixedUpdate -= FixedUpdate;
            Launcher.Instance.actionUpdate -= Update;
            Launcher.Instance.actionLateUpdate -= LateUpdate;
            Launcher.Instance.actionClose -= Close;
            Launcher.Instance.actionOnDrawGizmos -= DrawGizmos;
            Main.Current.Shutdown();
        }
    }
}
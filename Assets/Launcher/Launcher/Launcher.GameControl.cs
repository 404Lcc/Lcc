using System.Collections;
using UnityEngine;

namespace LccModel
{
    public partial class Launcher
    {
        public const int FPS_HIGH = 60;
        public const int FPS_DEFAULT = 60;
        public const int FPS_PVE = 30;
        public const int FPS_LOADING = 15;

        public bool isPause;

        public void Pause()
        {
            if (!isPause)
            {
                Time.timeScale = 0;
                isPause = true;
            }
        }

        public void Resume()
        {
            if (isPause)
            {
                Time.timeScale = mGameTimeScale * mSlowTimeScale;
                isPause = false;
            }
        }



        public float standardDeltaTime = 0.033f;
        public void ChangeFPS()
        {
            Application.targetFrameRate = FPS_DEFAULT;
            standardDeltaTime = 1f / Application.targetFrameRate;
        }

        public void ChangeFPS(int value)
        {
            Application.targetFrameRate = value;
            standardDeltaTime = 1f / Application.targetFrameRate;
        }

        private float mGameTimeScale = 1f;
        public void SetGameSpeed(float timeScale)
        {
            if (timeScale < 1)
            {
                Debug.LogError("SetGameSpeed = " + timeScale);
                return;
            }
            mGameTimeScale = timeScale;
            Time.timeScale = mGameTimeScale * mSlowTimeScale;
        }

        private float mSlowTimeScale = 1f;
        public void SetGameSlow(bool slow, float timeScale = 1f)
        {
            if (slow)
            {
                if (timeScale > 1)
                {
                    Debug.LogError("SetGameSpeed = " + timeScale);
                    return;
                }
                mSlowTimeScale = timeScale;
            }
            else
            {
                mSlowTimeScale = 1f;
            }
            Time.timeScale = mGameTimeScale * mSlowTimeScale;
        }

        public float GetGameTimeScale()
        {
            return mGameTimeScale;
        }
        public float GetSlowTimeScale()
        {
            return mSlowTimeScale;
        }

    }
}
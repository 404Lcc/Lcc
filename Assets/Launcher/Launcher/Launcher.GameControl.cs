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
                Time.timeScale = _gameTimeScale * _slowTimeScale;
                isPause = false;
            }
        }



        private float _standardDeltaTime = 0.033f;
        public void ChangeFPS()
        {
            Application.targetFrameRate = FPS_DEFAULT;
            _standardDeltaTime = 1f / Application.targetFrameRate;
        }

        public void ChangeFPS(int value)
        {
            Application.targetFrameRate = value;
            _standardDeltaTime = 1f / Application.targetFrameRate;
        }

        private float _gameTimeScale = 1f;
        public void SetGameSpeed(float timeScale)
        {
            if (timeScale < 1)
            {
                Debug.LogError("SetGameSpeed = " + timeScale);
                return;
            }
            _gameTimeScale = timeScale;
            Time.timeScale = _gameTimeScale * _slowTimeScale;
        }

        private float _slowTimeScale = 1f;
        public void SetGameSlow(bool slow, float timeScale = 1f)
        {
            if (slow)
            {
                if (timeScale > 1)
                {
                    Debug.LogError("SetGameSpeed = " + timeScale);
                    return;
                }
                _slowTimeScale = timeScale;
            }
            else
            {
                _slowTimeScale = 1f;
            }
            Time.timeScale = _gameTimeScale * _slowTimeScale;
        }

        public float GetGameTimeScale()
        {
            return _gameTimeScale;
        }
        public float GetSlowTimeScale()
        {
            return _slowTimeScale;
        }

    }
}
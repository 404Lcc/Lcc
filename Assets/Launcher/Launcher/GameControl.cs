using System.Collections;
using UnityEngine;

namespace LccModel
{
    public class GameControl
    {
        public const int FPS_HIGH = 60;
        public const int FPS_DEFAULT = 60;
        public const int FPS_PVE = 30;
        public const int FPS_LOADING = 15;

        private bool _isPause;
        private float _standardDeltaTime = 0.033f;
        private float _gameTimeScale = 1f;
        private float _slowTimeScale = 1f;

        public void Pause()
        {
            if (!_isPause)
            {
                Time.timeScale = 0;
                _isPause = true;
            }
        }

        public void Resume()
        {
            if (_isPause)
            {
                Time.timeScale = _gameTimeScale * _slowTimeScale;
                _isPause = false;
            }
        }

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
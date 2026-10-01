using System;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// TimeTransitionFloat 是一个会随时间变化的float值
    /// 音频控制中经常需要这样的float，故抽出来单独写
    /// </summary>
    public class TimeTransitionFloat
    {
        public float Transition;

        private float _value;
        private float _targetValue;
        public float Value
        {
            get => _value;
            set
            {
                IsTransiting = true;
                if (value > _maxValue)
                {
                    _targetValue = _maxValue;
                    return;
                }

                if (value < _minValue)
                {
                    _targetValue = _minValue;
                    return;
                }

                _targetValue = value;
            }
        }
        public static implicit operator float(TimeTransitionFloat ttf) => ttf.Value;

        public float CurrentValue
        {
            get => _value;
            set
            {
                IsTransiting = true;
                if (value > _maxValue)
                {
                    _value = _maxValue;
                    return;
                }

                if (value < _minValue)
                {
                    _value = _minValue;
                    return;
                }

                _value = value;
            }
        }

        public float TargetValue
        {
            get => _targetValue;
            set => Value = value;
        }

        private float _maxValue = float.MaxValue;
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (_targetValue > _maxValue)
                {
                    _targetValue = MaxValue;
                }

                if (_value > MaxValue)
                {
                    _value = _maxValue;
                }
                _maxValue = value;
            }
        }

        private float _minValue = float.MinValue;
        public float MinValue
        {
            get => _minValue;
            set
            {
                if (_targetValue < _minValue)
                {
                    _targetValue = MinValue;
                }

                if (_value < MinValue)
                {
                    _value = _minValue;
                }
                _minValue = value;
            }
        }

        public bool IsTransiting
        {
            get;
            private set;
        }

        private TimeTransitionFloat(float initValue, float initTransition = 0.01f)
        {
            Transition = initTransition;
            _targetValue = initValue;
            ForceTransitionEnd();
        }

        public void ForceSet(float forceValue)
        {
            _targetValue = forceValue;
            ForceTransitionEnd();
        }

        public void ForceTransitionEnd()
        {
            _value = _targetValue;
            IsTransiting = false;
        }

        public void Update()
        {
            if (!IsTransiting)
            {
                return;
            }
            var deltaTransition = Transition * Time.deltaTime;
            if (Math.Abs(_targetValue - _value) < deltaTransition)
            {
                _value = _targetValue;
                IsTransiting = false;
                return;
            }

            if (_targetValue > _value)
            {
                _value += deltaTransition;
            }
            else
            {
                _value -= deltaTransition;
            }
        }

        public static TimeTransitionFloat NewVolume(float initValue = 1.0f, float initTransition = 0.5f)
        {
            var ttf = new TimeTransitionFloat(initValue, initTransition)
            {
                MaxValue = 1.0f,
                MinValue = 0f
            };
            return ttf;
        }
        
        public static TimeTransitionFloat NewPanning(float initValue = 0f, float initTransition = 0.5f)
        {
            var ttf = new TimeTransitionFloat(initValue, initTransition)
            {
                MaxValue = 1.0f,
                MinValue = -1.0f
            };
            return ttf;
        }
        
        public static TimeTransitionFloat NewBetween(float minValue, float maxValue, float initTransition = 0.5f)
        {
            var ttf = new TimeTransitionFloat(minValue, initTransition)
            {
                MaxValue = maxValue,
                MinValue = minValue
            };
            return ttf;
        }
        
    }
}
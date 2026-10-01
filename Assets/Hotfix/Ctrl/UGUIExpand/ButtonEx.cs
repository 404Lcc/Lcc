using System;
using LitMotion;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class ButtonEx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public bool scalable = true;
        public Vector3 downTo = new Vector3(1f, 1f, 1f);
        public Vector3 upMidTo = new Vector3(0.9f, 0.9f, 1);
        public Vector3 upTo = new Vector3(1f, 1f, 1f);
        public float Duration = 0.1f;

        public bool hasSound = true;
        public string eventName = "ui_click";

        public Func<string> SetSound;
        public Action onPointDown;
        public Action onPointUp;
        public Action onPointEnter;
        public Action onPointExit;
        public bool scaleConfig = false;

        public void Awake()
        {
            if (scaleConfig)
            {
                var scale = transform.localScale.x;
                downTo = new Vector3(scale, scale, 1f);
                upMidTo = new Vector3(scale * 0.9f, scale * 0.9f, 1);
                upTo = new Vector3(scale, scale, 1f);
            }
            else
            {
                downTo = new Vector3(1, 1, 1f);
                upMidTo = new Vector3(0.9f, 0.9f, 1);
                upTo = new Vector3(1f, 1f, 1f);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointDown?.Invoke();

            if (scalable && transform != null)
            {
                LMotion.Create(transform.localScale, downTo, Duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithCancelOnError()
                    .Bind(x =>
                    {
                        if (transform != null)
                            transform.localScale = x;
                    });
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointUp?.Invoke();

            if (scalable && transform != null)
            {
                LMotion.Create(transform.localScale, upMidTo, Duration)
                    .WithCancelOnError()
                    .WithOnComplete(() =>
                    {
                        if (transform == null)
                            return;
                        LMotion.Create(transform.localScale, upTo, Duration)
                            .WithCancelOnError()
                            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                            .Bind(x =>
                            {
                                if (transform != null)
                                    transform.localScale = x;
                            });
                    })
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .Bind(x =>
                    {
                        if (transform != null)
                            transform.localScale = x;
                    });
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!hasSound)
                return;

            try
            {
                if (gameObject.name.ToLower().Contains("close"))
                {
                    eventName = "ui_close";
                }
            }
            catch { }

            var realEventName = (SetSound == null) ? eventName : SetSound?.Invoke();
            GameUtility.PlayUISound(realEventName);
        }

        private void OnDestroy()
        {
            Clear();
        }

        public void Clear()
        {
            if (gameObject != null)
                gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointExit?.Invoke();
        }
    }
}
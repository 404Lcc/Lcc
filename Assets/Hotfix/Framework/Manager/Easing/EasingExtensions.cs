using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public static class EasingExtensions
    {
        public static IEasingCoroutine DoAnchorPosition(this RectTransform rect, Vector2 targetPosition, float duration, float delay = 0)
        {
            return new VectorEasingCoroutine3(rect.anchoredPosition3D, new Vector3(targetPosition.x, targetPosition.y, rect.anchoredPosition3D.z), duration, delay, (position) => rect.anchoredPosition3D = position);
        }

        public static IEasingCoroutine DoSizeDelta(this RectTransform rect, Vector2 targetSizeDelta, float duration, float delay = 0)
        {
            Vector2 startSizeDelta = rect.sizeDelta;

            return new VectorEasingCoroutine2(startSizeDelta, targetSizeDelta, duration, delay, (position) => rect.sizeDelta = position);
        }

        public static IEasingCoroutine DoPosition(this Transform transform, Vector3 targetPosition, float duration, float delay = 0)
        {
            Vector3 startPosition = transform.position;

            return new VectorEasingCoroutine3(startPosition, targetPosition, duration, delay, (position) => transform.position = position);
        }

        public static IEasingCoroutine DoPosition(this Transform transform, Transform targetTransform, float duration, float delay = 0)
        {
            Vector3 startPosition = transform.position;

            IEasingCoroutine coroutine = Main.EasingService.DoFloat(0, 1, duration, (value) =>
            {
                transform.position = Vector3.LerpUnclamped(startPosition, targetTransform.position, value);
            }, delay);

            return coroutine;
        }

        public static IEasingCoroutine DoLocalScale(this Transform transform, Vector3 targetScale, float duration, float delay = 0)
        {
            Vector3 startScale = transform.localScale;

            return new VectorEasingCoroutine3(startScale, targetScale, duration, delay, (scale) => transform.localScale = scale);
        }

        public static void StopIfExists(this IEasingCoroutine coroutine)
        {
            if(coroutine != null && coroutine.IsActive) coroutine.Stop();
        }

        public static bool ExistsAndActive(this IEasingCoroutine coroutine)
        {
            return coroutine != null && coroutine.IsActive;
        }

        public static IEasingCoroutine DoColor(this MaterialPropertyBlock block, string property, Color color, float duration, float delay = 0)
        {
            var id = Shader.PropertyToID(property);

            var startColor = block.GetColor(id);

            return new ColorEasingCoroutine(startColor, color, duration, delay, (color) => block.SetColor(id, color));
        }

        public static IEasingCoroutine DoColor(this Material material, string property, Color color, float duration, float delay = 0)
        {
            var id = Shader.PropertyToID(property);

            var startColor = material.GetColor(id);

            return new ColorEasingCoroutine(startColor, color, duration, delay, (color) => material.SetColor(id, color));
        }

        public static IEasingCoroutine DoColor(this Material material, int propertyId, Color color, float duration, float delay = 0)
        {
            var startColor = material.GetColor(propertyId);

            return new ColorEasingCoroutine(startColor, color, duration, delay, (color) => material.SetColor(propertyId, color));
        }

        public static IEasingCoroutine DoFloat(this Material material, string property, float value, float duration, float delay = 0)
        {
            var id = Shader.PropertyToID(property);

            var startValue = material.GetFloat(id);

            return new FloatEasingCoroutine(startValue, value, duration, delay, (value) => material.SetFloat(id, value));
        }

        public static IEasingCoroutine DoFloat(this Material material, int propertyId, float value, float duration, float delay = 0)
        {
            var startValue = material.GetFloat(propertyId);

            return new FloatEasingCoroutine(startValue, value, duration, delay, (value) => material.SetFloat(propertyId, value));
        }

        public static IEasingCoroutine DoAlpha(this Graphic graphic, float targetAlpha, float duration, float delay = 0)
        {
            var initialAplha = graphic.color.a;

            return new FloatEasingCoroutine(initialAplha, targetAlpha, duration, delay,
                (alpha) =>
                {
                    var color = graphic.color;
                    color.a = alpha;
                    graphic.color = color;
                });
        }

        public static IEasingCoroutine DoAlpha(this CanvasGroup canvasGroup, float targetAlpha, float duration, float delay = 0)
        {
            var initialAplha = canvasGroup.alpha;

            return new FloatEasingCoroutine(initialAplha, targetAlpha, duration, delay, (alpha) => canvasGroup.alpha = alpha);
        }

        public static IEasingCoroutine DoAlpha(this SpriteRenderer spriteRederer, float targetAlpha, float duration, float delay = 0)
        {
            var initialAplha = spriteRederer.color.a;

            return new FloatEasingCoroutine(initialAplha, targetAlpha, duration, delay,
                (alpha) =>
                {
                    var color = spriteRederer.color;
                    color.a = alpha;
                    spriteRederer.color = color;
                });
        }

        public static IEasingCoroutine DoVolume(this AudioSource audioSource, float targetVolume, float duration, float delay = 0)
        {
            return new FloatEasingCoroutine(audioSource.volume, targetVolume, duration, delay, (volume) => audioSource.volume = volume);
        }
    }
}
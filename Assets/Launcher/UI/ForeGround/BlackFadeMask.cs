using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    // 黑屏淡入淡出
    public class BlackFadeMask : MonoBehaviour
    {
        float alpha;

        bool isFadeIn;

        bool isFading = false;

        System.Action fadeDel;
        public AnimationCurve fadeCurve;

        float timer;
        float fadeDuration;

        private RawImage _image;
        RawImage Image
        {
            get
            {
                if (_image == null)
                {
                    _image = gameObject.GetComponent<RawImage>();
                }
                return _image;
            }
        }

        /// <summary>
        /// 遮罩淡入
        /// 透明度从0到1
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="startFactor">起始值</param>
        /// <param name="tex"></param>
        /// <param name="del"></param>
        public void FadeIn(float duration, System.Action del = null, float start = 0, bool black = true)
        {
            if (fadeDel != null)
            {
                var lastDel = fadeDel;
                fadeDel = null;
                lastDel.Invoke();
            }
            if (duration <= 0)
            {
                if (start > 0)
                {
                    gameObject.SetActive(true);
                    alpha = Mathf.Clamp01(start);
                    var color = black ? Color.black : Color.white;
                    color.a = alpha;
                    Image.color = color;

                }
                else
                {
                    gameObject.SetActive(false);
                }
                if (del != null) del.Invoke();
                isFading = false;
            }
            else
            {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
                fadeDuration = duration;
                alpha = Mathf.Clamp01(start);
                var color = black ? Color.black : Color.white;
                color.a = alpha;
                Image.color = color;

                isFading = true;
                isFadeIn = true;
                fadeDel = del;
                timer = 0;
            }
        }

        /// <summary>
        /// 遮罩淡出
        /// 透明度从1到0；
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="start"></param>
        /// <param name="tex"></param>
        /// <param name="del"></param>
        public void FadeOut(float duration, System.Action del = null, float start = 1, bool black = true)
        {
            Image.color = black ? Color.black : Color.white;
            FadeOutInternal(duration, del, start);
        }


        private void FadeOutInternal(float duration, System.Action del = null, float start = 1)
        {
            if (fadeDel != null)
            {
                var lastDel = fadeDel;
                fadeDel = null;
                lastDel.Invoke();
            }
            if (start <= 0)
            {
                gameObject.SetActive(false);
                if (del != null) del.Invoke();
                isFading = false;
            }
            else
            {

                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
                alpha = Mathf.Clamp01(start);
                var color = Image.color;
                color.a = alpha;
                Image.color = color;

                if (duration <= 0)
                {
                    isFading = false;
                    if (del != null) del.Invoke();
                }
                else
                {
                    isFadeIn = false;
                    isFading = true;
                    fadeDuration = duration;
                    fadeDel = del;
                    timer = 0;
                }
            }
        }

        private void Update()
        {
            if (!isFading) return;

            timer += Time.unscaledDeltaTime;

            if (isFadeIn)
            {
                alpha = fadeCurve.Evaluate(timer / fadeDuration);
            }
            else
            {
                alpha = fadeCurve.Evaluate(1 - timer / fadeDuration);
            }
            var color = Image.color;
            color.a = Mathf.Clamp01(alpha);
            Image.color = color;

            if (timer >= fadeDuration)
            {
                isFading = false;
                if (alpha <= 0)
                    gameObject.SetActive(false);
                if (fadeDel != null)
                {
                    var lastDel = fadeDel;
                    fadeDel = null;
                    lastDel.Invoke();
                }
            }
        }
    }
}
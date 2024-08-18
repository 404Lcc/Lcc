using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class UIForeGroundPanel : MonoBehaviour
    {
        private static UIForeGroundPanel _instance;
        public static UIForeGroundPanel Instance => _instance;

        public BlackFadeMask blackMask;

        private void Awake()
        {
            _instance = this;

            blackMask.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _instance = null;
        }



        public void FadeIn(float duration, System.Action del = null, bool pingPong = false, float start = 0, bool black = true)
        {
            if (pingPong)
            {
                blackMask.FadeIn(duration, () =>
                {
                    if (del != null)
                        del();
                    FadeOut(duration, null, black);
                }, start, black);
            }
            else
            {
                blackMask.FadeIn(duration, del, start, black);
            }
        }


        public void FadeOut(float duration, System.Action del = null, bool black = true)
        {
            blackMask.FadeOut(duration, del, 1, black);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DamageNumbersPro.Demo
{
    public class DNP_PrefabSettings : MonoBehaviour
    {
        public int damage = 1;
        public int numberRange = 100;
        public List<string> texts;
        public List<TMP_FontAsset> fonts;
        public bool randomColor;

        public void Apply(DamageNumber target)
        {
            if (texts != null && texts.Count > 0)
            {
                int randomIndex = Random.Range(0, texts.Count);
                target.leftText = texts[randomIndex];

                if(fonts != null && randomIndex < fonts.Count)
                {
                    target.SetFontMaterial(fonts[randomIndex]);
                }

                if(texts.Count > 1)
                {
                    target.enableNumber = false;
                }
            }

            if (randomColor)
            {
                target.SetColor(Color.HSVToRGB(Random.value, 0.5f, 1f));
            }
        }
    }
}

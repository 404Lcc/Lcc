using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DamageNumbersPro.Demo
{
    public class DNP_UIArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static DNP_UIArea currentArea;
        public DNP_UIArea otherArea;
        public bool noSpawnArea = false;
        public bool breakCube = false;

        RectTransform rectTransform;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            currentArea = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(currentArea == this)
            {
                currentArea = null;
            }
        }
        public static RectTransform GetRect()
        {
            return currentArea != null ? (currentArea.otherArea == null ? currentArea.rectTransform : currentArea.otherArea.rectTransform) : null;
        }

        public static bool CanSpawn()
        {
            return currentArea == null || currentArea.noSpawnArea == false;
        }

        public static void OnSpawn()
        {
            if(currentArea != null && currentArea.breakCube)
            {
                DNP_FallingCube cube = currentArea.GetComponent<DNP_FallingCube>();
                cube.Break();
            }
        }
    }
}

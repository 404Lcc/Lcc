/* 
 * Hello dear user.
 * 
 * This is a script made to teach you how to use the asset.
 * 
 * Use DamageNumberGUI for spawning any popups in screen-space or in a GUI canvas.
 * Use DamageNumberMesh for spawning any popups in world-space.
 * 
 * If you need more help you can check out the documentation.
 * Or message me via discord or email.
 * 
 * Good Luck.
 * - Ekincan
 */

using UnityEngine;
using DamageNumbersPro; //Include DamageNumbersPro Namespace     <-----     [REQUIRED]

namespace DamageNumbersPro.Demo
{
    public class DNP_ExampleGUI : MonoBehaviour
    {
        public DamageNumber popupPrefab; //Reference DamageNumber Prefab     <-----     [REQUIRED]

        public RectTransform rectTarget;
        public Vector2 anchoredPosition;

        void Update()
        {
            if (DNP_InputHandler.GetRightClick())
            {
                SpawnPopup(Mathf.Round(Random.Range(1, 10)));
            }
        }

        public void SpawnPopup(float number)
        {
            DamageNumber newPopup = popupPrefab.SpawnGUI(rectTarget, anchoredPosition, number); //Spawn DamageNumber     <-----     [REQUIRED]

            //You can do any change you want on the DamageNumber returned by the Spawn(...) function.
            if(Random.value < 0.5f)
            {
                newPopup.number *= 2;
            }
        }
    }
}


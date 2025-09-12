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
    public class DNP_ExampleMesh : MonoBehaviour
    {
        public DamageNumber popupPrefab; //Reference DamageNumber Prefab     <-----     [REQUIRED]


        public Transform target;
        void Update()
        {
            if(DNP_InputHandler.GetLeftClick())
            {
                SpawnPopup(Mathf.Round(Random.Range(1, 10)));
            }
        }


        public void SpawnPopup(float number)
        {
            DamageNumber newPopup = popupPrefab.Spawn(target.position + new Vector3(0, 0.25f, -1), number); //Spawn DamageNumber At Target     <-----     [REQUIRED]


            //You can do any change you want on the DamageNumber returned by the Spawn(...) function.
            //The following code is [OPTIONAL] just to show you some examples.


            //Let's make the popup follow the target.
            newPopup.SetFollowedTarget(target);

            //Let's check if the number is greater than 5.
            if (number > 5)
            {
                //Let's increase the popup's scale.
                newPopup.SetScale(1.5f);

                //Let's change the color of the popup.
                newPopup.SetColor(new Color(1, 0.2f, 0.2f));
            }
            else
            {
                //The following lines reset the changes above.
                //This would only be neccesary for pooled popups.
                newPopup.SetScale(1);
                newPopup.SetColor(new Color(1, 0.7f, 0.5f));
            }

            //Flip the target.
            target.GetComponent<DNP_Target>().Hit();
        }
    }
}


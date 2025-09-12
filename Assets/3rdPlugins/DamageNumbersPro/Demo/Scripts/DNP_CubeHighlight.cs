using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamageNumbersPro.Demo
{
    public class DNP_CubeHighlight : MonoBehaviour
    {
        public string propertyName = "_Color";
        public AnimationCurve propertyCurve;
        public float destructionDelay = 0.2f;

        Material mat;

        int propertyID;
        float startTime;

        void Start()
        {
            startTime = Time.time;
            propertyID = Shader.PropertyToID(propertyName);

            MeshRenderer mr = GetComponent<MeshRenderer>();
            mat = mr.material;

            Destroy(gameObject, destructionDelay);
        }


        void FixedUpdate()
        {
            mat.SetColor(propertyID, new Color(1, 0, 0, propertyCurve.Evaluate(Time.time - startTime)));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamageNumbersPro.Demo
{
    public class DNP_CubeSpawner : MonoBehaviour
    {
        public float delay = 0.2f;
        public GameObject cube;

        void Start()
        {
            InvokeRepeating("SpawnCube", 0, delay);
        }

        void SpawnCube()
        {
            GameObject newCube = Instantiate<GameObject>(cube);
            newCube.SetActive(true);
            newCube.transform.SetParent(transform, true);
            newCube.transform.localScale = Vector3.one;
        }
    }
}

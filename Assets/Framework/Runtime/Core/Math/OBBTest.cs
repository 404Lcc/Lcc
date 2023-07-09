using LccModel;
using UnityEngine;

namespace LccModel
{
    public class OBBTest : MonoBehaviour
    {
        public GameObject obj1;
        public GameObject obj2;

        private OBB A;
        private OBB B;

        private Color gizmosColor;
        void Start()
        {
            CreateMesh();

            A = new OBB();
            A.size = new Vector3(1, 1, 1);
            B = new OBB();
            B.size = new Vector3(1, 1, 1);
        }
        private void Update()
        {
            A.pos = obj1.transform.position;//new Vector3(0, 0, 0);
            A.xAxis = obj1.transform.right;//new Vector3(1, 0, 0);
            A.yAxis = obj1.transform.up;//new Vector3(0, 1, 0);
            A.zAxis = obj1.transform.forward;// new Vector3(0, 0, 1);

            B.pos = obj2.transform.position;//new Vector3(1, 0, 0);
            B.xAxis = obj2.transform.right;//new Vector3(1, 0, 0);
            B.yAxis = obj2.transform.up;//new Vector3(0, 1, 0);
            B.zAxis = obj2.transform.forward;// new Vector3(0, 0, 1);


            if (A.IsCollision(B))
            {
                Debug.Log("Åö×²");
                gizmosColor = Color.black;
            }
            else
            {
                Debug.Log("Î´Åö×²");
                gizmosColor = Color.white;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = gizmosColor;

            Gizmos.DrawWireMesh(CreateMesh(), A.pos, obj1.transform.localRotation, A.size);

            Gizmos.DrawWireMesh(CreateMesh(), B.pos, obj2.transform.localRotation, B.size);
        }
        Mesh CreateMesh()
        {
            var mesh = new Mesh();
            Vector3[] v3s =
            {
            new Vector3(-0.5f,-0.5f,-0.5f),
            new Vector3(0.5f,-0.5f,-0.5f),
            new Vector3(0.5f,-0.5f,0.5f),
            new Vector3(-0.5f,-0.5f,0.5f),
            new Vector3(-0.5f,0.5f,-0.5f),
            new Vector3(0.5f,0.5f,-0.5f),
            new Vector3(0.5f,0.5f,0.5f),
            new Vector3(-0.5f,0.5f,0.5f)
        };
            Vector3[] v3ss =
            {
            v3s[0], v3s[1], v3s[2], v3s[3],
            v3s[4], v3s[7], v3s[6], v3s[5],
            v3s[0], v3s[4], v3s[5], v3s[1],
            v3s[1], v3s[5], v3s[6], v3s[2],
            v3s[2], v3s[6], v3s[7], v3s[3],
            v3s[3], v3s[7], v3s[4], v3s[0],
        };
            int[] iss =
            {
          0,1,2,
          0,2,3,
          4,5,6,
          4,6,7,
          8,9,10,
          8,10,11,
          12,13,14,
          12,14,15,
          16,17,18,
          16,18,19,
          20,21,22,
          20,22,23
        };
            Vector3[] normals =
            {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,

            Vector3.down,
            Vector3.down,
            Vector3.down,
            Vector3.down,

            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,

            Vector3.right,
            Vector3.right,
            Vector3.right,
            Vector3.right,

            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back,

            Vector3.left,
            Vector3.left,
            Vector3.left,
            Vector3.left,
        };

            mesh.vertices = v3ss;
            mesh.triangles = iss;
            mesh.normals = normals;
            return mesh;
        }
    }
}
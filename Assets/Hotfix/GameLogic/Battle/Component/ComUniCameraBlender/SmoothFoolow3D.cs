using LccHotfix;
using UnityEngine;

public class SmoothFoolow3D : ICameraBlender
{
    //x轴角度
    public float angleX;
    //y轴角度
    public float angleY = 45;
    //y轴距离
    public float distance = 20;

    //阻尼
    public float damping = 45;

    //相机的距离比例
    public float proportion = 1;
    //是否开启阻尼
    public bool isNeedDamping = true;
    //相机偏移
    public Vector3 offset = new Vector3(0, 5f, 0);

    public Transform Target { get; set; }
    public Camera Camera => Main.CameraService.CurrentCamera;

    public void PostInitialize()
    {
    }

    public void Update()
    {
    }

    public void LateUpdate()
    {
        if (Target != null)
        {
            Quaternion localRotation;
            localRotation = Quaternion.Euler(angleY, angleX, 0);

            Vector3 position = localRotation * new Vector3(0, 0, -distance * proportion) + Target.position + offset;
            if (isNeedDamping)
            {
                Camera.gameObject.transform.localRotation = Quaternion.Lerp(Camera.gameObject.transform.localRotation, localRotation, UnityEngine.Time.deltaTime * damping);
                Camera.gameObject.transform.position = Vector3.Lerp(Camera.gameObject.transform.position, position, UnityEngine.Time.deltaTime * damping);
            }
            else
            {
                Camera.gameObject.transform.localRotation = localRotation;
                Camera.gameObject.transform.position = position;
            }
        }
    }
    
    public void ChangeTarget(Transform target)
    {
        Target = target;
    }

    public void Dispose()
    {
    }
}
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
    public float damping = 3;

    //相机的距离比例
    public float proportion = 1;

    //是否开启阻尼
    public bool isNeedDamping = true;

    //相机偏移
    public Vector3 offset = new Vector3(0, 0, 0);

    //震动相关
    public float shakeIntensity;
    public float shakeTimer;
    public bool isShaking;

    public Transform Target { get; set; }
    public Camera Camera => Main.CameraService.CurrentCamera;

    public virtual void PostInitialize()
    {
    }

    public virtual void Update()
    {
    }

    public virtual void LateUpdate()
    {
        if (Target != null)
        {
            Quaternion localRotation;
            localRotation = Quaternion.Euler(angleY, angleX, 0);

            //应用震动偏移
            Vector3 currentOffset = isShaking ? offset + Random.insideUnitSphere * shakeIntensity : offset;

            Vector3 position = localRotation * new Vector3(0, 0, -distance * proportion) + Target.position + currentOffset;

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

            if (isShaking)
            {
                shakeTimer -= Time.deltaTime;
                if (shakeTimer <= 0)
                {
                    isShaking = false;
                }
            }
        }
    }

    public virtual void ChangeTarget(Transform target)
    {
        Target = target;
    }

    public virtual void ShakeCamera(float intensity = 0.5f, float duration = 0.5f)
    {
        if (intensity <= 0 || duration <= 0)
            return;

        shakeIntensity = intensity;
        shakeTimer = duration;
        isShaking = true;
    }

    public virtual void Dispose()
    {
    }
}
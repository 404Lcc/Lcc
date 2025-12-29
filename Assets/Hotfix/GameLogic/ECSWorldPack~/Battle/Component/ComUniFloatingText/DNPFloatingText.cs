using DamageNumbersPro;
using LccHotfix;
using UnityEngine;

public class DNPFloatingText : IFloatingText
{
    private AssetLoader loader;
    private DamageNumberMesh damageNumberMesh;

    public void PostInitialize()
    {
        loader = new AssetLoader();
        loader.LoadAssetAsync<GameObject>("DNP", x =>
        {
            var obj = x.AssetObject as GameObject;
            damageNumberMesh = obj.GetComponent<DamageNumberMesh>();
            damageNumberMesh.cameraOverride = Main.CameraService.CurrentCamera.transform;
        });

    }

    public void Dispose()
    {
        if (loader != null)
        {
            loader.Release();
            loader = null;
        }
    }

    public void Spawn(string text, Vector3 position)
    {
        if (damageNumberMesh == null)
            return;
        var obj = damageNumberMesh.Spawn(position, text);
    }
}
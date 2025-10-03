using DamageNumbersPro;
using LccHotfix;
using UnityEngine;

public class DNPFloatingText : IFloatingText
{
    private GameObject loader;
    private DamageNumberMesh damageNumberMesh;

    public void PostInitialize()
    {
        Main.AssetService.LoadGameObject("DNP", true, out loader);
        
        damageNumberMesh = loader.GetComponent<DamageNumberMesh>();
        damageNumberMesh.cameraOverride = Main.CameraService.CurrentCamera.transform;

        loader.SetActive(false);
    }

    public void Dispose()
    {
        if (loader != null)
        {
            GameObject.Destroy(loader);
            loader = null;
        }
    }

    public void Spawn(string text, Vector3 position)
    {
        var obj = damageNumberMesh.Spawn(position, text);
    }
}
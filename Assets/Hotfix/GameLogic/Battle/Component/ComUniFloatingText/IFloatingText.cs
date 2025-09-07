using UnityEngine;

public interface IFloatingText
{
    void PostInitialize();
    void Dispose();
    void Spawn(float text, Vector3 position);
}
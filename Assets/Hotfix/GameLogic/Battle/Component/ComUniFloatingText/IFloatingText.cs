using UnityEngine;

public interface IFloatingText
{
    void PostInitialize();
    void Dispose();
    void Spawn(string text, Vector3 position);
}
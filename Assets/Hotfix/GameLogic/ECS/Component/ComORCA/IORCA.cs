using UnityEngine;

namespace LccHotfix
{
    public interface IORCA
    {
        void InitOrca(LogicEntity entity);
        void SetDir(Vector3 dir);
        void SetSpeed(float speed);
        void StopORCA();
        void Dispose();
        void Update();
    }
}
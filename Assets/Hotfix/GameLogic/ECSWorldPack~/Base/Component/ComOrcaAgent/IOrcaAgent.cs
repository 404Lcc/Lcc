using LccHotfix;
using RVO;
using UnityEngine;

namespace LccHotfix
{
    public interface IOrcaAgent
    {
        public LogicEntity Entity { get; }
        void InitAgent(LogicEntity entity);
        void SetRadius(float radius);
        void SetSpeed(float speed);
        void SetVelocity(Vector3 dir);
        void Stop();
        void Update();
        void Dispose();
    }
}
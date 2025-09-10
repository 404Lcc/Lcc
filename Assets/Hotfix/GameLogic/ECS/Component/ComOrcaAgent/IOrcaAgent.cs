using LccHotfix;
using RVO;
using UnityEngine;

namespace LccHotfix
{
    public interface IOrcaAgent
    {
        public LogicEntity Entity { get; }
        void InitAgent(LogicEntity entity);
        void SetSpeed(float speed);
        void SetVelocity(Vector3 dir);
        void SetTarget(Vector3 target);

        void Stop();

        void Update();

        void Dispose();
    }
}
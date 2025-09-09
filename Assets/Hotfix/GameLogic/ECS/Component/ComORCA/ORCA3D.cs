using System;
using RVO;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = RVO.Vector2;

namespace LccHotfix
{
    public class ORCA3D : IORCA
    {
        private bool _isActive;
        public LogicEntity Entity { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (!_isActive)
                {
                    Simulator.Instance.setAgentPrefVelocity(AgentId, new Vector2(0, 0));
                }
                else
                {
                    Simulator.Instance.setAgentPrefVelocity(AgentId, new Vector2(Dir.x, Dir.z));
                }
            }
        }

        public Agent Agent { get; set; }
        public int AgentId => Agent.id_;
        public Vector3 Dir { get; set; }
        public float Speed { get; set; }

        public void InitOrca(LogicEntity entity)
        {
            Entity = entity;
            Agent = Simulator.Instance.addAgent(new Vector2(0, 0));
            Simulator.Instance.setAgentPosition(AgentId, new Vector2(Entity.comTransform.position.x, Entity.comTransform.position.z));
        }

        public void SetDir(Vector3 dir)
        {
            Dir = dir;
            IsActive = true;
        }

        public void SetSpeed(float speed)
        {
            this.Speed = speed;
        }

        public void StopORCA()
        {
            IsActive = false;
        }

        public void Dispose()
        {
            if (Agent == null)
                return;
            var id = Agent.id_;
            Simulator.Instance.delAgent(id);
            Agent = null;
        }


        public void Update()
        {
            if (!IsActive)
            {
                return;
            }

            if (Agent == null)
            {
                return;
            }

            if (AgentId < 0)
            {
                return;
            }

            var pos = Simulator.Instance.getAgentPosition(AgentId);

            if (float.IsNaN(pos.x()) || float.IsNaN(pos.y()))
            {
                return;
            }

            Entity.SetPosition(new Vector3(pos.x(), Entity.comTransform.position.y, pos.y()));

            if (Dir.sqrMagnitude <= 0)
            {
                Simulator.Instance.setAgentPrefVelocity(AgentId, new Vector2(0, 0));
            }
            else
            {
                Vector2 newVelocity = new Vector2(Dir.x, Dir.z) * Speed;
                float angle = Random.value * 2.0f * (float)Math.PI;
                float dist = Random.value * 0.0001f;
                Simulator.Instance.setAgentPrefVelocity(AgentId, newVelocity + dist * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
            }
        }
    }
}
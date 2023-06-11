using RVO;
using System;
using UnityEngine;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;

namespace LccModel
{
    public class OrcaComponent : Component, IFixedUpdate
    {
        public Random random;
        public RVO.Vector2 target;
        public Agent agent;

        public int ID
        {
            get
            {
                return agent.id_;
            }
        }
        public AttributeComponent AttributeComponent => GetParent<Combat>().GetComponent<AttributeComponent>();
        public TransformComponent TransformComponent => GetParent<Combat>().GetComponent<TransformComponent>();

        public override void Awake()
        {
            base.Awake();

            random = new Random();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemoveAgent();
        }

        public Agent AddAgent2D(Vector2 pos)
        {
            RVO.Vector2 vector = new RVO.Vector2(pos.x, pos.y);
            agent = Simulator.Instance.addAgent(vector);
            agent.maxSpeed_ = AttributeComponent.MoveSpeed.Value;
            agent.radius_ = 3;
            return agent;
        }
        public Agent AddAgent3D(Vector3 pos)
        {
            RVO.Vector2 vector = new RVO.Vector2(pos.x, pos.z);
            agent = Simulator.Instance.addAgent(vector);
            agent.maxSpeed_ = AttributeComponent.MoveSpeed.Value;
            agent.radius_ = 3;
            return agent;
        }
        public void RemoveAgent()
        {
            Simulator.Instance.delAgent(ID);
        }
        public Vector2 GetAgent2DPos()
        {
            Vector2 pos = Vector2.zero;
            var temp = Simulator.Instance.getAgentPosition(ID);
            pos.x = temp.x();
            pos.y = temp.y();
            return pos;
        }
        public Vector2 GetAgent3DPos()
        {
            Vector3 pos = Vector3.zero;
            var temp = Simulator.Instance.getAgentPosition(ID);
            pos.x = temp.x();
            pos.y = 0;
            pos.z = temp.y();
            return pos;
        }
        public void SetAgent2DPos(Vector2 pos)
        {
            RVO.Vector2 vector = new RVO.Vector2(pos.x, pos.y);
            Simulator.Instance.setAgentPosition(ID, vector);
        }
        public void SetAgent3DPos(Vector3 pos)
        {
            RVO.Vector2 vector = new RVO.Vector2(pos.x, pos.z);
            Simulator.Instance.setAgentPosition(ID, vector);
        }
        public void Set2DTarget(Vector2 pos)
        {
            target = new RVO.Vector2(pos.x, pos.y);
        }
        public void Set3DTarget(Vector3 pos)
        {
            target = new RVO.Vector2(pos.x, pos.z);
        }

        public void FixedUpdate()
        {
            var pos = Simulator.Instance.getAgentPosition(ID);
            TransformComponent.position = new Vector3(pos.x(), pos.y(), 0);


            if (target.x() == 0 || target.y() == 0) return;
            if (Simulator.Instance.isNeedDelete(ID)) return;
            var goalVector = target - Simulator.Instance.getAgentPosition(ID);
            if (RVOMath.absSq(goalVector) > 0.01f)
            {
                goalVector = RVOMath.normalize(goalVector) * agent.maxSpeed_;
                Simulator.Instance.setAgentPrefVelocity(ID, goalVector);

                float angle = (float)random.NextDouble() * 2.0f * (float)Math.PI;
                float dist = (float)random.NextDouble() * 0.0001f;

                Simulator.Instance.setAgentPrefVelocity(ID, Simulator.Instance.getAgentPrefVelocity(ID) + dist * new RVO.Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
            }
            else
            {
                Simulator.Instance.setAgentPrefVelocity(ID, new RVO.Vector2(0, 0));
            }


        }
    }
}
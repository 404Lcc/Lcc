using System;
using System.Collections.Generic;
using LccHotfix;
using RVO;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LccHotfix
{
    public class OrcaAgent2D : IOrcaAgent
    {
        private float _stoppingDistance = 5f;

        public LogicEntity Entity { get; private set; }

        public int AgentId { get; set; }
        public Simulator Simulator => WorldUtility.GetMetaContext().ComUniOrca.Simulator;

        public void InitAgent(LogicEntity entity)
        {
            Entity = entity;

            var position = new float2(Entity.comTransform.position.x, Entity.comTransform.position.y);
            AgentId = Simulator.AddAgent(position);

            var radius = Entity.comTransform.scale.x;
            SetRadius(radius);

            SetVelocity(Vector3.zero);
        }

        public void SetRadius(float radius)
        {
            if (AgentId < 0)
            {
                return;
            }

            Simulator.SetAgentRadius(AgentId, radius);
        }

        public void SetSpeed(float speed)
        {
            if (AgentId < 0)
            {
                return;
            }

            Simulator.SetAgentMaxSpeed(AgentId, speed);
        }

        public void SetVelocity(Vector3 velocity)
        {
            if (AgentId < 0)
            {
                return;
            }

            float2 dir = new float2(velocity.x, velocity.y);
            SetVelocity(dir);
        }

        private void SetVelocity(float2 dir)
        {
            if (AgentId < 0)
            {
                return;
            }

            if (math.lengthsq(dir) > _stoppingDistance * _stoppingDistance)
            {
                dir = math.normalize(dir);
                dir += (float2)Random.insideUnitCircle * 0.001f;
            }

            Simulator.SetAgentPrefVelocity(AgentId, dir);
        }


        public void Stop()
        {
            if (AgentId < 0)
            {
                return;
            }

            SetVelocity(Vector3.zero);
            float2 pos = new float2(Entity.comTransform.position.x, Entity.comTransform.position.y);
            Simulator.SetAgentPosition(AgentId, pos);
        }

        public void Update()
        {
            if (AgentId < 0)
            {
                return;
            }

            var pos = Simulator.GetAgentPosition(AgentId);

            if (float.IsNaN(pos.x) || float.IsNaN(pos.y))
            {
                return;
            }

            var prevPosition = Entity.comTransform.position;
            Entity.SetPosition(new Vector3(pos.x, pos.y, prevPosition.z));
        }

        public void Dispose()
        {
            if (AgentId < 0)
            {
                return;
            }

            if (Simulator == null)
            {
                return;
            }

            Simulator.RemoveAgent(AgentId);
            AgentId = -1;
        }
    }
}
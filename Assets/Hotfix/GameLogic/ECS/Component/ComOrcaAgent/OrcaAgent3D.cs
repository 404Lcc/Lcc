using System;
using System.Collections.Generic;
using LccHotfix;
using RVO;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LccHotfix
{
    public class OrcaAgent3D : IOrcaAgent
    {
        private float _stoppingDistance = 5f;

        public LogicEntity Entity { get; private set; }

        public int AgentId { get; set; }
        public Simulator Simulator => WorldUtility.GetMetaContext().ComUniOrca.Simulator;

        public void InitAgent(LogicEntity entity)
        {
            Entity = entity;

            var position = new float2(Entity.comTransform.position.x, Entity.comTransform.position.z);
            AgentId = Simulator.AddAgent(position);

            var radius = Entity.comTransform.scale.x;
            Simulator.SetAgentRadius(AgentId, radius);

            SetVelocity(Vector3.zero);
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

            float2 dir = new float2(velocity.x, velocity.z);
            SetVelocity(dir);
        }

        public void SetTarget(Vector3 targetPos)
        {
            if (AgentId < 0)
            {
                return;
            }

            float2 target = new float2(targetPos.x, targetPos.z);
            float2 dir = target - Simulator.GetAgentPosition(AgentId);
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
            float2 pos = new float2(Entity.comTransform.position.x, Entity.comTransform.position.z);
            Simulator.SetAgentPosition(AgentId, pos);
        }

        public void Update()
        {
            if (AgentId < 0)
            {
                return;
            }

            var pos = Simulator.GetAgentPosition(AgentId);
            var prevPosition = Entity.comTransform.position;
            Entity.SetPosition(new Vector3(pos.x, prevPosition.y, pos.y));
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
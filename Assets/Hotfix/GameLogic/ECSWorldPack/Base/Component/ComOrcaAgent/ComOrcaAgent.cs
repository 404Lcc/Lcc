using RVO;
using Unity.Mathematics;
using UnityEngine;

namespace LccHotfix
{
    public class ComOrcaAgent : LogicComponent
    {
        private IOrcaAgent _agent;

        public void SetAgent(IOrcaAgent agent)
        {
            this._agent = agent;
        }

        public void SetRadius(float radius)
        {
            _agent.SetRadius(radius);
        }

        public void SetSpeed(float speed)
        {
            _agent.SetSpeed(speed);
        }

        public void SetVelocity(Vector3 dir)
        {
            _agent.SetVelocity(dir);
        }

        public void Stop()
        {
            _agent.Stop();
        }

        public override void Dispose()
        {
            base.Dispose();

            _agent.Dispose();
        }

        public void Update()
        {
            _agent.Update();
        }
    }

    public partial class LogicEntity
    {
        public ComOrcaAgent ComOrcaAgent
        {
            get { return (ComOrcaAgent)GetComponent(LogicComponentsLookup.ComOrcaAgent); }
        }

        public bool hasComOrcaAgent
        {
            get { return HasComponent(LogicComponentsLookup.ComOrcaAgent); }
        }

        public void AddComOrcaAgent<T>() where T : IOrcaAgent, new()
        {
            var index = LogicComponentsLookup.ComOrcaAgent;
            var component = (ComOrcaAgent)CreateComponent(index, typeof(ComOrcaAgent));
            AddComponent(index, component);
            var agent = new T();
            agent.InitAgent(this);
            component.SetAgent(agent);
        }

    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComOrcaAgentIndex = new ComponentTypeIndex(typeof(ComOrcaAgent));
        public static int ComOrcaAgent => ComOrcaAgentIndex.index;
    }
}
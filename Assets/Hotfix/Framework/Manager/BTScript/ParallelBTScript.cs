using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ParallelBTScript : BTScript
    {
        private List<IBTScript> _nodeList = new List<IBTScript>();

        public override void Dispose()
        {
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                _nodeList[i].Dispose();
            }

            _nodeList.Clear();

            base.Dispose();
        }

        public override void InitAgent(BTAgent agent)
        {
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                _nodeList[i].InitAgent(agent);
            }

            base.InitAgent(agent);
        }


        public override void Update(BTAgent agent, float dt)
        {
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                _nodeList[i].Update(agent, dt);
            }

            base.Update(agent, dt);
        }

        public override void LateUpdate(BTAgent agent, float dt)
        {
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                _nodeList[i].LateUpdate(agent, dt);
            }

            base.LateUpdate(agent, dt);
        }

        public override void DestroyAgent(BTAgent agent)
        {
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                _nodeList[i].DestroyAgent(agent);
            }

            base.DestroyAgent(agent);
        }

        public override void Trigger(BTAction trigger, BTAgent agent, params object[] args)
        {
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                var iTrigger = _nodeList[i] as IBTScriptTrigger;
                if (iTrigger != null)
                {
                    iTrigger.Trigger(trigger, agent, args);
                }
            }

            base.Trigger(trigger, agent, args);
        }

        public override Action<BTAgent, object[]> GetTrigger(BTAction trigger)
        {
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                var iTrigger = _nodeList[i] as IBTScriptTrigger;
                if (iTrigger != null)
                {
                    var action = iTrigger.GetTrigger(trigger);
                    if (action != null)
                    {
                        return action;
                    }
                }
            }

            return base.GetTrigger(trigger);
        }

        public void AddBTScript(string name)
        {
            var btScript = Main.BTScriptService.GetScript(name);
            if (btScript != null)
            {
                _nodeList.Add(btScript);
            }
        }
    }
}
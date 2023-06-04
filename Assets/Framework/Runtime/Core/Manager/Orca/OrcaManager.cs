using NPOI.OpenXmlFormats.Dml.Chart;
using RVO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace LccModel
{
    public class OrcaManager : AObjectBase, IFixedUpdate
    {
        public static OrcaManager Instance;

        public Dictionary<int, Agent> agentDict = new Dictionary<int, Agent>();
        public Dictionary<int, ObstaclePolygon> obstacleDict = new Dictionary<int, ObstaclePolygon>();
        public override void Awake()
        {
            base.Awake();

            Instance = this;

            Simulator.Instance.setAgentDefaults(15f, 10, 10.0f, 10.0f, 0.125f, 0.5f, new RVO.Vector2(0.0f, 0.0f));
            Simulator.Instance.setTimeStep(0.02f);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
        }
        public Agent AddAgent2D(Vector2 pos)
        {
            RVO.Vector2 vector = new RVO.Vector2(pos.x, pos.y);
            Agent agent = Simulator.Instance.addAgent(vector);
            agentDict.Add(agent.id_, agent);
            return agent;
        }
        public Agent AddAgent3D(Vector3 pos)
        {
            RVO.Vector2 vector = new RVO.Vector2(pos.x, pos.z);
            Agent agent = Simulator.Instance.addAgent(vector);
            agentDict.Add(agent.id_, agent);
            return agent;
        }
        public void RemoveAgent(int id)
        {
            if (agentDict.TryGetValue(id, out var agent))
            {
                Simulator.Instance.delAgent(id);
                agentDict.Remove(id);
            }
        }
        public void SetAgentPos(int id, Vector2 pos)
        {
            RVO.Vector2 vector = new RVO.Vector2(pos.x, pos.y);
            Simulator.Instance.setAgentPosition(id, vector);
        }
        public void SetAgentPrefVelocity(int id, Vector2 pos)
        {
            RVO.Vector2 vector = new RVO.Vector2(pos.x, pos.y);
            Simulator.Instance.setAgentPrefVelocity(id, vector);
        }

        public ObstaclePolygon AddObstacle(Bounds bounds)
        {
            List<RVO.Vector2> obstacle = new List<RVO.Vector2>();

            obstacle.Add(new RVO.Vector2(bounds.max.x, bounds.max.y));
            obstacle.Add(new RVO.Vector2(bounds.min.x, bounds.max.y));
            obstacle.Add(new RVO.Vector2(bounds.min.x, bounds.min.y));
            obstacle.Add(new RVO.Vector2(bounds.max.x, bounds.min.y));

            ObstaclePolygon obstaclePolygon = Simulator.Instance.addObstacle(obstacle.ToArray());
            obstacleDict.Add(obstaclePolygon._id, obstaclePolygon);
            return obstaclePolygon;
        }
        public void RemoveObstacle(int id)
        {
            if (obstacleDict.TryGetValue(id, out var obstacle))
            {
                Simulator.Instance.delObstacle(id);
                obstacleDict.Remove(id);
            }
        }

        public void MoveTarget2D(int id, Vector2 target, float speed)
        {
            if (Simulator.Instance.isNeedDelete(id))
            {
                return;
            }
            RVO.Vector2 vector = new RVO.Vector2(target.x, target.y);
            var goalVector = vector - Simulator.Instance.getAgentPosition(id);
            if (RVOMath.absSq(goalVector) > 0.01f)
            {
                goalVector = RVOMath.normalize(goalVector) * speed;
                Simulator.Instance.setAgentPrefVelocity(id, goalVector);
            }
            else
            {
                Simulator.Instance.setAgentPrefVelocity(id, new RVO.Vector2(0, 0));
            }
        }
        public void MoveTarget3D(int id, Vector3 target, float speed)
        {
            if (Simulator.Instance.isNeedDelete(id))
            {
                return;
            }
            RVO.Vector2 vector = new RVO.Vector2(target.x, target.z);
            var goalVector = vector - Simulator.Instance.getAgentPosition(id);
            if (RVOMath.absSq(goalVector) > 0.01f)
            {
                goalVector = RVOMath.normalize(goalVector) * speed;
                Simulator.Instance.setAgentPrefVelocity(id, goalVector);
            }
            else
            {
                Simulator.Instance.setAgentPrefVelocity(id, new RVO.Vector2(0, 0));
            }
        }



        public void FixedUpdate()
        {
            Simulator.Instance.doStep();
        }
    }
}
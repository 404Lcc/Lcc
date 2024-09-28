using RVO;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class OrcaManager : Module
    {
        public static OrcaManager Instance { get; } = Entry.GetModule<OrcaManager>();

        public Dictionary<int, ObstaclePolygon> obstacleDict = new Dictionary<int, ObstaclePolygon>();
        public OrcaManager()
        {

            Simulator.Instance.setAgentDefaults(15f, 10, 10.0f, 10.0f, 3, 10, new RVO.Vector2(0.0f, 0.0f));
            Simulator.Instance.setTimeStep(0.02f);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            Simulator.Instance.doStep();
        }

        internal override void Shutdown()
        {
            obstacleDict.Clear();
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
    }
}
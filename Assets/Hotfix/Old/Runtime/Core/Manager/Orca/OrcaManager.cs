using RVO;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class OrcaManager : AObjectBase, IFixedUpdate
    {
        public static OrcaManager Instance;

        public Dictionary<int, ObstaclePolygon> obstacleDict = new Dictionary<int, ObstaclePolygon>();
        public override void Awake()
        {
            base.Awake();

            Instance = this;

            Simulator.Instance.setAgentDefaults(15f, 10, 10.0f, 10.0f, 3, 10, new RVO.Vector2(0.0f, 0.0f));
            Simulator.Instance.setTimeStep(0.02f);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
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



        public void FixedUpdate()
        {
            Simulator.Instance.doStep();
        }
    }
}
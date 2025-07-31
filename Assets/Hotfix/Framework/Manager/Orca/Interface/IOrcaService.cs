using RVO;
using UnityEngine;

namespace LccHotfix
{
    public interface IOrcaService : IService
    {
        ObstaclePolygon AddObstacle(Bounds bounds);

        void RemoveObstacle(int id);
    }
}
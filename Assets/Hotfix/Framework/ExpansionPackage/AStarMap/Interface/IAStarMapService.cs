using System.Collections;
using System.Collections.Generic;
using LccHotfix;
using UnityEngine;

namespace LccHotfix
{
    public interface IAStarMapService : IService
    {
        void CreateAStarMap(float radius, LayerMask layer, bool isShowWall, bool isShowPath, Bounds bounds, float ratio = 1, bool isDebugDraw = false);

        void CreateAStarMap(float radius, LayerMask layer, bool isShowWall, bool isShowPath, BoxCollider2D boxCollider2D, float ratio = 1, bool isDebugDraw = false);

        List<AStarNodeData> AStarFindPath(Vector3 start, Vector3 end);

        void Clear();
    }
}
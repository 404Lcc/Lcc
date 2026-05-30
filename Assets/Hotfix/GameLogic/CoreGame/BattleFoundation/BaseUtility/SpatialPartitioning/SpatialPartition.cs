using System;

namespace LccHotfix
{
    public class SpatialPartition
    {

        public static AQuadSpace Create(AABB fullSpace, int layer)
        {
            if (fullSpace.Width() <= 0 || fullSpace.Height() <= 0)
            {
                throw new ArgumentException();
            }

            var root = new QuadSpaceNode();
            Partiting(root, 0, layer);
            root.SetAABB(fullSpace);
            return root;
        }

        static void Partiting(QuadSpaceNode node, int index, int depth)
        {
            if (index < depth)
            {
                var nextDepth = index + 1;
                var quadrant0 = new QuadSpaceNode();
                var quadrant1 = new QuadSpaceNode();
                var quadrant2 = new QuadSpaceNode();
                var quadrant3 = new QuadSpaceNode();
                Partiting(quadrant0, nextDepth, depth);
                Partiting(quadrant1, nextDepth, depth);
                Partiting(quadrant2, nextDepth, depth);
                Partiting(quadrant3, nextDepth, depth);
                node.quadrant0 = quadrant0;
                node.quadrant1 = quadrant1;
                node.quadrant2 = quadrant2;
                node.quadrant3 = quadrant3;
            }
            else
            {
                node.quadrant0 = NewLeaf();
                node.quadrant1 = NewLeaf();
                node.quadrant2 = NewLeaf();
                node.quadrant3 = NewLeaf();
            }
        }

        private static AQuadSpace NewLeaf()
        {
            return new QuadSpaceLeafFaster();
        }
    }
}
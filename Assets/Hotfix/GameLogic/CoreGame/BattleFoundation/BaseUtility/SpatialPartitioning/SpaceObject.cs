namespace LccHotfix
{
    public struct SpaceObject
    {
        public AABB aabb;
        public object obj;
        public int layer;

        public SpaceObject(AABB aabb, object obj, int layer)
        {
            this.aabb = aabb;
            this.obj = obj;
            this.layer = layer;
        }
    }
}

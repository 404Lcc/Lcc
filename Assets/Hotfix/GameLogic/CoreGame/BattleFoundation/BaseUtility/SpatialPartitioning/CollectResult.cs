using System.Collections.Generic;

namespace LccHotfix
{
    public struct CollectPair
    {
        public object obj0;
        public object obj1;

        public CollectPair(SpaceObject item0, SpaceObject item1)
        {
            obj0 = item0.obj;
            obj1 = item1.obj;
        }
    }
    public class CollectResult
    {
        public HashSet<CollectPair> result = new HashSet<CollectPair>();
    }

}

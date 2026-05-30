using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public class QuadSpaceLeaf : AQuadSpace
    {
        public List<SpaceObject> objects = new List<SpaceObject>();
        protected override void CollectInternal(CollectResult results)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                for (int j = i + 1; j < objects.Count; j++)
                {
                    var itema = objects[i];
                    var itemb = objects[j];
                    if (itema.layer != itemb.layer && itema.aabb.Intersects(itemb.aabb))
                    {
                        var item0 = objects[i];
                        var item1 = objects[j];
                        results.result.Add(new CollectPair(item0, item1));
                    }
                }
            }
        }
        protected override void AddObjectInternal(SpaceObject objec)
        {
            this.objects.Add(objec);
        }
        private void CollectB(CollectResult results)
        {
            var idc = new Dictionary<int, List<SpaceObject>>();
            foreach (var item in objects)
            {
                if (!idc.TryGetValue(item.layer, out var list))
                {
                    list = new List<SpaceObject>();
                }
                list.Add(item);
            }
            var values = idc.Values.ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                for (int j = 0; j < values.Length; j++)
                {
                    var itemas = values[i];
                    var itembs = values[j];
                    foreach (var itema in itemas)
                    {
                        foreach (var itemb in itembs)
                        {
                            if (itema.aabb.Intersects(itemb.aabb))
                            {
                                results.result.Add(new CollectPair(itema, itemb));
                            }
                        }
                    }
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            objects.Clear();
        }
    }
}

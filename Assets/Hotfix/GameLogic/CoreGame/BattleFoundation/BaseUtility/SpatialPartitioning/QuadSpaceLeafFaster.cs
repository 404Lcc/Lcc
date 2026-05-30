using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public class QuadSpaceLeafFaster : AQuadSpace
    {
        public Dictionary<int, List<SpaceObject>> layers = new Dictionary<int, List<SpaceObject>>();

        public override void Clear()
        {
            base.Clear();
            foreach (var item in layers.Values)
            {
                item.Clear();
            }
        }

        protected override void AddObjectInternal(SpaceObject objec)
        {
            if (!layers.TryGetValue(objec.layer, out var spaces))
            {
                spaces = new List<SpaceObject>();
                layers.Add(objec.layer, spaces);
            }
            spaces.Add(objec);
        }

        protected override void CollectInternal(CollectResult results)
        {
            var layerArray = layers.Values.ToArray();

            for (int i = 0; i < layerArray.Length; i++)
            {
                for (int j = i + 1; j < layerArray.Length; j++)
                {
                    CollectLayer(results, layerArray[i], layerArray[j]);
                }
            }
        }

        private void CollectLayer(CollectResult results, List<SpaceObject> layera, List<SpaceObject> layerb)
        {
            for (int i = 0; i < layera.Count; i++)
            {
                for (int j = 0; j < layerb.Count; j++)
                {
                    var itema = layera[i];
                    var itemb = layerb[j];
                    if (itema.aabb.Intersects(itemb.aabb))
                        results.result.Add(new CollectPair(itema, itemb));
                }
            }
        }
    }
}

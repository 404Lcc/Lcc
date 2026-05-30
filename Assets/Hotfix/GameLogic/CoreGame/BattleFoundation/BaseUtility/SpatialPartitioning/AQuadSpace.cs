using System.Collections.Generic;

namespace LccHotfix
{
    public abstract class AQuadSpace
    {
        public AABB aabb { get; private set; }
        public int objectCount { get; private set; }

        public virtual void SetAABB(AABB fullSpace)
        {
            this.aabb = fullSpace;
        }
        public void AddObject(SpaceObject objec)
        {
            if (this.aabb.Intersects(objec.aabb))
            {
                objectCount++;
                AddObjectInternal(objec);
            }
        }

        public void AddObjects(IEnumerable<SpaceObject> spaceObject1)
        {
            foreach (var item in spaceObject1)
            {
                AddObject(item);
            }
        }
        private bool mayHasCollision()
        {
            return objectCount > 1;
        }
        public void Collect(CollectResult results)
        {
            if (!mayHasCollision())
                return;
            CollectInternal(results);
        }
        protected abstract void AddObjectInternal(SpaceObject objec);

        protected abstract void CollectInternal(CollectResult results);
        public virtual void Clear()
        {
            objectCount = 0;
        }
    }
}

namespace LccHotfix
{
    public class LoopScrollPool
    {
        public T Get<T>() where T : LoopScrollItem, new()
        {
            var item = ReferencePool.Acquire<T>();
            return item;
        }

        public void Release<T>(T item) where T : LoopScrollItem, new()
        {
            ReferencePool.Release(item);
        }

        public void Clear<T>() where T : LoopScrollItem, new()
        {
            ReferencePool.RemoveAll<T>();
        }
    }
}
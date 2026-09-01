namespace LccHotfix
{
    /// <summary>
    /// 子弹时间下需要时间补偿的标记。由上层在挂载交互物/地图物件时写入。
    /// </summary>
    public class BulletTimeCompensateComponent : LogicComponent
    {
    }

    public partial class LogicEntity
    {
        public BulletTimeCompensateComponent comBulletTimeCompensate
        {
            get { return (BulletTimeCompensateComponent)GetComponent(LogicComponentsLookup.ComBulletTimeCompensate); }
        }

        public bool hasComBulletTimeCompensate
        {
            get { return HasComponent(LogicComponentsLookup.ComBulletTimeCompensate); }
        }

        public void AddComBulletTimeCompensate()
        {
            if (hasComBulletTimeCompensate)
            {
                return;
            }

            var index = LogicComponentsLookup.ComBulletTimeCompensate;
            var component = (BulletTimeCompensateComponent)CreateComponent(index, typeof(BulletTimeCompensateComponent));
            AddComponent(index, component);
        }

        public void RemoveComBulletTimeCompensate()
        {
            if (hasComBulletTimeCompensate)
            {
                RemoveComponent(LogicComponentsLookup.ComBulletTimeCompensate);
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComBulletTimeCompensateIndex = new(typeof(BulletTimeCompensateComponent));
        public static int ComBulletTimeCompensate => _ComBulletTimeCompensateIndex.Index;
    }
}

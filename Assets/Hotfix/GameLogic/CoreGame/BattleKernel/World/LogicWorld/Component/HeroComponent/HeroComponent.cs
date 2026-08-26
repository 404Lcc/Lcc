namespace LccHotfix
{
    // 守护者身份组件
    public class HeroComponent : LogicComponent
    {
        // 守护者的配置
        public HeroInfo Info { get; protected set; }

        public void Init(HeroInfo info)
        {
            Info = info;
        }
        
        public override void DisposeOnRemove()
        {
            Info = null;
            base.DisposeOnRemove();
        }
    }

    public partial class LogicEntity
    {
        public HeroComponent comHero
        {
            get { return (HeroComponent)GetComponent(LogicComponentsLookup.ComHero); }
        }

        public bool hasComHero
        {
            get { return HasComponent(LogicComponentsLookup.ComHero); }
        }

        public void AddComHero(HeroInfo info)
        {
            var index = LogicComponentsLookup.ComHero;
            var component = (HeroComponent)CreateComponent(index, typeof(HeroComponent));
            component.Init(info);
            AddComponent(index, component);
        }

        public void RemoveComHero()
        {
            if (hasComHero)
            {
                RemoveComponent(LogicComponentsLookup.ComHero);
            }
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComHeroIndex = new(typeof(HeroComponent));
        public static int ComHero => _ComHeroIndex.Index;
    }
}
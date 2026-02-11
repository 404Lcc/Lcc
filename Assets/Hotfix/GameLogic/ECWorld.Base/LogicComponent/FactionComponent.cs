namespace LccHotfix
{
    //这里区分的是阵营不是身份类别
    //比如魏国、蜀国是阵营，士兵、将军、文官是身份。 城池、武将是类别
    public enum EFaction
    {
        Invalid = 0,
        Friend = 1, //友方
        Enemy = 2, //敌方
        Neutral = 3, //中立
    }

    public class FactionComponent : LogicComponent
    {
        public EFaction Faction { get; private set; }

        public void Init(EFaction faction)
        {
            Faction = faction;
        }
    }

    public partial class LogicEntity
    {
        public FactionComponent comFaction
        {
            get { return (FactionComponent)GetComponent(LogicComponentsLookup.ComFaction); }
        }

        public bool hasComFaction
        {
            get { return HasComponent(LogicComponentsLookup.ComFaction); }
        }
        
        public void ReplaceComFaction(EFaction faction)
        {
            var index = LogicComponentsLookup.ComFaction;
            if (!hasComTag)
            {
                var component = (FactionComponent)CreateComponent(index, typeof(FactionComponent));
                component.Init(faction);
                AddComponent(index, component);
            }
            else
            {
                var component = (FactionComponent)GetComponent(index);
                component.Init(faction);
                ReplaceComponent(index, component);
            }
        }
        
        public EFaction Faction
        {
            get
            {
                var index = LogicComponentsLookup.ComFaction;
                if (!hasComTag)
                {
                    return EFaction.Invalid;
                }

                var component = (FactionComponent)GetComponent(index);
                return component.Faction;
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComFactionIndex = new(typeof(FactionComponent));
        public static int ComFaction => _ComFactionIndex.Index;
    }
}
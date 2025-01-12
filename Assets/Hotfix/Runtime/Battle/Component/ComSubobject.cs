using cfg;

namespace LccHotfix
{
    public class ComSubobject : LogicComponent
    {
        public int OwnerId { get; set; }//施法者id
        public int SkillId { get; set; }//技能id
        public int ConfigId { get; set; }//子物体id
        public BTAgent Agent { get; set; }


        private Subobject _subobject = null;
        public Subobject Subobject
        {
            get
            {
                if (_subobject == null)
                {
                    _subobject = ConfigManager.Instance.Tables.TBSubobject.Get(ConfigId);
                }
                return _subobject;
            }
        }

        public override void Dispose()
        {
            base.Dispose();




            OwnerId = -1;
            SkillId = -1;
            ConfigId = -1;
            Agent.Dispose();
            _subobject = null;
        }

    }
    public partial class LogicEntity
    {

        public ComSubobject comSubobject { get { return (ComSubobject)GetComponent(LogicComponentsLookup.ComSubobject); } }
        public bool hasComSubobject { get { return HasComponent(LogicComponentsLookup.ComSubobject); } }

        public void AddComSubobject(int newOwnerId, int newSkillId, int newConfigId, BTAgent newAgent)
        {
            var index = LogicComponentsLookup.ComSubobject;
            var component = (ComSubobject)CreateComponent(index, typeof(ComSubobject));
            component.OwnerId = newOwnerId;
            component.SkillId = newSkillId;
            component.ConfigId = newConfigId;
            component.Agent = newAgent;
            AddComponent(index, component);
        }
    }

    public sealed partial class LogicMatcher
    {

        private static Entitas.IMatcher<LogicEntity> _matcherComSubobject;

        public static Entitas.IMatcher<LogicEntity> ComSubobject
        {
            get
            {
                if (_matcherComSubobject == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComSubobject);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComSubobject = matcher;
                }

                return _matcherComSubobject;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComSubobject;
    }
}
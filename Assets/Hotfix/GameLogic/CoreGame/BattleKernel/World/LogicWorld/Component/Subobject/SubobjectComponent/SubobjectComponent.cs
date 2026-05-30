using PBConfig;
using HotUpdate.Framework.PbCfg;

namespace LccHotfix
{
    public class SubobjectLogic : EntityCmdLogic
    {
    }

    public class ComSubobject : LogicComponent, IEntityCommandHandler
    {
        public long SpellEntityId { get; private set; } //施法者id
        public int SkillId { get; private set; } //技能id
        public int ConfigId { get; private set; } //子物体id

        public TSubobject Cfg { get; private set; }

        public SubobjectLogic Logic { get; private set; }

        public void Init(long newSpellEntityId, int newSkillId, int newConfigId, SubobjectLogic logic)
        {
            SpellEntityId = newSpellEntityId;
            SkillId = newSkillId;
            ConfigId = newConfigId;
            Cfg = PbCfg.GetData<TSubobject>((uint)newConfigId);
            Logic = logic;
        }

        public override void DisposeOnRemove()
        {
            Cfg = null;
            ConfigId = -1;
            SkillId = -1;
            SpellEntityId = -1;
            if (Logic != null)
            {
                Owner?.OwnerWorld?.CustomLogicService?.DestroyLogic(Logic);
                Logic = null;
            }

            base.DisposeOnRemove();
        }


        public bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            if (Logic is IEntityCommandHandler handler)
            {
                return handler.HandleEntityCommand(entity, cmd);
            }

            return false;
        }
    }

    public partial class LogicEntity
    {
        public ComSubobject comSubobject
        {
            get { return (ComSubobject)GetComponent(LogicComponentsLookup.ComSubobject); }
        }

        public bool hasComSubobject
        {
            get { return HasComponent(LogicComponentsLookup.ComSubobject); }
        }

        public void AddComSubobject(long newOwnerId, int newSkillId, int newConfigId, SubobjectLogic logic)
        {
            var index = LogicComponentsLookup.ComSubobject;
            var component = (ComSubobject)CreateComponent(index, typeof(ComSubobject));
            component.Init(newOwnerId, newSkillId, newConfigId, logic);
            AddComponent(index, component);
        }

        public void RemoveComSubobject()
        {
            RemoveComponent(LogicComponentsLookup.ComSubobject);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComSubobjectIndex = new(typeof(ComSubobject));
        public static int ComSubobject => _ComSubobjectIndex.Index;
    }
}
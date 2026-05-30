namespace LccHotfix
{
    public class AILogic : CustomLogic
    {
    }

    public class AIComponent : LogicComponent
    {
        private AILogic mLogic;

        public AILogic Logic => mLogic;

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
        }

        public void Init(AILogic ai)
        {
            mLogic = ai;
        }
    }

    public partial class LogicEntity
    {
        public AIComponent comAI
        {
            get { return (AIComponent)GetComponent(LogicComponentsLookup.ComAI); }
        }

        public bool hasComAI
        {
            get { return HasComponent(LogicComponentsLookup.ComAI); }
        }

        public void AddComAI(AILogic fsm)
        {
            var index = LogicComponentsLookup.ComAI;
            var component = (AIComponent)CreateComponent(index, typeof(AIComponent));
            component.Init(fsm);
            AddComponent(index, component);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComAIIndex = new(typeof(AIComponent));
        public static int ComAI => _ComAIIndex.Index;
    }
}
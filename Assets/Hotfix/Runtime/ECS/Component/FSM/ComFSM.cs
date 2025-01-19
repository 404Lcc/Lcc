using LccModel;

namespace LccHotfix
{
    public class ComFSM : LogicComponent
    {
        private StateMachine _fsm;
        private KVContext _context;

        public StateMachine FSM => _fsm;
        public KVContext Context => _context;
        public string CurState => _fsm.CurrentNode;

        public override void Dispose()
        {
            base.Dispose();
            _context.Clear();
        }

        public void Init(StateMachine fsm, KVContext context)
        {
            _fsm = fsm;
            _context = context;
        }

        public void ForceChangeState<TNode>() where TNode : IStateNode
        {
            _fsm.ChangeState<TNode>();
        }
    }
    public partial class LogicEntity
    {
        public ComFSM comFSM { get { return (ComFSM)GetComponent(LogicComponentsLookup.ComFSM); } }
        public bool hasComFSM { get { return HasComponent(LogicComponentsLookup.ComFSM); } }

        public void AddComFSM(KVContext preContext = null)
        {
            StateMachine fsm = new StateMachine(this);

            KVContext context = null;
            if (preContext == null)
            {
                context = new KVContext();
            }
            context = preContext;

            var index = LogicComponentsLookup.ComFSM;
            var component = (ComFSM)CreateComponent(index, typeof(ComFSM));
            component.Init(fsm, context);

            AddComponent(index, component);
        }

        public void AddState<T>() where T : IStateNode
        {
            comFSM.FSM.AddNode<T>();
        }

        public void RunState<T>() where T : IStateNode
        {
            comFSM.FSM.Run<T>();
        }

        public void ChangeState<T>() where T : IStateNode
        {
            comFSM.FSM.ChangeState<T>();
        }

    }
    public sealed partial class LogicMatcher
    {

        static Entitas.IMatcher<LogicEntity> _matcherComFSM;

        public static Entitas.IMatcher<LogicEntity> ComFSM
        {
            get
            {
                if (_matcherComFSM == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComFSM);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComFSM = matcher;
                }

                return _matcherComFSM;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComFSM;
    }
}
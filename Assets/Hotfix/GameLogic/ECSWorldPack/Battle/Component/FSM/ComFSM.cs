using System;
using LccModel;

namespace LccHotfix
{
    public class ComFSM : LogicComponent
    {
        private FSM _fsm;

        public FSM FSM => _fsm;
        public KVContext Context => _fsm.Context;
        public string CurState => _fsm.CurrentNode;



        public void Init(FSM fsm)
        {
            _fsm = fsm;
        }

        public void ForceChangeState<TNode>() where TNode : IStateNode
        {
            _fsm.ChangeState<TNode>();
        }

        public override void Dispose()
        {
            base.Dispose();
            _fsm.Dispose();
        }
    }

    public partial class LogicEntity
    {
        public ComFSM comFSM
        {
            get { return (ComFSM)GetComponent(LogicComponentsLookup.ComFSM); }
        }

        public bool hasComFSM
        {
            get { return HasComponent(LogicComponentsLookup.ComFSM); }
        }

        public void AddComFSM(string fsmName, KVContext preContext = null)
        {
            var obj = Activator.CreateInstance(Main.CodeTypesService.GetType(fsmName), new object[] { this });
            FSM fsm = obj as FSM;
            fsm.InitFSM(this, preContext, -1);

            var index = LogicComponentsLookup.ComFSM;
            var component = (ComFSM)CreateComponent(index, typeof(ComFSM));
            component.Init(fsm);

            AddComponent(index, component);
            fsm.StartFSM();
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

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComFSMIndex = new ComponentTypeIndex(typeof(ComFSM));
        public static int ComFSM => ComFSMIndex.index;
    }
}
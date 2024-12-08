using LccModel;

namespace LccHotfix
{
    public enum GameModeType
    {
        Battle,
    }

    public class GameModeState
    {
        public GameModeType gameModeType;
    }
    public class GameModeBase : ICoroutine
    {
        private GameModeState _state;
        private StateMachine _fsm;

        public GameModeType GameModeType => _state.gameModeType;
        public StateMachine FSM => _fsm;


        public virtual void Init(GameModeState state)
        {
            _state = state;
            _fsm = new StateMachine(this);
            InitFSM();
        }

        public virtual void InitFSM()
        {

        }

        public virtual void Start<T>() where T : IStateNode
        {
            _fsm.Run<IStateNode>();
        }

        public virtual void Update()
        {
            if (_fsm == null)
                return;
            _fsm.Update();
        }

        public virtual void Release()
        {
            if (_fsm == null)
                return;
            _fsm = null;
        }

    }
}
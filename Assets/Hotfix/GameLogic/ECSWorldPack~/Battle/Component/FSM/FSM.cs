using LccModel;

namespace LccHotfix
{
    public class FSM : StateMachine
    {
        protected LogicEntity entity;
        protected KVContext context;
        public LogicEntity Entity => entity;
        public KVContext Context => context;

        public FSM(object owner) : base(owner)
        {
        }

        public virtual void InitFSM(LogicEntity entity, KVContext preContext, int fighterParams)
        {
            this.entity = entity;
            if (preContext == null)
            {
                preContext = new KVContext();
            }

            context = preContext;
        }

        public virtual void StartFSM()
        {
        }

        public virtual void UpdateFSM()
        {
            Update();
        }

        public virtual void Dispose()
        {
            if (context != null)
            {
                context.Clear();
                context = null;
            }
        }
    }
}
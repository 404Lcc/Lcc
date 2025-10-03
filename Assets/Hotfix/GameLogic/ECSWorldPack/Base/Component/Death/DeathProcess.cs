namespace LccHotfix
{
    public class DeathProcess : IDeathProcess
    {
        private bool _isFinish;

        public LogicEntity Entity { get; set; }

        public void Start()
        {
            var list = Entity.OwnerContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComID, LogicComponentsLookup.ComOwnerEntity, LogicComponentsLookup.ComTag)).GetEntities();
            foreach (var entity in list)
            {
                if (entity.comOwnerEntity.ownerEntityID == Entity.comID.id)
                {
                    entity.ReplaceComLife(0);
                }
            }

            _isFinish = true;

        }

        public void Update()
        {
        }

        public bool IsFinished()
        {
            return _isFinish;
        }
    }
}
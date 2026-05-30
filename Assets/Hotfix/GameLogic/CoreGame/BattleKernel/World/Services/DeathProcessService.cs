namespace LccHotfix
{
    public interface IDeathProcessService
    {
        void RemoveExternalComponentsBeforeDestroy(LogicEntity entity);
    }

    public partial class LogicWorld
    {
        public IDeathProcessService DeathProcessService { get; private set; }

        public void SetDeathProcessService(IDeathProcessService deathProcessService)
        {
            DeathProcessService = deathProcessService;
        }
    }
}

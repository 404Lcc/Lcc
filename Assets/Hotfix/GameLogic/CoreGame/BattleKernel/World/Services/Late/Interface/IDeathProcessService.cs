namespace LccHotfix
{
    public interface IDeathProcessService
    {
        void RemoveExternalComponentsBeforeDestroy(LogicEntity entity);
    }
}

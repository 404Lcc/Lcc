namespace LccHotfix
{
    public interface ISubobjectTransferEffectService
    {
        void OnTransferred(CustomNode node, LogicEntity target);
    }

    public partial class LogicWorld
    {
        public ISubobjectTransferEffectService SubobjectTransferEffectService { get; private set; }

        public void SetSubobjectTransferEffectService(ISubobjectTransferEffectService service)
        {
            SubobjectTransferEffectService = service;
        }
    }
}

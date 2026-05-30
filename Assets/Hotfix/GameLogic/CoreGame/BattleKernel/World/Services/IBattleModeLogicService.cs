namespace LccHotfix
{
    public interface IBattleModeLogicService
    {
        BattleModeLogic CreateModeLogic(ICustomLogicGenInfo genInfo);

        void DestroyModeLogic(BattleModeLogic logic);
    }
}

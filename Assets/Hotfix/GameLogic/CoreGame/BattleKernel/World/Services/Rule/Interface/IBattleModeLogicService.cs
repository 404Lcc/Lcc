namespace LccHotfix
{
    public interface IBattleModeLogicService
    {
        BattleModeLogic CreateModeLogic(CustomLogicGenInfo genInfo);

        void DestroyModeLogic(BattleModeLogic logic);
    }
}

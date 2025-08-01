namespace LccHotfix
{
    public interface IProcedureService : IService
    {
        ProcedureType CurState { get; }

        bool IsLoading { get; }

        void SetProcedureHelper(IProcedureHelper procedureHelper);
        
        LoadProcedureHandler GetProcedure(ProcedureType type);

        void ChangeProcedure(ProcedureType type);
        
        void CleanProcedure();

        #region 切流程界面

        void OpenChangeProcedurePanel();

        void CleanChangeProcedureParam();

        #endregion
    }
}
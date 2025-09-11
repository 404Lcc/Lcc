namespace LccHotfix
{
    public interface IProcedureService : IService
    {
        int CurState { get; }

        bool IsLoading { get; }

        void SetProcedureHelper(IProcedureHelper procedureHelper);
        
        LoadProcedureHandler GetProcedure(int type);

        void ChangeProcedure(int type);
        
        void CleanProcedure();

        #region 切流程界面

        void OpenChangeProcedurePanel();

        void CleanChangeProcedureParam();

        #endregion
    }
}
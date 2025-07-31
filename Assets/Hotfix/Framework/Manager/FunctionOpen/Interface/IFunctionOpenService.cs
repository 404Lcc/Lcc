namespace LccHotfix
{
    public interface IFunctionOpenService : IService
    {
        public bool IsFuncOpened(int functionID, bool dataCheck = false);
        public bool GetFuncOpenState(FunctionID functionID);
        public bool IsFunctionOpenedAndShowTips(int functionID, bool useNotice = false, bool popTips = true);
    }
}
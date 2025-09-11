using System.Collections;
using LccHotfix;

namespace LccHotfix
{
    public interface IProcedureHelper
    {
        void UpdateLoadingTime(LoadProcedureHandler handler);
        void ResetSpeed();
        void UnloadAllPanel(LoadProcedureHandler last, LoadProcedureHandler cur);
        void OpenChangeProcedurePanel(LoadProcedureHandler handler);
        IEnumerator ShowProcedureLoading(LoadingType loadType);
    }
}
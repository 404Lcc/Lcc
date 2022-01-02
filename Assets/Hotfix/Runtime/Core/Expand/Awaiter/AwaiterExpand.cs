using LccModel;
using UnityEngine.Networking;

namespace LccHotfix
{
    public static class AwaiterExpand
    {
        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOperation)
        {
            return new UnityWebRequestAsyncOperationAwaiter(asyncOperation);
        }
    }
}
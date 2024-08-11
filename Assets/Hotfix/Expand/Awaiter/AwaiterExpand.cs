using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public static class AwaiterExpand
    {
        public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation asyncOperation)
        {
            return new AsyncOperationAwaiter(asyncOperation);
        }
        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOperation)
        {
            return new UnityWebRequestAsyncOperationAwaiter(asyncOperation);
        }
        public static AssetBundleRequestAwaiter GetAwaiter(this AssetBundleRequest asyncOperation)
        {
            return new AssetBundleRequestAwaiter(asyncOperation);
        }
    }
}
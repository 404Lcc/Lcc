using UnityEngine.Networking;

namespace LccHotfix
{
    public static class UnityWebRequestExtension
    {
        public static bool IsError(this UnityWebRequest unityWebRequest)
        {
#if UNITY_2020_2_OR_NEWER
            UnityWebRequest.Result result = unityWebRequest.result;
            return (result == UnityWebRequest.Result.ConnectionError) || (result == UnityWebRequest.Result.ProtocolError) || (result == UnityWebRequest.Result.DataProcessingError);
#else
            return unityWebRequest.isHttpError || unityWebRequest.isNetworkError;
#endif
        }
    }
}
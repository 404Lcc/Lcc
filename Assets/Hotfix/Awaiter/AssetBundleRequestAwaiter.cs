using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccModel
{
    public class AssetBundleRequestAwaiter : ICriticalNotifyCompletion
    {
        public AssetBundleRequest asyncOperation;
        public Action<AsyncOperation> completed;
        public AssetBundleRequestAwaiter()
        {
        }
        public AssetBundleRequestAwaiter(AssetBundleRequest asyncOperation)
        {
            this.asyncOperation = asyncOperation;
            this.completed = null;
        }
        public bool IsCompleted => asyncOperation.isDone;
        public Object GetResult()
        {
            if (completed != null)
            {
                asyncOperation.completed -= completed;
                completed = null;
                Object result = asyncOperation.asset;
                asyncOperation = null;
                return result;
            }
            else
            {
                Object result = asyncOperation.asset;
                asyncOperation = null;
                return result;
            }
        }
        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }
        public void UnsafeOnCompleted(Action continuation)
        {
            completed = ActionWrap<AsyncOperation>.Create(continuation);
            asyncOperation.completed += completed;
        }
    }
}
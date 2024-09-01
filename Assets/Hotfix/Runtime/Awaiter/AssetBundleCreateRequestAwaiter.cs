using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public class AssetBundleCreateRequestAwaiter : ICriticalNotifyCompletion
    {
        public AssetBundleCreateRequest asyncOperation;
        public Action<AsyncOperation> completed;
        public AssetBundleCreateRequestAwaiter()
        {
        }
        public AssetBundleCreateRequestAwaiter(AssetBundleCreateRequest asyncOperation)
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
                Object result = asyncOperation.assetBundle;
                asyncOperation = null;
                return result;
            }
            else
            {
                Object result = asyncOperation.assetBundle;
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
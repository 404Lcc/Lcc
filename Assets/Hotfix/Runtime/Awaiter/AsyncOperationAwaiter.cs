using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace LccHotfix
{
    public class AsyncOperationAwaiter : ICriticalNotifyCompletion
    {
        public AsyncOperation asyncOperation;
        public Action<AsyncOperation> completed;
        public AsyncOperationAwaiter()
        {
        }
        public AsyncOperationAwaiter(AsyncOperation asyncOperation)
        {
            this.asyncOperation = asyncOperation;
            this.completed = null;
        }
        public bool IsCompleted => asyncOperation.isDone;
        public void GetResult()
        {
            if (completed != null)
            {
                asyncOperation.completed -= completed;
                completed = null;
                asyncOperation = null;
            }
            else
            {
                asyncOperation = null;
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
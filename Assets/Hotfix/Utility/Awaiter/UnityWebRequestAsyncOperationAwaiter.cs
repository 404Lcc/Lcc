using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace LccHotfix
{
    public class UnityWebRequestAsyncOperationAwaiter : ICriticalNotifyCompletion
    {
        public UnityWebRequestAsyncOperation asyncOperation;
        public Action<AsyncOperation> completed;
        public UnityWebRequestAsyncOperationAwaiter()
        {
        }
        public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation asyncOperation)
        {
            this.asyncOperation = asyncOperation;
            this.completed = null;
        }
        public bool IsCompleted => asyncOperation.isDone;
        public UnityWebRequest GetResult()
        {
            if (completed != null)
            {
                asyncOperation.completed -= completed;
                completed = null;
                UnityWebRequest result = asyncOperation.webRequest;
                asyncOperation = null;
                if (result.IsError())
                {
                    return null;
                }
                return result;
            }
            else
            {
                UnityWebRequest result = asyncOperation.webRequest;
                asyncOperation = null;
                if (result.IsError())
                {
                    return null;
                }
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
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LccModel
{
    public struct AsyncETVoidMethodBuilder
    {
        [DebuggerHidden]
        public ETVoid Task
        {
            get
            {
                return default;
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncETVoidMethodBuilder Create()
        {
            AsyncETVoidMethodBuilder asyncETVoidMethodBuilder = new AsyncETVoidMethodBuilder();
            return asyncETVoidMethodBuilder;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            LogUtil.LogError(exception.ToString());
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult()
        {
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TAsyncStateMachine>(ref TAwaiter awaiter, ref TAsyncStateMachine asyncStateMachine) where TAwaiter : INotifyCompletion where TAsyncStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(asyncStateMachine.MoveNext);
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TAsyncStateMachine>(ref TAwaiter awaiter, ref TAsyncStateMachine asyncStateMachine) where TAwaiter : ICriticalNotifyCompletion where TAsyncStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(asyncStateMachine.MoveNext);
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TAsyncStateMachine>(ref TAsyncStateMachine asyncStateMachine) where TAsyncStateMachine : IAsyncStateMachine
        {
            asyncStateMachine.MoveNext();
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStateMachine(IAsyncStateMachine asyncStateMachine)
        {
        }
    }
}
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LccModel
{
    public struct AsyncETTaskMethodBuilder
    {
        private ETTask _task;
        [DebuggerHidden]
        public ETTask Task
        {
            get
            {
                return _task;
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncETTaskMethodBuilder(ETTask task)
        {
            _task = task;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncETTaskMethodBuilder Create()
        {
            AsyncETTaskMethodBuilder asyncETTaskMethodBuilder = new AsyncETTaskMethodBuilder(ETTask.Create(true));
            return asyncETTaskMethodBuilder;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            Task.SetException(exception);
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult()
        {
            Task.SetResult();
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
    public struct AsyncETTaskMethodBuilder<T>
    {
        private ETTask<T> _task;
        [DebuggerHidden]
        public ETTask<T> Task
        {
            get
            {
                return _task;
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncETTaskMethodBuilder(ETTask<T> task)
        {
            _task = task;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncETTaskMethodBuilder<T> Create()
        {
            AsyncETTaskMethodBuilder<T> asyncETTaskMethodBuilder = new AsyncETTaskMethodBuilder<T>(ETTask<T>.Create(true));
            return asyncETTaskMethodBuilder;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            Task.SetException(exception);
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(T value)
        {
            Task.SetResult(value);
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
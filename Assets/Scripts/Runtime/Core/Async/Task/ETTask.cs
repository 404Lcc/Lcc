using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace LccModel
{
    [AsyncMethodBuilder(typeof(AsyncETTaskMethodBuilder))]
    public class ETTask : ICriticalNotifyCompletion
    {
        private bool _fromPool;
        private AwaiterStatus _awaiterStatus;
        private object _callback;
        private static Queue<ETTask> _etTaskQueue = new Queue<ETTask>();
        public static ETTaskCompleted ETTaskCompleted
        {
            get
            {
                return new ETTaskCompleted();
            }
        }
        public bool IsCompleted
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _awaiterStatus != AwaiterStatus.Pending;
            }
        }
        public ETTask()
        {
        }
        public ETTask(bool fromPool = false)
        {
            _fromPool = fromPool;
        }
        public static ETTask Create(bool fromPool)
        {
            if (!fromPool)
            {
                return new ETTask(fromPool);
            }
            else
            {
                if (_etTaskQueue.Count == 0)
                {
                    return new ETTask(true);
                }
                return _etTaskQueue.Dequeue();
            }
        }
        public void Recycle()
        {
            if (!_fromPool) return;
            _awaiterStatus = AwaiterStatus.Pending;
            _callback = null;
            _etTaskQueue.Enqueue(this);
            if (_etTaskQueue.Count > 1000)
            {
                _etTaskQueue.Clear();
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ETTask GetAwaiter()
        {
            return this;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult()
        {
            switch (_awaiterStatus)
            {
                case AwaiterStatus.Succeeded:
                    Recycle();
                    break;
                case AwaiterStatus.Faulted:
                    ExceptionDispatchInfo exceptionDispatchInfo = (ExceptionDispatchInfo)_callback;
                    exceptionDispatchInfo?.Throw();
                    _callback = null;
                    Recycle();
                    break;
                default:
                    throw new NotSupportedException("任务未完成，请在函数调用前使用await");
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ETVoid InnerCoroutine()
        {
            await this;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            if (_awaiterStatus != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("任务已完成");
            }
            _awaiterStatus = AwaiterStatus.Faulted;
            Action callback = (Action)_callback;
            callback?.Invoke();
            _callback = ExceptionDispatchInfo.Capture(exception);
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult()
        {
            if (_awaiterStatus != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("任务已完成");
            }
            _awaiterStatus = AwaiterStatus.Succeeded;
            Action callback = (Action)_callback;
            callback?.Invoke();
            _callback = null;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            if (_awaiterStatus != AwaiterStatus.Pending)
            {
                continuation?.Invoke();
            }
            else
            {
                _callback = continuation;
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action continuation)
        {
            if (_awaiterStatus != AwaiterStatus.Pending)
            {
                continuation?.Invoke();
            }
            else
            {
                _callback = continuation;
            }
        }
    }
    [AsyncMethodBuilder(typeof(AsyncETTaskMethodBuilder<>))]
    public class ETTask<T> : ICriticalNotifyCompletion
    {
        private bool _fromPool;
        private AwaiterStatus _awaiterStatus;
        private object _callback;
        private T _value;
        private static Queue<ETTask<T>> _etTaskQueue = new Queue<ETTask<T>>();
        public static ETTaskCompleted ETTaskCompleted
        {
            get
            {
                return new ETTaskCompleted();
            }
        }
        public bool IsCompleted
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _awaiterStatus != AwaiterStatus.Pending;
            }
        }
        public ETTask()
        {
        }
        public ETTask(bool fromPool)
        {
            _fromPool = fromPool;
        }
        public static ETTask<T> Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask<T>(fromPool);
            }
            else
            {
                if (_etTaskQueue.Count == 0)
                {
                    return new ETTask<T>(true);
                }
                return _etTaskQueue.Dequeue();
            }
        }
        public void Recycle()
        {
            if (!_fromPool) return;
            _awaiterStatus = AwaiterStatus.Pending;
            _callback = null;
            _value = default;
            _etTaskQueue.Enqueue(this);
            if (_etTaskQueue.Count > 1000)
            {
                _etTaskQueue.Clear();
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ETTask<T> GetAwaiter()
        {
            return this;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetResult()
        {
            switch (_awaiterStatus)
            {
                case AwaiterStatus.Succeeded:
                    T value = _value;
                    Recycle();
                    return value;
                case AwaiterStatus.Faulted:
                    ExceptionDispatchInfo exceptionDispatchInfo = (ExceptionDispatchInfo)_callback;
                    exceptionDispatchInfo?.Throw();
                    _callback = null;
                    Recycle();
                    return default;
                default:
                    throw new NotSupportedException("任务未完成，请在函数调用前使用await");
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ETVoid InnerCoroutine()
        {
            await this;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            if (_awaiterStatus != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("任务已完成");
            }
            _awaiterStatus = AwaiterStatus.Faulted;
            Action callback = (Action)_callback;
            callback?.Invoke();
            _callback = ExceptionDispatchInfo.Capture(exception);
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(T value)
        {
            if (_awaiterStatus != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("任务已完成");
            }
            _awaiterStatus = AwaiterStatus.Succeeded;
            _value = value;
            Action callback = (Action)_callback;
            callback?.Invoke();
            _callback = null;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            if (_awaiterStatus != AwaiterStatus.Pending)
            {
                continuation?.Invoke();
            }
            else
            {
                _callback = continuation;
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action continuation)
        {
            if (_awaiterStatus != AwaiterStatus.Pending)
            {
                continuation?.Invoke();
            }
            else
            {
                _callback = continuation;
            }
        }
    }
}
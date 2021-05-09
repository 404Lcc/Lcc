using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LccModel
{
    [AsyncMethodBuilder(typeof(AsyncETTaskCompletedMethodBuilder))]
    public struct ETTaskCompleted : ICriticalNotifyCompletion
    {
        [DebuggerHidden]
        public bool IsCompleted
        {
            get
            {
                return true;
            }
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ETTaskCompleted GetAwaiter()
        {
            return this;
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult()
        {
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
        }
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action continuation)
        {
        }
    }
}
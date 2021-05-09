using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LccModel
{
    [AsyncMethodBuilder(typeof(AsyncETVoidMethodBuilder))]
    public struct ETVoid : ICriticalNotifyCompletion
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
        public void Coroutine()
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
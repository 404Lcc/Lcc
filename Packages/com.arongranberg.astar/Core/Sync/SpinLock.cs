using System.Threading;

/// <summary>Synchronization primitives</summary>
namespace Pathfinding.Sync {
	/// <summary>
	/// Spin lock which can be used in Burst.
	/// Good when the lock is generally uncontested.
	/// Very inefficient when the lock is contested.
	/// </summary>
	internal struct SpinLock {
		private volatile int locked;

		public void Lock () {
			while (Interlocked.CompareExchange(ref locked, 1, 0) != 0)
				Unity.Burst.Intrinsics.Common.Pause(); // spin

			// We need to ensure that any optimizer does not reorder loads to before we aquire the lock.
			System.Threading.Thread.MemoryBarrier();
		}

		public void Unlock () {
			// We need a memory barrier to ensure that all writes are visible to other threads, before we unlock.
			// We also need to ensure that any optimizer does not reorder stores to after the unlock.
			System.Threading.Thread.MemoryBarrier();
			// Release the lock by writing 0 to it. Use atomics to make it immediately visible to other threads.
			if (Interlocked.Exchange(ref locked, 0) == 0) throw new System.InvalidOperationException("Trying to unlock a lock which is not locked");
		}
	}
}

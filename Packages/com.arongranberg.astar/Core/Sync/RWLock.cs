// #define DEBUG_RWLOCK
using Unity.Jobs;

namespace Pathfinding.Sync {
	/// <summary>
	/// A simple read/write lock for use with the Unity Job System.
	///
	/// The RW-lock makes the following assumptions:
	/// - Only the main thread will call the methods on this lock.
	/// - If jobs are to use locked data, you should call <see cref="Read"/> or <see cref="Write"/> on the lock and pass the returned JobHandle as a dependency the job, and then call <see cref="WriteLockAsync.UnlockAfter"/> on the lock object, with the newly scheduled job's handle.
	/// - When taking a Read lock, you should only read data, but if you take a Write lock you may modify data.
	/// - On the main thread, multiple synchronous write locks may be nested.
	///
	/// You do not need to care about dependencies when calling the <see cref="ReadSync"/> and <see cref="WriteSync"/> methods. That's handled automatically for you.
	///
	/// See: https://en.wikipedia.org/wiki/Readers%E2%80%93writer_lock
	///
	/// <code>
	/// var readLock = AstarPath.active.LockGraphDataForReading();
	/// var handle = new MyJob {
	///     // ...
	/// }.Schedule(readLock.dependency);
	/// readLock.UnlockAfter(handle);
	/// </code>
	/// </summary>
	public class RWLock {
		JobHandle lastWrite;
		JobHandle lastRead;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		int heldSyncLocks;
		bool pendingAsync;
#if DEBUG_RWLOCK
		string pendingStackTrace;
#endif

		void CheckPendingAsync () {
#if DEBUG_RWLOCK
			if (pendingAsync) throw new System.InvalidOperationException("An async lock was previously aquired, but UnlockAfter was never called on it. The lock was aquired at\n" + pendingStackTrace + "\n\n");
#else
			if (pendingAsync) throw new System.InvalidOperationException("An async lock was previously aquired, but UnlockAfter was never called on it.");
#endif
		}
#endif

		void AddPendingSync () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			CheckPendingAsync();
#if DEBUG_RWLOCK
			pendingStackTrace = System.Environment.StackTrace;
#endif
			heldSyncLocks++;
#endif
		}

		void RemovePendingSync () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (heldSyncLocks <= 0) throw new System.InvalidOperationException("Tried to unlock a lock which was not locked. Did you call Unlock twice?");
			heldSyncLocks--;
#endif
		}

		void AddPendingAsync () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			CheckPendingAsync();
#if DEBUG_RWLOCK
			if (heldSyncWriteLocks > 0) throw new System.InvalidOperationException("A synchronous lock is already being held. You cannot lock it asynchronously at the same time. The sync lock was aquired at\n" + pendingStackTrace + "\n\n");
			pendingStackTrace = System.Environment.StackTrace;
#else
			if (heldSyncLocks > 0) throw new System.InvalidOperationException("A synchronous lock is already being held. You cannot lock it asynchronously at the same time.");
#endif
			pendingAsync = true;
#endif
		}

		void RemovePendingAsync () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			pendingAsync = false;
#endif
		}

		/// <summary>
		/// Aquire a read lock on the main thread.
		/// This method will block until all pending write locks have been released.
		/// </summary>
		public LockSync ReadSync () {
			AddPendingSync();
			lastWrite.Complete();
			lastWrite = default; // Setting this to default will avoid a call into unity's c++ parts next time we call Complete (improves perf slightly)
			return new LockSync(this);
		}

		/// <summary>
		/// Aquire a read lock on the main thread.
		/// This method will not block until all asynchronous write locks have been released, instead you should make sure to add the returned JobHandle as a dependency to any jobs that use the locked data.
		///
		/// If a synchronous write lock is currently held, this method will throw an exception.
		///
		/// <code>
		/// var readLock = AstarPath.active.LockGraphDataForReading();
		/// var handle = new MyJob {
		///     // ...
		/// }.Schedule(readLock.dependency);
		/// readLock.UnlockAfter(handle);
		/// </code>
		/// </summary>
		public ReadLockAsync Read () {
			AddPendingAsync();
			return new ReadLockAsync(this, lastWrite);
		}

		/// <summary>
		/// Aquire a write lock on the main thread.
		/// This method will block until all pending read and write locks have been released.
		/// </summary>
		public LockSync WriteSync () {
			AddPendingSync();
			lastWrite.Complete();
			lastWrite = default; // Setting this to default will avoid a call into unity's c++ parts next time we call Complete (improves perf slightly)
			lastRead.Complete();
			return new LockSync(this);
		}

		/// <summary>
		/// Aquire a write lock on the main thread.
		/// This method will not block until all asynchronous read and write locks have been released, instead you should make sure to add the returned JobHandle as a dependency to any jobs that use the locked data.
		///
		/// If a synchronous write lock is currently held, this method will throw an exception.
		///
		/// <code>
		/// var readLock = AstarPath.active.LockGraphDataForReading();
		/// var handle = new MyJob {
		///     // ...
		/// }.Schedule(readLock.dependency);
		/// readLock.UnlockAfter(handle);
		/// </code>
		/// </summary>
		public WriteLockAsync Write () {
			AddPendingAsync();
			return new WriteLockAsync(this, JobHandle.CombineDependencies(lastRead, lastWrite));
		}

		public readonly struct CombinedReadLockAsync {
			readonly RWLock lock1;
			readonly RWLock lock2;
			public readonly JobHandle dependency;

			public CombinedReadLockAsync(ReadLockAsync lock1, ReadLockAsync lock2) {
				this.lock1 = lock1.inner;
				this.lock2 = lock2.inner;
				dependency = JobHandle.CombineDependencies(lock1.dependency, lock2.dependency);
			}

			/// <summary>Release the lock after the given job has completed</summary>
			public readonly void UnlockAfter (JobHandle handle) {
				if (lock1 != null) {
					lock1.RemovePendingAsync();
					lock1.lastRead = JobHandle.CombineDependencies(lock1.lastRead, handle);
				}
				if (lock2 != null) {
					lock2.RemovePendingAsync();
					lock2.lastRead = JobHandle.CombineDependencies(lock2.lastRead, handle);
				}
			}
		}

		public readonly struct ReadLockAsync {
			internal readonly RWLock inner;
			public readonly JobHandle dependency;

			public ReadLockAsync(RWLock inner, JobHandle dependency) {
				this.inner = inner;
				this.dependency = dependency;
			}

			/// <summary>Release the lock after the given job has completed</summary>
			public readonly void UnlockAfter (JobHandle handle) {
				if (inner != null) {
					inner.RemovePendingAsync();
					inner.lastRead = JobHandle.CombineDependencies(inner.lastRead, handle);
				}
			}
		}

		public readonly struct WriteLockAsync {
			readonly RWLock inner;
			public readonly JobHandle dependency;

			public WriteLockAsync(RWLock inner, JobHandle dependency) {
				this.inner = inner;
				this.dependency = dependency;
			}

			/// <summary>Release the lock after the given job has completed</summary>
			public readonly void UnlockAfter (JobHandle handle) {
				if (inner != null) {
					inner.RemovePendingAsync();
					inner.lastWrite = handle;
				}
			}
		}

		public readonly struct LockSync : System.IDisposable {
			readonly RWLock inner;

			public LockSync(RWLock inner) {
				this.inner = inner;
			}

			/// <summary>Release the lock</summary>
			public readonly void Unlock () {
				if (inner != null) inner.RemovePendingSync();
			}

			readonly void System.IDisposable.Dispose () {
				Unlock();
			}
		}
	}
}

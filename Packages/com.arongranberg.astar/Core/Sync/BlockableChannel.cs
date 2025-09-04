using System.Threading;
using UnityEngine.Assertions;
using Pathfinding.Collections;

namespace Pathfinding.Sync {
	/// <summary>
	/// Multi-producer-multi-consumer (MPMC) channel.
	///
	/// This is a channel that can be used to send data between threads.
	/// It is thread safe and can be used by multiple threads at the same time.
	///
	/// Additionally, the channel can be put into a blocking mode, which will cause all calls to Receive to block until the channel is unblocked.
	/// </summary>
	internal class BlockableChannel<T> where T : class {
		public enum PopState {
			Ok,
			Wait,
			Closed,
		}

		readonly System.Object lockObj = new System.Object();

		CircularBuffer<T> queue = new CircularBuffer<T>(16);
		public int numReceivers { get; private set; }

		// Marked as volatile such that the compiler will not try to optimize the allReceiversBlocked property too much (this is more of a theoretical concern than a practical issue).
		volatile int waitingReceivers;
#if !UNITY_WEBGL
		ManualResetEvent starving = new ManualResetEvent(false);
#endif
		volatile bool blocked;

		/// <summary>True if <see cref="Close"/> has been called</summary>
		public bool isClosed { get; private set; }

		/// <summary>True if the queue is empty</summary>
		public bool isEmpty {
			get {
				lock (lockObj) {
					return queue.Length == 0;
				}
			}
		}

		/// <summary>True if blocking and all receivers are waiting for unblocking</summary>
		// Note: This is designed to be lock-free for performance. But it will only generate a useful value if called from the same thread that is blocking/unblocking the queue, otherwise the return value could become invalid at any time.
		public bool allReceiversBlocked => blocked && waitingReceivers == numReceivers;

		/// <summary>If true, all calls to Receive will block until this property is set to false</summary>
		public bool isBlocked {
			get => blocked;
			set {
				lock (lockObj) {
					blocked = value;
					if (isClosed) return;
					isStarving = value || queue.Length == 0;
				}
			}
		}

		/// <summary>All calls to Receive and ReceiveNoBlock will now return PopState.Closed</summary>
		public void Close () {
			lock (lockObj) {
				isClosed = true;
				isStarving = false;
			}
		}

		bool isStarving {
			get {
#if UNITY_WEBGL
				// In WebGL, semaphores are not supported.
				// They will compile, but they don't work properly.
				// So instead we directly use what the starving semaphore should indicate.
				return (blocked || queue.Length == 0) && !isClosed;
#else
				return !starving.WaitOne(0);
#endif
			}
			set {
#if !UNITY_WEBGL
				if (value) starving.Reset();
				else starving.Set();
#endif
			}
		}

		/// <summary>
		/// Resets a closed channel so that it can be used again.
		///
		/// The existing queue is preserved.
		///
		/// This will throw an exception if there are any receivers still active.
		/// </summary>
		public void Reopen () {
			lock (lockObj) {
				if (numReceivers != 0) throw new System.InvalidOperationException("Can only reopen a channel after Close has been called on all receivers");
				Assert.AreEqual(waitingReceivers, 0);
				isClosed = false;
				isBlocked = false;
			}
		}

		public Receiver AddReceiver () {
			lock (lockObj) {
				if (isClosed) throw new System.InvalidOperationException("Channel is closed");
				this.numReceivers++;
				return new Receiver(this);
			}
		}

		/// <summary>Push a path to the front of the queue</summary>
		public void PushFront (T path) {
			lock (lockObj) {
				if (isClosed) throw new System.InvalidOperationException("Channel is closed");
				queue.PushStart(path);
				if (!blocked) isStarving = false;
			}
		}

		/// <summary>Push a path to the end of the queue</summary>
		public void Push (T path) {
			lock (lockObj) {
				if (isClosed) throw new System.InvalidOperationException("Channel is closed");
				queue.PushEnd(path);
				if (!blocked) isStarving = false;
			}
		}

		/// <summary>Allows receiving items from a channel</summary>
		public struct Receiver {
			BlockableChannel<T> channel;

			public Receiver(BlockableChannel<T> channel) {
				this.channel = channel;
			}

			/// <summary>
			/// Call when a receiver was terminated.
			///
			/// After this call, this receiver cannot be used for anything.
			/// </summary>
			public void Close () {
				lock (channel.lockObj) {
					Assert.IsTrue(channel.numReceivers > 0);
					Assert.IsTrue(channel.waitingReceivers < channel.numReceivers);
					channel.numReceivers--;
				}
				channel = null;
			}

			/// <summary>
			/// Receives the next item from the channel.
			/// This call will block if there are no items in the channel or if the channel is currently blocked.
			///
			/// Returns: PopState.Ok and a non-null item in the normal case. Returns PopState.Closed if the channel has been closed.
			/// </summary>
			public PopState Receive (out T item) {
#if UNITY_WEBGL
				throw new System.Exception("Cannot block in WebGL. Use ReceiveNoBlock instead.");
#else
				Interlocked.Increment(ref channel.waitingReceivers);
				while (true) {
					channel.starving.WaitOne();
					// Note that right here, the channel could become blocked, closed or anything really. We have to check conditions again after locking.
					lock (channel.lockObj) {
						if (channel.isClosed) {
							Interlocked.Decrement(ref channel.waitingReceivers);
							item = null;
							return PopState.Closed;
						}
						if (channel.queue.Length == 0) channel.isStarving = true;
						if (channel.isStarving) continue;
						Assert.IsFalse(channel.blocked);
						Interlocked.Decrement(ref channel.waitingReceivers);
						item = channel.queue.PopStart();
						return PopState.Ok;
					}
				}
#endif
			}

			/// <summary>
			/// Receives the next item from the channel, this call will not block.
			/// To ensure a consistent state, the caller must follow this pattern.
			/// 1. Call ReceiveNoBlock(false), if PopState.Wait is returned, wait for a bit (e.g yield return null in a Unity coroutine)
			/// 2. try again with PopNoBlock(true), if PopState.Wait, wait for a bit
			/// 3. Repeat from step 2.
			/// </summary>
			public PopState ReceiveNoBlock (bool blockedBefore, out T item) {
				item = null;
				if (!blockedBefore) Interlocked.Increment(ref channel.waitingReceivers);
				while (true) {
					if (channel.isStarving) return PopState.Wait;
					// Note that right here, the channel could become blocked, closed or anything really. We have to check conditions again after locking.
					lock (channel.lockObj) {
						if (channel.isClosed) {
							Interlocked.Decrement(ref channel.waitingReceivers);
							return PopState.Closed;
						}
						if (channel.queue.Length == 0) channel.isStarving = true;
						if (channel.isStarving) continue;
						Assert.IsFalse(channel.blocked);
						Interlocked.Decrement(ref channel.waitingReceivers);
						item = channel.queue.PopStart();
						return PopState.Ok;
					}
				}
			}
		}
	}
}

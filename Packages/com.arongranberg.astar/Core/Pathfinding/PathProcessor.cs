using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Profiling;
using UnityEngine.Assertions;
using Pathfinding.Sync;

namespace Pathfinding {
#if NETFX_CORE
	using Thread = Pathfinding.WindowsStore.Thread;
#else
	using Thread = System.Threading.Thread;
#endif

	public class PathProcessor {
		public event System.Action<Path> OnPathPreSearch;
		public event System.Action<Path> OnPathPostSearch;
		public event System.Action OnQueueUnblocked;

		internal BlockableChannel<Path> queue;
		readonly AstarPath astar;
		readonly PathReturnQueue returnQueue;

		PathHandler[] pathHandlers;

		/// <summary>References to each of the pathfinding threads</summary>
		Thread[] threads;
		bool multithreaded;

		/// <summary>
		/// When no multithreading is used, the IEnumerator is stored here.
		/// When no multithreading is used, a coroutine is used instead. It is not directly called with StartCoroutine
		/// but a separate function has just a while loop which increments the main IEnumerator.
		/// This is done so other functions can step the thread forward at any time, without having to wait for Unity to update it.
		/// See: <see cref="CalculatePaths"/>
		/// See: <see cref="CalculatePathsThreaded"/>
		/// </summary>
		IEnumerator threadCoroutine;
		BlockableChannel<Path>.Receiver coroutineReceiver;

		readonly List<int> locks = new List<int>();
		int nextLockID = 0;

		static readonly Unity.Profiling.ProfilerMarker MarkerCalculatePath = new Unity.Profiling.ProfilerMarker("Calculating Path");
		static readonly Unity.Profiling.ProfilerMarker MarkerPreparePath = new Unity.Profiling.ProfilerMarker("Prepare Path");

		/// <summary>
		/// Number of parallel pathfinders.
		/// Returns the number of concurrent processes which can calculate paths at once.
		/// When using multithreading, this will be the number of threads, if not using multithreading it is always 1 (since only 1 coroutine is used).
		/// See: threadInfos
		/// See: IsUsingMultithreading
		/// </summary>
		public int NumThreads {
			get {
				return pathHandlers.Length;
			}
		}

		/// <summary>Returns whether or not multithreading is used</summary>
		public bool IsUsingMultithreading {
			get {
				return multithreaded;
			}
		}

		internal PathProcessor (AstarPath astar, PathReturnQueue returnQueue, int processors, bool multithreaded) {
			this.astar = astar;
			this.returnQueue = returnQueue;

			// Set up path queue with the specified number of receivers
			queue = new BlockableChannel<Path>();
			threads = null;
			threadCoroutine = null;
			pathHandlers = new PathHandler[0];
		}

		/// <summary>
		/// Changes the number of threads used for pathfinding.
		///
		/// If multithreading is disabled, processors must be equal to 1.
		/// </summary>
		public void SetThreadCount (int processors, bool multithreaded) {
			if (threads != null || threadCoroutine != null || pathHandlers.Length > 0) throw new System.Exception("Call StopThreads before setting the thread count");

			if (processors < 1) {
				throw new System.ArgumentOutOfRangeException("processors");
			}

			if (!multithreaded && processors != 1) {
				throw new System.Exception("Only a single non-multithreaded processor is allowed");
			}

			pathHandlers = new PathHandler[processors];
			this.multithreaded = multithreaded;

			for (int i = 0; i < processors; i++) {
				pathHandlers[i] = new PathHandler(astar.nodeStorage, i, processors);
			}
			astar.nodeStorage.SetThreadCount(processors);
			StartThreads();
		}

		void StartThreads () {
			if (threads != null || threadCoroutine != null) throw new System.Exception("Call StopThreads before starting threads");

			queue.Reopen();

			// Ensure the node storage is up to date.
			// Per-thread data may have been cleared if the AstarPath object
			// was disabled.
			astar.nodeStorage.SetThreadCount(pathHandlers.Length);

			if (multithreaded) {
				threads = new Thread[this.pathHandlers.Length];

				// Start lots of threads
				for (int i = 0; i < this.pathHandlers.Length; i++) {
					var pathHandler = pathHandlers[i];
					var receiver = queue.AddReceiver();
					threads[i] = new Thread(() => CalculatePathsThreaded(pathHandler, receiver));
#if !UNITY_SWITCH || UNITY_EDITOR
					// Note: Setting the thread name seems to crash when deploying for Switch: https://forum.arongranberg.com/t/path-processor-crashing-nintendo-switch-build/6584
					threads[i].Name = "Pathfinding Thread " + i;
#endif
					threads[i].IsBackground = true;
					threads[i].Start();
				}
			} else {
				coroutineReceiver = queue.AddReceiver();
				// Start coroutine if not using multithreading
				threadCoroutine = CalculatePaths(pathHandlers[0]);
			}
		}

		/// <summary>Prevents pathfinding from running while held</summary>
		public struct GraphUpdateLock : System.IDisposable {
			PathProcessor pathProcessor;
			int id;

			public GraphUpdateLock (PathProcessor pathProcessor, bool block) {
				this.pathProcessor = pathProcessor;
				Profiler.BeginSample("Pausing pathfinding");
				id = pathProcessor.Lock(block);
				Profiler.EndSample();
			}

			/// <summary>
			/// True while this lock is preventing the pathfinding threads from processing more paths.
			/// Note that the pathfinding threads may not be paused yet (if this lock was obtained using PausePathfinding(false)).
			/// </summary>
			public bool Held => pathProcessor != null && pathProcessor.locks.Contains(id);

			/// <summary>Allow pathfinding to start running again if no other locks are still held</summary>
			public void Release() => pathProcessor.Unlock(id);

			void System.IDisposable.Dispose () {
				Release();
			}
		}

		int Lock (bool block) {
			queue.isBlocked = true;
			if (block) {
				while (!queue.allReceiversBlocked) {
					Assert.IsTrue(threads != null || threadCoroutine != null);
					if (IsUsingMultithreading) {
						Thread.Sleep(1);
					} else {
						TickNonMultithreaded();
					}
				}
			}

			nextLockID++;
			locks.Add(nextLockID);
			return nextLockID;
		}

		void Unlock (int id) {
			if (!locks.Remove(id)) {
				throw new System.ArgumentException("This lock has already been released");
			}

			// Check if there are no remaining active locks
			if (locks.Count == 0) {
				if (OnQueueUnblocked != null) OnQueueUnblocked();

				queue.isBlocked = false;
			}
		}

		/// <summary>
		/// Prevents pathfinding threads from starting to calculate any new paths.
		///
		/// Returns: A lock object. You need to call Unlock on that object to allow pathfinding to resume.
		///
		/// Note: In most cases this should not be called from user code.
		/// </summary>
		/// <param name="block">If true, this call will block until all pathfinding threads are paused.
		/// otherwise the threads will be paused as soon as they are done with what they are currently doing.</param>
		public GraphUpdateLock PausePathfinding (bool block) {
			return new GraphUpdateLock(this, block);
		}

		/// <summary>
		/// Does pathfinding calculations when not using multithreading.
		///
		/// This method should be called once per frame if <see cref="IsUsingMultithreading"/> is true.
		/// </summary>
		public void TickNonMultithreaded () {
			// Process paths
			if (threadCoroutine == null) throw new System.InvalidOperationException("Cannot tick non-multithreaded pathfinding when no coroutine has been started");

			try {
				if (!threadCoroutine.MoveNext()) {
					threadCoroutine = null;
					coroutineReceiver.Close();
				}
			} catch (System.Exception e) {
				Debug.LogException(e);
				Debug.LogError("Unhandled exception during pathfinding. Terminating.");
				queue.Close();

				// This will kill pathfinding
				threadCoroutine = null;
				coroutineReceiver.Close();
			}
		}

		/// <summary>
		/// Calls 'Join' on each of the threads to block until they have completed.
		///
		/// This will also clean up any unmanaged memory used by the threads.
		/// </summary>
		public void StopThreads () {
			// Don't accept any more path calls to this AstarPath instance.
			// This will cause all pathfinding threads to exit (if any exist)
			queue.Close();

			if (threads != null) {
				for (int i = 0; i < threads.Length; i++) {
					if (!threads[i].Join(200)) {
						Debug.LogError("Could not terminate pathfinding thread["+i+"] in 200ms, trying Thread.Abort");
						threads[i].Abort();
					}
				}
				threads = null;
			}
			if (threadCoroutine != null) {
				Assert.IsTrue(queue.numReceivers > 0);
				while (queue.numReceivers > 0) TickNonMultithreaded();
				Assert.IsNull(threadCoroutine);
			}

			Assert.AreEqual(queue.numReceivers, 0, "Not all receivers were blocked and terminated when stopping threads");

			// Dispose unmanaged data
			for (int i = 0; i < pathHandlers.Length; i++) {
				pathHandlers[i].Dispose();
			}
			pathHandlers = new PathHandler[0];
		}

		/// <summary>
		/// Cleans up all native memory managed by this instance.
		///
		/// You may use this instance again by calling SetThreadCount.
		/// </summary>
		public void Dispose () {
			StopThreads();
		}

		/// <summary>
		/// Main pathfinding method (multithreaded).
		/// This method will calculate the paths in the pathfinding queue when multithreading is enabled.
		///
		/// See: CalculatePaths
		/// See: <see cref="AstarPath.StartPath"/>
		/// </summary>
		void CalculatePathsThreaded (PathHandler pathHandler, BlockableChannel<Path>.Receiver receiver) {
			UnityEngine.Profiling.Profiler.BeginThreadProfiling("Pathfinding", "Pathfinding thread #" + (pathHandler.threadID+1));

			try {
				// Max number of ticks we are allowed to continue working in one run.
				// One tick is 1/10000 of a millisecond.
				// We need to check once in a while if the thread should be stopped.
				long maxTicks = (long)(10*10000);
				long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				while (true) {
					// The path we are currently calculating
					if (receiver.Receive(out var path) == BlockableChannel<Path>.PopState.Closed) {
						if (astar.logPathResults == PathLog.Heavy)
							Debug.LogWarning("Shutting down pathfinding thread #" + pathHandler.threadID);
						receiver.Close();
						return;
					}
					MarkerCalculatePath.Begin();
					// Access the internal implementation methods
					IPathInternals ipath = (IPathInternals)path;


					MarkerPreparePath.Begin();
					ipath.PrepareBase(pathHandler);

					// Now processing the path
					// Will advance to Processing
					ipath.AdvanceState(PathState.Processing);

					// Call some callbacks
					if (OnPathPreSearch != null) {
						OnPathPreSearch(path);
					}

					// Tick for when the path started, used for calculating how long time the calculation took
					long startTicks = System.DateTime.UtcNow.Ticks;

					// Prepare the path
					ipath.Prepare();

					// When using a heuristic, break ties using the H score.
					// When not using a heuristic, break ties by the insertion order of the nodes.
					// This will make paths a lot prettier, especially on grid graphs.
					pathHandler.heap.tieBreaking = path.heuristicObjectiveInternal.hasHeuristic ? BinaryHeap.TieBreaking.HScore : BinaryHeap.TieBreaking.InsertionOrder;
					MarkerPreparePath.End();


					if (path.CompleteState == PathCompleteState.NotCalculated) {
						// For visualization purposes, we set the last computed path to p, so we can view debug info on it in the editor (scene view).
						astar.debugPathData = ipath.PathHandler;
						astar.debugPathID = path.pathID;

						// Loop while the path has not been fully calculated
						while (path.CompleteState == PathCompleteState.NotCalculated) {
							// Do some work on the path calculation.
							// The function will return when it has taken too much time
							// or when it has finished calculation
							ipath.CalculateStep(targetTick);

							targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

							// Cancel function (and thus the thread) if no more paths should be accepted.
							// This is done when the A* object is about to be destroyed
							// The path is returned and then this function will be terminated
							if (queue.isClosed) {
								path.FailWithError("AstarPath object destroyed");
							}
						}

						path.duration = (System.DateTime.UtcNow.Ticks - startTicks)*0.0001F;

#if ProfileAstar
						System.Threading.Interlocked.Increment(ref AstarPath.PathsCompleted);
						System.Threading.Interlocked.Add(ref AstarPath.TotalSearchTime, System.DateTime.UtcNow.Ticks - startTicks);
#endif
					}

					// Cleans up node tagging and other things
					ipath.Cleanup();
					pathHandler.heap.Clear(pathHandler.pathNodes);


					if (path.immediateCallback != null) path.immediateCallback(path);

					if (OnPathPostSearch != null) {
						OnPathPostSearch(path);
					}

					// Push the path onto the return stack
					// It will be detected by the main Unity thread and returned as fast as possible (the next late update hopefully)
					returnQueue.Enqueue(path);

					// Will advance to ReturnQueue
					ipath.AdvanceState(PathState.ReturnQueue);

					MarkerCalculatePath.End();
				}
			} catch (System.Exception e) {
#if !NETFX_CORE
				if (e is ThreadAbortException) {
					if (astar.logPathResults == PathLog.Heavy)
						Debug.LogWarning("Shutting down pathfinding thread #" + pathHandler.threadID);
					receiver.Close();
					return;
				}
#endif

				Debug.LogException(e);
				Debug.LogError("Unhandled exception during pathfinding. Terminating.");
				// Unhandled exception, kill pathfinding
				queue.Close();
			} finally {
				UnityEngine.Profiling.Profiler.EndThreadProfiling();
			}

			Debug.LogError("Error : This part should never be reached.");
			receiver.Close();
		}

		/// <summary>
		/// Main pathfinding method.
		/// This method will calculate the paths in the pathfinding queue.
		///
		/// See: CalculatePathsThreaded
		/// See: StartPath
		/// </summary>
		IEnumerator CalculatePaths (PathHandler pathHandler) {
			// Max number of ticks before yielding/sleeping
			long maxTicks = (long)(astar.maxFrameTime*10000);
			long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

			while (true) {
				// The path we are currently calculating
				Path p = null;

				// Try to get the next path to be calculated
				bool blockedBefore = false;
				while (p == null) {
					switch (coroutineReceiver.ReceiveNoBlock(blockedBefore, out p)) {
					case BlockableChannel<Path>.PopState.Ok:
						break;
					case BlockableChannel<Path>.PopState.Wait:
						blockedBefore = true;
						yield return null;
						break;
					case BlockableChannel<Path>.PopState.Closed:
						yield break;
					}
				}

				IPathInternals ip = (IPathInternals)p;

				// Max number of ticks we are allowed to use for pathfinding in one frame
				// One tick is 1/10000 of a millisecond
				maxTicks = (long)(astar.maxFrameTime*10000);

				ip.PrepareBase(pathHandler);

				// Now processing the path
				// Will advance to Processing
				ip.AdvanceState(PathState.Processing);

				// Call some callbacks
				// It needs to be stored in a local variable to avoid race conditions
				var tmpOnPathPreSearch = OnPathPreSearch;
				if (tmpOnPathPreSearch != null) tmpOnPathPreSearch(p);

				// Tick for when the path started, used for calculating how long time the calculation took
				long startTicks = System.DateTime.UtcNow.Ticks;
				long totalTicks = 0;

				ip.Prepare();

				// When using a heuristic, break ties using the H score.
				// When not using a heuristic, break ties by the insertion order of the nodes.
				// This will make paths a lot prettier, especially on grid graphs.
				pathHandler.heap.tieBreaking = p.heuristicObjectiveInternal.hasHeuristic ? BinaryHeap.TieBreaking.HScore : BinaryHeap.TieBreaking.InsertionOrder;

				// Check if the Prepare call caused the path to complete
				// If this happens the path usually failed
				if (p.CompleteState == PathCompleteState.NotCalculated) {
					// For debug uses, we set the last computed path to p, so we can view debug info on it in the editor (scene view).
					astar.debugPathData = ip.PathHandler;
					astar.debugPathID = p.pathID;

					// The error can turn up in the Init function
					while (p.CompleteState == PathCompleteState.NotCalculated) {
						// Run some pathfinding calculations.
						// The function will return when it has taken too much time
						// or when it has finished calculating the path.
						ip.CalculateStep(targetTick);


						// If the path has finished calculating, we can break here directly instead of sleeping
						// Improves latency
						if (p.CompleteState != PathCompleteState.NotCalculated) break;

						totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
						// Yield/sleep so other threads can work

						yield return null;

						startTicks = System.DateTime.UtcNow.Ticks;

						// Cancel function (and thus the thread) if no more paths should be accepted.
						// This is done when the A* object is about to be destroyed
						// The path is returned and then this function will be terminated (see similar IF statement higher up in the function)
						if (queue.isClosed) {
							p.FailWithError("AstarPath object destroyed");
						}

						targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
					}

					totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
					p.duration = totalTicks*0.0001F;

#if ProfileAstar
					System.Threading.Interlocked.Increment(ref AstarPath.PathsCompleted);
#endif
				}

				// Cleans up node tagging and other things
				ip.Cleanup();
				pathHandler.heap.Clear(pathHandler.pathNodes);


				// Call the immediate callback
				// It needs to be stored in a local variable to avoid race conditions
				var tmpImmediateCallback = p.immediateCallback;
				if (tmpImmediateCallback != null) tmpImmediateCallback(p);


				// It needs to be stored in a local variable to avoid race conditions
				var tmpOnPathPostSearch = OnPathPostSearch;
				if (tmpOnPathPostSearch != null) tmpOnPathPostSearch(p);


				// Push the path onto the return stack
				// It will be detected by the main Unity thread and returned as fast as possible (the next late update)
				returnQueue.Enqueue(p);

				ip.AdvanceState(PathState.ReturnQueue);


				// Wait a bit if we have calculated a lot of paths
				if (System.DateTime.UtcNow.Ticks > targetTick) {
					yield return null;
					targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				}
			}
		}
	}
}

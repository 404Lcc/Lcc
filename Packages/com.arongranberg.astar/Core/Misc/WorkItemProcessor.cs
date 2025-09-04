using UnityEngine;
using UnityEngine.Profiling;
using Unity.Jobs;
using UnityEngine.Assertions;

namespace Pathfinding {
	/// <summary>
	/// An item of work that can be executed when graphs are safe to update.
	/// See: <see cref="AstarPath.UpdateGraphs"/>
	/// See: <see cref="AstarPath.AddWorkItem"/>
	/// </summary>
	public struct AstarWorkItem {
		/// <summary>
		/// Init function.
		/// May be null if no initialization is needed.
		/// Will be called once, right before the first call to <see cref="update"/> or <see cref="updateWithContext"/>.
		/// </summary>
		public System.Action init;

		/// <summary>
		/// Init function.
		/// May be null if no initialization is needed.
		/// Will be called once, right before the first call to <see cref="update"/> or <see cref="updateWithContext"/>.
		///
		/// A context object is sent as a parameter. This can be used
		/// to for example queue a flood fill that will be executed either
		/// when a work item calls EnsureValidFloodFill or all work items have
		/// been completed. If multiple work items are updating nodes
		/// so that they need a flood fill afterwards, using the QueueFloodFill
		/// method is preferred since then only a single flood fill needs
		/// to be performed for all of the work items instead of one
		/// per work item.
		/// </summary>
		public System.Action<IWorkItemContext> initWithContext;

		/// <summary>
		/// Update function, called once per frame when the work item executes.
		/// Takes a param force. If that is true, the work item should try to complete the whole item in one go instead
		/// of spreading it out over multiple frames.
		///
		/// Warning: If you make modifications to the graphs, they must only be made during the last time the <see cref="update"/> method is called.
		/// Earlier invocations, as well as the <see cref="init"/>/<see cref="initWithContext"/> mehods, are only for pre-calculating information required for the update.
		///
		/// Returns: True when the work item is completed.
		/// </summary>
		public System.Func<bool, bool> update;

		/// <summary>
		/// Update function, called once per frame when the work item executes.
		/// Takes a param force. If that is true, the work item should try to complete the whole item in one go instead
		/// of spreading it out over multiple frames.
		/// Returns: True when the work item is completed.
		///
		/// Warning: If you make modifications to the graphs, they must only be made during the last time the <see cref="update"/> method is called.
		/// Earlier invocations, as well as the <see cref="init"/>/<see cref="initWithContext"/> mehods, are only for pre-calculating information required for the update.
		///
		/// A context object is sent as a parameter. This can be used
		/// to for example queue a flood fill that will be executed either
		/// when a work item calls EnsureValidFloodFill or all work items have
		/// been completed. If multiple work items are updating nodes
		/// so that they need a flood fill afterwards, using the QueueFloodFill
		/// method is preferred since then only a single flood fill needs
		/// to be performed for all of the work items instead of one
		/// per work item.
		/// </summary>
		public System.Func<IWorkItemContext, bool, bool> updateWithContext;

		/// <summary>Creates a work item which will call the specified functions when executed.</summary>
		/// <param name="update">Will be called once per frame when the work item executes. See #update for details.</param>
		public AstarWorkItem (System.Func<bool, bool> update) {
			this.init = null;
			this.initWithContext = null;
			this.updateWithContext = null;
			this.update = update;
		}

		/// <summary>Creates a work item which will call the specified functions when executed.</summary>
		/// <param name="update">Will be called once per frame when the work item executes. See #updateWithContext for details.</param>
		public AstarWorkItem (System.Func<IWorkItemContext, bool, bool> update) {
			this.init = null;
			this.initWithContext = null;
			this.updateWithContext = update;
			this.update = null;
		}

		/// <summary>Creates a work item which will call the specified functions when executed.</summary>
		/// <param name="init">Will be called once, right before the first call to update. See #init for details.</param>
		/// <param name="update">Will be called once per frame when the work item executes. See #update for details.</param>
		public AstarWorkItem (System.Action init, System.Func<bool, bool> update = null) {
			this.init = init;
			this.initWithContext = null;
			this.update = update;
			this.updateWithContext = null;
		}

		/// <summary>Creates a work item which will call the specified functions when executed.</summary>
		/// <param name="init">Will be called once, right before the first call to update. See #initWithContext for details.</param>
		/// <param name="update">Will be called once per frame when the work item executes. See #updateWithContext for details.</param>
		public AstarWorkItem (System.Action<IWorkItemContext> init, System.Func<IWorkItemContext, bool, bool> update = null) {
			this.init = null;
			this.initWithContext = init;
			this.update = null;
			this.updateWithContext = update;
		}
	}

	/// <summary>Interface to expose a subset of the WorkItemProcessor functionality</summary>
	public interface IWorkItemContext : IGraphUpdateContext {
		/// <summary>
		/// Call during work items to queue a flood fill.
		/// An instant flood fill can be done via FloodFill()
		/// but this method can be used to batch several updates into one
		/// to increase performance.
		/// WorkItems which require a valid Flood Fill in their execution can call EnsureValidFloodFill
		/// to ensure that a flood fill is done if any earlier work items queued one.
		///
		/// Once a flood fill is queued it will be done after all WorkItems have been executed.
		///
		/// Deprecated: You no longer need to call this method. Connectivity data is automatically kept up-to-date.
		/// </summary>
		[System.Obsolete("You no longer need to call this method. Connectivity data is automatically kept up-to-date.")]
		void QueueFloodFill();

		/// <summary>
		/// If a WorkItem needs to have a valid area information during execution, call this method to ensure there are no pending flood fills.
		/// If you are using the <see cref="GraphNode.Area"/> property or the <see cref="PathUtilities.IsPathPossible"/> method in your work items, then you may want to call this method before you use them,
		/// to ensure that the data is up to date.
		///
		/// See: <see cref="HierarchicalGraph"/>
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(new AstarWorkItem((IWorkItemContext ctx) => {
		///     // Update the graph in some way
		///     // ...
		///
		///     // Ensure that connectivity information is up to date.
		///     // This will also automatically run after all work items have been executed.
		///     ctx.EnsureValidFloodFill();
		///
		///     // Use connectivity information
		///     if (PathUtilities.IsPathPossible(someNode, someOtherNode)) {
		///         // Do something
		///     }
		/// }));
		/// </code>
		/// </summary>
		void EnsureValidFloodFill();

		/// <summary>
		/// Call to send a GraphModifier.EventType.PreUpdate event to all graph modifiers.
		/// The difference between this and GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate) is that using this method
		/// ensures that multiple PreUpdate events will not be issued during a single update.
		///
		/// Once an event has been sent no more events will be sent until all work items are complete and a PostUpdate or PostScan event is sent.
		///
		/// When scanning a graph PreUpdate events are never sent. However a PreScan event is always sent before a scan begins.
		/// </summary>
		void PreUpdate();

		/// <summary>
		/// Trigger a graph modification event.
		/// This will cause a <see cref="GraphModifier.EventType.PostUpdate"/> event to be issued after all graph updates have finished.
		/// Some scripts listen for this event. For example off-mesh links listen to it and will recalculate which nodes they are connected to when it it sent.
		/// If a graph is dirtied multiple times, or even if multiple graphs are dirtied, the event will only be sent once.
		/// </summary>
		// TODO: Deprecate?
		void SetGraphDirty(NavGraph graph);
	}

	class WorkItemProcessor : IWorkItemContext {
		public event System.Action OnGraphsUpdated;

		/// <summary>Used to prevent waiting for work items to complete inside other work items as that will cause the program to hang</summary>
		public bool workItemsInProgressRightNow { get; private set; }

		readonly AstarPath astar;
		readonly IndexedQueue<AstarWorkItem> workItems = new IndexedQueue<AstarWorkItem>();


		/// <summary>True if any work items are queued right now</summary>
		public bool anyQueued {
			get { return workItems.Count > 0; }
		}

		bool anyGraphsDirty = true;
		bool preUpdateEventSent = false;

		/// <summary>
		/// True while a batch of work items are being processed.
		/// Set to true when a work item is started to be processed, reset to false when all work items are complete.
		///
		/// Work item updates are often spread out over several frames, this flag will be true during the whole time the
		/// updates are in progress.
		/// </summary>
		public bool workItemsInProgress { get; private set; }

		/// <summary>Similar to Queue<T> but allows random access</summary>
		// TODO: Replace with CircularBuffer?
		class IndexedQueue<T> {
			T[] buffer = new T[4];
			int start;

			public T this[int index] {
				get {
					if (index < 0 || index >= Count) throw new System.IndexOutOfRangeException();
					return buffer[(start + index) % buffer.Length];
				}
				set {
					if (index < 0 || index >= Count) throw new System.IndexOutOfRangeException();
					buffer[(start + index) % buffer.Length] = value;
				}
			}

			public int Count { get; private set; }

			public void Enqueue (T item) {
				if (Count == buffer.Length) {
					var newBuffer = new T[buffer.Length*2];
					for (int i = 0; i < Count; i++) {
						newBuffer[i] = this[i];
					}
					buffer = newBuffer;
					start = 0;
				}

				buffer[(start + Count) % buffer.Length] = item;
				Count++;
			}

			public T Dequeue () {
				if (Count == 0) throw new System.InvalidOperationException();
				var item = buffer[start];
				start = (start + 1) % buffer.Length;
				Count--;
				return item;
			}
		}

		/// <summary>
		/// Call during work items to queue a flood fill.
		/// An instant flood fill can be done via FloodFill()
		/// but this method can be used to batch several updates into one
		/// to increase performance.
		/// WorkItems which require a valid Flood Fill in their execution can call EnsureValidFloodFill
		/// to ensure that a flood fill is done if any earlier work items queued one.
		///
		/// Once a flood fill is queued it will be done after all WorkItems have been executed.
		///
		/// Deprecated: This method no longer does anything.
		/// </summary>
		void IWorkItemContext.QueueFloodFill () {
		}

		void IWorkItemContext.PreUpdate () {
			if (!preUpdateEventSent && !astar.isScanning) {
				preUpdateEventSent = true;
				GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);
			}
		}

		// This will also call DirtyGraphs
		void IWorkItemContext.SetGraphDirty(NavGraph graph) => astar.DirtyBounds(graph.bounds);

		// This will also call DirtyGraphs
		void IGraphUpdateContext.DirtyBounds(Bounds bounds) => astar.DirtyBounds(bounds);

		internal void DirtyGraphs () {
			anyGraphsDirty = true;
		}

		/// <summary>If a WorkItem needs to have a valid area information during execution, call this method to ensure there are no pending flood fills</summary>
		public void EnsureValidFloodFill () {
			astar.hierarchicalGraph.RecalculateIfNecessary();
		}

		public WorkItemProcessor (AstarPath astar) {
			this.astar = astar;
		}

		/// <summary>
		/// Add a work item to be processed when pathfinding is paused.
		///
		/// See: ProcessWorkItems
		/// </summary>
		public void AddWorkItem (AstarWorkItem item) {
			workItems.Enqueue(item);
		}

		bool ProcessWorkItems (bool force, bool sendEvents) {
			if (workItemsInProgressRightNow) throw new System.Exception("Processing work items recursively. Please do not wait for other work items to be completed inside work items. " +
				"If you think this is not caused by any of your scripts, this might be a bug.");

			// Work items may update graph data arbitrarily
			// So we need to hold a write lock here so that for example
			// ECS jobs don't try to read the graph data while it is being updated
			var lockObj = astar.LockGraphDataForWritingSync();
			astar.data.LockGraphStructure(true);

			// Make sure the physics engine data is up to date.
			// Graph updates may use physics methods and it is very confusing if they
			// do not always pick up the latest changes made to the scene.
			UnityEngine.Physics.SyncTransforms();
			UnityEngine.Physics2D.SyncTransforms();

			workItemsInProgressRightNow = true;

			try {
				bool workRemaining = false;
				bool anyFinished = false;
				while (workItems.Count > 0) {
					// Working on a new batch
					if (!workItemsInProgress) {
						workItemsInProgress = true;
					}

					// Peek at first item in the queue
					AstarWorkItem itm = workItems[0];
					bool status;

					try {
						// Call init the first time the item is seen
						if (itm.init != null) {
							itm.init();
							itm.init = null;
						}

						if (itm.initWithContext != null) {
							itm.initWithContext(this);
							itm.initWithContext = null;
						}

						// Make sure the item in the queue is up to date
						workItems[0] = itm;

						if (itm.update != null) {
							status = itm.update(force);
						} else if (itm.updateWithContext != null) {
							status = itm.updateWithContext(this, force);
						} else {
							status = true;
						}
					} catch {
						workItems.Dequeue();
						throw;
					}

					if (!status) {
						if (force) {
							Debug.LogError("Misbehaving WorkItem. 'force'=true but the work item did not complete.\nIf force=true is passed to a WorkItem it should always return true.");
						}

						// There's more work to do on this work item
						workRemaining = true;
						break;
					} else {
						workItems.Dequeue();
						anyFinished = true;
					}
				}

				if (sendEvents && anyFinished) {
					if (anyGraphsDirty) {
						Profiler.BeginSample("PostUpdateBeforeAreaRecalculation");
						GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdateBeforeAreaRecalculation);
						Profiler.EndSample();
					}

					astar.offMeshLinks.Refresh();
					EnsureValidFloodFill();

					if (anyGraphsDirty) {
						Profiler.BeginSample("PostUpdate");
						GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
						if (OnGraphsUpdated != null) OnGraphsUpdated();
						Profiler.EndSample();
					}
				}
				if (workRemaining) return false;
			} finally {
				lockObj.Unlock();
				astar.data.UnlockGraphStructure();
				workItemsInProgressRightNow = false;
			}

			// Reset flags at the end
			anyGraphsDirty = false;
			preUpdateEventSent = false;

			workItemsInProgress = false;
			return true;
		}

		/// <summary>
		/// Process graph updating work items.
		/// Process all queued work items, e.g graph updates and the likes.
		///
		/// Returns:
		/// - false if there are still items to be processed.
		/// - true if the last work items was processed and pathfinding threads are ready to be resumed.
		///
		/// This will not call <see cref="EnsureValidFloodFill"/>	in contrast to <see cref="ProcessWorkItemsForUpdate"/>.
		///
		/// See: <see cref="AstarPath.AddWorkItem"/>
		/// </summary>
		public bool ProcessWorkItemsForScan (bool force) {
			return ProcessWorkItems(force, false);
		}

		/// <summary>
		/// Process graph updating work items.
		/// Process all queued work items, e.g graph updates and the likes.
		///
		/// Returns:
		/// - false if there are still items to be processed.
		/// - true if the last work items was processed and pathfinding threads are ready to be resumed.
		///
		/// See: <see cref="AstarPath.AddWorkItem"/>
		///
		/// This method also calls GraphModifier.TriggerEvent(PostUpdate) if any graphs were dirtied.
		/// It also calls <see cref="EnsureValidFloodFill"/> after the work items are done
		/// </summary>
		public bool ProcessWorkItemsForUpdate (bool force) {
			return ProcessWorkItems(force, true);
		}
	}
}

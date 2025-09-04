using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine.Profiling;

namespace Pathfinding {
	using Pathfinding.Drawing;

	/// <summary>
	/// Handles path calls for a single unit.
	///
	/// This is a component which is meant to be attached to a single unit (AI, Robot, Player, whatever) to handle its pathfinding calls.
	/// It also handles post-processing of paths using modifiers.
	///
	/// See: calling-pathfinding (view in online documentation for working links)
	/// See: modifiers (view in online documentation for working links)
	/// </summary>
	[AddComponentMenu("Pathfinding/Seeker")]
	[DisallowMultipleComponent]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/seeker.html")]
	public class Seeker : VersionedMonoBehaviour {
		/// <summary>
		/// Enables drawing of the last calculated path using Gizmos.
		/// The path will show up in green.
		///
		/// See: OnDrawGizmos
		/// </summary>
		public bool drawGizmos = true;

		/// <summary>
		/// Enables drawing of the non-postprocessed path using Gizmos.
		/// The path will show up in orange.
		///
		/// Requires that <see cref="drawGizmos"/> is true.
		///
		/// This will show the path before any post processing such as smoothing is applied.
		///
		/// See: drawGizmos
		/// See: OnDrawGizmos
		/// </summary>
		public bool detailedGizmos;

		/// <summary>Path modifier which tweaks the start and end points of a path</summary>
		[HideInInspector]
		public StartEndModifier startEndModifier = new StartEndModifier();

		/// <summary>
		/// The tags which the Seeker can traverse.
		///
		/// Note: This field is a bitmask.
		/// See: bitmasks (view in online documentation for working links)
		/// </summary>
		[HideInInspector]
		public int traversableTags = -1;

		/// <summary>
		/// Penalties for each tag.
		/// Tag 0 which is the default tag, will have added a penalty of tagPenalties[0].
		/// These should only be positive values since the A* algorithm cannot handle negative penalties.
		///
		/// The length of this array should be exactly 32, one for each tag.
		///
		/// See: Pathfinding.Path.tagPenalties
		/// </summary>
		[HideInInspector]
		public int[] tagPenalties = new int[32];

		/// <summary>
		/// Graphs that this Seeker can use.
		/// This field determines which graphs will be considered when searching for the start and end nodes of a path.
		/// It is useful in numerous situations, for example if you want to make one graph for small units and one graph for large units.
		///
		/// This is a bitmask so if you for example want to make the agent only use graph index 3 then you can set this to:
		/// <code> seeker.graphMask = 1 << 3; </code>
		///
		/// See: bitmasks (view in online documentation for working links)
		///
		/// Note that this field only stores which graph indices that are allowed. This means that if the graphs change their ordering
		/// then this mask may no longer be correct.
		///
		/// If you know the name of the graph you can use the <see cref="Pathfinding.GraphMask.FromGraphName"/> method:
		/// <code>
		/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
		/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
		///
		/// NNConstraint nn = NNConstraint.Walkable;
		///
		/// nn.graphMask = mask1 | mask2;
		///
		/// // Find the node closest to somePoint which is either in 'My Grid Graph' OR in 'My Other Grid Graph'
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		///
		/// Some overloads of the <see cref="StartPath"/> methods take a graphMask parameter. If those overloads are used then they
		/// will override the graph mask for that path request.
		///
		/// [Open online documentation to see images]
		///
		/// See: multiple-agent-types (view in online documentation for working links)
		/// </summary>
		[HideInInspector]
		public GraphMask graphMask = GraphMask.everything;

		/// <summary>
		/// Custom traversal provider to calculate which nodes are traversable and their penalties.
		///
		/// This can be used to override the built-in pathfinding logic.
		///
		/// <code>
		/// seeker.traversalProvider = new MyCustomTraversalProvider();
		/// </code>
		///
		/// See: traversal_provider (view in online documentation for working links)
		/// </summary>
		public ITraversalProvider traversalProvider;

		/// <summary>Used for serialization backwards compatibility</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("graphMask")]
		int graphMaskCompatibility = -1;

		/// <summary>
		/// Callback for when a path is completed.
		/// Movement scripts should register to this delegate.
		/// A temporary callback can also be set when calling StartPath, but that delegate will only be called for that path
		///
		/// <code>
		/// public void Start () {
		///     // Assumes a Seeker component is attached to the GameObject
		///     Seeker seeker = GetComponent<Seeker>();
		///
		///     // seeker.pathCallback is a OnPathDelegate, we add the function OnPathComplete to it so it will be called whenever a path has finished calculating on that seeker
		///     seeker.pathCallback += OnPathComplete;
		/// }
		///
		/// public void OnPathComplete (Path p) {
		///     Debug.Log("This is called when a path is completed on the seeker attached to this GameObject");
		/// }
		/// </code>
		///
		/// Deprecated: Pass a callback every time to the StartPath method instead, or use ai.SetPath+ai.pathPending on the movement script. You can cache it in your own script if you want to avoid the GC allocation of creating a new delegate.
		/// </summary>
		[System.Obsolete("Pass a callback every time to the StartPath method instead, or use ai.SetPath+ai.pathPending on the movement script. You can cache it in your own script if you want to avoid the GC allocation of creating a new delegate.")]
		public OnPathDelegate pathCallback;

		/// <summary>Called before pathfinding is started</summary>
		public OnPathDelegate preProcessPath;

		/// <summary>Called after a path has been calculated, right before modifiers are executed.</summary>
		public OnPathDelegate postProcessPath;

#if UNITY_EDITOR
		/// <summary>Used for drawing gizmos</summary>
		[System.NonSerialized]
		List<Vector3> lastCompletedVectorPath;

		/// <summary>Used for drawing gizmos</summary>
		[System.NonSerialized]
		List<GraphNode> lastCompletedNodePath;
#endif

		/// <summary>The current path</summary>
		[System.NonSerialized]
		protected Path path;

		/// <summary>Previous path. Used to draw gizmos</summary>
		[System.NonSerialized]
		private Path prevPath;

		/// <summary>Cached delegate to avoid allocating one every time a path is started</summary>
		private readonly OnPathDelegate onPathDelegate;
		/// <summary>Cached delegate to avoid allocating one every time a path is started</summary>
		private readonly OnPathDelegate onPartialPathDelegate;

		/// <summary>Temporary callback only called for the current path. This value is set by the StartPath functions</summary>
		private OnPathDelegate tmpPathCallback;

		/// <summary>The path ID of the last path queried</summary>
		protected uint lastPathID;

		/// <summary>Internal list of all modifiers</summary>
		readonly List<IPathModifier> modifiers = new List<IPathModifier>();

		public enum ModifierPass {
			PreProcess,
			// An obsolete item occupied index 1 previously
			PostProcess = 2,
		}

		public Seeker () {
			onPathDelegate = OnPathComplete;
			onPartialPathDelegate = OnPartialPathComplete;
		}

		/// <summary>Initializes a few variables</summary>
		protected override void Awake () {
			base.Awake();
			startEndModifier.Awake(this);
		}

		/// <summary>
		/// Path that is currently being calculated or was last calculated.
		/// You should rarely have to use this. Instead get the path when the path callback is called.
		///
		/// See: <see cref="StartPath"/>
		/// </summary>
		public Path GetCurrentPath() => path;

		/// <summary>
		/// Stop calculating the current path request.
		/// If this Seeker is currently calculating a path it will be canceled.
		/// The callback (usually to a method named OnPathComplete) will soon be called
		/// with a path that has the 'error' field set to true.
		///
		/// This does not stop the character from moving, it just aborts
		/// the path calculation.
		/// </summary>
		/// <param name="pool">If true then the path will be pooled when the pathfinding system is done with it.</param>
		public void CancelCurrentPathRequest (bool pool = true) {
			if (!IsDone()) {
				path.FailWithError("Canceled by script (Seeker.CancelCurrentPathRequest)");
				if (pool) {
					// Make sure the path has had its reference count incremented and decremented once.
					// If this is not done the system will think no pooling is used at all and will not pool the path.
					// The particular object that is used as the parameter (in this case 'path') doesn't matter at all
					// it just has to be *some* object.
					path.Claim(path);
					path.Release(path);
				}
			}
		}

		/// <summary>
		/// Cleans up some variables.
		/// Releases any eventually claimed paths.
		/// Calls OnDestroy on the <see cref="startEndModifier"/>.
		///
		/// See: <see cref="ReleaseClaimedPath"/>
		/// See: <see cref="startEndModifier"/>
		/// </summary>
		void OnDestroy () {
			ReleaseClaimedPath();
			startEndModifier.OnDestroy(this);
		}

		/// <summary>
		/// Releases the path used for gizmos (if any).
		/// The seeker keeps the latest path claimed so it can draw gizmos.
		/// In some cases this might not be desireable and you want it released.
		/// In that case, you can call this method to release it (not that path gizmos will then not be drawn).
		///
		/// If you didn't understand anything from the description above, you probably don't need to use this method.
		///
		/// See: pooling (view in online documentation for working links)
		/// </summary>
		void ReleaseClaimedPath () {
			if (prevPath != null) {
				prevPath.Release(this, true);
				prevPath = null;
			}
		}

		/// <summary>Called by modifiers to register themselves</summary>
		public void RegisterModifier (IPathModifier modifier) {
			// Modifier might already be registered if pathfinding is used outside of play mode
			if (!modifiers.Contains(modifier)) {
				modifiers.Add(modifier);

				// Sort the modifiers based on their specified order
				modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
			}
		}

		/// <summary>Called by modifiers when they are disabled or destroyed</summary>
		public void DeregisterModifier (IPathModifier modifier) {
			modifiers.Remove(modifier);
		}

		void ForceRegisterModifiers () {
			GetComponents<IPathModifier>(modifiers);
			modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
		}

		/// <summary>
		/// Post-processes a path.
		/// This will run any modifiers attached to this GameObject on the path.
		/// This is identical to calling RunModifiers(ModifierPass.PostProcess, path)
		/// See: <see cref="RunModifiers"/>
		/// </summary>
		public void PostProcess (Path path) {
			RunModifiers(ModifierPass.PostProcess, path);
		}

		/// <summary>Runs modifiers on a path</summary>
		public void RunModifiers (ModifierPass pass, Path path) {
			if (!Application.isPlaying) ForceRegisterModifiers();

			if (pass == ModifierPass.PreProcess) {
				if (preProcessPath != null) preProcessPath(path);

				for (int i = 0; i < modifiers.Count; i++) modifiers[i].PreProcess(path);
			} else if (pass == ModifierPass.PostProcess) {
				Profiler.BeginSample("Running Path Modifiers");
				// Call delegates if they exist
				if (postProcessPath != null) postProcessPath(path);

				// Loop through all modifiers and apply post processing
				for (int i = 0; i < modifiers.Count; i++) modifiers[i].Apply(path);
				Profiler.EndSample();
			}
		}

		/// <summary>
		/// Is the current path done calculating.
		/// Returns true if the current <see cref="path"/> has been returned or if the <see cref="path"/> is null.
		///
		/// Note: Do not confuse this with Pathfinding.Path.IsDone. They usually return the same value, but not always.
		/// The path might be completely calculated, but has not yet been processed by the Seeker.
		///
		/// Inside the OnPathComplete callback this method will return true.
		///
		/// Version: Before version 4.2.13 this would return false inside the OnPathComplete callback. However, this behaviour was unintuitive.
		/// </summary>
		public bool IsDone() => path == null || path.PipelineState >= PathState.Returning;

		/// <summary>Called when a path has completed</summary>
		void OnPathComplete (Path path) {
			OnPathComplete(path, true, true);
		}

		/// <summary>
		/// Called when a path has completed.
		/// Will post process it and return it by calling <see cref="tmpPathCallback"/>
		/// </summary>
		void OnPathComplete (Path p, bool runModifiers, bool sendCallbacks) {
			if (p != null && p != path && sendCallbacks) {
				return;
			}

			if (this == null || p == null || p != path)
				return;

			if (!path.error && runModifiers) {
				// This will send the path for post processing to modifiers attached to this Seeker
				RunModifiers(ModifierPass.PostProcess, path);
			}

			if (sendCallbacks) {
				p.Claim(this);

#if UNITY_EDITOR
				lastCompletedNodePath = p.path;
				lastCompletedVectorPath = p.vectorPath;
#endif

				#pragma warning disable 618
				if (tmpPathCallback == null && pathCallback == null) {
#if UNITY_EDITOR
					// This checks for a common error that people make when they upgrade from an older version
					// This will be removed in a future version to avoid the slight performance cost.
					if (TryGetComponent<IAstarAI>(out var ai)) {
						Debug.LogWarning("A path was calculated, but no callback was specified when calling StartPath. If you wanted a movement script to use this path, use <b>ai.SetPath</b> instead of calling StartPath on the Seeker directly. The path will be forwarded to the attached movement script, but this behavior will be removed in the future.", this);
						ai.SetPath(p);
					}
#endif
				} else {
					// This will send the path to the callback (if any) specified when calling StartPath
					if (tmpPathCallback != null) {
						tmpPathCallback(p);
					}

					// This will send the path to any script which has registered to the callback
					if (pathCallback != null) {
						pathCallback(p);
					}
				}
				#pragma warning restore 618

				// Note: it is important that #prevPath is kept alive (i.e. not pooled)
				// if we are drawing gizmos.
				// It is also important that #path is kept alive since it can be returned
				// from the GetCurrentPath method.
				// Since #path will be copied to #prevPath it is sufficient that #prevPath
				// is kept alive until it is replaced.

				// Recycle the previous path to reduce the load on the GC
				if (prevPath != null) {
					prevPath.Release(this, true);
				}

				prevPath = p;
			}
		}

		/// <summary>
		/// Called for each path in a MultiTargetPath.
		/// Only post processes the path, does not return it.
		/// </summary>
		void OnPartialPathComplete (Path p) {
			OnPathComplete(p, true, false);
		}

		/// <summary>Called once for a MultiTargetPath. Only returns the path, does not post process.</summary>
		void OnMultiPathComplete (Path p) {
			OnPathComplete(p, false, true);
		}

		/// <summary>
		/// Queue a path to be calculated.
		/// Since this method does not take a callback parameter, you should set the <see cref="pathCallback"/> field before calling this method.
		///
		/// <code>
		/// void Start () {
		///     // Get the seeker component attached to this GameObject
		///     var seeker = GetComponent<Seeker>();
		///
		///     // Schedule a new path request from the current position to a position 10 units forward.
		///     // When the path has been calculated, the OnPathComplete method will be called, unless it was canceled by another path request
		///     seeker.StartPath(transform.position, transform.position + Vector3.forward * 10, OnPathComplete);
		///
		///     // Note that the path is NOT calculated at this point
		///     // It has just been queued for calculation
		/// }
		///
		/// void OnPathComplete (Path path) {
		///     // The path is now calculated!
		///
		///     if (path.error) {
		///         Debug.LogError("Path failed: " + path.errorLog);
		///         return;
		///     }
		///
		///     // Cast the path to the path type we were using
		///     var abPath = path as ABPath;
		///
		///     // Draw the path in the scene view for 10 seconds
		///     for (int i = 0; i < abPath.vectorPath.Count - 1; i++) {
		///         Debug.DrawLine(abPath.vectorPath[i], abPath.vectorPath[i+1], Color.red, 10);
		///     }
		/// }
		/// </code>
		///
		/// Deprecated: Use <see cref="StartPath(Vector3,Vector3,OnPathDelegate)"/> instead.
		/// </summary>
		/// <param name="start">The start point of the path</param>
		/// <param name="end">The end point of the path</param>
		[System.Obsolete("Use the overload that takes a callback instead")]
		public Path StartPath (Vector3 start, Vector3 end) {
			return StartPath(start, end, null);
		}

		/// <summary>
		/// Queue a path to be calculated.
		///
		/// The callback will be called when the path has been calculated (which may be several frames into the future).
		/// Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed)
		///
		/// <code>
		/// void Start () {
		///     // Get the seeker component attached to this GameObject
		///     var seeker = GetComponent<Seeker>();
		///
		///     // Schedule a new path request from the current position to a position 10 units forward.
		///     // When the path has been calculated, the OnPathComplete method will be called, unless it was canceled by another path request
		///     seeker.StartPath(transform.position, transform.position + Vector3.forward * 10, OnPathComplete);
		///
		///     // Note that the path is NOT calculated at this point
		///     // It has just been queued for calculation
		/// }
		///
		/// void OnPathComplete (Path path) {
		///     // The path is now calculated!
		///
		///     if (path.error) {
		///         Debug.LogError("Path failed: " + path.errorLog);
		///         return;
		///     }
		///
		///     // Cast the path to the path type we were using
		///     var abPath = path as ABPath;
		///
		///     // Draw the path in the scene view for 10 seconds
		///     for (int i = 0; i < abPath.vectorPath.Count - 1; i++) {
		///         Debug.DrawLine(abPath.vectorPath[i], abPath.vectorPath[i+1], Color.red, 10);
		///     }
		/// }
		/// </code>
		/// </summary>
		/// <param name="start">The start point of the path</param>
		/// <param name="end">The end point of the path</param>
		/// <param name="callback">The function to call when the path has been calculated. If you don't want a callback (e.g. if you instead poll path.IsDone or use a similar method) you can set this to null.</param>
		public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback) {
			return StartPath(ABPath.Construct(start, end, null), callback);
		}

		/// <summary>
		/// Queue a path to be calculated.
		///
		/// The callback will be called when the path has been calculated (which may be several frames into the future).
		/// Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed)
		///
		/// <code>
		/// // Schedule a path search that will only start searching the graphs with index 0 and 3
		/// seeker.StartPath(startPoint, endPoint, null, 1 << 0 | 1 << 3);
		/// </code>
		/// </summary>
		/// <param name="start">The start point of the path</param>
		/// <param name="end">The end point of the path</param>
		/// <param name="callback">The function to call when the path has been calculated. If you don't want a callback (e.g. if you instead poll path.IsDone or use a similar method) you can set this to null.</param>
		/// <param name="graphMask">Mask used to specify which graphs should be searched for close nodes. See #Pathfinding.NNConstraint.graphMask. This will override the #graphMask on this Seeker.</param>
		public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback, GraphMask graphMask) {
			return StartPath(ABPath.Construct(start, end, null), callback, graphMask);
		}

		/// <summary>
		/// Queue a path to be calculated.
		///
		/// Version: Since 4.1.x this method will no longer overwrite the graphMask on the path unless it is explicitly passed as a parameter (see other overloads of this method).
		///
		/// This overload takes no callback parameter. Instead, it is expected that you poll the path for completion, or block until it is completed.
		///
		/// See: <see cref="IsDone"/>
		/// See: <see cref="Path.WaitForPath"/>
		/// See: <see cref="Path.BlockUntilCalculated"/>
		///
		/// However, <see cref="Path.IsDone"/> should not be used with the Seeker component. This is because while the path itself may be calculated, the Seeker may not have had time to run post processing modifiers on the path yet.
		/// </summary>
		/// <param name="p">The path to start calculating</param>
		public Path StartPath (Path p) {
			return StartPath(p, null);
		}

		/// <summary>
		/// Queue a path to be calculated.
		///
		/// The callback will be called when the path has been calculated (which may be several frames into the future).
		/// The callback will not be called if a new path request is started before this path request has been calculated.
		/// </summary>
		/// <param name="p">The path to start calculating</param>
		/// <param name="callback">The function to call when the path has been calculated. If you don't want a callback (e.g. if you instead poll path.IsDone or use a similar method) you can set this to null.</param>
		public Path StartPath (Path p, OnPathDelegate callback) {
			// Set the graph mask only if the user has not changed it from the default value.
			// This is not perfect as the user may have wanted it to be precisely -1
			// however it is the best detection that I can do.
			// The non-default check is primarily for compatibility reasons to avoid breaking peoples existing code.
			// The StartPath overloads with an explicit graphMask field should be used instead to set the graphMask.
			if (p.nnConstraint.graphMask == -1) p.nnConstraint.graphMask = graphMask;
			StartPathInternal(p, callback);
			return p;
		}

		/// <summary>
		/// Queue a path to be calculated.
		///
		/// The callback will be called when the path has been calculated (which may be several frames into the future).
		/// The callback will not be called if a new path request is started before this path request has been calculated.
		/// </summary>
		/// <param name="p">The path to start calculating</param>
		/// <param name="callback">The function to call when the path has been calculated. If you don't want a callback (e.g. if you instead poll path.IsDone or use a similar method) you can set this to null.</param>
		/// <param name="graphMask">Mask used to specify which graphs should be searched for close nodes. See \reflink{GraphMask}.  This will override the #graphMask on this Seeker.</param>
		public Path StartPath (Path p, OnPathDelegate callback, GraphMask graphMask) {
			p.nnConstraint.graphMask = graphMask;
			StartPathInternal(p, callback);
			return p;
		}

		/// <summary>Internal method to start a path and mark it as the currently active path</summary>
		void StartPathInternal (Path p, OnPathDelegate callback) {
			var mtp = p as MultiTargetPath;
			if (mtp != null) {
				// TODO: Allocation, cache
				var callbacks = new OnPathDelegate[mtp.targetPoints.Length];

				for (int i = 0; i < callbacks.Length; i++) {
					callbacks[i] = onPartialPathDelegate;
				}

				mtp.callbacks = callbacks;
				p.callback += OnMultiPathComplete;
			} else {
				p.callback += onPathDelegate;
			}

			p.enabledTags = traversableTags;
			p.tagPenalties = tagPenalties;
			if (traversalProvider != null) p.traversalProvider = traversalProvider;

			// Cancel a previously requested path is it has not been processed yet and also make sure that it has not been recycled and used somewhere else
			if (path != null && path.PipelineState <= PathState.Processing && path.CompleteState != PathCompleteState.Error && lastPathID == path.pathID) {
				path.FailWithError("Canceled path because a new one was requested.\n"+
					"This happens when a new path is requested from the seeker when one was already being calculated.\n" +
					"For example if a unit got a new order, you might request a new path directly instead of waiting for the now" +
					" invalid path to be calculated. Which is probably what you want.\n" +
					"If you are getting this a lot, you might want to consider how you are scheduling path requests.");
				// No callback will be sent for the canceled path
			}

			// Set p as the active path
			path = p;
			tmpPathCallback = callback;

			// Save the path id so we can make sure that if we cancel a path (see above) it should not have been recycled yet.
			lastPathID = path.pathID;

			// Pre process the path
			RunModifiers(ModifierPass.PreProcess, path);

			// Send the request to the pathfinder
			AstarPath.StartPath(path);
		}

		/// <summary>
		/// Starts a Multi Target Path from one start point to multiple end points.
		/// A Multi Target Path will search for all the end points in one search and will return all paths if pathsForAll is true, or only the shortest one if pathsForAll is false.
		///
		/// callback will be called when the path has completed. Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed)
		///
		/// See: Pathfinding.MultiTargetPath
		/// See: MultiTargetPathExample.cs (view in online documentation for working links) "Example of how to use multi-target-paths"
		///
		/// <code>
		/// var endPoints = new Vector3[] {
		///     transform.position + Vector3.forward * 5,
		///     transform.position + Vector3.right * 10,
		///     transform.position + Vector3.back * 15
		/// };
		/// // Start a multi target path, where endPoints is a Vector3[] array.
		/// // The pathsForAll parameter specifies if a path to every end point should be searched for
		/// // or if it should only try to find the shortest path to any end point.
		/// var path = seeker.StartMultiTargetPath(transform.position, endPoints, pathsForAll: true, callback: null);
		/// path.BlockUntilCalculated();
		///
		/// if (path.error) {
		///     Debug.LogError("Error calculating path: " + path.errorLog);
		///     return;
		/// }
		///
		/// Debug.Log("The closest target was index " + path.chosenTarget);
		///
		/// // Draw the path to all targets
		/// foreach (var subPath in path.vectorPaths) {
		///     for (int i = 0; i < subPath.Count - 1; i++) {
		///         Debug.DrawLine(subPath[i], subPath[i+1], Color.green, 10);
		///     }
		/// }
		///
		/// // Draw the path to the closest target
		/// for (int i = 0; i < path.vectorPath.Count - 1; i++) {
		///     Debug.DrawLine(path.vectorPath[i], path.vectorPath[i+1], Color.red, 10);
		/// }
		/// </code>
		/// </summary>
		/// <param name="start">The start point of the path</param>
		/// <param name="endPoints">The end points of the path</param>
		/// <param name="pathsForAll">Indicates whether or not a path to all end points should be searched for or only to the closest one</param>
		/// <param name="callback">The function to call when the path has been calculated. If you don't want a callback (e.g. if you instead poll path.IsDone or use a similar method) you can set this to null.</param>
		/// <param name="graphMask">Mask used to specify which graphs should be searched for close nodes. See Pathfinding.NNConstraint.graphMask. This will override the #graphMask on this Seeker.</param>
		public MultiTargetPath StartMultiTargetPath (Vector3 start, Vector3[] endPoints, bool pathsForAll, OnPathDelegate callback, GraphMask graphMask) {
			MultiTargetPath p = MultiTargetPath.Construct(start, endPoints, null, null);

			p.pathsForAll = pathsForAll;
			StartPath(p, callback, graphMask);
			return p;
		}

		/// <summary>
		/// Starts a Multi Target Path from one start point to multiple end points.
		/// A Multi Target Path will search for all the end points in one search and will return all paths if pathsForAll is true, or only the shortest one if pathsForAll is false.
		///
		/// callback will be called when the path has completed. Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed)
		///
		/// See: Pathfinding.MultiTargetPath
		/// See: MultiTargetPathExample.cs (view in online documentation for working links) "Example of how to use multi-target-paths"
		///
		/// <code>
		/// var endPoints = new Vector3[] {
		///     transform.position + Vector3.forward * 5,
		///     transform.position + Vector3.right * 10,
		///     transform.position + Vector3.back * 15
		/// };
		/// // Start a multi target path, where endPoints is a Vector3[] array.
		/// // The pathsForAll parameter specifies if a path to every end point should be searched for
		/// // or if it should only try to find the shortest path to any end point.
		/// var path = seeker.StartMultiTargetPath(transform.position, endPoints, pathsForAll: true, callback: null);
		/// path.BlockUntilCalculated();
		///
		/// if (path.error) {
		///     Debug.LogError("Error calculating path: " + path.errorLog);
		///     return;
		/// }
		///
		/// Debug.Log("The closest target was index " + path.chosenTarget);
		///
		/// // Draw the path to all targets
		/// foreach (var subPath in path.vectorPaths) {
		///     for (int i = 0; i < subPath.Count - 1; i++) {
		///         Debug.DrawLine(subPath[i], subPath[i+1], Color.green, 10);
		///     }
		/// }
		///
		/// // Draw the path to the closest target
		/// for (int i = 0; i < path.vectorPath.Count - 1; i++) {
		///     Debug.DrawLine(path.vectorPath[i], path.vectorPath[i+1], Color.red, 10);
		/// }
		/// </code>
		/// </summary>
		/// <param name="start">The start point of the path</param>
		/// <param name="endPoints">The end points of the path</param>
		/// <param name="pathsForAll">Indicates whether or not a path to all end points should be searched for or only to the closest one</param>
		/// <param name="callback">The function to call when the path has been calculated. If you don't want a callback (e.g. if you instead poll path.IsDone or use a similar method) you can set this to null.</param>
		public MultiTargetPath StartMultiTargetPath(Vector3 start, Vector3[] endPoints, bool pathsForAll, OnPathDelegate callback) => StartMultiTargetPath(start, endPoints, pathsForAll, callback, graphMask);

		/// <summary>
		/// Starts a Multi Target Path from multiple start points to a single target point.
		/// A Multi Target Path will search from all start points to the target point in one search and will return all paths if pathsForAll is true, or only the shortest one if pathsForAll is false.
		///
		/// callback will be called when the path has completed. Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed)
		///
		/// See: Pathfinding.MultiTargetPath
		/// See: MultiTargetPathExample.cs (view in online documentation for working links) "Example of how to use multi-target-paths"
		/// </summary>
		/// <param name="startPoints">The start points of the path</param>
		/// <param name="end">The end point of the path</param>
		/// <param name="pathsForAll">Indicates whether or not a path from all start points should be searched for or only to the closest one</param>
		/// <param name="callback">The function to call when the path has been calculated. If you don't want a callback (e.g. if you instead poll path.IsDone or use a similar method) you can set this to null.</param>
		/// <param name="graphMask">Mask used to specify which graphs should be searched for close nodes. See Pathfinding.NNConstraint.graphMask. This will override the #graphMask on this Seeker.</param>
		public MultiTargetPath StartMultiTargetPath (Vector3[] startPoints, Vector3 end, bool pathsForAll, OnPathDelegate callback, GraphMask graphMask) {
			MultiTargetPath p = MultiTargetPath.Construct(startPoints, end, null, null);

			p.pathsForAll = pathsForAll;
			StartPath(p, callback, graphMask);
			return p;
		}

		/// <summary>
		/// Starts a Multi Target Path from multiple start points to a single target point.
		/// A Multi Target Path will search from all start points to the target point in one search and will return all paths if pathsForAll is true, or only the shortest one if pathsForAll is false.
		///
		/// callback will be called when the path has completed. Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed)
		///
		/// See: Pathfinding.MultiTargetPath
		/// See: MultiTargetPathExample.cs (view in online documentation for working links) "Example of how to use multi-target-paths"
		/// </summary>
		/// <param name="startPoints">The start points of the path</param>
		/// <param name="end">The end point of the path</param>
		/// <param name="pathsForAll">Indicates whether or not a path from all start points should be searched for or only to the closest one</param>
		/// <param name="callback">The function to call when the path has been calculated. If you don't want a callback (e.g. if you instead poll path.IsDone or use a similar method) you can set this to null.</param>
		public MultiTargetPath StartMultiTargetPath(Vector3[] startPoints, Vector3 end, bool pathsForAll, OnPathDelegate callback) => StartMultiTargetPath(startPoints, end, pathsForAll, callback, graphMask);

#if UNITY_EDITOR
		/// <summary>Draws gizmos for the Seeker</summary>
		public override void DrawGizmos () {
			if (lastCompletedNodePath == null || !drawGizmos) {
				return;
			}

			if (detailedGizmos && lastCompletedNodePath != null) {
				using (Draw.WithColor(new Color(0.7F, 0.5F, 0.1F, 0.5F))) {
					for (int i = 0; i < lastCompletedNodePath.Count-1; i++) {
						Draw.Line((Vector3)lastCompletedNodePath[i].position, (Vector3)lastCompletedNodePath[i+1].position);
					}
				}
			}

			if (lastCompletedVectorPath != null) {
				using (Draw.WithColor(new Color(0, 1F, 0, 1F))) {
					for (int i = 0; i < lastCompletedVectorPath.Count-1; i++) {
						Draw.Line(lastCompletedVectorPath[i], lastCompletedVectorPath[i+1]);
					}
				}
			}
		}
#endif

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			if (graphMaskCompatibility != -1) {
				graphMask = graphMaskCompatibility;
				graphMaskCompatibility = -1;
			}
			base.OnUpgradeSerializedData(ref migrations, unityThread);
		}
	}
}

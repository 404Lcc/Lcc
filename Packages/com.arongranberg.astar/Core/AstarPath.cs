using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Drawing;
using UnityEngine.Profiling;
using Pathfinding.Util;
using Pathfinding.Graphs.Navmesh;
using Pathfinding.Graphs.Util;
using Pathfinding.Jobs;
using Pathfinding.Collections;
using Pathfinding.Sync;
using Unity.Jobs;

#if NETFX_CORE
using Thread = Pathfinding.WindowsStore.Thread;
#else
using Thread = System.Threading.Thread;
#endif

[ExecuteInEditMode]
[AddComponentMenu("Pathfinding/AstarPath")]
[DisallowMultipleComponent]
/// <summary>
/// Core component for the A* Pathfinding System.
/// This class handles all of the pathfinding system, calculates all paths and stores the info.
/// This class is a singleton class, meaning there should only exist at most one active instance of it in the scene.
/// It might be a bit hard to use directly, usually interfacing with the pathfinding system is done through the <see cref="Pathfinding.Seeker"/> class.
/// </summary>
[HelpURL("https://arongranberg.com/astar/documentation/stable/astarpath.html")]
public class AstarPath : VersionedMonoBehaviour {
	/// <summary>The version number for the A* Pathfinding Project</summary>
	public static readonly System.Version Version = new System.Version(5, 3, 8);

	/// <summary>Information about where the package was downloaded</summary>
	public enum AstarDistribution { WebsiteDownload, AssetStore, PackageManager };

	/// <summary>Used by the editor to guide the user to the correct place to download updates</summary>
	public static readonly AstarDistribution Distribution = AstarDistribution.AssetStore;

	/// <summary>
	/// Which branch of the A* Pathfinding Project is this release.
	/// Used when checking for updates so that users of the development
	/// versions can get notifications of development updates.
	/// </summary>
	public static readonly string Branch = "master";

	/// <summary>Holds all graph data</summary>
	[UnityEngine.Serialization.FormerlySerializedAs("astarData")]
	public AstarData data;

	/// <summary>
	/// Returns the active AstarPath object in the scene.
	/// Note: This is only set if the AstarPath object has been initialized (which happens in Awake).
	/// </summary>
	public static AstarPath active;

	/// <summary>Shortcut to <see cref="AstarData.graphs"/></summary>
	public NavGraph[] graphs => data.graphs;

	bool hasScannedGraphAtStartup = false;

	#region InspectorDebug
	/// <summary>
	/// Visualize graphs in the scene view (editor only).
	/// [Open online documentation to see images]
	/// </summary>
	public bool showNavGraphs = true;

	/// <summary>
	/// Toggle to show unwalkable nodes.
	///
	/// Note: Only relevant in the editor
	///
	/// See: <see cref="unwalkableNodeDebugSize"/>
	/// </summary>
	public bool showUnwalkableNodes = true;

	/// <summary>
	/// The mode to use for drawing nodes in the sceneview.
	///
	/// Note: Only relevant in the editor
	///
	/// See: <see cref="GraphDebugMode"/>
	/// </summary>
	public GraphDebugMode debugMode;

	/// <summary>
	/// Low value to use for certain <see cref="debugMode"/> modes.
	/// For example if <see cref="debugMode"/> is set to G, this value will determine when the node will be completely red.
	///
	/// Note: Only relevant in the editor
	///
	/// See: <see cref="debugRoof"/>
	/// See: <see cref="debugMode"/>
	/// </summary>
	public float debugFloor = 0;

	/// <summary>
	/// High value to use for certain <see cref="debugMode"/> modes.
	/// For example if <see cref="debugMode"/> is set to G, this value will determine when the node will be completely green.
	///
	/// For the penalty debug mode, the nodes will be colored green when they have a penalty less than <see cref="debugFloor"/> and red
	/// when their penalty is greater or equal to this value and something between red and green otherwise.
	///
	/// Note: Only relevant in the editor
	///
	/// See: <see cref="debugFloor"/>
	/// See: <see cref="debugMode"/>
	/// </summary>
	public float debugRoof = 20000;

	/// <summary>
	/// If set, the <see cref="debugFloor"/> and <see cref="debugRoof"/> values will not be automatically recalculated.
	///
	/// Note: Only relevant in the editor
	/// </summary>
	public bool manualDebugFloorRoof = false;


	/// <summary>
	/// If enabled, nodes will draw a line to their 'parent'.
	/// This will show the search tree for the latest path.
	///
	/// Note: Only relevant in the editor
	/// </summary>
	public bool showSearchTree = false;

	/// <summary>
	/// Size of the red cubes shown in place of unwalkable nodes.
	///
	/// Note: Only relevant in the editor. Does not apply to grid graphs.
	/// See: <see cref="showUnwalkableNodes"/>
	/// </summary>
	public float unwalkableNodeDebugSize = 0.3F;

	/// <summary>
	/// The amount of debugging messages.
	/// Use less debugging to improve performance (a bit) or just to get rid of the Console spamming.
	/// Use more debugging (heavy) if you want more information about what the pathfinding scripts are doing.
	/// The InGame option will display the latest path log using in-game GUI.
	///
	/// [Open online documentation to see images]
	/// </summary>
	public PathLog logPathResults = PathLog.Normal;

	#endregion

	#region InspectorSettings
	/// <summary>
	/// Maximum distance to search for nodes.
	/// When searching for the nearest node to a point, this is the limit (in world units) for how far away it is allowed to be.
	///
	/// This is relevant if you try to request a path to a point that cannot be reached and it thus has to search for
	/// the closest node to that point which can be reached (which might be far away). If it cannot find a node within this distance
	/// then the path will fail.
	///
	/// [Open online documentation to see images]
	///
	/// See: <see cref="NNConstraint.constrainDistance"/>
	/// </summary>
	public float maxNearestNodeDistance = 100;

	/// <summary>
	/// Max Nearest Node Distance Squared.
	/// See: <see cref="maxNearestNodeDistance"/>
	/// </summary>
	public float maxNearestNodeDistanceSqr => maxNearestNodeDistance*maxNearestNodeDistance;

	/// <summary>
	/// If true, all graphs will be scanned when the game starts, during OnEnable.
	/// If you disable this, you will have to call <see cref="AstarPath.active.Scan"/> yourself to enable pathfinding.
	/// Alternatively you could load a saved graph from a file.
	///
	/// If a startup cache has been generated (see save-load-graphs) (view in online documentation for working links), it always takes priority, and the graphs will be loaded from the cache instead of scanned.
	///
	/// This can be useful to disable if you want to scan your graphs asynchronously, or if you have a procedural world which has not been created yet
	/// at the start of the game.
	///
	/// See: <see cref="Scan"/>
	/// See: <see cref="ScanAsync"/>
	/// </summary>
	public bool scanOnStartup = true;

	/// <summary>
	/// Do a full GetNearest search for all graphs.
	/// Additional searches will normally only be done on the graph which in the first fast search seemed to have the closest node.
	/// With this setting on, additional searches will be done on all graphs since the first check is not always completely accurate.
	/// More technically: GetNearestForce on all graphs will be called if true, otherwise only on the one graph which's GetNearest search returned the best node.
	/// Usually faster when disabled, but higher quality searches when enabled.
	/// Note: For the PointGraph this setting doesn't matter much as it has only one search mode.
	/// </summary>
	[System.Obsolete("This setting has been removed. It is now always true", true)]
	public bool fullGetNearestSearch = false;

	/// <summary>
	/// Prioritize graphs.
	/// Graphs will be prioritized based on their order in the inspector.
	/// The first graph which has a node closer than <see cref="prioritizeGraphsLimit"/> will be chosen instead of searching all graphs.
	///
	/// Deprecated: This setting has been removed. It was always a bit of a hack. Use NNConstraint.graphMask if you want to choose which graphs are searched.
	/// </summary>
	[System.Obsolete("This setting has been removed. It was always a bit of a hack. Use NNConstraint.graphMask if you want to choose which graphs are searched.", true)]
	public bool prioritizeGraphs = false;

	/// <summary>
	/// Distance limit for <see cref="prioritizeGraphs"/>.
	/// See: <see cref="prioritizeGraphs"/>
	///
	/// Deprecated: This setting has been removed. It was always a bit of a hack. Use NNConstraint.graphMask if you want to choose which graphs are searched.
	/// </summary>
	[System.Obsolete("This setting has been removed. It was always a bit of a hack. Use NNConstraint.graphMask if you want to choose which graphs are searched.", true)]
	public float prioritizeGraphsLimit = 1F;

	/// <summary>
	/// Reference to the color settings for this AstarPath object.
	/// Color settings include for example which color the nodes should be in, in the sceneview.
	/// </summary>
	public AstarColor colorSettings;

	/// <summary>
	/// Stored tag names.
	/// See: AstarPath.FindTagNames
	/// See: AstarPath.GetTagNames
	/// </summary>
	[SerializeField]
	protected string[] tagNames = null;

	/// <summary>
	/// The distance function to use as a heuristic.
	/// The heuristic, often referred to as just 'H' is the estimated cost from a node to the target.
	/// Different heuristics affect how the path picks which one to follow from multiple possible with the same length
	/// See: <see cref="Pathfinding.Heuristic"/> for more details and descriptions of the different modes.
	/// See: <a href="https://en.wikipedia.org/wiki/Admissible_heuristic">Wikipedia: Admissible heuristic</a>
	/// See: <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">Wikipedia: A* search algorithm</a>
	/// See: <a href="https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm">Wikipedia: Dijkstra's Algorithm</a>
	///
	/// Warning: Reducing the heuristic scale below 1, or disabling the heuristic, can significantly increase the cpu cost for pathfinding, especially for large graphs.
	/// </summary>
	public Heuristic heuristic = Heuristic.Euclidean;

	/// <summary>
	/// The scale of the heuristic.
	/// If a value lower than 1 is used, the pathfinder will search more nodes (slower).
	/// If 0 is used, the pathfinding algorithm will be reduced to dijkstra's algorithm. This is equivalent to setting <see cref="heuristic"/> to None.
	/// If a value larger than 1 is used the pathfinding will (usually) be faster because it expands fewer nodes, but the paths may no longer be the optimal (i.e the shortest possible paths).
	///
	/// Usually you should leave this to the default value of 1.
	///
	/// Warning: Reducing the heuristic scale below 1, or disabling the heuristic, can significantly increase the cpu cost for pathfinding, especially for large graphs.
	///
	/// See: <a href="https://en.wikipedia.org/wiki/Admissible_heuristic">Wikipedia: Admissible heuristic</a>
	/// See: <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">Wikipedia: A* search algorithm</a>
	/// See: <a href="https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm">Wikipedia: Dijkstra's Algorithm</a>
	/// </summary>
	public float heuristicScale = 1F;

	/// <summary>
	/// Number of pathfinding threads to use.
	/// Multithreading puts pathfinding in another thread, this is great for performance on 2+ core computers since the framerate will barely be affected by the pathfinding at all.
	/// - None indicates that the pathfinding is run in the Unity thread as a coroutine
	/// - Automatic will try to adjust the number of threads to the number of cores and memory on the computer.
	///  Less than 512mb of memory or a single core computer will make it revert to using no multithreading.
	///
	/// It is recommended that you use one of the "Auto" settings that are available.
	/// The reason is that even if your computer might be beefy and have 8 cores.
	/// Other computers might only be quad core or dual core in which case they will not benefit from more than
	/// 1 or 3 threads respectively (you usually want to leave one core for the unity thread).
	/// If you use more threads than the number of cores on the computer it is mostly just wasting memory, it will not run any faster.
	/// The extra memory usage is not trivially small. Each thread needs to keep a small amount of data for each node in all the graphs.
	/// It is not the full graph data but it is proportional to the number of nodes.
	/// The automatic settings will inspect the machine it is running on and use that to determine the number of threads so that no memory is wasted.
	///
	/// The exception is if you only have one (or maybe two characters) active at time. Then you should probably just go with one thread always since it is very unlikely
	/// that you will need the extra throughput given by more threads. Keep in mind that more threads primarily increases throughput by calculating different paths on different
	/// threads, it will not calculate individual paths any faster.
	///
	/// Warning: If you are modifying the pathfinding core scripts or if you are directly modifying graph data without using any of the
	/// safe wrappers (like <see cref="AddWorkItem)"/>, multithreading can cause strange errors and cause pathfinding to stop working if you are not careful.
	///
	/// Note: WebGL does not support threads at all (since javascript is single-threaded) so no threads will be used on that platform.
	///
	/// Note: This setting only applies to pathfinding. Graph updates use the Unity Job System, which uses a different thread pool.
	///
	/// See: CalculateThreadCount
	/// </summary>
	public ThreadCount threadCount = ThreadCount.One;

	/// <summary>
	/// Max number of milliseconds to spend on pathfinding during each frame.
	/// At least 500 nodes will be searched each frame (if there are that many to search).
	/// When using multithreading this value is irrelevant.
	/// </summary>
	public float maxFrameTime = 1F;

	/// <summary>
	/// Throttle graph updates and batch them to improve performance.
	/// If toggled, graph updates will batched and executed less often (specified by <see cref="graphUpdateBatchingInterval)"/>.
	///
	/// This can have a positive impact on pathfinding throughput since the pathfinding threads do not need
	/// to be stopped as often, and it reduces the overhead per graph update.
	/// All graph updates are still applied, they are just batched together so that more of them are
	/// applied at the same time.
	///
	/// Do not use this if you want minimal latency between a graph update being requested
	/// and it being applied.
	///
	/// This only applies to graph updates requested using the <see cref="UpdateGraphs"/> method. Not those requested
	/// using <see cref="AddWorkItem"/>.
	///
	/// If you want to apply graph updates immediately at some point, you can call <see cref="FlushGraphUpdates"/>.
	///
	/// See: graph-updates (view in online documentation for working links)
	/// </summary>
	public bool batchGraphUpdates = false;

	/// <summary>
	/// Minimum number of seconds between each batch of graph updates.
	/// If <see cref="batchGraphUpdates"/> is true, this defines the minimum number of seconds between each batch of graph updates.
	///
	/// This can have a positive impact on pathfinding throughput since the pathfinding threads do not need
	/// to be stopped as often, and it reduces the overhead per graph update.
	/// All graph updates are still applied however, they are just batched together so that more of them are
	/// applied at the same time.
	///
	/// Do not use this if you want minimal latency between a graph update being requested
	/// and it being applied.
	///
	/// This only applies to graph updates requested using the <see cref="UpdateGraphs"/> method. Not those requested
	/// using <see cref="AddWorkItem"/>.
	///
	/// See: graph-updates (view in online documentation for working links)
	/// </summary>
	public float graphUpdateBatchingInterval = 0.2F;

	#endregion

	#region DebugVariables
#if ProfileAstar
	/// <summary>
	/// How many paths has been computed this run. From application start.
	/// Debugging variable
	/// </summary>
	public static int PathsCompleted = 0;

	public static System.Int64 TotalSearchedNodes = 0;
	public static System.Int64 TotalSearchTime = 0;
#endif

	/// <summary>The time it took for the last call to <see cref="Scan"/> to complete</summary>
	public float lastScanTime { get; private set; }

	/// <summary>
	/// The path to debug using gizmos.
	/// This is the path handler used to calculate the last path.
	/// It is used in the editor to draw debug information using gizmos.
	/// </summary>
	[System.NonSerialized]
	internal PathHandler debugPathData;

	/// <summary>The path ID to debug using gizmos</summary>
	[System.NonSerialized]
	internal ushort debugPathID;

	/// <summary>
	/// Debug string from the last completed path.
	/// Will be updated if <see cref="logPathResults"/> == PathLog.InGame
	/// </summary>
	string inGameDebugPath;

	#endregion

	#region StatusVariables

	/// <summary>
	/// True while any graphs are being scanned.
	///
	/// This is primarily relevant when scanning graph asynchronously.
	///
	/// Note: Not to be confused with graph updates.
	///
	/// Note: This will be false during <see cref="OnLatePostScan"/> and during the <see cref="GraphModifier.EventType"/>.LatePostScan event.
	///
	/// See: IsAnyGraphUpdateQueued
	/// See: IsAnyGraphUpdateInProgress
	/// </summary>
	[field: System.NonSerialized]
	public bool isScanning { get; private set; }

	/// <summary>
	/// Number of parallel pathfinders.
	/// Returns the number of concurrent processes which can calculate paths at once.
	/// When using multithreading, this will be the number of threads, if not using multithreading it is always 1 (since only 1 coroutine is used).
	/// See: IsUsingMultithreading
	/// </summary>
	public int NumParallelThreads => pathProcessor.NumThreads;

	/// <summary>
	/// Returns whether or not multithreading is used.
	/// \exception System.Exception Is thrown when it could not be decided if multithreading was used or not.
	/// This should not happen if pathfinding is set up correctly.
	/// Note: This uses info about if threads are running right now, it does not use info from the settings on the A* object.
	/// </summary>
	public bool IsUsingMultithreading => pathProcessor.IsUsingMultithreading;

	/// <summary>
	/// Returns if any graph updates are waiting to be applied.
	/// Note: This is false while the updates are being performed.
	/// Note: This does *not* includes other types of work items such as navmesh cutting or anything added by <see cref="AddWorkItem"/>.
	/// </summary>
	public bool IsAnyGraphUpdateQueued => graphUpdates.IsAnyGraphUpdateQueued;

	/// <summary>
	/// Returns if any graph updates are being calculated right now.
	/// Note: This does *not* includes other types of work items such as navmesh cutting or anything added by <see cref="AddWorkItem"/>.
	///
	/// See: IsAnyWorkItemInProgress
	/// </summary>
	public bool IsAnyGraphUpdateInProgress => graphUpdates.IsAnyGraphUpdateInProgress;

	/// <summary>
	/// Returns if any work items are in progress right now.
	/// Note: This includes pretty much all types of graph updates.
	/// Such as normal graph updates, navmesh cutting and anything added by <see cref="AddWorkItem"/>.
	/// </summary>
	public bool IsAnyWorkItemInProgress => workItems.workItemsInProgress;

	/// <summary>
	/// Returns if this code is currently being exectuted inside a work item.
	/// Note: This includes pretty much all types of graph updates.
	/// Such as normal graph updates, navmesh cutting and anything added by <see cref="AddWorkItem"/>.
	///
	/// In contrast to <see cref="IsAnyWorkItemInProgress"/> this is only true when work item code is being executed, it is not
	/// true in-between the updates to a work item that takes several frames to complete.
	/// </summary>
	internal bool IsInsideWorkItem => workItems.workItemsInProgressRightNow;

	#endregion

	#region Callbacks
	/// <summary>
	/// Called on Awake before anything else is done.
	/// This is called at the start of the Awake call, right after <see cref="active"/> has been set, but this is the only thing that has been done.
	/// Use this when you want to set up default settings for an AstarPath component created during runtime since some settings can only be changed in Awake
	/// (such as multithreading related stuff)
	/// <code>
	/// // Create a new AstarPath object on Start and apply some default settings
	/// public void Start () {
	///     AstarPath.OnAwakeSettings += ApplySettings;
	///     AstarPath astar = gameObject.AddComponent<AstarPath>();
	/// }
	///
	/// public void ApplySettings () {
	///     // Unregister from the delegate
	///     AstarPath.OnAwakeSettings -= ApplySettings;
	///     // For example threadCount should not be changed after the Awake call
	///     // so here's the only place to set it if you create the component during runtime
	///     AstarPath.active.threadCount = ThreadCount.One;
	/// }
	/// </code>
	/// </summary>
	public static System.Action OnAwakeSettings;

	/// <summary>Called for each graph before they are scanned. In most cases it is recommended to create a custom class which inherits from Pathfinding.GraphModifier instead.</summary>
	public static OnGraphDelegate OnGraphPreScan;

	/// <summary>Called for each graph after they have been scanned. All other graphs might not have been scanned yet. In most cases it is recommended to create a custom class which inherits from Pathfinding.GraphModifier instead.</summary>
	public static OnGraphDelegate OnGraphPostScan;

	/// <summary>Called for each path before searching. Be careful when using multithreading since this will be called from a different thread.</summary>
	public static OnPathDelegate OnPathPreSearch;

	/// <summary>Called for each path after searching. Be careful when using multithreading since this will be called from a different thread.</summary>
	public static OnPathDelegate OnPathPostSearch;

	/// <summary>Called before starting the scanning. In most cases it is recommended to create a custom class which inherits from Pathfinding.GraphModifier instead.</summary>
	public static OnScanDelegate OnPreScan;

	/// <summary>Called after scanning. This is called before applying links, flood-filling the graphs and other post processing. In most cases it is recommended to create a custom class which inherits from Pathfinding.GraphModifier instead.</summary>
	public static OnScanDelegate OnPostScan;

	/// <summary>Called after scanning has completed fully. This is called as the last thing in the Scan function. In most cases it is recommended to create a custom class which inherits from Pathfinding.GraphModifier instead.</summary>
	public static OnScanDelegate OnLatePostScan;

	/// <summary>Called when any graphs are updated. Register to for example recalculate the path whenever a graph changes. In most cases it is recommended to create a custom class which inherits from Pathfinding.GraphModifier instead.</summary>
	public static OnScanDelegate OnGraphsUpdated;

	/// <summary>
	/// Called when pathID overflows 65536 and resets back to zero.
	/// Note: This callback will be cleared every time it is called, so if you want to register to it repeatedly, register to it directly on receiving the callback as well.
	/// </summary>
	public static System.Action On65KOverflow;

	/// <summary>
	/// Called right after callbacks on paths have been called.
	///
	/// A path's callback function runs on the main thread when the path has been calculated.
	/// This is done in batches for all paths that have finished their calculation since the last frame.
	/// This event will trigger right after a batch of callbacks have been called.
	///
	/// If you do not want to use individual path callbacks, you can use this instead to poll all pending paths
	/// and see which ones have completed. This is better than doing it in e.g. the Update loop, because
	/// here you will have a guarantee that all calculated paths are still valid.
	/// Immediately after this callback has finished, other things may invalidate calculated paths, like for example
	/// graph updates.
	///
	/// This is used by the ECS integration to update all entities' pending paths, without having to store
	/// a callback for each agent, and also to avoid the ECS synchronization overhead that having individual
	/// callbacks would entail.
	/// </summary>
	public static System.Action OnPathsCalculated;

	#endregion

	#region MemoryStructures

	/// <summary>Processes graph updates</summary>
	readonly GraphUpdateProcessor graphUpdates;

	/// <summary>Holds a hierarchical graph to speed up some queries like if there is a path between two nodes</summary>
	internal readonly HierarchicalGraph hierarchicalGraph;

	/// <summary>Holds all active off-mesh links</summary>
	public readonly OffMeshLinks offMeshLinks;

	/// <summary>
	/// Handles navmesh cuts.
	/// See: <see cref="Pathfinding.NavmeshCut"/>
	/// </summary>
	public NavmeshUpdates navmeshUpdates = new NavmeshUpdates();

	/// <summary>Processes work items</summary>
	readonly WorkItemProcessor workItems;

	/// <summary>Holds all paths waiting to be calculated and calculates them</summary>
	readonly PathProcessor pathProcessor;

	/// <summary>Holds global node data that cannot be stored in individual graphs</summary>
	internal GlobalNodeStorage nodeStorage;

	/// <summary>
	/// Global read-write lock for graph data.
	///
	/// Graph data is always consistent from the main-thread's perspective, but if you are using jobs to read from graph data, you may need this.
	///
	/// A write lock is held automatically...
	/// - During graph updates. During async graph updates, the lock is only held once per frame while the graph update is actually running, not for the whole duration.
	/// - During work items. Async work items work similarly to graph updates, the lock is only held once per frame while the work item is actually running.
	/// - When <see cref="GraphModifier"/> events run.
	/// - When graph related callbacks, such as <see cref="OnGraphsUpdated"/>, run.
	/// - During the last step of a graph's scanning process. See <see cref="ScanningStage"/>.
	///
	/// To use e.g. AstarPath.active.GetNearest from an ECS job, you'll need to acquire a read lock first, and make sure the lock is only released when the job is finished.
	///
	/// <code>
	/// var readLock = AstarPath.active.LockGraphDataForReading();
	/// var handle = new MyJob {
	///     // ...
	/// }.Schedule(readLock.dependency);
	/// readLock.UnlockAfter(handle);
	/// </code>
	///
	/// See: <see cref="LockGraphDataForReading"/>
	/// </summary>
	RWLock graphDataLock = new RWLock();

	bool graphUpdateRoutineRunning = false;

	/// <summary>Makes sure QueueGraphUpdates will not queue multiple graph update orders</summary>
	bool graphUpdatesWorkItemAdded = false;

	/// <summary>
	/// Time the last graph update was done.
	/// Used to group together frequent graph updates to batches
	/// </summary>
	float lastGraphUpdate = -9999F;

	/// <summary>Held if any work items are currently queued</summary>
	PathProcessor.GraphUpdateLock workItemLock;

	/// <summary>Holds all completed paths waiting to be returned to where they were requested</summary>
	internal readonly PathReturnQueue pathReturnQueue;

	/// <summary>
	/// Holds settings for heuristic optimization.
	/// See: heuristic-opt (view in online documentation for working links)
	/// </summary>
	public EuclideanEmbedding euclideanEmbedding = new EuclideanEmbedding();

	/// <summary>
	/// If an async scan is running, this will be set to the coroutine.
	///
	/// This primarily used to be able to force the async scan to complete immediately,
	/// if the AstarPath component should happen to be destroyed while an async scan is running.
	/// </summary>
	IEnumerator<Progress> asyncScanTask;

	#endregion

	/// <summary>
	/// Shows or hides graph inspectors.
	/// Used internally by the editor
	/// </summary>
	public bool showGraphs = false;

	/// <summary>
	/// The next unused Path ID.
	/// Incremented for every call to GetNextPathID
	/// </summary>
	private ushort nextFreePathID = 1;

	private AstarPath () {
		pathReturnQueue = new PathReturnQueue(this, () => {
			if (OnPathsCalculated != null) OnPathsCalculated();
		});

		// Make sure that the pathProcessor and node storage is never null
		nodeStorage = new GlobalNodeStorage(this);
		hierarchicalGraph = new HierarchicalGraph(nodeStorage);
		pathProcessor = new PathProcessor(this, pathReturnQueue, 1, false);
		offMeshLinks = new OffMeshLinks(this);

		workItems = new WorkItemProcessor(this);
		graphUpdates = new GraphUpdateProcessor(this);
		navmeshUpdates.astar = this;
		data = new AstarData(this);

		// Forward graphUpdates.OnGraphsUpdated to AstarPath.OnGraphsUpdated
		workItems.OnGraphsUpdated += () => {
			if (OnGraphsUpdated != null) {
				try {
					OnGraphsUpdated(this);
				} catch (System.Exception e) {
					Debug.LogException(e);
				}
			}
		};

		pathProcessor.OnPathPreSearch += path => {
			var tmp = OnPathPreSearch;
			if (tmp != null) tmp(path);
		};

		pathProcessor.OnPathPostSearch += path => {
			LogPathResults(path);
			var tmp = OnPathPostSearch;
			if (tmp != null) tmp(path);
		};

		// Sent every time the path queue is unblocked
		pathProcessor.OnQueueUnblocked += () => {
			if (euclideanEmbedding.dirty) {
				euclideanEmbedding.RecalculateCosts();
			}
		};
	}

	/// <summary>
	/// Returns tag names.
	/// Makes sure that the tag names array is not null and of length 32.
	/// If it is null or not of length 32, it creates a new array and fills it with 0,1,2,3,4 etc...
	/// See: AstarPath.FindTagNames
	/// </summary>
	public string[] GetTagNames () {
		if (tagNames == null || tagNames.Length != 32) {
			tagNames = new string[32];
			for (int i = 0; i < tagNames.Length; i++) {
				tagNames[i] = ""+i;
			}
			tagNames[0] = "Basic Ground";
		}
		return tagNames;
	}

	/// <summary>
	/// Used outside of play mode to initialize the AstarPath object even if it has not been selected in the inspector yet.
	/// This will set the <see cref="active"/> property and deserialize all graphs.
	///
	/// This is useful if you want to do changes to the graphs in the editor outside of play mode, but cannot be sure that the graphs have been deserialized yet.
	/// In play mode this method does nothing.
	/// </summary>
	public static void FindAstarPath () {
		if (Application.isPlaying) return;
		if (active == null) active = UnityCompatibility.FindAnyObjectByType<AstarPath>();
		if (active != null && (active.data.graphs == null || active.data.graphs.Length == 0)) active.data.DeserializeGraphs();
	}

	/// <summary>
	/// Tries to find an AstarPath object and return tag names.
	/// If an AstarPath object cannot be found, it returns an array of length 1 with an error message.
	/// See: AstarPath.GetTagNames
	/// </summary>
	public static string[] FindTagNames () {
		FindAstarPath();
		return active != null? active.GetTagNames () : new string[1] { "There is no AstarPath component in the scene" };
	}

	/// <summary>Returns the next free path ID</summary>
	internal ushort GetNextPathID () {
		if (nextFreePathID == 0) {
			nextFreePathID++;

			if (On65KOverflow != null) {
				System.Action tmp = On65KOverflow;
				On65KOverflow = null;
				tmp();
			}
		}
		return nextFreePathID++;
	}

	void RecalculateDebugLimits () {
#if UNITY_EDITOR
		debugFloor = float.PositiveInfinity;
		debugRoof = float.NegativeInfinity;

		bool ignoreSearchTree = !showSearchTree || debugPathData == null;
		UnsafeSpan<GlobalNodeStorage.DebugPathNode> debugPathNodes;
		if (debugPathData != null && debugPathData.threadID < active.nodeStorage.pathfindingThreadData.Length) debugPathNodes = active.nodeStorage.pathfindingThreadData[debugPathData.threadID].debugPathNodes;
		else debugPathNodes = default;

		for (int i = 0; i < graphs.Length; i++) {
			if (graphs[i] != null && graphs[i].drawGizmos) {
				graphs[i].GetNodes(node => {
					if (node.Walkable && (ignoreSearchTree || Pathfinding.Util.GraphGizmoHelper.InSearchTree(node, debugPathNodes, debugPathID))) {
						float value;
						if (debugMode == GraphDebugMode.Penalty) {
							value = node.Penalty;
						} else if (debugPathNodes.Length > 0) {
							var rnode = debugPathNodes[node.NodeIndex];
							switch (debugMode) {
							case GraphDebugMode.F:
								value = rnode.g + rnode.h;
								break;
							case GraphDebugMode.G:
								value = rnode.g;
								break;
							default:
							case GraphDebugMode.H:
								value = rnode.h;
								break;
							}
						} else {
							value = 0;
						}
						debugFloor = Mathf.Min(debugFloor, value);
						debugRoof = Mathf.Max(debugRoof, value);
					}
				});
			}
		}

		if (float.IsInfinity(debugFloor)) {
			debugFloor = 0;
			debugRoof = 1;
		}

		// Make sure they are not identical, that will cause the color interpolation to fail
		if (debugRoof-debugFloor < 1) debugRoof += 1;
#else
		debugFloor = 0;
		debugRoof = 1;
#endif
	}

	RedrawScope redrawScope;

	/// <summary>Calls OnDrawGizmos on all graphs</summary>
	public override void DrawGizmos () {
		if (active != this || graphs == null) {
			return;
		}

		InitializeColors();

		if (!redrawScope.isValid) redrawScope = DrawingManager.GetRedrawScope(gameObject);

		if (!workItems.workItemsInProgress && !isScanning) {
			// When updating graphs, graph info might not be valid,
			// and we cannot render anything during those frames.
			// Therefore we use a redraw scope which will continue drawing
			// until we dispose it.
			redrawScope.Rewind();
			if (showNavGraphs && !manualDebugFloorRoof) {
				RecalculateDebugLimits();
			}

			Profiler.BeginSample("Graph.OnDrawGizmos");
			// Loop through all graphs and draw their gizmos
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null && graphs[i].drawGizmos)
					graphs[i].OnDrawGizmos(DrawingManager.instance.gizmos, showNavGraphs, redrawScope);
			}
			Profiler.EndSample();

			if (showNavGraphs) {
				euclideanEmbedding.OnDrawGizmos();
				if (debugMode == GraphDebugMode.HierarchicalNode) hierarchicalGraph.OnDrawGizmos(DrawingManager.instance.gizmos, redrawScope);
				if (debugMode == GraphDebugMode.NavmeshBorderObstacles) hierarchicalGraph.navmeshEdges.OnDrawGizmos(DrawingManager.instance.gizmos, redrawScope);
			}
		}
	}

#if !ASTAR_NO_GUI
	/// <summary>
	/// Draws the InGame debugging (if enabled)
	/// See: <see cref="logPathResults"/> PathLog
	/// </summary>
	private void OnGUI () {
		if (logPathResults == PathLog.InGame && inGameDebugPath != "") {
			GUI.Label(new Rect(5, 5, 400, 600), inGameDebugPath);
		}
	}
#endif

	/// <summary>
	/// Prints path results to the log. What it prints can be controled using <see cref="logPathResults"/>.
	/// See: <see cref="logPathResults"/>
	/// See: PathLog
	/// See: Pathfinding.Path.DebugString
	/// </summary>
	private void LogPathResults (Path path) {
		if (logPathResults != PathLog.None && (path.error || logPathResults != PathLog.OnlyErrors)) {
			string debug = (path as IPathInternals).DebugString(logPathResults);

			if (logPathResults == PathLog.InGame) {
				inGameDebugPath = debug;
			} else if (path.error) {
				Debug.LogWarning(debug);
			} else {
				Debug.Log(debug);
			}
		}
	}

	/// <summary>
	/// Checks if any work items need to be executed
	/// then runs pathfinding for a while (if not using multithreading because
	/// then the calculation happens in other threads)
	/// and then returns any calculated paths to the
	/// scripts that requested them.
	///
	/// See: PerformBlockingActions
	/// See: PathProcessor.TickNonMultithreaded
	/// See: PathReturnQueue.ReturnPaths
	/// </summary>
	private void Update () {
		navmeshUpdates.Update();

		// This class uses the [ExecuteInEditMode] attribute
		// So Update is called even when not playing
		// Don't do anything when not in play mode
		if (!Application.isPlaying) return;

		// Execute blocking actions such as graph updates
		// when not scanning
		if (!isScanning) {
			PerformBlockingActions();
		}

		// Calculates paths when not using multithreading
		if (!pathProcessor.IsUsingMultithreading) pathProcessor.TickNonMultithreaded();

		// Return calculated paths
		pathReturnQueue.ReturnPaths(true);
	}

	private void PerformBlockingActions (bool force = false) {
		if (workItemLock.Held && pathProcessor.queue.allReceiversBlocked) {
			// Return all paths before starting blocking actions
			// since these might change the graph and make returned paths invalid (at least the nodes)
			pathReturnQueue.ReturnPaths(false);

			Profiler.BeginSample("Work Items");
			if (workItems.ProcessWorkItemsForUpdate(force)) {
				// At this stage there are no more work items, resume pathfinding threads
				workItemLock.Release();
			}
			Profiler.EndSample();
		}
	}

	/// <summary>
	/// Add a work item to be processed when pathfinding is paused.
	///
	/// The callback will be called once when it is safe to update graphs.
	///
	/// This is a convenience method that is equivalent to
	/// <code>
	/// AddWorkItem(new AstarWorkItem(callback));
	/// </code>
	///
	/// See: <see cref="AddWorkItem(AstarWorkItem)"/>
	/// </summary>
	public void AddWorkItem (System.Action callback) {
		AddWorkItem(new AstarWorkItem(callback));
	}

	/// <summary>
	/// Add a work item to be processed when pathfinding is paused.
	///
	/// THe callback will be called once when it is safe to update graphs.
	///
	/// This is a convenience method that is equivalent to
	/// <code>
	/// AddWorkItem(new AstarWorkItem(callback));
	/// </code>
	///
	/// See: <see cref="AddWorkItem(AstarWorkItem)"/>
	/// </summary>
	public void AddWorkItem (System.Action<IWorkItemContext> callback) {
		AddWorkItem(new AstarWorkItem(callback));
	}

	/// <summary>
	/// Add a work item to be processed when pathfinding is paused.
	///
	/// The work item will be executed when it is safe to update nodes. This is defined as between the path searches.
	/// When using more threads than one, calling this often might decrease pathfinding performance due to a lot of idling in the threads.
	/// Not performance as in it will use much CPU power, but performance as in the number of paths per second will probably go down
	/// (though your framerate might actually increase a tiny bit).
	///
	/// You should only call this function from the main unity thread (i.e normal game code).
	///
	/// <code>
	/// AstarPath.active.AddWorkItem(new AstarWorkItem(() => {
	///     // Safe to update graphs here
	///     var node = AstarPath.active.GetNearest(transform.position).node;
	///     node.Walkable = false;
	/// }));
	/// </code>
	///
	/// <code>
	/// AstarPath.active.AddWorkItem(() => {
	///     // Safe to update graphs here
	///     var node = AstarPath.active.GetNearest(transform.position).node;
	///     node.position = (Int3)transform.position;
	/// });
	/// </code>
	///
	/// You can run work items over multiple frames:
	/// <code>
	/// AstarPath.active.AddWorkItem(new AstarWorkItem(() => {
	///     // Called once, right before the
	///     // first call to the method below
	/// },
	///     force => {
	///     // Called every frame until complete.
	///     // Signal that the work item is
	///     // complete by returning true.
	///     // The "force" parameter will
	///     // be true if the work item is
	///     // required to complete immediately.
	///     // In that case this method should
	///     // block and return true when done.
	///     return true;
	/// }));
	/// </code>
	///
	/// See: <see cref="FlushWorkItems"/>
	/// </summary>
	public void AddWorkItem (AstarWorkItem item) {
		workItems.AddWorkItem(item);

		// Make sure pathfinding is stopped and work items are processed
		if (!workItemLock.Held) {
			workItemLock = PausePathfindingSoon();
		}

#if UNITY_EDITOR
		// If not playing, execute instantly
		if (!Application.isPlaying) {
			FlushWorkItems();
		}
#endif
	}

	#region GraphUpdateMethods

	/// <summary>
	/// Will apply queued graph updates as soon as possible, regardless of <see cref="batchGraphUpdates"/>.
	/// Calling this multiple times will not create multiple callbacks.
	/// This function is useful if you are limiting graph updates, but you want a specific graph update to be applied as soon as possible regardless of the time limit.
	/// Note that this does not block until the updates are done, it merely bypasses the <see cref="batchGraphUpdates"/> time limit.
	///
	/// See: <see cref="FlushGraphUpdates"/>
	/// </summary>
	public void QueueGraphUpdates () {
		if (!graphUpdatesWorkItemAdded) {
			graphUpdatesWorkItemAdded = true;
			var workItem = graphUpdates.GetWorkItem();

			// Add a new work item which first
			// sets the graphUpdatesWorkItemAdded flag to false
			// and then processes the graph updates
			AddWorkItem(new AstarWorkItem(context => {
				graphUpdatesWorkItemAdded = false;
				lastGraphUpdate = Time.realtimeSinceStartup;

				workItem.initWithContext(context);
			}, workItem.updateWithContext));
		}
	}

	/// <summary>
	/// Waits a moment with updating graphs.
	/// If batchGraphUpdates is set, we want to keep some space between them to let pathfinding threads running and then calculate all queued calls at once
	/// </summary>
	IEnumerator DelayedGraphUpdate () {
		graphUpdateRoutineRunning = true;

		yield return new WaitForSeconds(graphUpdateBatchingInterval-(Time.realtimeSinceStartup-lastGraphUpdate));
		QueueGraphUpdates();
		graphUpdateRoutineRunning = false;
	}

	/// <summary>
	/// Update all graphs within bounds after delay seconds.
	/// The graphs will be updated as soon as possible.
	///
	/// See: FlushGraphUpdates
	/// See: batchGraphUpdates
	/// See: graph-updates (view in online documentation for working links)
	/// </summary>
	public void UpdateGraphs (Bounds bounds, float delay) {
		UpdateGraphs(new GraphUpdateObject(bounds), delay);
	}

	/// <summary>
	/// Update all graphs using the GraphUpdateObject after delay seconds.
	/// This can be used to, e.g make all nodes in a region unwalkable, or set them to a higher penalty.
	///
	/// See: FlushGraphUpdates
	/// See: batchGraphUpdates
	/// See: graph-updates (view in online documentation for working links)
	/// </summary>
	public void UpdateGraphs (GraphUpdateObject ob, float delay) {
		StartCoroutine(UpdateGraphsInternal(ob, delay));
	}

	/// <summary>Update all graphs using the GraphUpdateObject after delay seconds</summary>
	IEnumerator UpdateGraphsInternal (GraphUpdateObject ob, float delay) {
		yield return new WaitForSeconds(delay);
		UpdateGraphs(ob);
	}

	/// <summary>
	/// Update all graphs within bounds.
	/// The graphs will be updated as soon as possible.
	///
	/// This is equivalent to
	/// <code>
	/// UpdateGraphs(new GraphUpdateObject(bounds));
	/// </code>
	///
	/// See: FlushGraphUpdates
	/// See: batchGraphUpdates
	/// See: graph-updates (view in online documentation for working links)
	/// </summary>
	public void UpdateGraphs (Bounds bounds) {
		UpdateGraphs(new GraphUpdateObject(bounds));
	}

	/// <summary>
	/// Update all graphs using the GraphUpdateObject.
	/// This can be used to, e.g make all nodes in a region unwalkable, or set them to a higher penalty.
	/// The graphs will be updated as soon as possible (with respect to <see cref="batchGraphUpdates)"/>
	///
	/// See: FlushGraphUpdates
	/// See: batchGraphUpdates
	/// See: graph-updates (view in online documentation for working links)
	/// </summary>
	public void UpdateGraphs (GraphUpdateObject ob) {
		if (ob.internalStage != GraphUpdateObject.STAGE_CREATED) {
			throw new System.Exception("You are trying to update graphs using the same graph update object twice. Please create a new GraphUpdateObject instead.");
		}
		ob.internalStage = GraphUpdateObject.STAGE_PENDING;
		graphUpdates.AddToQueue(ob);

		// If we should limit graph updates, start a coroutine which waits until we should update graphs
		if (batchGraphUpdates && Time.realtimeSinceStartup-lastGraphUpdate < graphUpdateBatchingInterval) {
			if (!graphUpdateRoutineRunning) {
				StartCoroutine(DelayedGraphUpdate());
			}
		} else {
			// Otherwise, graph updates should be carried out as soon as possible
			QueueGraphUpdates();
		}
	}

	/// <summary>
	/// Forces graph updates to complete in a single frame.
	/// This will force the pathfinding threads to finish calculating the path they are currently calculating (if any) and then pause.
	/// When all threads have paused, graph updates will be performed.
	/// Warning: Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	/// But you probably wont have to worry about that.
	///
	/// Note: This is almost identical to <see cref="FlushWorkItems"/>, but added for more descriptive name.
	/// This function will also override any time limit delays for graph updates.
	/// This is because graph updates are implemented using work items.
	/// So calling this function will also execute any other work items (if any are queued).
	///
	/// Will not do anything if there are no graph updates queued (not even execute other work items).
	/// </summary>
	public void FlushGraphUpdates () {
		if (IsAnyGraphUpdateQueued || IsAnyGraphUpdateInProgress) {
			QueueGraphUpdates();
			FlushWorkItems();
		}
	}

	#endregion

	/// <summary>
	/// Forces work items to complete in a single frame.
	/// This will force all work items to run immidiately.
	/// This will force the pathfinding threads to finish calculating the path they are currently calculating (if any) and then pause.
	/// When all threads have paused, work items will be executed (which can be e.g graph updates).
	///
	/// Warning: Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	/// But you probably wont have to worry about that
	///
	/// Note: This is almost (note almost) identical to <see cref="FlushGraphUpdates"/>, but added for more descriptive name.
	///
	/// Will not do anything if there are no queued work items waiting to run.
	/// </summary>
	public void FlushWorkItems () {
		if (workItems.anyQueued || workItems.workItemsInProgress) {
			if (active != this) throw new System.Exception("This AstarPath component is not initialized in a scene. Are you trying to add work items to a prefab or a disabled AstarPath component?");
			using (PausePathfinding()) {
				PerformBlockingActions(true);
			}
		}
	}

	/// <summary>
	/// Calculates number of threads to use.
	/// If count is not Automatic, simply returns count casted to an int.
	/// Returns: An int specifying how many threads to use, 0 means a coroutine should be used for pathfinding instead of a separate thread.
	///
	/// If count is set to Automatic it will return a value based on the number of processors and memory for the current system.
	/// If memory is <= 512MB or logical cores are <= 1, it will return 0. If memory is <= 1024 it will clamp threads to max 2.
	/// Otherwise it will return the number of logical cores clamped to 6.
	///
	/// When running on WebGL this method always returns 0
	/// </summary>
	public static int CalculateThreadCount (ThreadCount count) {
#if UNITY_WEBGL
		return 0;
#else
		if (count == ThreadCount.AutomaticLowLoad || count == ThreadCount.AutomaticHighLoad) {
#if ASTARDEBUG
			Debug.Log(SystemInfo.systemMemorySize + " " + SystemInfo.processorCount + " " + SystemInfo.processorType);
#endif

			int logicalCores = Mathf.Max(1, SystemInfo.processorCount);
			int memory = SystemInfo.systemMemorySize;

			if (memory <= 0) {
				Debug.LogError("Machine reporting that is has <= 0 bytes of RAM. This is definitely not true, assuming 1 GiB");
				memory = 1024;
			}

			if (logicalCores <= 1) return 0;

			if (memory <= 512) return 0;

			if (count == ThreadCount.AutomaticHighLoad) {
				if (memory <= 1024) logicalCores = System.Math.Min(logicalCores, 2);
			} else {
				//Always run at at most processorCount-1 threads (one core reserved for unity thread).
				// Many computers use hyperthreading, so dividing by two is used to remove the hyperthreading cores, pathfinding
				// doesn't scale well past the number of physical cores anyway
				logicalCores /= 2;
				logicalCores = Mathf.Max(1, logicalCores);

				if (memory <= 1024) logicalCores = System.Math.Min(logicalCores, 2);

				logicalCores = System.Math.Min(logicalCores, 6);
			}

			return logicalCores;
		} else {
			int val = (int)count;
			return val;
		}
#endif
	}

	/// <summary>Initializes the <see cref="pathProcessor"/> field</summary>
	void InitializePathProcessor () {
		int numThreads = CalculateThreadCount(threadCount);

		// Outside of play mode everything is synchronous, so no threads are used.
		if (!Application.isPlaying) numThreads = 0;


		int numProcessors = Mathf.Max(numThreads, 1);
		bool multithreaded = numThreads > 0;
		pathProcessor.StopThreads();
		pathProcessor.SetThreadCount(numProcessors, multithreaded);
	}

	void InitializeColors () {
		colorSettings = colorSettings ?? new AstarColor();
		colorSettings.PushToStatic();
	}

	void ShutdownPathfindingThreads () {
		// Block until the pathfinding threads have
		// completed their current path calculation
		var graphLock = PausePathfinding();

		navmeshUpdates.OnDisable();

		euclideanEmbedding.dirty = false;

		// Discard all queued graph updates. Graph updates that are already in progress will still be allowed to finish,
		// as they may be allocating unmanaged data which we don't know how to safely deallocate.
		graphUpdates.DiscardQueued();

		// TODO: Add unit test that verifies that work items that are added will always complete
		// Ensure work items complete before disabling this component.
		// This is important because work items may allocate temporary unmanaged memory, so we cannot just forget about them.
		FlushWorkItems();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Processing Possible Work Items");

		// Try to join pathfinding threads
		pathProcessor.StopThreads();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Returning Paths");


		// Return all paths
		pathReturnQueue.ReturnPaths(false);
		graphLock.Release();
		euclideanEmbedding.OnDisable();
	}

	/// <summary>
	/// Called after this component is enabled.
	///
	/// Unless the component has already been activated in Awake, this method should:
	/// - Ensure the singleton holds (setting <see cref="active"/> to this).
	/// - Make sure all subsystems that were disabled in OnDisable are again enabled.
	///   - This includes starting pathfinding threads.
	/// </summary>
	void OnEnable () {
		// If the component gets re-enabled during runtime.
		// Note that the first time the component loads, then Awake will run first
		// and will already have set the #active field.
		// In the editor, OnDisable -> OnEnable will be called when an undo or redo event happens (both in and outside of play mode).
		if (active != null) {
			if (active != this && Application.isPlaying) {
				if (this.enabled) {
					Debug.LogWarning("Another A* component is already in the scene. More than one A* component cannot be active at the same time. Disabling this one.", this);
				}
				enabled = false;
			}
			return;
		}

		// Very important to set this. Ensures the singleton pattern holds
		active = this;

		// Disable GUILayout to gain some performance, it is not used in the OnGUI call
		useGUILayout = false;

		if (OnAwakeSettings != null) {
			OnAwakeSettings();
		}

		hierarchicalGraph.OnEnable();

		// To make sure all graph modifiers have been enabled before scan (to avoid script execution order issues)
		GraphModifier.FindAllModifiers();
		RelevantGraphSurface.FindAllGraphSurfaces();

		InitializeColors();

		navmeshUpdates.OnEnable();

		// This will load the graph settings, or whole initialized graphs from the cache, if one has been supplied.
		data.OnEnable();

		// Flush work items, possibly added when loading the graph data
		FlushWorkItems();

		euclideanEmbedding.dirty = true;

		InitializePathProcessor();

		// This class uses the [ExecuteInEditMode] attribute
		// So OnEnable is called even when not playing
		// Don't scan the graphs unless we are in play mode
		if (Application.isPlaying) {
			// Scan the graphs if #scanOnStartup is enabled, and we have not loaded a graph cache already.
			// We only do this the first time the AstarPath component is enabled.
			if (scanOnStartup && !hasScannedGraphAtStartup && (!data.cacheStartup || data.file_cachedStartup == null)) {
				hasScannedGraphAtStartup = true;
				Scan();
			}
		}
	}


	/// <summary>
	/// Cleans up graphs to avoid memory leaks.
	///
	/// This is called by Unity when:
	/// - The component is explicitly disabled in play mode or editor mode.
	/// - When the component is about to be destroyed
	///   - Including when the game stops
	/// - When an undo/redo event takes place (Unity will first disable the component and then enable it again).
	///
	/// During edit and play mode this method should:
	/// - Destroy all node data (but not the graphs themselves)
	/// - Dispose all unmanaged data
	/// - Shutdown pathfinding threads if they are running (any pending path requests are left in the queue)
	/// </summary>
	void OnDisable () {
		redrawScope.Dispose();
		if (active == this) {
			if (asyncScanTask != null) {
				Debug.LogWarning("An async scan was running when the AstarPath component was disabled. Blocking until the async scan is complete.", this);
				BlockUntilAsyncScanComplete();
			}

			// Ensure there are no jobs running that might read or write graph data
			graphDataLock.WriteSync().Unlock();

			ShutdownPathfindingThreads();

			// We need to call dispose data here because in the editor the OnDestroy
			// method is not called but OnDisable is. It is vital that graph data
			// is destroyed even in the editor (e.g. when going from edit mode to play mode)
			// because a lot of data is stored as NativeArrays which need to be disposed.

			// There is also another case where this is important. When the unity
			// editor is configured to stop play mode after recompiling scripts
			// it seems to not call OnDestroy (or at least not reliably across all versions of Unity).
			// So we need to ensure we dispose of all the data during OnDisable.
			data.DestroyAllNodes();
			data.DisposeUnmanagedData();
			hierarchicalGraph.OnDisable();
			nodeStorage.OnDisable();
			offMeshLinks.OnDisable();
			active = null;
		}
	}

	/// <summary>
	/// Clears up variables and other stuff, destroys graphs.
	/// Note that when destroying an AstarPath object, all static variables such as callbacks will be cleared.
	/// </summary>
	void OnDestroy () {
		if (logPathResults == PathLog.Heavy)
			Debug.Log("AstarPath Component Destroyed - Cleaning Up Pathfinding Data");

		// active has already been set to null during OnDisable.
		// We temporarily make this object the active one just during the destruction.
		var prevActive = active;
		active = this;

		ShutdownPathfindingThreads();

		pathProcessor.Dispose();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Destroying Graphs");

		// Clean up graph data
		// Data may be null if this object was never enabled because another A* instance existed.
		if (data != null) data.OnDestroy();

		active = prevActive;

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Cleaning up variables");

		// Clear all static variables, otherwise the next scene might get weird data
		if (active == this) {
			// Clear all callbacks
			OnAwakeSettings         = null;
			OnGraphPreScan          = null;
			OnGraphPostScan         = null;
			OnPathPreSearch         = null;
			OnPathPostSearch        = null;
			OnPreScan               = null;
			OnPostScan              = null;
			OnLatePostScan          = null;
			On65KOverflow           = null;
			OnGraphsUpdated         = null;

			active = null;
		}
	}

	#region ScanMethods

	/// <summary>
	/// Allocate a bunch of nodes at once.
	/// This is faster than allocating each individual node separately and it can be done in a separate thread by using jobs.
	///
	/// <code>
	/// var nodes = new PointNode[128];
	/// var job = AstarPath.active.AllocateNodes(nodes, 128, () => new PointNode(), 1);
	///
	/// job.Complete();
	/// </code>
	///
	/// See: <see cref="InitializeNode"/>
	/// </summary>
	/// <param name="result">Node array to fill</param>
	/// <param name="count">How many nodes to allocate</param>
	/// <param name="createNode">Delegate which creates a node. () => new T(). Note that new T(AstarPath.active) should *not* be used as that will cause the node to be initialized twice.</param>
	/// <param name="variantsPerNode">How many variants of the node to allocate. Should be the same as \reflink{GraphNode.PathNodeVariants} for this node type.</param>
	public Unity.Jobs.JobHandle AllocateNodes<T>(T[] result, int count, System.Func<T> createNode, uint variantsPerNode) where T : GraphNode {
		if (!pathProcessor.queue.allReceiversBlocked) {
			throw new System.Exception("Trying to initialize a node when it is not safe to initialize any nodes. Must be done during a graph update. See http://arongranberg.com/astar/docs/graph-updates.html#direct");
		}
		return nodeStorage.AllocateNodesJob(result, count, createNode, variantsPerNode);
	}

	/// <summary>
	/// Initializes temporary path data for a node.
	///
	/// Use like: InitializeNode(new PointNode())
	///
	/// See: <see cref="AstarPath.AllocateNodes"/>
	/// </summary>
	internal void InitializeNode (GraphNode node) {
		if (!pathProcessor.queue.allReceiversBlocked) {
			throw new System.Exception("Trying to initialize a node when it is not safe to initialize any nodes. Must be done during a graph update. See http://arongranberg.com/astar/docs/graph-updates.html#direct");
		}
		nodeStorage.InitializeNode(node);
	}

	internal void InitializeNodes (GraphNode[] nodes) {
		if (!pathProcessor.queue.allReceiversBlocked) {
			throw new System.Exception("Trying to initialize a node when it is not safe to initialize any nodes. Must be done during a graph update. See http://arongranberg.com/astar/docs/graph-updates.html#direct");
		}

		for (int i = 0; i < nodes.Length; i++) nodeStorage.InitializeNode(nodes[i]);
	}

	/// <summary>
	/// Internal method to destroy a given node.
	/// This is to be called after the node has been disconnected from the graph so that it cannot be reached from any other nodes.
	/// It should only be called during graph updates, that is when the pathfinding threads are either not running or paused.
	///
	/// Warning: This method should not be called by user code. It is used internally by the system.
	/// </summary>
	internal void DestroyNode (GraphNode node) {
		nodeStorage.DestroyNode(node);
	}

	/// <summary>
	/// Blocks until all pathfinding threads are paused and blocked.
	///
	/// <code>
	/// var graphLock = AstarPath.active.PausePathfinding();
	/// // Here we can modify the graphs safely. For example by increasing the penalty of a node
	/// AstarPath.active.data.gridGraph.GetNode(0, 0).Penalty += 1000;
	///
	/// // Allow pathfinding to resume
	/// graphLock.Release();
	/// </code>
	///
	/// Returns: A lock object. You need to call <see cref="Pathfinding.PathProcessor.GraphUpdateLock.Release"/> on that object to allow pathfinding to resume.
	/// Note: In most cases this should not be called from user code. Use the <see cref="AddWorkItem"/> method instead.
	///
	/// See: <see cref="AddWorkItem"/>
	/// </summary>
	public PathProcessor.GraphUpdateLock PausePathfinding () {
		// Ensure there are no jobs running that might read or write graph data,
		// as this method is typically used right before one modifies graph data.
		graphDataLock.WriteSync().Unlock();
		return pathProcessor.PausePathfinding(true);
	}

	/// <summary>
	/// Blocks the path queue so that e.g work items can be performed.
	///
	/// Pathfinding threads will stop accepting new path requests and will finish the ones they are currently calculating asynchronously.
	/// When the lock is released, the pathfinding threads will resume as normal.
	///
	/// Note: You are unlikely to need to use this method. It is primarily for internal use.
	/// </summary>
	public PathProcessor.GraphUpdateLock PausePathfindingSoon () {
		return pathProcessor.PausePathfinding(false);
	}

	/// <summary>Blocks until the currently running async scan (if any) has completed</summary>
	void BlockUntilAsyncScanComplete () {
		// We can't block and wait for the async scan, so we have to spin.
		// Not great, but this is not something that should happen during normal gameplay.
		// It's more a fallback if the user doesn't wait for the async scan to complete before starting a new one.
		// Note: The ProgressScanningIteratorsConcurrently method used internally by the scan will ensure
		// that the thread yields its time slice in case it's just waiting for other threads.
		while (asyncScanTask != null && asyncScanTask.MoveNext()) {}
		asyncScanTask = null;
	}

	/// <summary>
	/// Scans a particular graph.
	/// Calling this method will recalculate the specified graph from scratch.
	/// This method is pretty slow (depending on graph type and graph complexity of course), so it is advisable to use
	/// smaller graph updates whenever possible.
	///
	/// <code>
	/// // Recalculate all graphs
	/// AstarPath.active.Scan();
	///
	/// // Recalculate only the first grid graph
	/// var graphToScan = AstarPath.active.data.gridGraph;
	/// AstarPath.active.Scan(graphToScan);
	///
	/// // Recalculate only the first and third graphs
	/// var graphsToScan = new [] { AstarPath.active.data.graphs[0], AstarPath.active.data.graphs[2] };
	/// AstarPath.active.Scan(graphsToScan);
	/// </code>
	///
	/// See: graph-updates (view in online documentation for working links)
	/// See: ScanAsync
	/// </summary>
	public void Scan (NavGraph graphToScan) {
		if (graphToScan == null) throw new System.ArgumentNullException();
		Scan(new NavGraph[] { graphToScan });
	}

	/// <summary>
	/// Scans all specified graphs.
	///
	/// Calling this method will recalculate all specified graphs (or all graphs if the graphsToScan parameter is null) from scratch.
	/// This method is pretty slow (depending on graph type and graph complexity of course), so it is advisable to use
	/// smaller graph updates whenever possible.
	///
	/// <code>
	/// // Recalculate all graphs
	/// AstarPath.active.Scan();
	///
	/// // Recalculate only the first grid graph
	/// var graphToScan = AstarPath.active.data.gridGraph;
	/// AstarPath.active.Scan(graphToScan);
	///
	/// // Recalculate only the first and third graphs
	/// var graphsToScan = new [] { AstarPath.active.data.graphs[0], AstarPath.active.data.graphs[2] };
	/// AstarPath.active.Scan(graphsToScan);
	/// </code>
	///
	/// See: graph-updates (view in online documentation for working links)
	/// See: ScanAsync
	/// </summary>
	/// <param name="graphsToScan">The graphs to scan. If this parameter is null then all graphs will be scanned</param>
	public void Scan (NavGraph[] graphsToScan = null) {
		var prevStage = (ScanningStage)(-1);

		if (asyncScanTask != null) {
			Debug.LogWarning("An async scan was already running when a new scan was requested. Blocking until it is complete. You can check if a scan is currently in progress using the AstarPath.active.isScanning property.", this);
			BlockUntilAsyncScanComplete();
		}

		Profiler.BeginSample("Scan");
		Profiler.BeginSample("Init");
		foreach (var p in ScanInternal(graphsToScan, false)) {
			if (prevStage != p.stage) {
				Profiler.EndSample();
				Profiler.BeginSample(p.stage.ToString());
#if !NETFX_CORE && UNITY_EDITOR
				// Log progress to the console
				System.Console.WriteLine(p.stage);
#endif
				prevStage = p.stage;
			}
		}
		Profiler.EndSample();
		Profiler.EndSample();
	}

	/// <summary>
	/// Scans a particular graph asynchronously. This is a IEnumerable, you can loop through it to get the progress
	///
	/// You can scan graphs asyncronously by yielding when you iterate through the returned IEnumerable.
	/// Note that this does not guarantee a good framerate, but it will allow you
	/// to at least show a progress bar while scanning.
	///
	/// <code>
	/// IEnumerator Start () {
	///     foreach (Progress progress in AstarPath.active.ScanAsync()) {
	///         Debug.Log("Scanning... " + progress.ToString());
	///         yield return null;
	///     }
	/// }
	/// </code>
	///
	/// See: Scan
	/// </summary>
	public IEnumerable<Progress> ScanAsync (NavGraph graphToScan) {
		if (graphToScan == null) throw new System.ArgumentNullException();
		return ScanAsync(new NavGraph[] { graphToScan });
	}

	/// <summary>
	/// Scans all specified graphs asynchronously. This is a IEnumerable, you can loop through it to get the progress
	///
	/// You can scan graphs asyncronously by yielding when you loop through the progress.
	/// Note that this does not guarantee a good framerate, but it will allow you
	/// to at least show a progress bar during scanning.
	///
	/// <code>
	/// IEnumerator Start () {
	///     foreach (Progress progress in AstarPath.active.ScanAsync()) {
	///         Debug.Log("Scanning... " + progress.ToString());
	///         yield return null;
	///     }
	/// }
	/// </code>
	///
	/// Note: If the graphs are already scanned, doing an async scan will temporarily cause increased memory usage, since two copies of the graphs will be kept in memory during the async scan.
	/// This may not be desirable on some platforms. A non-async scan will not cause this temporary increased memory usage.
	///
	/// See: Scan
	/// </summary>
	/// <param name="graphsToScan">The graphs to scan. If this parameter is null then all graphs will be scanned</param>
	public IEnumerable<Progress> ScanAsync (NavGraph[] graphsToScan = null) {
		if (asyncScanTask != null) {
			Debug.LogWarning("An async scan was already running when a new async scan was requested. Blocking until the previous one is complete. You can check if a scan is currently in progress using the AstarPath.active.isScanning property.", this);
			BlockUntilAsyncScanComplete();
		}
		asyncScanTask = ScanInternal(graphsToScan, true).GetEnumerator();
		// We cannot inline the TickAsyncScanUntilCompletion function, because we want *this* function to
		// not be a coroutine, so that the setup runs immediately when calling ScanAsync,
		// instead of defering until the coroutine is ticked for the first time.

		// We tick the coroutine once here to do some inital setup.
		// This includes setting isScanning to true.
		try {
			asyncScanTask.MoveNext();
		} catch {
			asyncScanTask = null;
			throw;
		}
		return TickAsyncScanUntilCompletion(asyncScanTask);
	}

	IEnumerable<Progress> TickAsyncScanUntilCompletion (IEnumerator<Progress> task) {
		while (true) {
			try {
				if (!task.MoveNext()) break;
			} catch {
				if (asyncScanTask == task) asyncScanTask = null;
				throw;
			}
			yield return task.Current;
		}
		if (asyncScanTask == task) asyncScanTask = null;
	}

	class DummyGraphUpdateContext : IGraphUpdateContext {
		public void DirtyBounds (Bounds bounds) {}
	}

	class DestroyGraphPromise : IGraphUpdatePromise {
		public IGraphInternals graph;
		public IEnumerator<JobHandle> Prepare () {
			return null;
		}
		public void Apply (IGraphUpdateContext context) {
			graph.DestroyAllNodes();
		}
	}

	IEnumerable<Progress> ScanInternal (NavGraph[] graphsToScan, bool async) {
		if (graphsToScan == null) graphsToScan = graphs;

		if (graphsToScan == null || graphsToScan.Length == 0) {
			yield break;
		}

		// Guard to ensure the A* object is always enabled if the graphs have any valid data.
		// This is because otherwise the OnDisable method will not be called and some unmanaged data
		// in NativeArrays may end up leaking.
		if (!enabled) throw new System.InvalidOperationException("The AstarPath object must be enabled to scan graphs");
		if (active != this) throw new System.InvalidOperationException("The AstarPath object is not enabled in a scene");

		isScanning = true;

		var graphUpdateLock = PausePathfinding();

		// Make sure all paths that are in the queue to be returned
		// are returned immediately
		// Some modifiers (e.g the funnel modifier) rely on
		// the nodes being valid when the path is returned
		pathReturnQueue.ReturnPaths(false);

		// Ensure all graph updates that are in progress get completed immediately.
		// Graph updates that are in progress may use graph data, and we don't want to re-scan the graphs under their feet.
		workItems.ProcessWorkItemsForScan(true);

		if (!Application.isPlaying) {
			data.FindGraphTypes();
			GraphModifier.FindAllModifiers();
		}


		yield return new Progress(0.05F, ScanningStage.PreProcessingGraphs);


		using (var writeLock2 = graphDataLock.WriteSync()) {
			try {
				if (OnPreScan != null) {
					OnPreScan(this);
				}

				GraphModifier.TriggerEvent(GraphModifier.EventType.PreScan);
				GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);
			} catch {
				isScanning = false;
				graphUpdateLock.Release();
				throw;
			}
		}

		data.LockGraphStructure();

		// Make sure the physics engine data is up to date.
		// Scanning graphs may use physics methods and it is very confusing if they
		// do not always pick up the latest changes made to the scene.
		Physics.SyncTransforms();
		Physics2D.SyncTransforms();

		var watch = System.Diagnostics.Stopwatch.StartNew();

		// Destroy previous nodes, unless we are doing an async scan.
		// We do not want the graphs to be in an invalid state during the async scan,
		// so we cannot eagerly destroy them here.
		// This means that during an async scan we may have two copies of the graphs in memory.
		// Most of the data will be destroyed at the end of the async scan, but some memory will
		// still be reserved. So a non-async scan is more memory efficient.
		if (!async) {
			using (var writeLock2 = graphDataLock.WriteSync()) {
				Profiler.BeginSample("Destroy previous nodes");
				for (int i = 0; i < graphsToScan.Length; i++) {
					if (graphsToScan[i] != null) {
						((IGraphInternals)graphsToScan[i]).DestroyAllNodes();
					}
				}
				Profiler.EndSample();
			}
		}

		if (OnGraphPreScan != null) {
			using (var writeLock2 = graphDataLock.WriteSync()) {
				try {
					for (int i = 0; i < graphsToScan.Length; i++) {
						if (graphsToScan[i] != null) OnGraphPreScan(graphsToScan[i]);
					}
				} catch {
					isScanning = false;
					data.UnlockGraphStructure();
					graphUpdateLock.Release();
					throw;
				}
			}
		}

		// Loop through all graphs and start scanning them
		var promises = new List<(IGraphUpdatePromise, IEnumerator<JobHandle>)>(graphsToScan.Length);
		for (int i = 0; i < graphsToScan.Length; i++) {
			if (graphsToScan[i] != null) {
				var promise = ((IGraphInternals)graphsToScan[i]).ScanInternal(async) ?? new DestroyGraphPromise { graph = (IGraphInternals)graphsToScan[i] };
				var iterator = promise.Prepare();
				promises.Add((promise, iterator));
			}
		}

		// Scan all graphs concurrently by progressing all scanning iterators.
		// If the graphs use the job system internally (like the grid, recast and navmesh graphs),
		// then multiple graphs will even be scanned in parallel.
		while (true) {
			int firstNonFinished;
			try {
				firstNonFinished = GraphUpdateProcessor.PrepareGraphUpdatePromises(promises, async ? TimeSlice.MillisFromNow(2) : TimeSlice.Infinite);
			} catch {
				isScanning = false;
				data.UnlockGraphStructure();
				graphUpdateLock.Release();
				throw;
			}
			if (firstNonFinished == -1) {
				break;
			} else {
				// Just used for progress information
				// This graph will advance the progress bar from minp to maxp
				float meanProgress = 0;
				for (int i = 0; i < promises.Count; i++) meanProgress += promises[i].Item1.Progress;
				meanProgress /= promises.Count;
				yield return new Progress(Mathf.Lerp(0.1f, 0.8f, meanProgress), ScanningStage.ScanningGraph, firstNonFinished, promises.Count);
			}
		}

		yield return new Progress(0.95f, ScanningStage.FinishingScans);

		// Now we proceed with the last step of each graph's scanning process
		// This part will make the results of the scan visible to the rest of the game.
		// As a consequence, we must make sure to *not* yield anymore after this point,
		// since that would make the rest of the game run while the graphs may be in an invalid state.
		var writeLock = graphDataLock.WriteSync();

		var ctx = new DummyGraphUpdateContext();
		try {
			GraphUpdateProcessor.ApplyGraphUpdatePromises(promises, ctx);
		} catch {
			isScanning = false;
			data.UnlockGraphStructure();
			graphUpdateLock.Release();
			writeLock.Unlock();
			throw;
		}


		for (int i = 0; i < graphsToScan.Length; i++) {
			if (graphsToScan[i] != null) {
				if (OnGraphPostScan != null) {
					try {
						OnGraphPostScan(graphsToScan[i]);
					} catch {
						isScanning = false;
						data.UnlockGraphStructure();
						graphUpdateLock.Release();
						writeLock.Unlock();
						throw;
					}
				}
				// Notify the off mesh links subsystem that graphs have been recalculated, and we may need to recalculate off mesh links.
				// But skip this for the link graph, since that's the graph that holds the off mesh link nodes themselves.
				if (!(graphsToScan[i] is LinkGraph)) offMeshLinks.DirtyBounds(graphsToScan[i].bounds);
			}
		}

		// Unlock the graph structure here so that e.g. off-mesh-links can add the point graph required for them to work
		data.UnlockGraphStructure();

		try {
			// Graph Modifiers and the OnGraphsUpdated callback may modify graphs arbitrarily, so this also needs to be inside the write lock
			if (OnPostScan != null) OnPostScan(this);
			GraphModifier.TriggerEvent(GraphModifier.EventType.PostScan);
		} catch {
			isScanning = false;
			graphUpdateLock.Release();
			writeLock.Unlock();
			throw;
		}

		// This lock may not be held if there are no work items pending
		if (workItemLock.Held) {
			Profiler.BeginSample("Work Items");
			// Note that this never sends PostUpdate (or similar) events. Those are sent below instead.
			workItems.ProcessWorkItemsForScan(true);
			Profiler.EndSample();
			workItemLock.Release();
		}

		offMeshLinks.Refresh();

		GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdateBeforeAreaRecalculation);

		// Recalculate connected components synchronously
		hierarchicalGraph.RecalculateIfNecessary();

		// Scanning a graph *is* a type of update
		GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
		if (OnGraphsUpdated != null) {
			try {
				OnGraphsUpdated(this);
			} catch {
				isScanning = false;
				graphUpdateLock.Release();
				writeLock.Unlock();
				throw;
			}
		}

		// Signal that we have stopped scanning here
		isScanning = false;

		try {
			if (OnLatePostScan != null) OnLatePostScan(this);
			GraphModifier.TriggerEvent(GraphModifier.EventType.LatePostScan);
		} catch {
			graphUpdateLock.Release();
			writeLock.Unlock();
			throw;
		}

		writeLock.Unlock();

		euclideanEmbedding.dirty = true;
		euclideanEmbedding.RecalculatePivots();

		// Perform any blocking actions
		FlushWorkItems();
		// Resume pathfinding threads
		graphUpdateLock.Release();

		watch.Stop();
		lastScanTime = (float)watch.Elapsed.TotalSeconds;

		if (logPathResults != PathLog.None && logPathResults != PathLog.OnlyErrors) {
			Debug.Log("Scanned graphs in " + (lastScanTime*1000).ToString("0") + " ms");
		}
	}

	#endregion

	internal void DirtyBounds (Bounds bounds) {
		offMeshLinks.DirtyBounds(bounds);
		workItems.DirtyGraphs();
	}

	private static int waitForPathDepth = 0;

	/// <summary>
	/// Blocks until the path has been calculated.
	///
	/// Normally it takes a few frames for a path to be calculated and returned.
	/// This function will ensure that the path will be calculated when this function returns
	/// and that the callback for that path has been called.
	///
	/// If requesting a lot of paths in one go and waiting for the last one to complete,
	/// it will calculate most of the paths in the queue (only most if using multithreading, all if not using multithreading).
	///
	/// Use this function only if you really need to.
	/// There is a point to spreading path calculations out over several frames.
	/// It smoothes out the framerate and makes sure requesting a large
	/// number of paths at the same time does not cause lag.
	///
	/// Note: Graph updates and other callbacks might get called during the execution of this function.
	///
	/// When the pathfinder is shutting down. I.e in OnDestroy, this function will not do anything.
	///
	/// Throws: Exception if pathfinding is not initialized properly for this scene (most likely no AstarPath object exists)
	/// or if the path has not been started yet.
	/// Also throws an exception if critical errors occur such as when the pathfinding threads have crashed (which should not happen in normal cases).
	/// This prevents an infinite loop while waiting for the path.
	///
	/// See: Pathfinding.Path.WaitForPath
	/// See: Pathfinding.Path.BlockUntilCalculated
	/// </summary>
	/// <param name="path">The path to wait for. The path must be started, otherwise an exception will be thrown.</param>
	public static void BlockUntilCalculated (Path path) {
		if (active == null)
			throw new System.Exception("Pathfinding is not correctly initialized in this scene (yet?). " +
				"AstarPath.active is null.\nDo not call this function in Awake");

		if (path == null) throw new System.ArgumentNullException(nameof(path));

		if (active.pathProcessor.queue.isClosed) return;

		if (path.PipelineState == PathState.Created) {
			throw new System.Exception("The specified path has not been started yet.");
		}

		waitForPathDepth++;

		if (waitForPathDepth == 5) {
			Debug.LogError("You are calling the BlockUntilCalculated function recursively (maybe from a path callback). Please don't do this.");
		}

		if (path.PipelineState < PathState.ReturnQueue) {
			if (active.IsUsingMultithreading) {
				while (path.PipelineState < PathState.ReturnQueue) {
					if (active.pathProcessor.queue.isClosed) {
						waitForPathDepth--;
						throw new System.Exception("Pathfinding Threads seem to have crashed.");
					}

					// Wait for threads to calculate paths
					Thread.Sleep(1);
					active.PerformBlockingActions(true);
				}
			} else {
				while (path.PipelineState < PathState.ReturnQueue) {
					if (active.pathProcessor.queue.isEmpty && path.PipelineState != PathState.Processing) {
						waitForPathDepth--;
						throw new System.Exception("Critical error. Path Queue is empty but the path state is '" + path.PipelineState + "'");
					}

					// Calculate some paths
					active.pathProcessor.TickNonMultithreaded();
					active.PerformBlockingActions(true);
				}
			}
		}

		active.pathReturnQueue.ReturnPaths(false);
		waitForPathDepth--;
	}

	/// <summary>
	/// Adds the path to a queue so that it will be calculated as soon as possible.
	/// The callback specified when constructing the path will be called when the path has been calculated.
	/// Usually you should use the Seeker component instead of calling this function directly.
	///
	/// <code>
	/// // There must be an AstarPath instance in the scene
	/// if (AstarPath.active == null) return;
	///
	/// // We can calculate multiple paths asynchronously
	/// for (int i = 0; i < 10; i++) {
	///     var path = ABPath.Construct(transform.position, transform.position+transform.forward*i*10, OnPathComplete);
	///
	///     // Calculate the path by using the AstarPath component directly
	///     AstarPath.StartPath(path);
	/// }
	/// </code>
	/// </summary>
	/// <param name="path">The path that should be enqueued.</param>
	/// <param name="pushToFront">If true, the path will be pushed to the front of the queue, bypassing all waiting paths and making it the next path to be calculated.
	///    This can be useful if you have a path which you want to prioritize over all others. Be careful to not overuse it though.
	///    If too many paths are put in the front of the queue often, this can lead to normal paths having to wait a very long time before being calculated.</param>
	/// <param name="assumeInPlayMode">Typically path.BlockUntilCalculated will be called when not in play mode. However, the play mode check will not work if
	///    you call this from a separate thread, or a job. In that case you can set this to true to skip the check.</param>
	public static void StartPath (Path path, bool pushToFront = false, bool assumeInPlayMode = false) {
		// Copy to local variable to avoid multithreading issues
		var astar = active;

		if (System.Object.ReferenceEquals(astar, null)) {
			Debug.LogError("There is no AstarPath object in the scene or it has not been initialized yet");
			return;
		}

		if (path.PipelineState != PathState.Created) {
			throw new System.Exception("The path has an invalid state. Expected " + PathState.Created + " found " + path.PipelineState + "\n" +
				"Make sure you are not requesting the same path twice");
		}

		if (astar.pathProcessor.queue.isClosed) {
			path.FailWithError("No new paths are accepted");
			return;
		}

		if (astar.graphs == null || astar.graphs.Length == 0) {
			Debug.LogError("There are no graphs in the scene");
			path.FailWithError("There are no graphs in the scene");
			Debug.LogError(path.errorLog);
			return;
		}

		path.Claim(astar);

		// Will increment p.state to PathState.PathQueue
		((IPathInternals)path).AdvanceState(PathState.PathQueue);
		if (pushToFront) {
			astar.pathProcessor.queue.PushFront(path);
		} else {
			astar.pathProcessor.queue.Push(path);
		}

		// Outside of play mode, all path requests are synchronous.
		// However, inside a job we cannot check this, because Unity will throw an exception.
		// But luckily pretty much all jobs will run in game mode anyway. So we assume that if we are in a job, we are in game mode.
		if (!assumeInPlayMode && !Unity.Jobs.LowLevel.Unsafe.JobsUtility.IsExecutingJob && !Application.isPlaying) {
			BlockUntilCalculated(path);
		}
	}

	/// <summary>
	/// Cached NNConstraint to avoid unnecessary allocations.
	/// This should ideally be fixed by making NNConstraint an immutable class/struct.
	/// </summary>
	internal static readonly NNConstraint NNConstraintClosestAsSeenFromAbove = new NNConstraint() {
		constrainWalkability = false,
		constrainTags = false,
		constrainDistance = true,
		distanceMetric = DistanceMetric.ClosestAsSeenFromAbove(),
	};

	/// <summary>
	/// True if the point is on a walkable part of the navmesh, as seen from above.
	///
	/// A point is considered on the navmesh if it is above or below a walkable navmesh surface, at any distance,
	/// and if it is not above/below a closer unwalkable node.
	///
	/// Note: This means that, for example, in multi-story building a point will be considered on the navmesh if any walkable floor is below or above the point.
	/// If you want more complex behavior then you can use the GetNearest method together with the appropriate <see cref="NNConstraint.distanceMetric"/> settings for your use case.
	///
	/// This uses the graph's natural up direction to determine which way is up.
	/// Therefore, it will also work on rotated graphs, as well as graphs in 2D mode.
	///
	/// This method works for all graph types.
	/// However, for <see cref="PointGraph"/>s, this will never return true unless you pass in the exact coordinate of a node, since point nodes do not have a surface.
	///
	/// Note: For spherical navmeshes (or other weird shapes), this method will not work as expected, as there's no well defined "up" direction.
	///
	/// [Open online documentation to see images]
	///
	/// See: <see cref="NavGraph.IsPointOnNavmesh"/> to check if a point is on the navmesh of a specific graph.
	/// </summary>
	/// <param name="position">The point to check</param>
	public bool IsPointOnNavmesh (Vector3 position) {
		// We use the None constraint, instead of Walkable, to avoid ignoring unwalkable nodes that are closer to the point.
		var nearest = GetNearest(position, NNConstraintClosestAsSeenFromAbove);
		const float MaxHorizontalDistance = 0.01f;
		const float MaxCostSqr = MaxHorizontalDistance * MaxHorizontalDistance;
		// TODO: Set a distance threshold in the NNConstraint, to optimize it
		return nearest.node != null && nearest.node.Walkable && nearest.distanceCostSqr < MaxCostSqr;
	}

	/// <summary>
	/// Returns the nearest node to a position.
	/// This method will search through all graphs and query them for the closest node to this position, and then it will return the closest one of those.
	///
	/// Equivalent to GetNearest(position, NNConstraint.None).
	///
	/// <code>
	/// // Find the closest node to this GameObject's position
	/// GraphNode node = AstarPath.active.GetNearest(transform.position).node;
	///
	/// if (node.Walkable) {
	///     // Yay, the node is walkable, we can place a tower here or something
	/// }
	/// </code>
	///
	/// See: Pathfinding.NNConstraint
	/// </summary>
	public NNInfo GetNearest (Vector3 position) {
		return GetNearest(position, null);
	}

	/// <summary>
	/// Returns the nearest node to a point using the specified NNConstraint.
	///
	/// Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
	/// The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
	///
	/// <code>
	/// GraphNode node = AstarPath.active.GetNearest(transform.position, NNConstraint.Walkable).node;
	/// </code>
	///
	/// <code>
	/// var constraint = NNConstraint.None;
	///
	/// // Constrain the search to walkable nodes only
	/// constraint.constrainWalkability = true;
	/// constraint.walkable = true;
	///
	/// // Constrain the search to only nodes with tag 3 or tag 5
	/// // The 'tags' field is a bitmask
	/// constraint.constrainTags = true;
	/// constraint.tags = (1 << 3) | (1 << 5);
	///
	/// var info = AstarPath.active.GetNearest(transform.position, constraint);
	/// var node = info.node;
	/// var closestPoint = info.position;
	/// </code>
	///
	/// See: <see cref="NNConstraint"/>
	/// </summary>
	/// <param name="position">The point to find nodes close to</param>
	/// <param name="constraint">The constraint which determines which graphs and nodes are acceptable to search on. May be null, in which case all nodes will be considered acceptable.</param>
	public NNInfo GetNearest (Vector3 position, NNConstraint constraint) {
		// Cache property lookups
		var graphs = this.graphs;
		var maxNearestNodeDistanceSqr = constraint == null || constraint.constrainDistance ? this.maxNearestNodeDistanceSqr : float.PositiveInfinity;
		NNInfo nearestNode = NNInfo.Empty;

		if (graphs == null || graphs.Length == 0) return nearestNode;

		// Use a fast path in case there is only one graph.
		// This improves performance by about 10% when there is only one graph.
		if (graphs.Length == 1) {
			var graph = graphs[0];
			if (graph == null || (constraint != null && !constraint.SuitableGraph(0, graph))) {
				return nearestNode;
			}

			nearestNode = graph.GetNearest(position, constraint, maxNearestNodeDistanceSqr);
			UnityEngine.Assertions.Assert.IsTrue(nearestNode.node == null || nearestNode.distanceCostSqr <= maxNearestNodeDistanceSqr);
		} else {
			UnsafeSpan<(float, int)> distances;
			unsafe {
				// The number of graphs is limited to GraphNode.MaxGraphIndex (256),
				// and typically there are only a few graphs, so allocating this on the stack is fine.
				var distancesPtr = stackalloc (float, int)[graphs.Length];
				distances = new UnsafeSpan<(float, int)>(distancesPtr, graphs.Length);
			}

			// Iterate through all graphs and find a lower bound on the distance to the nearest node.
			// We then sort these distances and run the full get nearest search on the graphs in order of increasing distance.
			// This is an optimization to avoid running the full get nearest search on graphs which are far away.
			int numCandidateGraphs = 0;
			for (int i = 0; i < graphs.Length; i++) {
				NavGraph graph = graphs[i];

				// Check if this graph should be searched
				if (graph == null || (constraint != null && !constraint.SuitableGraph(i, graph))) {
					continue;
				}
				var lowerBound = graph.NearestNodeDistanceSqrLowerBound(position, constraint);
				if (lowerBound > maxNearestNodeDistanceSqr) continue;

				distances[numCandidateGraphs++] = (lowerBound, i);
			}
			distances = distances.Slice(0, numCandidateGraphs);
			distances.Sort();
			for (int i = 0; i < distances.Length; i++) {
				if (distances[i].Item1 > maxNearestNodeDistanceSqr) break;
				var graph = graphs[distances[i].Item2];
				NNInfo nnInfo = graph.GetNearest(position, constraint, maxNearestNodeDistanceSqr);
				if (nnInfo.distanceCostSqr < maxNearestNodeDistanceSqr) {
					maxNearestNodeDistanceSqr = nnInfo.distanceCostSqr;
					nearestNode = nnInfo;
				}
			}
		}
		return nearestNode;
	}

	/// <summary>
	/// True if there is an obstacle between start and end on the navmesh.
	///
	/// This is a simple api to check if there is an obstacle between two points.
	/// If you need more detailed information, you can use <see cref="GridGraph.Linecast"/> or <see cref="NavmeshBase.Linecast"/> (for navmesh/recast graphs).
	/// Those overloads can also return which nodes the line passed through, and allow you use custom node filtering.
	///
	/// <code>
	/// var start = transform.position;
	/// var end = start + Vector3.forward * 10;
	/// if (AstarPath.active.Linecast(start, end)) {
	///     Debug.DrawLine(start, end, Color.red);
	/// } else {
	///     Debug.DrawLine(start, end, Color.green);
	/// }
	/// </code>
	///
	/// Note: Only grid, recast and navmesh graphs support linecasts. The closest raycastable graph to the start point will be used for the linecast.
	/// Note: Linecasts cannot pass through off-mesh links.
	///
	/// See: <see cref="NavmeshBase.Linecast"/>
	/// See: <see cref="GridGraph.Linecast"/>
	/// See: <see cref="IRaycastableGraph"/>
	/// See: linecasting (view in online documentation for working links), for more details about linecasting
	/// </summary>
	public bool Linecast (Vector3 start, Vector3 end) {
		var startGraph = ClosestRaycastableGraph(start);
		return startGraph == null || startGraph.Linecast(start, end);
	}

	/// <summary>
	/// True if there is an obstacle between start and end on the navmesh.
	///
	/// This is a simple api to check if there is an obstacle between two points.
	/// If you need more detailed information, you can use <see cref="GridGraph.Linecast"/> or <see cref="NavmeshBase.Linecast"/> (for navmesh/recast graphs).
	/// Those overloads can also return which nodes the line passed through, and allow you use custom node filtering.
	///
	/// <code>
	/// var start = transform.position;
	/// var end = start + Vector3.forward * 10;
	/// if (AstarPath.active.Linecast(start, end, out var hit)) {
	///     Debug.DrawLine(start, end, Color.red);
	///     Debug.DrawRay(hit.point, Vector3.up, Color.red);
	/// } else {
	///     Debug.DrawLine(start, end, Color.green);
	/// }
	/// </code>
	///
	/// Note: Only grid, recast and navmesh graphs support linecasts. The closest raycastable graph to the start point will be used for the linecast.
	/// Note: Linecasts cannot pass through off-mesh links.
	///
	/// See: <see cref="NavmeshBase.Linecast"/>
	/// See: <see cref="GridGraph.Linecast"/>
	/// See: <see cref="IRaycastableGraph"/>
	/// See: linecasting (view in online documentation for working links), for more details about linecasting
	/// </summary>
	public bool Linecast (Vector3 start, Vector3 end, out GraphHitInfo hit) {
		var startGraph = ClosestRaycastableGraph(start);
		if (startGraph == null) {
			hit = new GraphHitInfo {
				origin = start,
				point = start,
			};
			return true;
		}
		return startGraph.Linecast(start, end, out hit);
	}

	IRaycastableGraph ClosestRaycastableGraph (Vector3 point) {
		if (data.graphs == null) return null;

		// Most games have just a single raycastable graph.
		IRaycastableGraph graph = null;
		int found = 0;
		for (int i = 0; i < data.graphs.Length; i++) {
			if (data.graphs[i] is IRaycastableGraph g) {
				graph = g;
				found++;
			}
		}

		// If there's more than one graph that can perform linecasts,
		// then find the nearest graph to the point.
		if (found > 1) {
			var startNode = GetNearest(point);
			graph = startNode.node?.Graph as IRaycastableGraph;
		}
		return graph;
	}

	/// <summary>
	/// Returns the node closest to the ray (slow).
	/// Warning: This function is brute-force and very slow, use with caution
	/// </summary>
	public GraphNode GetNearest (Ray ray) {
		if (graphs == null) return null;

		float minDist = Mathf.Infinity;
		GraphNode nearestNode = null;

		Vector3 lineDirection = ray.direction;
		Vector3 lineOrigin = ray.origin;

		for (int i = 0; i < graphs.Length; i++) {
			NavGraph graph = graphs[i];

			graph.GetNodes(node => {
				Vector3 pos = (Vector3)node.position;
				Vector3 p = lineOrigin+(Vector3.Dot(pos-lineOrigin, lineDirection)*lineDirection);

				float tmp = Mathf.Abs(p.x-pos.x);
				tmp *= tmp;
				if (tmp > minDist) return;

				tmp = Mathf.Abs(p.z-pos.z);
				tmp *= tmp;
				if (tmp > minDist) return;

				float dist = (p-pos).sqrMagnitude;

				if (dist < minDist) {
					minDist = dist;
					nearestNode = node;
				}
			});
		}

		return nearestNode;
	}

	/// <summary>
	/// Captures a snapshot of a part of the graphs, to allow restoring it later.
	///
	/// This is useful if you want to do a graph update, but you want to be able to restore the graph to the previous state.
	///
	/// The snapshot will capture enough information to restore the graphs, assuming the world only changed within the given bounding box.
	/// This means the captured region may be larger than the bounding box.
	///
	/// <b>Limitations:</b>
	/// - Currently, the <see cref="GridGraph"/> and <see cref="LayerGridGraph"/> supports snapshots. Other graph types do not support it.
	/// - The graph must not change its dimensions or other core parameters between the time the snapshot is taken and the time it is restored.
	/// - Custom node connections may not be preserved. Unless they are added as off-mesh links using e.g. a <see cref="NodeLink2"/> component.
	/// - The snapshot must not be captured during a work item, graph update or when the graphs are being scanned, as the graphs may not be in a consistent state during those times.
	///
	/// See: <see cref="GraphUpdateUtilities.UpdateGraphsNoBlock"/>, which uses this method internally.
	/// See: <see cref="NavGraph.Snapshot"/>
	///
	/// Note: You must dispose the returned snapshot when you are done with it, to avoid leaking memory.
	/// </summary>
	public GraphSnapshot Snapshot (Bounds bounds, GraphMask graphMask) {
		Profiler.BeginSample("Capturing Graph Snapshot");
		var inner = new List<IGraphSnapshot>();
		for (int i = 0; i < graphs.Length; i++) {
			if (graphs[i] != null && graphMask.Contains(i)) {
				var s = graphs[i].Snapshot(bounds);
				if (s != null) inner.Add(s);
			}
		}
		Profiler.EndSample();
		return new GraphSnapshot(inner);
	}

	/// <summary>
	/// Allows you to access read-only graph data in jobs safely.
	///
	/// You can for example use AstarPath.active.GetNearest(...) in a job.
	///
	/// Using <see cref="AstarPath.StartPath"/> is always safe to use in jobs even without calling this method.
	///
	/// When a graph update, work item, or graph scan would start, it will first block on the given dependency
	/// to ensure no race conditions occur.
	///
	/// If you do not call this method, then a graph update might start in the middle of your job, causing race conditions
	/// and all manner of other hard-to-diagnose bugs.
	///
	/// <code>
	/// var readLock = AstarPath.active.LockGraphDataForReading();
	/// var handle = new MyJob {
	///     // ...
	/// }.Schedule(readLock.dependency);
	/// readLock.UnlockAfter(handle);
	/// </code>
	///
	/// See: <see cref="LockGraphDataForWriting"/>
	/// See: <see cref="graphDataLock"/>
	/// </summary>
	public RWLock.ReadLockAsync LockGraphDataForReading() => graphDataLock.Read();

	/// <summary>
	/// Aquires an exclusive lock on the graph data asynchronously.
	/// This is used when graphs want to modify graph data.
	///
	/// This is a low-level primitive, usually you do not need to use this method.
	///
	/// <code>
	/// var readLock = AstarPath.active.LockGraphDataForReading();
	/// var handle = new MyJob {
	///     // ...
	/// }.Schedule(readLock.dependency);
	/// readLock.UnlockAfter(handle);
	/// </code>
	///
	/// See: <see cref="LockGraphDataForReading"/>
	/// See: <see cref="graphDataLock"/>
	/// </summary>
	public RWLock.WriteLockAsync LockGraphDataForWriting() => graphDataLock.Write();

	/// <summary>
	/// Aquires an exclusive lock on the graph data.
	/// This is used when graphs want to modify graph data.
	///
	/// This is a low-level primitive, usually you do not need to use this method.
	///
	/// <code>
	/// var readLock = AstarPath.active.LockGraphDataForReading();
	/// var handle = new MyJob {
	///     // ...
	/// }.Schedule(readLock.dependency);
	/// readLock.UnlockAfter(handle);
	/// </code>
	///
	/// See: <see cref="LockGraphDataForReading"/>
	/// See: <see cref="graphDataLock"/>
	/// </summary>
	public RWLock.LockSync LockGraphDataForWritingSync() => graphDataLock.WriteSync();

	/// <summary>
	/// Obstacle data for navmesh edges.
	///
	/// This can be used to get information about the edge/borders of the navmesh.
	/// It can also be queried in burst jobs. Just make sure you release the read lock after you are done with it.
	///
	/// Note: This is not a method that you are likely to need to use.
	/// It is used internally for things like local avoidance.
	/// </summary>
	public NavmeshEdges.NavmeshBorderData GetNavmeshBorderData(out RWLock.CombinedReadLockAsync readLock) => hierarchicalGraph.navmeshEdges.GetNavmeshEdgeData(out readLock);
}

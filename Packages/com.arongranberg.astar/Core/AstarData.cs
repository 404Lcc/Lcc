using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.WindowsStore;
using Pathfinding.Serialization;
using Pathfinding.Util;
using Pathfinding.Sync;

namespace Pathfinding {
	[System.Serializable]
	/// <summary>
	/// Stores the navigation graphs for the A* Pathfinding System.
	///
	/// An instance of this class is assigned to <see cref="AstarPath.data"/>. From it you can access all graphs loaded through the <see cref="graphs"/> variable.
	/// This class also handles a lot of the high level serialization.
	/// </summary>
	public class AstarData {
		/// <summary>The AstarPath component which owns this AstarData</summary>
		AstarPath active;

		#region Fields
		/// <summary>
		/// Shortcut to the first <see cref="NavMeshGraph"/>
		///
		/// Deprecated: Use <see cref="navmeshGraph"/> instead
		/// </summary>
		[System.Obsolete("Use navmeshGraph instead")]
		public NavMeshGraph navmesh => navmeshGraph;

		/// <summary>Shortcut to the first <see cref="NavMeshGraph"/></summary>
		public NavMeshGraph navmeshGraph { get; private set; }

#if !ASTAR_NO_GRID_GRAPH
		/// <summary>Shortcut to the first <see cref="GridGraph"/></summary>
		public GridGraph gridGraph { get; private set; }

		/// <summary>Shortcut to the first <see cref="LayerGridGraph"/>.</summary>
		public LayerGridGraph layerGridGraph { get; private set; }
#endif

#if !ASTAR_NO_POINT_GRAPH
		/// <summary>Shortcut to the first <see cref="PointGraph"/>.</summary>
		public PointGraph pointGraph { get; private set; }
#endif

		/// <summary>Shortcut to the first <see cref="RecastGraph"/>.</summary>
		public RecastGraph recastGraph { get; private set; }

		/// <summary>Shortcut to the first <see cref="LinkGraph"/>.</summary>
		public LinkGraph linkGraph { get; private set; }

		/// <summary>
		/// All supported graph types.
		/// Populated through reflection search
		/// </summary>
		public static System.Type[] graphTypes { get; private set; }

#if ASTAR_FAST_NO_EXCEPTIONS || UNITY_WINRT
		/// <summary>
		/// Graph types to use when building with Fast But No Exceptions for iPhone.
		/// If you add any custom graph types, you need to add them to this hard-coded list.
		/// </summary>
		public static readonly System.Type[] DefaultGraphTypes = new System.Type[] {
#if !ASTAR_NO_GRID_GRAPH
			typeof(GridGraph),
			typeof(LayerGridGraph),
#endif
#if !ASTAR_NO_POINT_GRAPH
			typeof(PointGraph),
#endif
			typeof(NavMeshGraph),
			typeof(RecastGraph),
			typeof(LinkGraph),
		};
#endif

		/// <summary>
		/// All graphs.
		/// This will be filled only after deserialization has completed.
		/// May contain null entries if graph have been removed.
		/// </summary>
		[System.NonSerialized]
		public NavGraph[] graphs = new NavGraph[0];

		/// <summary>
		/// Serialized data for all graphs and settings.
		/// Stored as a base64 encoded string because otherwise Unity's Undo system would sometimes corrupt the byte data (because it only stores deltas).
		///
		/// This can be accessed as a byte array from the <see cref="data"/> property.
		/// </summary>
		[SerializeField]
		string dataString;

		/// <summary>Serialized data for all graphs and settings</summary>
		private byte[] data {
			get {
				var d = dataString != null? System.Convert.FromBase64String(dataString) : null;
				// Unity can initialize the dataString to an empty string, but that's not a valid zip file
				if (d != null && d.Length == 0) return null;
				return d;
			}
			set {
				dataString = value != null? System.Convert.ToBase64String(value) : null;
			}
		}

		/// <summary>
		/// Serialized data for cached startup.
		/// If set, and <see cref="cacheStartup"/> is enabled, graphs will be deserialized from this file when the game starts.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public TextAsset file_cachedStartup;

		/// <summary>
		/// Should graph-data be cached.
		/// Caching the startup means saving the whole graphs - not only the settings - to a file (<see cref="file_cachedStartup)"/> which can
		/// be loaded when the game starts. This is usually much faster than scanning the graphs when the game starts. This is configured from the editor under the "Save & Load" tab.
		///
		/// [Open online documentation to see images]
		///
		/// See: save-load-graphs (view in online documentation for working links)
		/// </summary>
		[SerializeField]
		public bool cacheStartup;

		List<bool> graphStructureLocked = new List<bool>();

		static readonly Unity.Profiling.ProfilerMarker MarkerLoadFromCache = new Unity.Profiling.ProfilerMarker("LoadFromCache");
		static readonly Unity.Profiling.ProfilerMarker MarkerDeserializeGraphs = new Unity.Profiling.ProfilerMarker("DeserializeGraphs");
		static readonly Unity.Profiling.ProfilerMarker MarkerSerializeGraphs = new Unity.Profiling.ProfilerMarker("SerializeGraphs");
		static readonly Unity.Profiling.ProfilerMarker MarkerFindGraphTypes = new Unity.Profiling.ProfilerMarker("FindGraphTypes");

		#endregion

		internal AstarData (AstarPath active) {
			this.active = active;
		}

		/// <summary>Get the serialized data for all graphs and their settings</summary>
		public byte[] GetData() => data;

		/// <summary>
		/// Set the serialized data for all graphs and their settings.
		///
		/// During runtime you usually want to deserialize the graphs immediately, in which case you should use <see cref="DeserializeGraphs(byte"/>[]) instead.
		/// </summary>
		public void SetData (byte[] data) {
			this.data = data;
		}

		/// <summary>Loads the graphs from memory, will load cached graphs if any exists</summary>
		public void OnEnable () {
			FindGraphTypes();

			if (graphs == null) graphs = new NavGraph[0];

			if (cacheStartup && file_cachedStartup != null && Application.isPlaying) {
				LoadFromCache();
			} else {
				DeserializeGraphs();
			}
		}

		/// <summary>
		/// Prevent the graph structure from changing during the time this lock is held.
		/// This prevents graphs from being added or removed and also prevents graphs from being serialized or deserialized.
		/// This is used when e.g an async scan is happening to ensure that for example a graph that is being scanned is not destroyed.
		///
		/// Each call to this method *must* be paired with exactly one call to <see cref="UnlockGraphStructure"/>.
		/// The calls may be nested.
		/// </summary>
		internal void LockGraphStructure (bool allowAddingGraphs = false) {
			graphStructureLocked.Add(allowAddingGraphs);
		}

		/// <summary>
		/// Allows the graph structure to change again.
		/// See: <see cref="LockGraphStructure"/>
		/// </summary>
		internal void UnlockGraphStructure () {
			if (graphStructureLocked.Count == 0) throw new System.InvalidOperationException();
			graphStructureLocked.RemoveAt(graphStructureLocked.Count - 1);
		}

		PathProcessor.GraphUpdateLock AssertSafe (bool onlyAddingGraph = false) {
			if (graphStructureLocked.Count > 0) {
				bool allowAdding = true;
				for (int i = 0; i < graphStructureLocked.Count; i++) allowAdding &= graphStructureLocked[i];
				if (!(onlyAddingGraph && allowAdding)) throw new System.InvalidOperationException("Graphs cannot be added, removed or serialized while the graph structure is locked. This is the case when a graph is currently being scanned and when executing graph updates and work items.\nHowever as a special case, graphs can be added inside work items.");
			}

			// Pause the pathfinding threads
			var graphLock = active.PausePathfinding();
			if (!active.IsInsideWorkItem) {
				// Make sure all graph updates and other callbacks are done
				// Only do this if this code is not being called from a work item itself as that would cause a recursive wait that could never complete.
				// There are some valid cases when this can happen. For example it may be necessary to add a new graph inside a work item.
				active.FlushWorkItems();

				// Paths that are already calculated and waiting to be returned to the Seeker component need to be
				// processed immediately as their results usually depend on graphs that currently exist. If this was
				// not done then after destroying a graph one could get a path result with destroyed nodes in it.
				active.pathReturnQueue.ReturnPaths(false);
			}
			return graphLock;
		}

		/// <summary>
		/// Calls the callback with every node in all graphs.
		/// This is the easiest way to iterate through every existing node.
		///
		/// <code>
		/// AstarPath.active.data.GetNodes(node => {
		///     Debug.Log("I found a node at position " + (Vector3)node.position);
		/// });
		/// </code>
		///
		/// See: <see cref="Pathfinding.NavGraph.GetNodes"/> for getting the nodes of a single graph instead of all.
		/// See: graph-updates (view in online documentation for working links)
		/// </summary>
		public void GetNodes (System.Action<GraphNode> callback) {
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null) graphs[i].GetNodes(callback);
			}
		}

		/// <summary>
		/// Updates shortcuts to the first graph of different types.
		/// Hard coding references to some graph types is not really a good thing imo. I want to keep it dynamic and flexible.
		/// But these references ease the use of the system, so I decided to keep them.
		/// </summary>
		public void UpdateShortcuts () {
			navmeshGraph = (NavMeshGraph)FindGraphOfType(typeof(NavMeshGraph));

#if !ASTAR_NO_GRID_GRAPH
			gridGraph = (GridGraph)FindGraphOfType(typeof(GridGraph));
			layerGridGraph = (LayerGridGraph)FindGraphOfType(typeof(LayerGridGraph));
#endif

#if !ASTAR_NO_POINT_GRAPH
			pointGraph = (PointGraph)FindGraphOfType(typeof(PointGraph));
#endif

			recastGraph = (RecastGraph)FindGraphOfType(typeof(RecastGraph));
			linkGraph = (LinkGraph)FindGraphOfType(typeof(LinkGraph));
		}

		/// <summary>Load from data from <see cref="file_cachedStartup"/></summary>
		public void LoadFromCache () {
			using var _ = MarkerLoadFromCache.Auto();
			using (AssertSafe()) {
				if (file_cachedStartup != null) {
					var bytes = file_cachedStartup.bytes;
					DeserializeGraphs(bytes);

					GraphModifier.TriggerEvent(GraphModifier.EventType.PostCacheLoad);
				} else {
					Debug.LogError("Can't load from cache since the cache is empty");
				}
			}
		}

		#region Serialization

		/// <summary>
		/// Serializes all graphs settings to a byte array.
		/// See: DeserializeGraphs(byte[])
		/// </summary>
		public byte[] SerializeGraphs () {
			return SerializeGraphs(SerializeSettings.Settings);
		}

		/// <summary>
		/// Serializes all graphs settings and optionally node data to a byte array.
		/// See: DeserializeGraphs(byte[])
		/// See: Pathfinding.Serialization.SerializeSettings
		/// </summary>
		public byte[] SerializeGraphs (SerializeSettings settings) {
			return SerializeGraphs(settings, out var _);
		}

		/// <summary>
		/// Main serializer function.
		/// Serializes all graphs to a byte array
		/// A similar function exists in the AstarPathEditor.cs script to save additional info
		/// </summary>
		public byte[] SerializeGraphs (SerializeSettings settings, out uint checksum) {
			return SerializeGraphs(settings, out checksum, graphs);
		}

		byte[] SerializeGraphs (SerializeSettings settings, out uint checksum, NavGraph[] graphs) {
			MarkerSerializeGraphs.Begin();
			using (AssertSafe()) {
				var sr = new AstarSerializer(this, settings, active.gameObject);

				sr.OpenSerialize();
				sr.SerializeGraphs(graphs);
				sr.SerializeExtraInfo();
				byte[] bytes = sr.CloseSerialize();
				checksum = sr.GetChecksum();
#if ASTARDEBUG
				Debug.Log("Got a whole bunch of data, "+bytes.Length+" bytes");
#endif

				MarkerSerializeGraphs.End();
				return bytes;
			}
		}

		/// <summary>Deserializes graphs from <see cref="data"/></summary>
		public void DeserializeGraphs () {
			var dataBytes = data;
			if (dataBytes != null) {
				DeserializeGraphs(dataBytes);
			}
		}

		/// <summary>
		/// Destroys all graphs and sets <see cref="graphs"/> to null.
		/// See: <see cref="RemoveGraph"/>
		/// </summary>
		public void ClearGraphs () {
			using (AssertSafe()) {
				ClearGraphsInternal();
			}
		}

		void ClearGraphsInternal () {
			if (graphs == null) return;
			using (AssertSafe()) {
				for (int i = 0; i < graphs.Length; i++) {
					if (graphs[i] != null) {
						active.DirtyBounds(graphs[i].bounds);
						((IGraphInternals)graphs[i]).OnDestroy();
						graphs[i].active = null;
					}
				}
				graphs = new NavGraph[0];
				UpdateShortcuts();
			}
		}

		public void DisposeUnmanagedData () {
			if (graphs == null) return;
			using (AssertSafe()) {
				for (int i = 0; i < graphs.Length; i++) {
					if (graphs[i] != null) {
						((IGraphInternals)graphs[i]).DisposeUnmanagedData();
					}
				}
			}
		}

		/// <summary>Makes all graphs become unscanned</summary>
		internal void DestroyAllNodes () {
			if (graphs == null) return;
			using (AssertSafe()) {
				for (int i = 0; i < graphs.Length; i++) {
					if (graphs[i] != null) {
						((IGraphInternals)graphs[i]).DestroyAllNodes();
					}
				}
			}
		}

		public void OnDestroy () {
			ClearGraphsInternal();
		}

		/// <summary>
		/// Deserializes and loads graphs from the specified byte array.
		/// An error will be logged if deserialization fails.
		///
		/// Returns: The deserialized graphs
		/// </summary>
		public NavGraph[] DeserializeGraphs (byte[] bytes) {
			using (AssertSafe()) {
				ClearGraphs();
				return DeserializeGraphsAdditive(bytes);
			}
		}

		/// <summary>
		/// Deserializes and loads graphs from the specified byte array additively.
		/// An error will be logged if deserialization fails.
		/// This function will add loaded graphs to the current ones.
		///
		/// Returns: The deserialized graphs
		/// </summary>
		public NavGraph[] DeserializeGraphsAdditive (byte[] bytes) {
			return DeserializeGraphsAdditive(bytes, true);
		}

		NavGraph[] DeserializeGraphsAdditive (byte[] bytes, bool warnIfDuplicateGuids) {
			using (AssertSafe()) {
				try {
					MarkerDeserializeGraphs.Begin();
					NavGraph[] result;
					if (bytes != null) {
						var sr = new AstarSerializer(this, active.gameObject);

						if (sr.OpenDeserialize(bytes)) {
							result = DeserializeGraphsPartAdditive(sr, warnIfDuplicateGuids);
							sr.CloseDeserialize();
						} else {
							throw new System.ArgumentException("Invalid data file (cannot read zip).\nThe data is either corrupt or it was saved using a 3.0.x or earlier version of the system");
						}
					} else {
						throw new System.ArgumentNullException(nameof(bytes));
					}
					UpdateShortcuts();
					GraphModifier.TriggerEvent(GraphModifier.EventType.PostGraphLoad);
					return result;
				} catch (System.Exception e) {
					Debug.LogException(new System.Exception("Caught exception while deserializing data.", e));
					graphs = new NavGraph[0];
					UpdateShortcuts();
					throw;
				} finally {
					MarkerDeserializeGraphs.End();
				}
			}
		}

		/// <summary>Helper function for deserializing graphs</summary>
		NavGraph[] DeserializeGraphsPartAdditive (AstarSerializer sr, bool warnIfDuplicateGuids) {
			if (graphs == null) graphs = new NavGraph[0];

			var gr = new List<NavGraph>(graphs);

			// Trim nulls at the end
			while (gr.Count > 0 && gr[gr.Count-1] == null) gr.RemoveAt(gr.Count-1);

			FindGraphTypes();
			// This may be false if the user is editing a prefab, for example.
			// If it is false, we must not try to load any nodes
			bool astarInitialized = active == AstarPath.active;
			int lastUsedGraphIndex = -1;
			var newGraphs = sr.DeserializeGraphs(graphTypes, astarInitialized, () => {
				// Find the index to insert the new graph at
				// This is the first index which is not yet filled with a graph
				lastUsedGraphIndex++;
				while (lastUsedGraphIndex < gr.Count && gr[lastUsedGraphIndex] != null) {
					lastUsedGraphIndex++;
				}
				return lastUsedGraphIndex;
			});

			for (int i = 0; i < newGraphs.Length; i++) {
				while (gr.Count < (int)newGraphs[i].graphIndex + 1) gr.Add(null);
				gr[(int)newGraphs[i].graphIndex] = newGraphs[i];
			}

			if (gr.Count > GraphNode.MaxGraphIndex + 1) {
				throw new System.InvalidOperationException("Graph Count Limit Reached. You cannot have more than " + GraphNode.MaxGraphIndex + " graphs.");
			}

			graphs = gr.ToArray();

			// Assign correct graph indices.
			bool anyScanned = false;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] == null) continue;
				graphs[i].GetNodes(node => node.GraphIndex = (uint)i);
				anyScanned |= graphs[i].isScanned;
			}

			for (int i = 0; i < graphs.Length; i++) {
				for (int j = i+1; j < graphs.Length; j++) {
					if (graphs[i] != null && graphs[j] != null && graphs[i].guid == graphs[j].guid) {
						if (warnIfDuplicateGuids) Debug.LogWarning("Guid Conflict when importing graphs additively. Imported graph will get a new Guid.\nThis message is (relatively) harmless.");
						graphs[i].guid = Pathfinding.Util.Guid.NewGuid();
						break;
					}
				}
			}

			sr.PostDeserialization();

			if (anyScanned) {
				// This will refresh off-mesh links,
				// and also recalculate the hierarchical graph if necessary.
				//
				// It's important that this does not run if no graphs are scanned,
				// which is the case when just deserializing graph settings in the editor.
				// This is because we may be in a prefab, and prefabs should never be able
				// to actually load graphs with nodes.
				active.AddWorkItem(ctx => {
					for (int i = 0; i < newGraphs.Length; i++) {
						if (newGraphs[i].isScanned) {
							ctx.DirtyBounds(newGraphs[i].bounds);
						}
					}
				});
				active.FlushWorkItems();
			}
			return newGraphs;
		}

		#endregion

		/// <summary>
		/// Find all graph types supported in this build.
		/// Using reflection, the assembly is searched for types which inherit from NavGraph.
		/// </summary>
		public void FindGraphTypes () {
			if (graphTypes != null) return;

			MarkerFindGraphTypes.Begin();
#if !ASTAR_FAST_NO_EXCEPTIONS && !UNITY_WINRT
			graphTypes = AssemblySearcher.FindTypesInheritingFrom<NavGraph>().ToArray();
#else
			graphTypes = DefaultGraphTypes;
#endif
			MarkerFindGraphTypes.End();
		}

		#region GraphCreation

		/// <summary>Creates a new graph instance of type type</summary>
		internal NavGraph CreateGraph (System.Type type) {
			var graph = System.Activator.CreateInstance(type) as NavGraph;

			graph.active = active;
			return graph;
		}

		/// <summary>
		/// Adds a graph of type T to the <see cref="graphs"/> array.
		/// See: runtime-graphs (view in online documentation for working links)
		/// </summary>
		public T AddGraph<T> () where T : NavGraph => AddGraph(typeof(T)) as T;

		/// <summary>
		/// Adds a graph of type type to the <see cref="graphs"/> array.
		/// See: runtime-graphs (view in online documentation for working links)
		/// </summary>
		public NavGraph AddGraph (System.Type type) {
			NavGraph graph = null;

			for (int i = 0; i < graphTypes.Length; i++) {
				if (System.Type.Equals(graphTypes[i], type)) {
					graph = CreateGraph(graphTypes[i]);
				}
			}

			if (graph == null) {
				Debug.LogError("No NavGraph of type '"+type+"' could be found, "+graphTypes.Length+" graph types are avaliable");
				return null;
			}

			AddGraph(graph);

			return graph;
		}

		/// <summary>Adds the specified graph to the <see cref="graphs"/> array</summary>
		void AddGraph (NavGraph graph) {
			// Make sure to not interfere with pathfinding
			using (AssertSafe(true)) {
				// Try to fill in an empty position
				int graphIndex = System.Array.IndexOf(graphs, null);

				if (graphIndex == -1) {
					if (graphs.Length >= GraphNode.MaxGraphIndex) {
						throw new System.Exception($"Graph Count Limit Reached. You cannot have more than {GraphNode.MaxGraphIndex} graphs.");
					}

					// Add a new entry
					Memory.Realloc(ref graphs, graphs.Length + 1);
					graphIndex = graphs.Length-1;
				}
				graphs[graphIndex] = graph;
				graph.graphIndex = (uint)graphIndex;
				graph.active = active;

				UpdateShortcuts();
			}
		}

		/// <summary>
		/// Removes the specified graph from the <see cref="graphs"/> array and Destroys it in a safe manner.
		/// To avoid changing graph indices for the other graphs, the graph is simply nulled in the array instead
		/// of actually removing it from the array.
		/// The empty position will be reused if a new graph is added.
		///
		/// Returns: True if the graph was sucessfully removed (i.e it did exist in the <see cref="graphs"/> array). False otherwise.
		///
		/// See: <see cref="ClearGraphs"/>
		/// </summary>
		public bool RemoveGraph (NavGraph graph) {
			// Make sure the pathfinding threads are paused
			using (AssertSafe()) {
				active.DirtyBounds(graph.bounds);
				((IGraphInternals)graph).OnDestroy();
				graph.active = null;

				int i = System.Array.IndexOf(graphs, graph);
				if (i != -1) graphs[i] = null;

				UpdateShortcuts();

				// If we are working on a prefab, this may not be true
				if (AstarPath.active == active) {
					active.AddWorkItem(() => active.offMeshLinks.Refresh());
					active.FlushWorkItems();
				}
				return i != -1;
			}
		}

		/// <summary>
		/// Duplicates the given graph and adds the duplicate to the <see cref="graphs"/> array.
		///
		/// Note: Only graph settings are duplicated, not the nodes in the graph. You may want to scan the graph after duplicating it.
		///
		/// Returns: The duplicated graph.
		/// </summary>
		public NavGraph DuplicateGraph (NavGraph graph) {
			if (graph == null) throw new System.ArgumentNullException(nameof(graph));

			int i = System.Array.IndexOf(graphs, graph);
			if (i == -1) throw new System.ArgumentException("Graph doesn't exist");

			var bytes = SerializeGraphs(SerializeSettings.Settings, out var _, new NavGraph[] { graph });
			var newGraphs = DeserializeGraphsAdditive(bytes, false);
			UnityEngine.Assertions.Assert.AreEqual(1, newGraphs.Length);

#if UNITY_EDITOR
			foreach (var g in newGraphs) {
				var existingNames = new string[graphs.Length];
				for (int j = 0; j < graphs.Length; j++) existingNames[j] = graphs[j].name;
				g.name = UnityEditor.ObjectNames.GetUniqueName(existingNames, g.name);
			}
#endif
			return newGraphs[0];
		}

		#endregion

		#region GraphUtility

		/// <summary>
		/// Graph which contains the specified node.
		/// The graph must be in the <see cref="graphs"/> array.
		///
		/// Returns: Returns the graph which contains the node. Null if the graph wasn't found
		/// </summary>
		public static NavGraph GetGraph (GraphNode node) {
			if (node == null || node.Destroyed) return null;

			AstarPath script = AstarPath.active;
			if (System.Object.ReferenceEquals(script, null)) return null;

			AstarData data = script.data;
			if (data == null || data.graphs == null) return null;

			uint graphIndex = node.GraphIndex;
			return data.graphs[(int)graphIndex];
		}

		/// <summary>Returns the first graph which satisfies the predicate. Returns null if no graph was found.</summary>
		public NavGraph FindGraph (System.Func<NavGraph, bool> predicate) {
			if (graphs != null) {
				for (int i = 0; i < graphs.Length; i++) {
					if (graphs[i] != null && predicate(graphs[i])) {
						return graphs[i];
					}
				}
			}
			return null;
		}

		/// <summary>Returns the first graph of type type found in the <see cref="graphs"/> array. Returns null if no graph was found.</summary>
		public NavGraph FindGraphOfType (System.Type type) {
			return FindGraph(graph => System.Type.Equals(graph.GetType(), type));
		}

		/// <summary>Returns the first graph which inherits from the type type. Returns null if no graph was found.</summary>
		public NavGraph FindGraphWhichInheritsFrom (System.Type type) {
			return FindGraph(graph => WindowsStoreCompatibility.GetTypeInfo(type).IsAssignableFrom(WindowsStoreCompatibility.GetTypeInfo(graph.GetType())));
		}

		/// <summary>
		/// Loop through this function to get all graphs of type 'type'
		/// <code>
		/// foreach (GridGraph graph in AstarPath.data.FindGraphsOfType (typeof(GridGraph))) {
		///     //Do something with the graph
		/// }
		/// </code>
		/// See: <see cref="AstarPath.AddWorkItem"/>
		/// </summary>
		public IEnumerable FindGraphsOfType (System.Type type) {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null && System.Type.Equals(graphs[i].GetType(), type)) {
					yield return graphs[i];
				}
			}
		}

		/// <summary>
		/// All graphs which implements the UpdateableGraph interface
		/// <code> foreach (IUpdatableGraph graph in AstarPath.data.GetUpdateableGraphs ()) {
		///  //Do something with the graph
		/// } </code>
		/// See: <see cref="AstarPath.AddWorkItem"/>
		/// See: <see cref="IUpdatableGraph"/>
		/// </summary>
		public IEnumerable GetUpdateableGraphs () {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is IUpdatableGraph) {
					yield return graphs[i];
				}
			}
		}

		/// <summary>Gets the index of the graph in the <see cref="graphs"/> array</summary>
		public int GetGraphIndex (NavGraph graph) {
			if (graph == null) throw new System.ArgumentNullException("graph");
			if (graphs == null) throw new System.ArgumentException("No graphs exist");

			var index = System.Array.IndexOf(graphs, graph);
			if (index == -1) throw new System.ArgumentException("Graph doesn't exist");
			return index;
		}

		#endregion
	}
}

using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Serialization;
using Unity.Collections;

namespace Pathfinding {
	using Pathfinding.Drawing;

	/// <summary>
	/// Exposes internal methods for graphs.
	/// This is used to hide methods that should not be used by any user code
	/// but still have to be 'public' or 'internal' (which is pretty much the same as 'public'
	/// as this library is distributed with source code).
	///
	/// Hiding the internal methods cleans up the documentation and IntelliSense suggestions.
	/// </summary>
	public interface IGraphInternals {
		string SerializedEditorSettings { get; set; }
		void OnDestroy();
		void DisposeUnmanagedData();
		void DestroyAllNodes();
		IGraphUpdatePromise ScanInternal(bool async);
		void SerializeExtraInfo(GraphSerializationContext ctx);
		void DeserializeExtraInfo(GraphSerializationContext ctx);
		void PostDeserialization(GraphSerializationContext ctx);
	}

	/// <summary>Base class for all graphs</summary>
	public abstract class NavGraph : IGraphInternals {
		/// <summary>Reference to the AstarPath object in the scene</summary>
		public AstarPath active;

		/// <summary>
		/// Used as an ID of the graph, considered to be unique.
		/// Note: This is Pathfinding.Util.Guid not System.Guid. A replacement for System.Guid was coded for better compatibility with iOS
		/// </summary>
		[JsonMember]
		public Guid guid;

		/// <summary>
		/// Default penalty to apply to all nodes.
		///
		/// See: graph-updates (view in online documentation for working links)
		/// See: <see cref="GraphNode.Penalty"/>
		/// See: tags (view in online documentation for working links)
		/// </summary>
		[JsonMember]
		public uint initialPenalty;

		/// <summary>Is the graph open in the editor</summary>
		[JsonMember]
		public bool open;

		/// <summary>Index of the graph, used for identification purposes</summary>
		public uint graphIndex;

		/// <summary>
		/// Name of the graph.
		/// Can be set in the unity editor
		/// </summary>
		[JsonMember]
		public string name;

		/// <summary>
		/// Enable to draw gizmos in the Unity scene view.
		/// In the inspector this value corresponds to the state of
		/// the 'eye' icon in the top left corner of every graph inspector.
		/// </summary>
		[JsonMember]
		public bool drawGizmos = true;

		/// <summary>
		/// Used in the editor to check if the info screen is open.
		/// Should be inside UNITY_EDITOR only \<see cref="ifs"/> but just in case anyone tries to serialize a NavGraph instance using Unity, I have left it like this as it would otherwise cause a crash when building.
		/// Version 3.0.8.1 was released because of this bug only
		/// </summary>
		[JsonMember]
		public bool infoScreenOpen;

		/// <summary>Used in the Unity editor to store serialized settings for graph inspectors</summary>
		[JsonMember]
		string serializedEditorSettings;


		/// <summary>True if the graph exists, false if it has been destroyed</summary>
		internal bool exists => active != null;

		/// <summary>
		/// True if the graph has been scanned and contains nodes.
		///
		/// Graphs are typically scanned when the game starts, but they can also be scanned manually.
		///
		/// If a graph has not been scanned, it does not contain any nodes and it not possible to use it for pathfinding.
		///
		/// See: <see cref="AstarPath.Scan(NavGraph)"/>
		/// </summary>
		public abstract bool isScanned { get; }

		/// <summary>
		/// True if the graph will be included when serializing graph data.
		///
		/// If false, the graph will be ignored when saving graph data.
		///
		/// Most graphs are persistent, but the <see cref="LinkGraph"/> is not persistent because links are always re-created from components at runtime.
		/// </summary>
		public virtual bool persistent => true;

		/// <summary>
		/// True if the graph should be visible in the editor.
		///
		/// False is used for some internal graph types that users don't have to worry about.
		/// </summary>
		public virtual bool showInInspector => true;

		/// <summary>
		/// World bounding box for the graph.
		///
		/// This always contains the whole graph.
		///
		/// Note: Since this an axis aligned bounding box, it may not be particularly tight if the graph is rotated.
		///
		/// It is ok for a graph type to return an infinitely large bounding box, but this may make some operations less efficient.
		/// The point graph will always return an infinitely large bounding box.
		/// </summary>
		public virtual Bounds bounds => new Bounds(Vector3.zero, Vector3.positiveInfinity);

		/// <summary>
		/// Number of nodes in the graph.
		/// Note that this is, unless the graph type has overriden it, an O(n) operation.
		///
		/// This is an O(1) operation for grid graphs and point graphs.
		/// For layered grid graphs it is an O(n) operation.
		/// </summary>
		public virtual int CountNodes () {
			int count = 0;

			GetNodes(_ => count++);
			return count;
		}

		/// <summary>Calls a delegate with all nodes in the graph until the delegate returns false</summary>
		public void GetNodes (System.Func<GraphNode, bool> action) {
			bool cont = true;

			GetNodes(node => {
				if (cont) cont &= action(node);
			});
		}

		/// <summary>
		/// Calls a delegate with all nodes in the graph.
		/// This is the primary way of iterating through all nodes in a graph.
		///
		/// Do not change the graph structure inside the delegate.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		///
		/// gg.GetNodes(node => {
		///     // Here is a node
		///     Debug.Log("I found a node at position " + (Vector3)node.position);
		/// });
		/// </code>
		///
		/// If you want to store all nodes in a list you can do this
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		///
		/// List<GraphNode> nodes = new List<GraphNode>();
		///
		/// gg.GetNodes((System.Action<GraphNode>)nodes.Add);
		/// </code>
		///
		/// See: <see cref="Pathfinding.AstarData.GetNodes"/>
		/// </summary>
		public abstract void GetNodes(System.Action<GraphNode> action);

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
		/// See: <see cref="AstarPath.IsPointOnNavmesh"/> to check all graphs, instead of a single one.
		/// </summary>
		/// <param name="position">The point to check</param>
		public virtual bool IsPointOnNavmesh (Vector3 position) {
			// We use the None constraint, instead of Walkable, to avoid ignoring unwalkable nodes that are closer to the point.
			const float MaxHorizontalDistance = 0.01f;
			const float MaxCostSqr = MaxHorizontalDistance * MaxHorizontalDistance;
			var nearest = GetNearest(position, AstarPath.NNConstraintClosestAsSeenFromAbove, MaxCostSqr);
			return nearest.node != null && nearest.node.Walkable && nearest.distanceCostSqr < MaxCostSqr;
		}

		/// <summary>
		/// True if the point is inside the bounding box of this graph.
		///
		/// This method may be able to use a tighter (non-axis aligned) bounding box than using the one returned by <see cref="bounds"/>.
		///
		/// It is valid for a graph to return true for all points in the world.
		/// In particular the PointGraph will always return true, since it has no limits on its bounding box.
		/// </summary>
		public virtual bool IsInsideBounds(Vector3 point) => true;

		/// <summary>
		/// Throws an exception if it is not safe to update internal graph data right now.
		///
		/// It is safe to update graphs when graphs are being scanned, or inside a work item.
		/// In other cases pathfinding could be running at the same time, which would not appreciate graph data changing under its feet.
		///
		/// See: <see cref="AstarPath.AddWorkItem(System.Action)"/>
		/// </summary>
		protected void AssertSafeToUpdateGraph () {
			if (!active.IsAnyWorkItemInProgress && !active.isScanning) {
				throw new System.Exception("Trying to update graphs when it is not safe to do so. Graph updates must be done inside a work item or when a graph is being scanned. See AstarPath.AddWorkItem");
			}
		}

		/// <summary>
		/// Notifies the system that changes have been made inside these bounds.
		///
		/// This should be called by graphs whenever they are changed.
		/// It will cause off-mesh links to be updated, and it will also ensure <see cref="GraphModifier"/> events are callled.
		///
		/// The bounding box should cover the surface of all nodes that have been updated.
		/// It is fine to use a larger bounding box than necessary (even an infinite one), though this may be slower, since more off-mesh links need to be recalculated.
		/// You can even use an infinitely large bounding box if you don't want to bother calculating a more accurate one.
		/// You can also call this multiple times to dirty multiple bounding boxes.
		///
		/// When scanning the graph, this is called automatically - with the value from the <see cref="bounds"/> property - for all graphs after the scanning has finished.
		///
		/// Note: If possible, it is recommended to use <see cref="IGraphUpdateContext.DirtyBounds"/> or <see cref="IWorkItemContext.DirtyBounds"/> instead of this method.
		/// They currently do the same thing, but that may change in future versions.
		/// </summary>
		protected void DirtyBounds(Bounds bounds) => active.DirtyBounds(bounds);

		/// <summary>
		/// Moves the nodes in this graph.
		/// Multiplies all node positions by deltaMatrix.
		///
		/// For example if you want to move all your nodes in e.g a point graph 10 units along the X axis from the initial position
		/// <code>
		/// var graph = AstarPath.data.pointGraph;
		/// var m = Matrix4x4.TRS (new Vector3(10,0,0), Quaternion.identity, Vector3.one);
		/// graph.RelocateNodes (m);
		/// </code>
		///
		/// Note: For grid graphs, navmesh graphs and recast graphs it is recommended to
		/// use their custom overloads of the RelocateNodes method which take parameters
		/// for e.g center and nodeSize (and additional parameters) instead since
		/// they are both easier to use and are less likely to mess up pathfinding.
		///
		/// Warning: This method is lossy for PointGraphs, so calling it many times may
		/// cause node positions to lose precision. For example if you set the scale
		/// to 0 in one call then all nodes will be scaled/moved to the same point and
		/// you will not be able to recover their original positions. The same thing
		/// happens for other - less extreme - values as well, but to a lesser degree.
		/// </summary>
		public virtual void RelocateNodes (Matrix4x4 deltaMatrix) {
			AssertSafeToUpdateGraph();
			GetNodes(node => node.position = ((Int3)deltaMatrix.MultiplyPoint((Vector3)node.position)));
		}

		/// <summary>
		/// Lower bound on the squared distance from the given point to the closest node in this graph.
		///
		/// This is used to speed up searching for the closest node when there is more than one graph in the scene,
		/// by checking the graphs in order of increasing estimated distance to the point.
		///
		/// Implementors may return 0 at all times if it is hard to come up with a good lower bound.
		/// It is more important that this function is fast than that it is accurate.
		/// </summary>
		/// <param name="position">The position to check from</param>
		/// <param name="constraint">A constraint on which nodes are valid. This may or may not be used by the function. You may pass null if you consider all nodes valid.</param>
		public virtual float NearestNodeDistanceSqrLowerBound (Vector3 position, NNConstraint constraint = null) {
			// If the graph doesn't provide a way to calculate a lower bound, just return 0, since that is always a valid lower bound
			return 0;
		}

		/// <summary>
		/// Returns the nearest node to a position using the specified NNConstraint.
		///
		/// The returned <see cref="NNInfo"/> will contain both the closest node, and the closest point on the surface of that node.
		/// The distance is measured to the closest point on the surface of the node.
		///
		/// See: You can use <see cref="AstarPath.GetNearest(Vector3)"/> instead, if you want to check all graphs.
		///
		/// Version: Before 4.3.63, this method would not use the NNConstraint in all cases.
		/// </summary>
		/// <param name="position">The position to try to find the closest node to.</param>
		/// <param name="constraint">Used to limit which nodes are considered acceptable.
		///                   You may, for example, only want to consider walkable nodes.
		///                   If null, all nodes will be considered acceptable.</param>
		public NNInfo GetNearest (Vector3 position, NNConstraint constraint = null) {
			var maxDistanceSqr = constraint == null || constraint.constrainDistance ? active.maxNearestNodeDistanceSqr : float.PositiveInfinity;
			return GetNearest(position, constraint, maxDistanceSqr);
		}

		/// <summary>
		/// Nearest node to a position using the specified NNConstraint.
		///
		/// The returned <see cref="NNInfo"/> will contain both the closest node, and the closest point on the surface of that node.
		/// The distance is measured to the closest point on the surface of the node.
		///
		/// See: You can use <see cref="AstarPath.GetNearest"/> instead, if you want to check all graphs.
		/// </summary>
		/// <param name="position">The position to try to find the closest node to.</param>
		/// <param name="constraint">Used to limit which nodes are considered acceptable.
		///                   You may, for example, only want to consider walkable nodes.
		///                   If null, all nodes will be considered acceptable.</param>
		/// <param name="maxDistanceSqr">The maximum squared distance from the position to the node.
		///                       If the node is further away than this, the function will return an empty NNInfo.
		///                       You may pass infinity if you do not want to limit the distance.</param>
		public virtual NNInfo GetNearest (Vector3 position, NNConstraint constraint, float maxDistanceSqr) {
			// This is a default implementation and it is pretty slow
			// Graphs usually override this to provide faster and more specialised implementations

			float minDistSqr = maxDistanceSqr;
			GraphNode minNode = null;

			// Loop through all nodes and find the closest suitable node
			GetNodes(node => {
				float dist = (position-(Vector3)node.position).sqrMagnitude;

				if (dist < minDistSqr && (constraint == null || constraint.Suitable(node))) {
					minDistSqr = dist;
					minNode = node;
				}
			});

			return minNode != null ? new NNInfo(minNode, (Vector3)minNode.position, minDistSqr) : NNInfo.Empty;
		}

		/// <summary>
		/// Returns the nearest node to a position using the specified NNConstraint.
		/// Deprecated: Use GetNearest instead
		/// </summary>
		[System.Obsolete("Use GetNearest instead")]
		public NNInfo GetNearestForce (Vector3 position, NNConstraint constraint) {
			return GetNearest(position, constraint);
		}

		/// <summary>
		/// A random point on the graph.
		///
		/// If there are no suitable nodes in the graph, <see cref="NNInfo.Empty"/> will be returned.
		///
		/// <code>
		/// // Pick a random walkable point on the graph, sampled uniformly over the graph's surface
		/// var sample = AstarPath.active.graphs[0].RandomPointOnSurface(NNConstraint.Walkable);
		///
		/// // Use a random point on the surface of the node as the destination.
		/// var destination1 = sample.position;
		/// // Or use the center of the node as the destination
		/// var destination2 = (Vector3)sample.node.position;
		/// </code>
		///
		/// See: <see cref="GraphNode.RandomPointOnSurface"/>
		/// See: <see cref="PathUtilities.GetPointsOnNodes"/>
		/// See: wander (view in online documentation for working links)
		/// </summary>
		/// <param name="nnConstraint">Optionally set to constrain which nodes are allowed to be returned. If null, all nodes are allowed, including unwalkable ones.</param>
		/// <param name="highQuality">If true, this method is allowed to be more computationally heavy, in order to pick a random point more uniformly (based on the nodes' surface area).
		///        If false, this method should be fast as possible, but the distribution of sampled points may be a bit skewed. This setting only affects recast and navmesh graphs.</param>
		public virtual NNInfo RandomPointOnSurface (NNConstraint nnConstraint, bool highQuality = true) {
			// Use reservoir sampling to pick a random node
			GraphNode bestNode = null;
			var weight = 0f;
			GetNodes(node => {
				if (nnConstraint == null || nnConstraint.Suitable(node)) {
					var w = node.SurfaceArea();
					// Make sure the code works even for nodes that have zero surface area (like point nodes)
					if (w <= 0) w = 0.001f;
					weight += w;
					if (bestNode == null || Random.value < w / weight) {
						bestNode = node;
					}
				}
			});
			return bestNode != null ? new NNInfo(bestNode, bestNode.RandomPointOnSurface(), 0) : NNInfo.Empty;
		}

		/// <summary>
		/// Function for cleaning up references.
		/// This will be called on the same time as OnDisable on the gameObject which the AstarPath script is attached to (remember, not in the editor).
		/// Use for any cleanup code such as cleaning up static variables which otherwise might prevent resources from being collected.
		/// Use by creating a function overriding this one in a graph class, but always call base.OnDestroy () in that function.
		/// All nodes should be destroyed in this function otherwise a memory leak will arise.
		/// </summary>
		protected virtual void OnDestroy () {
			DestroyAllNodes();
			DisposeUnmanagedData();
		}

		/// <summary>
		/// Cleans up any unmanaged data that the graph has.
		/// Note: The graph has to stay valid after this. However it need not be in a scanned state.
		/// </summary>
		protected virtual void DisposeUnmanagedData () {
		}

		/// <summary>
		/// Destroys all nodes in the graph.
		/// Warning: This is an internal method. Unless you have a very good reason, you should probably not call it.
		/// </summary>
		protected virtual void DestroyAllNodes () {
			GetNodes(node => node.Destroy());
		}

		/// <summary>
		/// Captures a snapshot of a part of the graph, to allow restoring it later.
		///
		/// See: <see cref="AstarPath.Snapshot"/> for more details
		///
		/// If this graph type does not support taking snapshots, or if the bounding box does not intersect with the graph, this method returns null.
		/// </summary>
		public virtual IGraphSnapshot Snapshot(Bounds bounds) => null;

		/// <summary>
		/// Scan the graph.
		///
		/// Consider using AstarPath.Scan() instead since this function only scans this graph, and if you are using multiple graphs
		/// with connections between them, then it is better to scan all graphs at once.
		/// </summary>
		public void Scan () {
			active.Scan(this);
		}

		/// <summary>
		/// Internal method to scan the graph.
		///
		/// Deprecated: You should use ScanInternal(bool) instead.
		/// </summary>
		protected virtual IGraphUpdatePromise ScanInternal () {
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Internal method to scan the graph.
		/// Override this function to implement custom scanning logic.
		/// </summary>
		protected virtual IGraphUpdatePromise ScanInternal (bool async) {
			return ScanInternal();
		}

		/// <summary>
		/// Serializes graph type specific node data.
		/// This function can be overriden to serialize extra node information (or graph information for that matter)
		/// which cannot be serialized using the standard serialization.
		/// Serialize the data in any way you want and return a byte array.
		/// When loading, the exact same byte array will be passed to the DeserializeExtraInfo function.
		/// These functions will only be called if node serialization is enabled.
		/// </summary>
		protected virtual void SerializeExtraInfo (GraphSerializationContext ctx) {
		}

		/// <summary>
		/// Deserializes graph type specific node data.
		/// See: SerializeExtraInfo
		/// </summary>
		protected virtual void DeserializeExtraInfo (GraphSerializationContext ctx) {
		}

		/// <summary>
		/// Called after all deserialization has been done for all graphs.
		/// Can be used to set up more graph data which is not serialized
		/// </summary>
		protected virtual void PostDeserialization (GraphSerializationContext ctx) {
		}

		/// <summary>Draw gizmos for the graph</summary>
		public virtual void OnDrawGizmos (DrawingData gizmos, bool drawNodes, RedrawScope redrawScope) {
			if (!drawNodes) {
				return;
			}

			// This is a relatively slow default implementation.
			// subclasses of the base graph class may override
			// this method to draw gizmos in a more optimized way

			var hasher = new NodeHasher(active);
			GetNodes(node => hasher.HashNode(node));

			// Update the gizmo mesh if necessary
			if (!gizmos.Draw(hasher, redrawScope)) {
				using (var helper = GraphGizmoHelper.GetGizmoHelper(gizmos, active, hasher, redrawScope)) {
					if (helper.showSearchTree) helper.builder.PushLineWidth(2);
					GetNodes((System.Action<GraphNode>)helper.DrawConnections);
					if (helper.showSearchTree) helper.builder.PopLineWidth();
				}
			}

			if (active.showUnwalkableNodes) DrawUnwalkableNodes(gizmos, active.unwalkableNodeDebugSize, redrawScope);
		}

		protected void DrawUnwalkableNodes (DrawingData gizmos, float size, RedrawScope redrawScope) {
			var hasher = new DrawingData.Hasher();
			hasher.Add(this);

			GetNodes(node => {
				hasher.Add(node.Walkable);
				if (!node.Walkable) hasher.Add(node.position);
			});

			if (!gizmos.Draw(hasher, redrawScope)) {
				using (var builder = gizmos.GetBuilder(hasher)) {
					using (builder.WithColor(AstarColor.UnwalkableNode)) {
						GetNodes(node => {
							if (!node.Walkable) builder.SolidBox((Vector3)node.position, new Unity.Mathematics.float3(size, size, size));
						});
					}
				}
			}
		}

		#region IGraphInternals implementation
		string IGraphInternals.SerializedEditorSettings { get { return serializedEditorSettings; } set { serializedEditorSettings = value; } }
		void IGraphInternals.OnDestroy() => OnDestroy();
		void IGraphInternals.DisposeUnmanagedData() => DisposeUnmanagedData();
		void IGraphInternals.DestroyAllNodes() => DestroyAllNodes();
		IGraphUpdatePromise IGraphInternals.ScanInternal(bool async) => ScanInternal(async);
		void IGraphInternals.SerializeExtraInfo(GraphSerializationContext ctx) => SerializeExtraInfo(ctx);
		void IGraphInternals.DeserializeExtraInfo(GraphSerializationContext ctx) => DeserializeExtraInfo(ctx);
		void IGraphInternals.PostDeserialization(GraphSerializationContext ctx) => PostDeserialization(ctx);

		#endregion
	}
}

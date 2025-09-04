using UnityEngine;
using System.Collections.Generic;

// Empty namespace declaration to avoid errors in the free version
// Which does not have any classes in the RVO namespace
namespace Pathfinding.RVO {}

namespace Pathfinding {
	using Pathfinding.Jobs;
	using Pathfinding.Util;
	using Pathfinding.Pooling;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;

	[System.Serializable]
	/// <summary>Stores editor colors</summary>
	public class AstarColor : ISerializationCallbackReceiver {
		public Color _SolidColor;
		public Color _UnwalkableNode;
		public Color _BoundsHandles;

		public Color _ConnectionLowLerp;
		public Color _ConnectionHighLerp;

		public Color _MeshEdgeColor;

		/// <summary>
		/// Holds user set area colors.
		/// Use GetAreaColor to get an area color
		/// </summary>
		public Color[] _AreaColors;

		public static Color SolidColor = new Color(30/255f, 102/255f, 201/255f, 0.9F);
		public static Color UnwalkableNode = new Color(1, 0, 0, 0.5F);
		public static Color BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F);

		public static Color ConnectionLowLerp = new Color(0, 1, 0, 0.5F);
		public static Color ConnectionHighLerp = new Color(1, 0, 0, 0.5F);

		public static Color MeshEdgeColor = new Color(0, 0, 0, 0.5F);

		private static Color[] AreaColors = new Color[1];

		public static int ColorHash () {
			var hash = SolidColor.GetHashCode() ^ UnwalkableNode.GetHashCode() ^ BoundsHandles.GetHashCode() ^ ConnectionLowLerp.GetHashCode() ^ ConnectionHighLerp.GetHashCode() ^ MeshEdgeColor.GetHashCode();

			for (int i = 0; i < AreaColors.Length; i++) hash = 7*hash ^ AreaColors[i].GetHashCode();
			return hash;
		}

		/// <summary>
		/// Returns an color for an area, uses both user set ones and calculated.
		/// If the user has set a color for the area, it is used, but otherwise the color is calculated using AstarMath.IntToColor
		/// </summary>
		public static Color GetAreaColor (uint area) {
			if (area >= AreaColors.Length) return AstarMath.IntToColor((int)area, 1F);
			return AreaColors[(int)area];
		}

		/// <summary>
		/// Returns an color for a tag, uses both user set ones and calculated.
		/// If the user has set a color for the tag, it is used, but otherwise the color is calculated using AstarMath.IntToColor
		/// See: <see cref="AreaColors"/>
		/// </summary>
		public static Color GetTagColor (uint tag) {
			if (tag >= AreaColors.Length) return AstarMath.IntToColor((int)tag, 1F);
			return AreaColors[(int)tag];
		}

		/// <summary>
		/// Pushes all local variables out to static ones.
		/// This is done because that makes it so much easier to access the colors during Gizmo rendering
		/// and it has a positive performance impact as well (gizmo rendering is hot code).
		/// It is a bit ugly though, but oh well.
		/// </summary>
		public void PushToStatic () {
			_AreaColors  = _AreaColors ?? new Color[0];

			SolidColor = _SolidColor;
			UnwalkableNode = _UnwalkableNode;
			BoundsHandles = _BoundsHandles;
			ConnectionLowLerp = _ConnectionLowLerp;
			ConnectionHighLerp = _ConnectionHighLerp;
			MeshEdgeColor = _MeshEdgeColor;
			AreaColors = _AreaColors;
		}

		public void OnBeforeSerialize () {}

		public void OnAfterDeserialize () {
			// Patch bad initialization code in earlier versions that made it start out with a single transparent color.
			if (_AreaColors != null && _AreaColors.Length == 1 && _AreaColors[0] == default) {
				_AreaColors = new Color[0];
			}
		}

		public AstarColor () {
			// Set default colors
			_SolidColor = new Color(30/255f, 102/255f, 201/255f, 0.9F);
			_UnwalkableNode = new Color(1, 0, 0, 0.5F);
			_BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F);
			_ConnectionLowLerp = new Color(0, 1, 0, 0.5F);
			_ConnectionHighLerp = new Color(1, 0, 0, 0.5F);
			_MeshEdgeColor = new Color(0, 0, 0, 0.5F);
		}
	}


	/// <summary>
	/// Info about what a ray- or linecasts hit.
	///
	/// This is the return value of the <see cref="IRaycastableGraph.Linecast"/> methods.
	/// Some members will also be initialized even if nothing was hit, see the individual member descriptions for more info.
	///
	/// [Open online documentation to see images]
	/// </summary>
	public struct GraphHitInfo {
		/// <summary>
		/// Start of the segment/ray.
		/// Note that the point passed to the Linecast method will be clamped to the surface on the navmesh, but it will be identical when seen from above.
		/// </summary>
		public Vector3 origin;
		/// <summary>
		/// Hit point.
		///
		/// This is typically a point on the border of the navmesh.
		///
		/// In case no obstacle was hit then this will be set to the endpoint of the segment.
		///
		/// If the origin was inside an unwalkable node, then this will be set to the origin point.
		/// </summary>
		public Vector3 point;
		/// <summary>
		/// Node which contained the edge which was hit.
		/// If the linecast did not hit anything then this will be set to the last node along the line's path (the one which contains the endpoint).
		///
		/// For layered grid graphs the linecast will return true (i.e: no free line of sight) if, when walking the graph, we ended up at the right X,Z coordinate for the end node,
		/// but the end node was on a different level (e.g the floor below or above in a building). In this case no node edge was really hit so this field will still be null.
		///
		/// If the origin was inside an unwalkable node, then this field will be set to that node.
		///
		/// If no node could be found which contained the origin, then this field will be set to null.
		/// </summary>
		public GraphNode node;
		/// <summary>
		/// Where the tangent starts. <see cref="tangentOrigin"/> and <see cref="tangent"/> together actually describes the edge which was hit.
		/// [Open online documentation to see images]
		///
		/// If nothing was hit, this will be Vector3.zero.
		/// </summary>
		public Vector3 tangentOrigin;
		/// <summary>
		/// Tangent of the edge which was hit.
		/// [Open online documentation to see images]
		///
		/// If nothing was hit, this will be Vector3.zero.
		/// </summary>
		public Vector3 tangent;

		/// <summary>Distance from <see cref="origin"/> to <see cref="point"/></summary>
		public readonly float distance => (point-origin).magnitude;
	}

	/// <summary>
	/// Determines how to measure distances to the navmesh.
	///
	/// The default is a euclidean distance, which works well for most things.
	/// However, another option is to find nodes which are directly below a point.
	/// This is very useful if you have a character, and you want to find the closest node to the character's feet.
	/// Then projecting the distance so that we find the closest node as seen from above is a good idea.
	///
	/// See <see cref="projectionAxis"/> for more info.
	///
	/// [Open online documentation to see images]
	/// The default distance metric is euclidean distance. This is the same as the distance you would measure in 3D space.
	/// Shown in the image above are two parts of a navmesh in a side view. One part is colored blue, and one part colored red. Both are walkable.
	/// In the background you can see if points are closer to the blue part, or to the red part.
	/// You can also see a few query points in black, which show the closest point on the navmesh to that point.
	/// The dashed circle around the first query point shows all points which are at the same distance from the query point.
	///
	/// [Open online documentation to see images]
	/// When using <see cref="ClosestAsSeenFromAbove"/>, the distance along the up direction is ignored. You can see this in the image
	/// above. Note how all query points find their closest point directly below them.
	/// Notice in particular the difference in behavior for the query point on the slope.
	///
	/// [Open online documentation to see images]
	/// When using <see cref="ClosestAsSeenFromAboveSoft"/>, the distance along the up direction is instead scaled by <see cref="distanceScaleAlongProjectionDirection"/> (set to 0.2 by default).
	/// This makes it behave almost like <see cref="ClosestAsSeenFromAbove"/>, but it allows small deviations from just looking directly below the query point.
	/// The dashed diamond in the image is similarly the set of points equidistant from the query point.
	/// This mode is recommended for character movement since it finds points below the agent, but can better handle situations where the agent, for example, steps off a ledge.
	/// </summary>
	public struct DistanceMetric {
		/// <summary>
		/// Normal of the plane on which nodes will be projected before finding the closest point on them.
		///
		/// When zero, this has no effect.
		///
		/// When set to the special value (inf, inf, inf) then the graph's natural up direction will be used.
		///
		/// Often you do not want to find the closest point on a node in 3D space, but rather
		/// find for example the closest point on the node directly below the agent.
		///
		/// This allows you to project the nodes onto a plane before finding the closest point on them.
		/// For example, if you set this to Vector3.up, then the nodes will be projected onto the XZ plane.
		/// Running a GetNearest query will then find the closest node as seen from above.
		///
		/// [Open online documentation to see images]
		///
		/// This is more flexible, however. You can set the <see cref="distanceScaleAlongProjectionDirection"/> to any value (though usually somewhere between 0 and 1).
		/// With a value of 0, the closest node will be found as seen from above.
		/// When the distance is greater than 0, moving along the projectionAxis from the query point will only cost <see cref="distanceScaleAlongProjectionDirection"/> times the regular distance,
		/// but moving sideways will cost the normal amount.
		///
		/// [Open online documentation to see images]
		///
		/// A nice value to use is 0.2 for <see cref="distanceScaleAlongProjectionDirection"/>. This will make moving upwards or downwards (along the projection direction)
		/// only appear like 20% the original distance to the nearest node search.
		/// This allows you to find the closest position directly under the agent, if there is a navmesh directly under the agent, but also to search
		/// not directly below the agent if that is necessary.
		///
		/// Note: This is only supported by some graph types. The navmesh/recast graphs fully support this.
		/// The grid graphs, however, only support this if the direction is parallel to the natural 'up' direction of the graph.
		/// The grid graphs will search as if the direction is in the graph's up direction if this value is anything other than Vector3.zero.
		/// Point graphs do not support this field at all, and will completely ignore it.
		///
		/// Note: On recast/navmesh graphs there is a performance penalty when using this feature with a direction which is not parallel to the natural up direction of the graph.
		/// Typically you should only set it to such values if you have a spherical or non-planar world (see spherical) (view in online documentation for working links).
		/// </summary>
		public Vector3 projectionAxis;

		/// <summary>
		/// Distance scaling along the <see cref="projectionAxis"/>.
		///
		/// See: <see cref="projectionAxis"/> for details
		/// </summary>
		public float distanceScaleAlongProjectionDirection;

		/// <summary>True when using the ClosestAsSeenFromAbove or ClosestAsSeenFromAboveSoft modes</summary>
		public bool isProjectedDistance => projectionAxis != Vector3.zero;

		/// <summary>
		/// Returns a DistanceMetric which will find the closest node in euclidean 3D space.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="projectionAxis"/>
		/// </summary>
		public static readonly DistanceMetric Euclidean = new DistanceMetric { projectionAxis = Vector3.zero, distanceScaleAlongProjectionDirection = 0 };

		/// <summary>
		/// Returns a DistanceMetric which will usually find the closest node as seen from above, but allows some sideways translation if that gives us a node which is significantly closer.
		///
		/// The "upwards" direction will be set to the graph's natural up direction.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="projectionAxis"/>
		///
		/// Note: This is only supported by some graph types. The navmesh/recast/grid graph fully support this.
		/// Point graphs do not support this field at all, and will completely ignore it.
		///
		/// See: <see cref="ClosestAsSeenFromAbove(Vector3)"/>
		/// </summary>
		public static DistanceMetric ClosestAsSeenFromAboveSoft () => new DistanceMetric { projectionAxis = Vector3.positiveInfinity, distanceScaleAlongProjectionDirection = 0.2f };

		/// <summary>
		/// Returns a DistanceMetric which will usually find the closest node as seen from above, but allows some sideways translation if that gives us a node which is significantly closer.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="projectionAxis"/>
		///
		/// Note: This is only supported by some graph types. The navmesh/recast graphs fully support this.
		/// The grid graphs, however, only support this if the direction is parallel to the natural 'up' direction of the graph.
		/// The grid graphs will search as if the direction is in the graph's up direction if this value is anything other than Vector3.zero.
		/// Point graphs do not support this field at all, and will completely ignore it.
		///
		/// Note: On recast/navmesh graphs there is a performance penalty when using this feature with a direction which is not parallel to the natural up direction of the graph.
		/// Typically you should only set it to such values if you have a spherical or non-planar world (see spherical) (view in online documentation for working links).
		///
		/// Note: If you want to use the graph's natural up direction, use the overload which does not take any parameters.
		/// </summary>
		/// <param name="up">Defines what 'from above' means. Usually this should be the same as the natural up direction of the graph. Does not need to be normalized.</param>
		public static DistanceMetric ClosestAsSeenFromAboveSoft (Vector3 up) => new DistanceMetric { projectionAxis = up, distanceScaleAlongProjectionDirection = 0.2f };

		/// <summary>
		/// Returns a DistanceMetric which will find the closest node as seen from above.
		///
		/// The "upwards" direction will be set to the graph's natural up direction.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="projectionAxis"/>
		///
		/// Note: This is only supported by some graph types. The navmesh/recast/grid graph fully support this.
		/// Point graphs do not support this field at all, and will completely ignore it.
		///
		/// See: <see cref="ClosestAsSeenFromAbove(Vector3)"/>
		/// </summary>
		public static DistanceMetric ClosestAsSeenFromAbove () => new DistanceMetric { projectionAxis = Vector3.positiveInfinity, distanceScaleAlongProjectionDirection = 0.0f };

		/// <summary>
		/// Returns a DistanceMetric which will find the closest node as seen from above.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="projectionAxis"/>
		///
		/// Note: This is only supported by some graph types. The navmesh/recast graphs fully support this.
		/// The grid graphs, however, only support this if the direction is parallel to the natural 'up' direction of the graph.
		/// The grid graphs will search as if the direction is in the graph's up direction if this value is anything other than Vector3.zero.
		/// Point graphs do not support this field at all, and will completely ignore it.
		///
		/// Note: On recast/navmesh graphs there is a performance penalty when using this feature with a direction which is not parallel to the natural up direction of the graph.
		/// Typically you should only set it to such values if you have a spherical or non-planar world (see spherical) (view in online documentation for working links).
		///
		/// Note: If you want to use the graph's natural up direction, use the overload which does not take any parameters.
		/// </summary>
		/// <param name="up">Defines what 'from above' means. Usually this should be the same as the natural up direction of the graph. Does not need to be normalized.</param>
		public static DistanceMetric ClosestAsSeenFromAbove (Vector3 up) => new DistanceMetric { projectionAxis = up, distanceScaleAlongProjectionDirection = 0.0f };
	}

	/// <summary>Nearest node constraint. Constrains which nodes will be returned by the <see cref="AstarPath.GetNearest"/> function</summary>
	public class NNConstraint {
		/// <summary>
		/// Graphs treated as valid to search on.
		/// This is a bitmask meaning that bit 0 specifies whether or not the first graph in the graphs list should be able to be included in the search,
		/// bit 1 specifies whether or not the second graph should be included and so on.
		/// <code>
		/// // Enables the first and third graphs to be included, but not the rest
		/// myNNConstraint.graphMask = (1 << 0) | (1 << 2);
		/// </code>
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
		/// Note: This does only affect which nodes are returned from a <see cref="AstarPath.GetNearest"/> call, if a valid graph is connected to an invalid graph using a node link then it might be searched anyway.
		///
		/// See: <see cref="AstarPath.GetNearest"/>
		/// See: <see cref="SuitableGraph"/>
		/// See: bitmasks (view in online documentation for working links)
		/// </summary>
		public GraphMask graphMask = -1;

		/// <summary>Only treat nodes in the area <see cref="area"/> as suitable. Does not affect anything if <see cref="area"/> is less than 0 (zero)</summary>
		public bool constrainArea;

		/// <summary>Area ID to constrain to. Will not affect anything if less than 0 (zero) or if <see cref="constrainArea"/> is false</summary>
		public int area = -1;

		/// <summary>
		/// Determines how to measure distances to the navmesh.
		///
		/// The default is a euclidean distance, which works well for most things.
		///
		/// See: <see cref="DistanceMetric"/>
		/// </summary>
		public DistanceMetric distanceMetric;

		/// <summary>Constrain the search to only walkable or unwalkable nodes depending on <see cref="walkable"/>.</summary>
		public bool constrainWalkability = true;

		/// <summary>
		/// Only search for walkable or unwalkable nodes if <see cref="constrainWalkability"/> is enabled.
		/// If true, only walkable nodes will be searched for, otherwise only unwalkable nodes will be searched for.
		/// Does not affect anything if <see cref="constrainWalkability"/> if false.
		/// </summary>
		public bool walkable = true;

		/// <summary>
		/// if available, do an XZ check instead of checking on all axes.
		/// The navmesh/recast graph as well as the grid/layered grid graph supports this.
		///
		/// This can be important on sloped surfaces. See the image below in which the closest point for each blue point is queried for:
		/// [Open online documentation to see images]
		///
		/// Deprecated: Use <see cref="distanceMetric"/> = DistanceMetric.ClosestAsSeenFromAbove() instead
		/// </summary>
		[System.Obsolete("Use distanceMetric = DistanceMetric.ClosestAsSeenFromAbove() instead")]
		public bool distanceXZ {
			get {
				return distanceMetric.isProjectedDistance && distanceMetric.distanceScaleAlongProjectionDirection == 0;
			}
			set {
				if (value) {
					distanceMetric = DistanceMetric.ClosestAsSeenFromAbove();
				} else {
					distanceMetric = DistanceMetric.Euclidean;
				}
			}
		}

		/// <summary>
		/// Sets if tags should be constrained.
		/// See: <see cref="tags"/>
		/// </summary>
		public bool constrainTags = true;

		/// <summary>
		/// Nodes which have any of these tags set are suitable.
		/// This is a bitmask, i.e bit 0 indicates that tag 0 is good, bit 3 indicates tag 3 is good etc.
		/// See: <see cref="constrainTags"/>
		/// See: <see cref="graphMask"/>
		/// See: bitmasks (view in online documentation for working links)
		/// </summary>
		public int tags = -1;

		/// <summary>
		/// Constrain distance to node.
		/// Uses distance from <see cref="AstarPath.maxNearestNodeDistance"/>.
		/// If this is false, it will completely ignore the distance limit.
		///
		/// If there are no suitable nodes within the distance limit then the search will terminate with a null node as a result.
		/// Note: This value is not used in this class, it is used by the AstarPath.GetNearest function.
		/// </summary>
		public bool constrainDistance = true;

		/// <summary>
		/// Returns whether or not the graph conforms to this NNConstraint's rules.
		/// Note that only the first 31 graphs are considered using this function.
		/// If the <see cref="graphMask"/> has bit 31 set (i.e the last graph possible to fit in the mask), all graphs
		/// above index 31 will also be considered suitable.
		/// </summary>
		public virtual bool SuitableGraph (int graphIndex, NavGraph graph) {
			return graphMask.Contains(graphIndex);
		}

		/// <summary>Returns whether or not the node conforms to this NNConstraint's rules</summary>
		public virtual bool Suitable (GraphNode node) {
			if (constrainWalkability && node.Walkable != walkable) return false;

			if (constrainArea && area >= 0 && node.Area != area) return false;

			if (constrainTags && ((tags >> (int)node.Tag) & 0x1) == 0) return false;

			return true;
		}

		public void UseSettings (PathRequestSettings settings) {
			graphMask = settings.graphMask;
			constrainTags = true;
			tags = settings.traversableTags;
			constrainWalkability = true;
			walkable = true;
		}

		/// <summary>
		/// The default NNConstraint.
		/// Equivalent to new NNConstraint ().
		/// This NNConstraint has settings which works for most, it only finds walkable nodes
		/// and it constrains distance set by A* Inspector -> Settings -> Max Nearest Node Distance
		///
		/// Deprecated: Use <see cref="NNConstraint.Walkable"/> instead. It is equivalent, but the name is more descriptive.
		/// </summary>
		[System.Obsolete("Use NNConstraint.Walkable instead. It is equivalent, but the name is more descriptive")]
		public static NNConstraint Default {
			get {
				return new NNConstraint();
			}
		}

		/// <summary>
		/// An NNConstraint which filters out unwalkable nodes.
		/// This is the most commonly used NNConstraint.
		///
		/// It also constrains the nearest node to be within the distance set by A* Inspector -> Settings -> Max Nearest Node Distance
		/// </summary>
		public static NNConstraint Walkable {
			get {
				return new NNConstraint();
			}
		}

		/// <summary>Returns a constraint which does not filter the results</summary>
		public static NNConstraint None {
			get {
				return new NNConstraint {
						   constrainWalkability = false,
						   constrainArea = false,
						   constrainTags = false,
						   constrainDistance = false,
						   graphMask = -1,
				};
			}
		}

		/// <summary>Default constructor. Equals to the property <see cref="Default"/></summary>
		public NNConstraint () {
		}
	}

	/// <summary>
	/// A special NNConstraint which can use different logic for the start node and end node in a path.
	/// A PathNNConstraint can be assigned to the Path.nnConstraint field, the path will first search for the start node, then it will call <see cref="SetStart"/> and proceed with searching for the end node (nodes in the case of a MultiTargetPath).
	/// The default PathNNConstraint will constrain the end point to lie inside the same area as the start point.
	/// </summary>
	public class PathNNConstraint : NNConstraint {
		public static new PathNNConstraint Walkable {
			get {
				return new PathNNConstraint {
						   constrainArea = true
				};
			}
		}

		/// <summary>Called after the start node has been found. This is used to get different search logic for the start and end nodes in a path</summary>
		public virtual void SetStart (GraphNode node) {
			if (node != null) {
				area = (int)node.Area;
			} else {
				constrainArea = false;
			}
		}
	}

	/// <summary>
	/// Result of a nearest node query.
	///
	/// See: <see cref="AstarPath.GetNearest"/>
	/// </summary>
	public readonly struct NNInfo {
		/// <summary>
		/// Closest node.
		/// May be null if there was no node which satisfied all constraints of the search.
		/// </summary>
		public readonly GraphNode node;

		/// <summary>
		/// Closest point on the navmesh.
		/// This is the query position clamped to the closest point on the <see cref="node"/>.
		///
		/// If node is null, then this value is (+inf, +inf, +inf).
		/// </summary>
		public readonly Vector3 position;

		/// <summary>
		/// Cost for picking this node as the closest node.
		/// This is typically the squared distance from the query point to <see cref="position"/>.
		///
		/// However, it may be different if the <see cref="NNConstraint"/> used a different cost function.
		/// For example, if <see cref="NNConstraint.distanceMetric"/> is <see cref="DistanceMetric.ClosestAsSeenFromAbove()"/>,
		/// then this value will be the squared distance in the XZ plane.
		///
		/// This value is not guaranteed to be smaller or equal to the squared euclidean distance from the query point to <see cref="position"/>.
		/// In particular for a navmesh/recast graph with a <see cref="DistanceMetric.ClosestAsSeenFromAboveSoft"/> NNConstraint it may
		/// be slightly greater for some configurations. This is fine because we are only using this value for the rough
		/// distance limit performanced by <see cref="AstarPath.maxNearestNodeDistance"/>, and it's not a problem if it is slightly inaccurate.
		///
		/// See: <see cref="NNConstraint.distanceMetric"/>
		///
		/// If <see cref="node"/> is null, then this value is positive infinity.
		/// </summary>
		public readonly float distanceCostSqr;

		/// <summary>
		/// Closest point on the navmesh.
		/// Deprecated: This field has been renamed to <see cref="position"/>
		/// </summary>
		[System.Obsolete("This field has been renamed to 'position'", true)]
		public Vector3 clampedPosition {
			get {
				return position;
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public NNInfo (GraphNode node, Vector3 position, float distanceCostSqr) {
			this.node = node;
			if (node == null) {
				// Guarantee that a null node always gives specific values for position and distanceCostSqr
				this.position = Vector3.positiveInfinity;
				this.distanceCostSqr = float.PositiveInfinity;
			} else {
				this.position = position;
				this.distanceCostSqr = distanceCostSqr;
			}
		}

		public static readonly NNInfo Empty = new NNInfo(null, Vector3.positiveInfinity, float.PositiveInfinity);

		public static explicit operator Vector3(NNInfo ob) {
			return ob.position;
		}

		public static explicit operator GraphNode(NNInfo ob) {
			return ob.node;
		}
	}

	/// <summary>Info about where in the scanning process a graph is</summary>
	public enum ScanningStage {
		PreProcessingGraphs,
		PreProcessingGraph,
		ScanningGraph,
		PostProcessingGraph,
		FinishingScans,
	}

	/// <summary>
	/// Progress info for e.g a progressbar.
	/// Used by the scan functions in the project
	/// See: <see cref="AstarPath.ScanAsync"/>
	/// </summary>
	public readonly struct Progress {
		/// <summary>Current progress as a value between 0 and 1</summary>
		public readonly float progress;
		internal readonly ScanningStage stage;
		internal readonly int graphIndex;
		internal readonly int graphCount;

		public Progress (float progress, ScanningStage stage, int graphIndex = 0, int graphCount = 0) {
			this.progress = progress;
			this.stage = stage;
			this.graphIndex = graphIndex;
			this.graphCount = graphCount;
		}

		public Progress MapTo (float min, float max) {
			return new Progress(Mathf.Lerp(min, max, progress), stage, graphIndex, graphCount);
		}

		public override string ToString () {
			var s = progress.ToString("0%") + " ";
			switch (stage) {
			case ScanningStage.PreProcessingGraphs:
				s += "Pre-processing graphs";
				break;
			case ScanningStage.PreProcessingGraph:
				s += "Pre-processing graph " + (graphIndex+1) + " of " + graphCount;
				break;
			case ScanningStage.ScanningGraph:
				s += "Scanning graph " + (graphIndex+1) + " of " + graphCount;
				break;
			case ScanningStage.PostProcessingGraph:
				s += "Post-processing graph " + (graphIndex+1) + " of " + graphCount;
				break;
			case ScanningStage.FinishingScans:
				s += "Finalizing graph scans";
				break;
			}
			return s;
		}
	}

	/// <summary>Graphs which can be updated during runtime</summary>
	public interface IUpdatableGraph {
		/// <summary>
		/// Schedules a number of graph updates.
		///
		/// This method should return a promise. It should not execute the graph updates immediately.
		/// The Apply method on the promise will be called when the graph updates should be applied.
		/// In the meantime, the graph may perform calculations using e.g. the unity job system.
		///
		/// Notes to implementators.
		/// This function should (in order):
		/// -# Call o.WillUpdateNode on the GUO for every node it will update, it is important that this is called BEFORE any changes are made to the nodes.
		/// -# Update walkabilty using special settings such as the usePhysics flag used with the GridGraph.
		/// -# Call Apply on the GUO for every node which should be updated with the GUO.
		/// -# Update connectivity info if appropriate (GridGraphs updates connectivity, but most other graphs don't since then the connectivity cannot be recovered later).
		///
		/// It is guaranteed that the Apply function will be called on the returned promise before this function is called again.
		///
		/// Null may be returned if no updates need to be done to the graph.
		/// </summary>
		IGraphUpdatePromise ScheduleGraphUpdates(List<GraphUpdateObject> graphUpdates);
	}

	/// <summary>Info about if a graph update has been applied or not</summary>
	public enum GraphUpdateStage {
		/// <summary>
		/// The graph update object has been created, but not used for anything yet.
		/// This is the default value.
		/// </summary>
		Created,
		/// <summary>The graph update has been sent to the pathfinding system and is scheduled to be applied to the graphs</summary>
		Pending,
		/// <summary>The graph update has been applied to all graphs</summary>
		Applied,
		/// <summary>
		/// The graph update has been aborted and will not be applied.
		/// This can happen if the AstarPath component is destroyed while a graph update is queued to be applied.
		/// </summary>
		Aborted,
	}

	/// <summary>
	/// Represents a collection of settings used to update nodes in a specific region of a graph.
	/// See: AstarPath.UpdateGraphs
	/// See: graph-updates (view in online documentation for working links)
	/// </summary>
	public class GraphUpdateObject {
		/// <summary>
		/// The bounds to update nodes within.
		/// Defined in world space.
		/// </summary>
		public Bounds bounds;

		/// <summary>
		/// Use physics checks to update nodes.
		/// When updating a grid graph and this is true, the nodes' position and walkability will be updated using physics checks
		/// with settings from "Collision Testing" and "Height Testing".
		///
		/// When updating a PointGraph, setting this to true will make it re-evaluate all connections in the graph which passes through the <see cref="bounds"/>.
		///
		/// This has no effect when updating GridGraphs if <see cref="modifyWalkability"/> is turned on.
		/// You should not combine <see cref="updatePhysics"/> and <see cref="modifyWalkability"/>.
		///
		/// On RecastGraphs, having this enabled will trigger a complete recalculation of all tiles intersecting the bounds.
		/// This is quite slow (but powerful). If you only want to update e.g penalty on existing nodes, leave it disabled.
		/// </summary>
		public bool updatePhysics = true;

		/// <summary>
		/// Reset penalties to their initial values when updating grid graphs and <see cref="updatePhysics"/> is true.
		/// If you want to keep old penalties even when you update the graph you may want to disable this option.
		///
		/// The images below shows two overlapping graph update objects, the right one happened to be applied before the left one. They both have updatePhysics = true and are
		/// set to increase the penalty of the nodes by some amount.
		///
		/// The first image shows the result when resetPenaltyOnPhysics is false. Both penalties are added correctly.
		/// [Open online documentation to see images]
		///
		/// This second image shows when resetPenaltyOnPhysics is set to true. The first GUO is applied correctly, but then the second one (the left one) is applied
		/// and during its updating, it resets the penalties first and then adds penalty to the nodes. The result is that the penalties from both GUOs are not added together.
		/// The green patch in at the border is there because physics recalculation (recalculation of the position of the node, checking for obstacles etc.) affects a slightly larger
		/// area than the original GUO bounds because of the Grid Graph -> Collision Testing -> Diameter setting (it is enlarged by that value). So some extra nodes have their penalties reset.
		///
		/// [Open online documentation to see images]
		///
		/// Bug: Not working with burst
		/// </summary>
		public bool resetPenaltyOnPhysics = true;

		/// <summary>
		/// Update Erosion for GridGraphs.
		/// When enabled, erosion will be recalculated for grid graphs
		/// after the GUO has been applied.
		///
		/// In the below image you can see the different effects you can get with the different values.
		/// The first image shows the graph when no GUO has been applied. The blue box is not identified as an obstacle by the graph, the reason
		/// there are unwalkable nodes around it is because there is a height difference (nodes are placed on top of the box) so erosion will be applied (an erosion value of 2 is used in this graph).
		/// The orange box is identified as an obstacle, so the area of unwalkable nodes around it is a bit larger since both erosion and collision has made
		/// nodes unwalkable.
		/// The GUO used simply sets walkability to true, i.e making all nodes walkable.
		///
		/// [Open online documentation to see images]
		///
		/// When updateErosion=True, the reason the blue box still has unwalkable nodes around it is because there is still a height difference
		/// so erosion will still be applied. The orange box on the other hand has no height difference and all nodes are set to walkable.
		///
		/// When updateErosion=False, all nodes walkability are simply set to be walkable in this example.
		///
		/// See: Pathfinding.GridGraph
		///
		/// Bug: Not working with burst
		/// </summary>
		public bool updateErosion = true;

		/// <summary>
		/// NNConstraint to use.
		/// The Pathfinding.NNConstraint.SuitableGraph function will be called on the NNConstraint to enable filtering of which graphs to update.
		/// Note: As the Pathfinding.NNConstraint.SuitableGraph function is A* Pathfinding Project Pro only, this variable doesn't really affect anything in the free version.
		/// </summary>
		public NNConstraint nnConstraint = NNConstraint.None;

		/// <summary>
		/// Penalty to add to the nodes.
		/// A penalty of 1000 is equivalent to the cost of moving 1 world unit.
		/// </summary>
		public int addPenalty;

		/// <summary>
		/// If true, all nodes' walkable variable will be set to <see cref="setWalkability"/>.
		/// It is not recommended to combine this with <see cref="updatePhysics"/> since then you will just overwrite
		/// what <see cref="updatePhysics"/> calculated.
		/// </summary>
		public bool modifyWalkability;

		/// <summary>If <see cref="modifyWalkability"/> is true, the nodes' walkable variable will be set to this value</summary>
		public bool setWalkability;

		/// <summary>If true, all nodes' tag will be set to <see cref="setTag"/></summary>
		public bool modifyTag;

		/// <summary>If <see cref="modifyTag"/> is true, all nodes' tag will be set to this value</summary>
		public PathfindingTag setTag;

		/// <summary>
		/// Track which nodes are changed and save backup data.
		/// Used internally to revert changes if needed.
		///
		/// Deprecated: This field does not do anything anymore. Use <see cref="AstarPath.Snapshot"/> instead.
		/// </summary>
		[System.Obsolete("This field does not do anything anymore. Use AstarPath.Snapshot instead.")]
		public bool trackChangedNodes;

		/// <summary>
		/// A shape can be specified if a bounds object does not give enough precision.
		/// Note that if you set this, you should set the bounds so that it encloses the shape
		/// because the bounds will be used as an initial fast check for which nodes that should
		/// be updated.
		/// </summary>
		public GraphUpdateShape shape;

		/// <summary>
		/// Info about if a graph update has been applied or not.
		/// Either an enum (see STAGE_CREATED and associated constants)
		/// or a non-negative count of the number of graphs that are waiting to apply this graph update.
		/// </summary>
		internal int internalStage = STAGE_CREATED;

		internal const int STAGE_CREATED = -1;
		internal const int STAGE_PENDING = -2;
		internal const int STAGE_ABORTED = -3;
		internal const int STAGE_APPLIED = 0;

		/// <summary>Info about if a graph update has been applied or not</summary>
		public GraphUpdateStage stage {
			get {
				switch (internalStage) {
				case STAGE_CREATED:
					return GraphUpdateStage.Created;
				case STAGE_APPLIED:
					return GraphUpdateStage.Applied;
				case STAGE_ABORTED:
					return GraphUpdateStage.Aborted;
				// Positive numbers means it is currently being applied, so it is also pending.
				default:
				case STAGE_PENDING:
					return GraphUpdateStage.Pending;
				}
			}
		}

		/// <summary>Should be called on every node which is updated with this GUO before it is updated.</summary>
		/// <param name="node">The node to save fields for. If null, nothing will be done</param>
		public virtual void WillUpdateNode (GraphNode node) {
		}

		/// <summary>
		/// Reverts penalties and flags (which includes walkability) on every node which was updated using this GUO.
		/// Data for reversion is only saved if <see cref="trackChangedNodes"/> is true.
		///
		/// See: blocking (view in online documentation for working links)
		/// See: <see cref="GraphUpdateUtilities.UpdateGraphsNoBlock"/>
		///
		/// Deprecated: Use <see cref="AstarPath.Snapshot"/> instead
		/// </summary>
		[System.Obsolete("Use AstarPath.Snapshot instead", true)]
		public virtual void RevertFromBackup () {}

		/// <summary>
		/// Updates the specified node using this GUO's settings.
		///
		/// Note: Some graphs may call <see cref="ApplyJob"/> instead, for better performance.
		/// </summary>
		public virtual void Apply (GraphNode node) {
			if (shape == null || shape.Contains(node)) {
				// Update penalty and walkability
				node.Penalty = (uint)(node.Penalty+addPenalty);
				if (modifyWalkability) {
					node.Walkable = setWalkability;
				}

				// Update tags
				if (modifyTag) node.Tag = (uint)setTag;
			}
		}

		/// <summary>Provides burst-readable data to a graph update job</summary>
		public struct GraphUpdateData {
			public NativeArray<Vector3> nodePositions;
			public NativeArray<uint> nodePenalties;
			public NativeArray<bool> nodeWalkable;
			public NativeArray<int> nodeTags;
			public NativeArray<float4> nodeNormals;
			/// <summary>
			/// Node indices to update.
			/// Remaining nodes should be left alone.
			/// Additionally, if math.any(nodeNormals[i]) is false, then the node should not be updated, since it is not a valid node.
			/// </summary>
			public NativeArray<int> nodeIndices;
		};

		/// <summary>Job for applying a graph update object</summary>
		[BurstCompile]
		public struct JobGraphUpdate : IJob {
			public GraphUpdateShape.BurstShape shape;
			public GraphUpdateData data;

			public Bounds bounds;
			public int penaltyDelta;
			public bool modifyWalkability;
			public bool walkabilityValue;
			public bool modifyTag;
			public int tagValue;

			public void Execute () {
				for (int i = 0; i < data.nodeIndices.Length; i++) {
					var node = data.nodeIndices[i];
					if (!math.any(data.nodeNormals[node])) continue;

					if (bounds.Contains(data.nodePositions[node]) && shape.Contains(data.nodePositions[node])) {
						data.nodePenalties[node] += (uint)penaltyDelta;
						if (modifyWalkability) data.nodeWalkable[node] = walkabilityValue;
						if (modifyTag) data.nodeTags[node] = tagValue;
					}
				}
			}
		};

		/// <summary>
		/// Update a set of nodes using this GUO's settings.
		/// This is far more efficient since it can utilize the Burst compiler.
		///
		/// This method may be called by graph generators instead of the <see cref="Apply"/> method to update the graph more efficiently.
		/// </summary>
		public virtual void ApplyJob (GraphUpdateData data, JobDependencyTracker dependencyTracker) {
			if (addPenalty == 0 && !modifyWalkability && !modifyTag) return;

			new JobGraphUpdate {
				shape = shape != null ? new GraphUpdateShape.BurstShape(shape, Allocator.Persistent) : GraphUpdateShape.BurstShape.Everything,
				data = data,
				bounds = bounds,
				penaltyDelta = addPenalty,
				modifyWalkability = modifyWalkability,
				walkabilityValue = setWalkability,
				modifyTag = modifyTag,
				tagValue = (int)setTag.value,
			}.Schedule(dependencyTracker);
		}

		public GraphUpdateObject () {
		}

		/// <summary>Creates a new GUO with the specified bounds</summary>
		public GraphUpdateObject (Bounds b) {
			bounds = b;
		}
	}

	/// <summary>Graph which has a well defined transformation from graph space to world space</summary>
	public interface ITransformedGraph {
		GraphTransform transform { get; }
	}

	/// <summary>Graph which supports the Linecast method</summary>
	public interface IRaycastableGraph {
		/// <summary>
		/// Checks if the straight line of sight between the two points on the graph is obstructed.
		///
		/// Returns: True if an obstacle was hit, and false otherwise.
		/// </summary>
		/// <param name="start">The start point of the raycast.</param>
		/// <param name="end">The end point of the raycast.</param>
		bool Linecast(Vector3 start, Vector3 end);
		/// <summary>Deprecated:</summary>
		[System.Obsolete]
		bool Linecast(Vector3 start, Vector3 end, GraphNode startNodeHint);
		/// <summary>Deprecated:</summary>
		[System.Obsolete]
		bool Linecast(Vector3 start, Vector3 end, GraphNode startNodeHint, out GraphHitInfo hit);
		/// <summary>
		/// Checks if the straight line of sight between the two points on the graph is obstructed.
		///
		/// Returns: True if an obstacle was hit, and false otherwise.
		/// </summary>
		/// <param name="start">The start point of the raycast.</param>
		/// <param name="end">The end point of the raycast.</param>
		/// <param name="startNodeHint">If you know which node contains the start point, you may pass it here to save a GetNearest call. Otherwise, pass null. If the start point is not actually inside the give node, you may get different behavior on different graph types. Some will clamp the start point to the surface of the hint node, and some will ignore the hint parameter completely.</param>
		/// <param name="hit">Additional information about what was hit.</param>
		/// <param name="trace">If you supply a list, it will be filled with all nodes that the linecast traversed. You may pass null if you don't care about this.</param>
		/// <param name="filter">You may supply a callback to indicate which nodes should be considered unwalkable. Note that already unwalkable nodes cannot be made walkable in this way.</param>
		bool Linecast(Vector3 start, Vector3 end, GraphNode startNodeHint, out GraphHitInfo hit, List<GraphNode> trace, System.Func<GraphNode, bool> filter = null);
		/// <summary>
		/// Checks if the straight line of sight between the two points on the graph is obstructed.
		///
		/// Returns: True if an obstacle was hit, and false otherwise.
		/// </summary>
		/// <param name="start">The start point of the raycast.</param>
		/// <param name="end">The end point of the raycast.</param>
		/// <param name="hit">Additional information about what was hit.</param>
		/// <param name="trace">If you supply a list, it will be filled with all nodes that the linecast traversed. You may pass null if you don't care about this.</param>
		/// <param name="filter">You may supply a callback to indicate which nodes should be considered unwalkable. Note that already unwalkable nodes cannot be made walkable in this way.</param>
		bool Linecast(Vector3 start, Vector3 end, out GraphHitInfo hit, List<GraphNode> trace = null, System.Func<GraphNode, bool> filter = null);
	}

	/// <summary>
	/// Integer Rectangle.
	/// Uses an inclusive coordinate range.
	///
	/// Works almost like UnityEngine.Rect but with integer coordinates
	/// </summary>
	[System.Serializable]
	public struct IntRect {
		public int xmin, ymin, xmax, ymax;

		public IntRect (int xmin, int ymin, int xmax, int ymax) {
			this.xmin = xmin;
			this.xmax = xmax;
			this.ymin = ymin;
			this.ymax = ymax;
		}

		public bool Contains (int x, int y) {
			return !(x < xmin || y < ymin || x > xmax || y > ymax);
		}

		public bool Contains (IntRect other) {
			return xmin <= other.xmin && xmax >= other.xmax && ymin <= other.ymin && ymax >= other.ymax;
		}

		public Vector2Int Min {
			get {
				return new Vector2Int(xmin, ymin);
			}
		}

		public Vector2Int Max {
			get {
				return new Vector2Int(xmax, ymax);
			}
		}

		public int Width {
			get {
				return xmax-xmin+1;
			}
		}

		public int Height {
			get {
				return ymax-ymin+1;
			}
		}

		public int Area {
			get {
				return Width * Height;
			}
		}

		/// <summary>
		/// Returns if this rectangle is valid.
		/// An invalid rect could have e.g xmin > xmax.
		/// Rectangles are valid iff they contain at least one point.
		/// </summary>
		// TODO: Make into property
		public bool IsValid () {
			return xmin <= xmax && ymin <= ymax;
		}

		public static bool operator == (IntRect a, IntRect b) {
			return a.xmin == b.xmin && a.xmax == b.xmax && a.ymin == b.ymin && a.ymax == b.ymax;
		}

		public static bool operator != (IntRect a, IntRect b) {
			return a.xmin != b.xmin || a.xmax != b.xmax || a.ymin != b.ymin || a.ymax != b.ymax;
		}

		public static explicit operator Rect(IntRect r) => new Rect(r.xmin, r.ymin, r.Width, r.Height);

		public override bool Equals (System.Object obj) {
			if (!(obj is IntRect)) return false;
			var rect = (IntRect)obj;

			return xmin == rect.xmin && xmax == rect.xmax && ymin == rect.ymin && ymax == rect.ymax;
		}

		public override int GetHashCode () {
			return xmin*131071 ^ xmax*3571 ^ ymin*3109 ^ ymax*7;
		}

		/// <summary>
		/// Returns the intersection rect between the two rects.
		/// The intersection rect is the area which is inside both rects.
		/// If the rects do not have an intersection, an invalid rect is returned.
		/// See: IsValid
		/// </summary>
		public static IntRect Intersection (IntRect a, IntRect b) {
			return new IntRect(
				System.Math.Max(a.xmin, b.xmin),
				System.Math.Max(a.ymin, b.ymin),
				System.Math.Min(a.xmax, b.xmax),
				System.Math.Min(a.ymax, b.ymax)
				);
		}

		/// <summary>Returns if the two rectangles intersect each other</summary>
		public static bool Intersects (IntRect a, IntRect b) {
			return !(a.xmin > b.xmax || a.ymin > b.ymax || a.xmax < b.xmin || a.ymax < b.ymin);
		}

		/// <summary>
		/// Returns a new rect which contains both input rects.
		/// This rectangle may contain areas outside both input rects as well in some cases.
		/// </summary>
		public static IntRect Union (IntRect a, IntRect b) {
			return new IntRect(
				System.Math.Min(a.xmin, b.xmin),
				System.Math.Min(a.ymin, b.ymin),
				System.Math.Max(a.xmax, b.xmax),
				System.Math.Max(a.ymax, b.ymax)
				);
		}

		/// <summary>
		/// Returns a new rect that contains all of a except for the parts covered by b.
		///
		/// Throws: An exception if the difference is not a rectangle (e.g. if they only overlap in a corner).
		///
		/// <code>
		/// ┌───────────┐
		/// │     B     │
		/// │  ┌─────┐  │
		/// │  │     │  │  ─►
		/// └──┼─────┼──┘       ┌─────┐
		///    │  A  │          │  A  │
		///    └─────┘          └─────┘
		/// </code>
		/// </summary>
		public static IntRect Exclude (IntRect a, IntRect b) {
			if (!b.IsValid() || !a.IsValid()) return a;
			var intersection = Intersection(a, b);
			if (!intersection.IsValid()) return a;
			if (a.xmin == intersection.xmin && a.xmax == intersection.xmax) {
				if (a.ymin == intersection.ymin) {
					a.ymin = intersection.ymax + 1;
					return a;
				} else if (a.ymax == intersection.ymax) {
					a.ymax = intersection.ymin - 1;
					return a;
				} else {
					throw new System.ArgumentException("B splits A into two disjoint parts");
				}
			} else if (a.ymin == intersection.ymin && a.ymax == intersection.ymax) {
				if (a.xmin == intersection.xmin) {
					a.xmin = intersection.xmax + 1;
					return a;
				} else if (a.xmax == intersection.xmax) {
					a.xmax = intersection.xmin - 1;
					return a;
				} else {
					throw new System.ArgumentException("B splits A into two disjoint parts");
				}
			} else {
				throw new System.ArgumentException("B covers either a corner of A, or does not touch the edges of A at all");
			}
		}

		/// <summary>Returns a new IntRect which is expanded to contain the point</summary>
		public IntRect ExpandToContain (int x, int y) {
			return new IntRect(
				System.Math.Min(xmin, x),
				System.Math.Min(ymin, y),
				System.Math.Max(xmax, x),
				System.Math.Max(ymax, y)
				);
		}

		/// <summary>Returns a new IntRect which has been moved by an offset</summary>
		public IntRect Offset (Vector2Int offset) {
			return new IntRect(xmin + offset.x, ymin + offset.y, xmax + offset.x, ymax + offset.y);
		}

		/// <summary>Returns a new rect which is expanded by range in all directions.</summary>
		/// <param name="range">How far to expand. Negative values are permitted.</param>
		public IntRect Expand (int range) {
			return new IntRect(xmin-range,
				ymin-range,
				xmax+range,
				ymax+range
				);
		}

		public override string ToString () {
			return "[x: "+xmin+"..."+xmax+", y: " + ymin +"..."+ymax+"]";
		}

		/// <summary>Returns a list of all integer coordinates inside the rectangle, in row-major order</summary>
		public List<Vector2Int> GetInnerCoordinates () {
			var list = ListPool<Vector2Int>.Claim(Width*Height);
			for (int y = ymin; y <= ymax; y++) {
				for (int x = xmin; x <= xmax; x++) {
					list.Add(new Vector2Int(x, y));
				}
			}
			return list;
		}
	}

	/// <summary>
	/// Holds a bitmask of graphs.
	/// This bitmask can hold up to 32 graphs.
	///
	/// The bitmask can be converted to and from integers implicitly.
	///
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
	/// See: bitmasks (view in online documentation for working links)
	/// </summary>
	[System.Serializable]
	public struct GraphMask {
		/// <summary>Bitmask representing the mask</summary>
		public int value;

		/// <summary>A mask containing every graph</summary>
		public static GraphMask everything => new GraphMask(-1);

		public GraphMask (int value) {
			this.value = value;
		}

		public static implicit operator int(GraphMask mask) {
			return mask.value;
		}

		public static implicit operator GraphMask (int mask) {
			return new GraphMask(mask);
		}

		/// <summary>Combines two masks to form the intersection between them</summary>
		public static GraphMask operator & (GraphMask lhs, GraphMask rhs) {
			return new GraphMask(lhs.value & rhs.value);
		}

		/// <summary>Combines two masks to form the union of them</summary>
		public static GraphMask operator | (GraphMask lhs, GraphMask rhs) {
			return new GraphMask(lhs.value | rhs.value);
		}

		/// <summary>Inverts the mask</summary>
		public static GraphMask operator ~ (GraphMask lhs) {
			return new GraphMask(~lhs.value);
		}

		/// <summary>True if this mask contains the graph with the given graph index</summary>
		public bool Contains (int graphIndex) {
			return ((value >> graphIndex) & 1) != 0;
		}

		/// <summary>A bitmask containing the given graph</summary>
		public static GraphMask FromGraph (NavGraph graph) {
			return 1 << (int)graph.graphIndex;
		}

		public override string ToString () {
			return value.ToString();
		}

		/// <summary>A bitmask containing the given graph index.</summary>
		public static GraphMask FromGraphIndex(uint graphIndex) => new GraphMask(1 << (int)graphIndex);

		/// <summary>
		/// A bitmask containing the first graph with the given name.
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
		/// </summary>
		public static GraphMask FromGraphName (string graphName) {
			var graph = AstarPath.active.data.FindGraph(g => g.name == graphName);

			if (graph == null) throw new System.ArgumentException("Could not find any graph with the name '" + graphName + "'");
			return FromGraph(graph);
		}
	}

	#region Delegates

	/// <summary>
	/// Delegate with on Path object as parameter.
	/// This is used for callbacks for when a path has been calculated.
	/// </summary>
	public delegate void OnPathDelegate(Path p);

	public delegate void OnGraphDelegate(NavGraph graph);

	public delegate void OnScanDelegate(AstarPath script);

	#endregion

	#region Enums

	[System.Flags]
	public enum GraphUpdateThreading {
		/// <summary>
		/// Call UpdateArea in the unity thread.
		/// This is the default value.
		/// </summary>
		UnityThread = 0,
		/// <summary>Calls UpdateAreaInit in the Unity thread before everything else</summary>
		UnityInit = 1 << 1,
		/// <summary>Calls UpdateAreaPost in the Unity thread after everything else</summary>
		UnityPost = 1 << 2
	}

	/// <summary>How path results are logged by the system</summary>
	public enum PathLog {
		/// <summary>Does not log anything. This is recommended for release since logging path results has a performance overhead.</summary>
		None,
		/// <summary>Logs basic info about the paths</summary>
		Normal,
		/// <summary>Includes additional info</summary>
		Heavy,
		/// <summary>Same as heavy, but displays the info in-game using GUI</summary>
		InGame,
		/// <summary>Same as normal, but logs only paths which returned an error</summary>
		OnlyErrors
	}

	/// <summary>
	/// How to estimate the cost of moving to the destination during pathfinding.
	///
	/// The heuristic is the estimated cost from the current node to the target.
	/// The different heuristics have roughly the same performance except not using any heuristic at all (<see cref="Heuristic.None"/>)
	/// which is usually significantly slower.
	///
	/// In the image below you can see a comparison of the different heuristic options for an 8-connected grid and
	/// for a 4-connected grid.
	/// Note that all paths within the green area will all have the same length. The only difference between the heuristics
	/// is which of those paths of the same length that will be chosen.
	/// Note that while the Diagonal Manhattan and Manhattan options seem to behave very differently on an 8-connected grid
	/// they only do it in this case because of very small rounding errors. Usually they behave almost identically on 8-connected grids.
	///
	/// [Open online documentation to see images]
	///
	/// Generally for a 4-connected grid graph the Manhattan option should be used as it is the true distance on a 4-connected grid.
	/// For an 8-connected grid graph the Diagonal Manhattan option is the mathematically most correct option, however the Euclidean option
	/// is often preferred, especially if you are simplifying the path afterwards using modifiers.
	///
	/// For any graph that is not grid based the Euclidean option is the best one to use.
	///
	/// See: <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">Wikipedia: A* search_algorithm</a>
	/// </summary>
	public enum Heuristic {
		/// <summary>Manhattan distance. See: https://en.wikipedia.org/wiki/Taxicab_geometry</summary>
		Manhattan,
		/// <summary>
		/// Manhattan distance, but allowing diagonal movement as well.
		/// Note: This option is currently hard coded for the XZ plane. It will be equivalent to Manhattan distance if you try to use it in the XY plane (i.e for a 2D game).
		/// </summary>
		DiagonalManhattan,
		/// <summary>Ordinary distance. See: https://en.wikipedia.org/wiki/Euclidean_distance</summary>
		Euclidean,
		/// <summary>
		/// Use no heuristic at all.
		/// This reduces the pathfinding algorithm to Dijkstra's algorithm.
		/// This is usually significantly slower compared to using a heuristic, which is why the A* algorithm is usually preferred over Dijkstra's algorithm.
		/// You may have to use this if you have a very non-standard graph. For example a world with a <a href="https://en.wikipedia.org/wiki/Wraparound_(video_games)">wraparound playfield</a> (think Civilization or Asteroids) and you have custom links
		/// with a zero cost from one end of the map to the other end. Usually the A* algorithm wouldn't find the wraparound links because it wouldn't think to look in that direction.
		/// See: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
		/// </summary>
		None
	}

	/// <summary>How to visualize the graphs in the editor</summary>
	public enum GraphDebugMode {
		/// <summary>Draw the graphs with a single solid color</summary>
		SolidColor,
		/// <summary>
		/// Use the G score of the last calculated paths to color the graph.
		/// The G score is the cost from the start node to the given node.
		/// See: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		G,
		/// <summary>
		/// Use the H score (heuristic) of the last calculated paths to color the graph.
		/// The H score is the estimated cost from the current node to the target.
		/// See: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		H,
		/// <summary>
		/// Use the F score of the last calculated paths to color the graph.
		/// The F score is the G score + the H score, or in other words the estimated cost total cost of the path.
		/// See: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		F,
		/// <summary>
		/// Use the penalty of each node to color the graph.
		/// This does not show penalties added by tags.
		/// See: graph-updates (view in online documentation for working links)
		/// See: <see cref="Pathfinding.GraphNode.Penalty"/>
		/// </summary>
		Penalty,
		/// <summary>
		/// Visualize the connected components of the graph.
		/// A node with a given color can reach any other node with the same color.
		///
		/// See: <see cref="Pathfinding.HierarchicalGraph"/>
		/// See: https://en.wikipedia.org/wiki/Connected_component_(graph_theory)
		/// </summary>
		Areas,
		/// <summary>
		/// Use the tag of each node to color the graph.
		/// See: tags (view in online documentation for working links)
		/// See: <see cref="Pathfinding.GraphNode.Tag"/>
		/// </summary>
		Tags,
		/// <summary>
		/// Visualize the hierarchical graph structure of the graph.
		/// This is mostly for internal use.
		/// See: <see cref="Pathfinding.HierarchicalGraph"/>
		/// </summary>
		HierarchicalNode,
		/// <summary>
		/// Visualize the obstacles generated from the navmesh border.
		///
		/// These obstacles are used for local avoidance, as well as for the <see cref="FollowerEntity"/> in its internal navigation.
		///
		/// The graph will be colored the same as for <see cref="GraphDebugMode.HierarchicalNode"/>.
		/// </summary>
		NavmeshBorderObstacles,
	}

	/// <summary>Number of threads to use</summary>
	public enum ThreadCount {
		AutomaticLowLoad = -1,
		AutomaticHighLoad = -2,
		None = 0,
		One = 1,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight
	}

	/// <summary>Internal state of a path in the pipeline</summary>
	public enum PathState {
		/// <summary>Path has been created but not yet scheduled</summary>
		Created = 0,
		/// <summary>Path is waiting to be calculated</summary>
		PathQueue = 1,
		/// <summary>Path is being calculated</summary>
		Processing = 2,
		/// <summary>Path is calculated and is waiting to have its callback called</summary>
		ReturnQueue = 3,
		/// <summary>The path callback is being called right now (only set inside the callback itself)</summary>
		Returning = 4,
		/// <summary>The path has been calculated and its callback has been called</summary>
		Returned = 5,
	}


	/// <summary>State of a path request</summary>
	public enum PathCompleteState {
		/// <summary>
		/// The path has not been calculated yet.
		/// See: <see cref="Pathfinding.Path.IsDone()"/>
		/// </summary>
		NotCalculated = 0,
		/// <summary>
		/// The path calculation is done, but it failed.
		/// See: <see cref="Pathfinding.Path.error"/>
		/// </summary>
		Error = 1,
		/// <summary>The path has been successfully calculated</summary>
		Complete = 2,
		/// <summary>
		/// The path has been calculated, but only a partial path could be found.
		/// See: <see cref="Pathfinding.ABPath.calculatePartial"/>
		/// </summary>
		Partial = 3,
	}

	/// <summary>What to do when the character is close to the destination</summary>
	public enum CloseToDestinationMode {
		/// <summary>The character will stop as quickly as possible when within endReachedDistance (field that exist on most movement scripts) units from the destination</summary>
		Stop,
		/// <summary>The character will continue to the exact position of the destination</summary>
		ContinueToExactDestination,
	}

	/// <summary>Indicates the side of a line that a point lies on</summary>
	public enum Side : byte {
		/// <summary>The point lies exactly on the line</summary>
		Colinear = 0,
		/// <summary>The point lies on the left side of the line</summary>
		Left = 1,
		/// <summary>The point lies on the right side of the line</summary>
		Right = 2
	}

	public enum InspectorGridHexagonNodeSize {
		/// <summary>Value is the distance between two opposing sides in the hexagon</summary>
		Width,
		/// <summary>Value is the distance between two opposing vertices in the hexagon</summary>
		Diameter,
		/// <summary>Value is the raw node size of the grid</summary>
		NodeSize
	}

	public enum InspectorGridMode {
		Grid,
		IsometricGrid,
		Hexagonal,
		Advanced
	}

	/// <summary>
	/// Determines which direction the agent moves in.
	/// For 3D games you most likely want the ZAxisIsForward option as that is the convention for 3D games.
	/// For 2D games you most likely want the YAxisIsForward option as that is the convention for 2D games.
	/// </summary>
	public enum OrientationMode : byte {
		ZAxisForward,
		YAxisForward,
	}

	#endregion
}

namespace Pathfinding.Util {
	/// <summary>Prevents code stripping. See: https://docs.unity3d.com/Manual/ManagedCodeStripping.html</summary>
	public class PreserveAttribute : System.Attribute {
	}
}

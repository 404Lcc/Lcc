using System.Collections.Generic;
using Math = System.Math;
using UnityEngine;
using System.Linq;
using UnityEngine.Profiling;


namespace Pathfinding {
	using Pathfinding.Serialization;
	using Pathfinding.Util;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Pathfinding.Jobs;
	using Pathfinding.Graphs.Grid.Jobs;
	using Pathfinding.Collections;
	using Pathfinding.Drawing;
	using Pathfinding.Graphs.Grid;
	using Pathfinding.Graphs.Grid.Rules;
	using Pathfinding.Pooling;
	using UnityEngine.Assertions;

	/// <summary>
	/// Generates a grid of nodes.
	/// [Open online documentation to see images]
	/// The GridGraph does exactly what the name implies, generates nodes in a grid pattern.
	///
	/// Grid graphs are excellent for when you already have a grid-based world. But they also work well for free-form worlds.
	///
	/// See: get-started-grid (view in online documentation for working links)
	/// See: graphTypes (view in online documentation for working links)
	///
	/// \section gridgraph-features Features
	/// - Throw any scene at it, and with minimal configurations you can get a good graph from it.
	/// - Predictable pattern.
	/// - Grid graphs work well with penalties and tags.
	/// - You can update parts of the graph during runtime.
	/// - Graph updates are fast.
	/// - Scanning the graph is comparatively fast.
	/// - Supports linecasting.
	/// - Supports the funnel modifier.
	/// - Supports both 2D and 3D physics.
	/// - Supports isometric and hexagonal node layouts.
	/// - Can apply penalty and walkability values from a supplied image.
	/// - Perfect for terrains since it can make nodes walkable or unwalkable depending on the slope.
	/// - Only supports a single layer, but you can use a <see cref="LayerGridGraph"/> if you need more layers.
	///
	/// \section gridgraph-inspector Inspector
	/// [Open online documentation to see images]
	///
	/// \inspectorField{Shape, inspectorGridMode}
	/// \inspectorField{2D, is2D}
	/// \inspectorField{Align  to tilemap, AlignToTilemap}
	/// \inspectorField{Width, width}
	/// \inspectorField{Depth, depth}
	/// \inspectorField{Node size, nodeSize}
	/// \inspectorField{Aspect ratio (isometric/advanced shape), aspectRatio}
	/// \inspectorField{Isometric angle (isometric/advanced shape), isometricAngle}
	/// \inspectorField{Center, center}
	/// \inspectorField{Rotation, rotation}
	/// \inspectorField{Connections, neighbours}
	/// \inspectorField{Cut corners, cutCorners}
	/// \inspectorField{Max step height, maxStepHeight}
	/// \inspectorField{Account for slopes, maxStepUsesSlope}
	/// \inspectorField{Max slope, maxSlope}
	/// \inspectorField{Erosion iterations, erodeIterations}
	/// \inspectorField{Erosion → Erosion Uses Tags, erosionUseTags}
	/// \inspectorField{Use 2D physics, collision.use2D}
	///
	/// <i>Collision testing</i>
	/// \inspectorField{Collider type, collision.type}
	/// \inspectorField{Diameter, collision.diameter}
	/// \inspectorField{Height/length, collision.height}
	/// \inspectorField{Offset, collision.collisionOffset}
	/// \inspectorField{Obstacle layer mask, collision.mask}
	/// \inspectorField{Preview, GridGraphEditor.collisionPreviewOpen}
	///
	/// <i>Height testing</i>
	/// \inspectorField{Ray length, collision.fromHeight}
	/// \inspectorField{Mask, collision.heightMask}
	/// \inspectorField{Thick raycast, collision.thickRaycast}
	/// \inspectorField{Unwalkable when no ground, collision.unwalkableWhenNoGround}
	///
	/// <i>Rules</i>
	/// Take a look at grid-rules (view in online documentation for working links) for a list of available rules.
	///
	/// <i>Other settings</i>
	/// \inspectorField{Show surface, showMeshSurface}
	/// \inspectorField{Show outline, showMeshOutline}
	/// \inspectorField{Show connections, showNodeConnections}
	/// \inspectorField{Initial penalty, NavGraph.initialPenalty}
	///
	/// \section gridgraph-updating Updating the graph during runtime
	/// Any graph which implements the IUpdatableGraph interface can be updated during runtime.
	/// For grid graphs this is a great feature since you can update only a small part of the grid without causing any lag like a complete rescan would.
	///
	/// If you for example just have instantiated an obstacle in the scene and you want to update the grid where that obstacle was instantiated, you can do this:
	///
	/// <code> AstarPath.active.UpdateGraphs (obstacle.collider.bounds); </code>
	/// Where obstacle is the GameObject you just instantiated.
	///
	/// As you can see, the UpdateGraphs function takes a Bounds parameter and it will send an update call to all updateable graphs.
	///
	/// A grid graph will assume anything could have changed inside that bounding box, and recalculate all nodes that could possibly be affected.
	/// Thus it may end up updating a few more nodes than just those covered by the bounding box.
	///
	/// See: graph-updates (view in online documentation for working links) for more info about updating graphs during runtime
	///
	/// \section gridgraph-hexagonal Hexagonal graphs
	/// The graph can be configured to work like a hexagon graph with some simple settings. The grid graph has a Shape dropdown.
	/// If you set it to 'Hexagonal' the graph will behave as a hexagon graph.
	/// Often you may want to rotate the graph +45 or -45 degrees.
	/// [Open online documentation to see images]
	///
	/// Note: Snapping to the closest node is not exactly as you would expect in a real hexagon graph,
	/// but it is close enough that you will likely not notice.
	///
	/// \section gridgraph-configure-code Configure using code
	///
	/// A grid graph can be added and configured completely at runtime via code.
	///
	/// <code>
	/// // This holds all graph data
	/// AstarData data = AstarPath.active.data;
	///
	/// // This creates a Grid Graph
	/// GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
	///
	/// // Setup a grid graph with some values
	/// int width = 50;
	/// int depth = 50;
	/// float nodeSize = 1;
	///
	/// gg.center = new Vector3(10, 0, 0);
	///
	/// // Updates internal size from the above values
	/// gg.SetDimensions(width, depth, nodeSize);
	///
	/// // Scans all graphs
	/// AstarPath.active.Scan();
	/// </code>
	///
	/// See: runtime-graphs (view in online documentation for working links)
	///
	/// \section gridgraph-trees Tree colliders
	/// It seems that Unity will only generate tree colliders at runtime when the game is started.
	/// For this reason, the grid graph will not pick up tree colliders when outside of play mode
	/// but it will pick them up once the game starts. If it still does not pick them up
	/// make sure that the trees actually have colliders attached to them and that the tree prefabs are
	/// in the correct layer (the layer should be included in the 'Collision Testing' mask).
	///
	/// See: <see cref="GraphCollision"/> for documentation on the 'Height Testing' and 'Collision Testing' sections
	/// of the grid graph settings.
	/// See: <see cref="LayerGridGraph"/>
	/// </summary>
	[JsonOptIn]
	[Pathfinding.Util.Preserve]
	public class GridGraph : NavGraph, IUpdatableGraph, ITransformedGraph
		, IRaycastableGraph {
		protected override void DisposeUnmanagedData () {
			// Destroy all nodes to make the graph go into an unscanned state
			DestroyAllNodes();

			// Clean up a reference in a static variable which otherwise should point to this graph forever and stop the GC from collecting it
			GridNode.ClearGridGraph((int)graphIndex, this);

			// Dispose of native arrays. This is very important to avoid memory leaks!
			rules.DisposeUnmanagedData();
			this.nodeData.Dispose();
		}

		protected override void DestroyAllNodes () {
			GetNodes(node => {
				// If the grid data happens to be invalid (e.g we had to abort a graph update while it was running) using 'false' as
				// the parameter will prevent the Destroy method from potentially throwing IndexOutOfRange exceptions due to trying
				// to access nodes outside the graph. It is safe to do this because we are destroying all nodes in the graph anyway.
				// We do however need to clear custom connections in both directions
				(node as GridNodeBase).ClearCustomConnections(true);
				node.ClearConnections(false);
				node.Destroy();
			});
			// Important: so that multiple calls to DestroyAllNodes still works
			nodes = null;
		}


		/// <summary>
		/// Number of layers in the graph.
		/// For grid graphs this is always 1, for layered grid graphs it can be higher.
		/// The nodes array has the size width*depth*layerCount.
		/// </summary>
		public virtual int LayerCount {
			get => 1;
			protected set {
				if (value != 1) throw new System.NotSupportedException("Grid graphs cannot have multiple layers");
			}
		}

		public virtual int MaxLayers => 1;

		public override int CountNodes () {
			return nodes != null ? nodes.Length : 0;
		}

		public override void GetNodes (System.Action<GraphNode> action) {
			if (nodes == null) return;
			for (int i = 0; i < nodes.Length; i++) action(nodes[i]);
		}

		/// <summary>
		/// Determines the layout of the grid graph inspector in the Unity Editor.
		///
		/// A grid graph can be set up as a normal grid, isometric grid or hexagonal grid.
		/// Each of these modes use a slightly different inspector layout.
		/// When changing the shape in the inspector, it will automatically set other relevant fields
		/// to appropriate values. For example, when setting the shape to hexagonal it will automatically set
		/// the <see cref="neighbours"/> field to Six.
		///
		/// This field is only used in the editor, it has no effect on the rest of the game whatsoever.
		///
		/// If you want to change the grid shape like in the inspector you can use the <see cref="SetGridShape"/> method.
		/// </summary>
		[JsonMember]
		public InspectorGridMode inspectorGridMode = InspectorGridMode.Grid;

		/// <summary>
		/// Determines how the size of each hexagon is set in the inspector.
		/// For hexagons the normal nodeSize field doesn't really correspond to anything specific on the hexagon's geometry, so this enum is used to give the user the opportunity to adjust more concrete dimensions of the hexagons
		/// without having to pull out a calculator to calculate all the square roots and complicated conversion factors.
		///
		/// This field is only used in the graph inspector, the <see cref="nodeSize"/> field will always use the same internal units.
		/// If you want to set the node size through code then you can use <see cref="ConvertHexagonSizeToNodeSize"/>.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="InspectorGridHexagonNodeSize"/>
		/// See: <see cref="ConvertHexagonSizeToNodeSize"/>
		/// See: <see cref="ConvertNodeSizeToHexagonSize"/>
		/// </summary>
		[JsonMember]
		public InspectorGridHexagonNodeSize inspectorHexagonSizeMode = InspectorGridHexagonNodeSize.Width;

		/// <summary>
		/// Width of the grid in nodes.
		///
		/// Grid graphs are typically anywhere from 10-500 nodes wide. But it can go up to 1024 nodes wide by default.
		/// Consider using a recast graph instead, if you find yourself needing a very high resolution grid.
		///
		/// This value will be clamped to at most 1024 unless ASTAR_LARGER_GRIDS has been enabled in the A* Inspector -> Optimizations tab.
		///
		/// See: <see cref="depth"/>
		/// See: SetDimensions
		/// </summary>
		public int width;

		/// <summary>
		/// Depth (height) of the grid in nodes.
		///
		/// Grid graphs are typically anywhere from 10-500 nodes wide. But it can go up to 1024 nodes wide by default.
		/// Consider using a recast graph instead, if you find yourself needing a very high resolution grid.
		///
		/// This value will be clamped to at most 1024 unless ASTAR_LARGER_GRIDS has been enabled in the A* Inspector -> Optimizations tab.
		///
		/// See: <see cref="width"/>
		/// See: SetDimensions
		/// </summary>
		public int depth;

		/// <summary>
		/// Scaling of the graph along the X axis.
		/// This should be used if you want different scales on the X and Y axis of the grid
		///
		/// This option is only visible in the inspector if the graph shape is set to isometric or advanced.
		/// </summary>
		[JsonMember]
		public float aspectRatio = 1F;

		/// <summary>
		/// Angle in degrees to use for the isometric projection.
		/// If you are making a 2D isometric game, you may want to use this parameter to adjust the layout of the graph to match your game.
		/// This will essentially scale the graph along one of its diagonals to produce something like this:
		///
		/// A perspective view of an isometric graph.
		/// [Open online documentation to see images]
		///
		/// A top down view of an isometric graph. Note that the graph is entirely 2D, there is no perspective in this image.
		/// [Open online documentation to see images]
		///
		/// For commonly used values see <see cref="StandardIsometricAngle"/> and <see cref="StandardDimetricAngle"/>.
		///
		/// Usually the angle that you want to use is either 30 degrees (alternatively 90-30 = 60 degrees) or atan(1/sqrt(2)) which is approximately 35.264 degrees (alternatively 90 - 35.264 = 54.736 degrees).
		/// You might also want to rotate the graph plus or minus 45 degrees around the Y axis to get the oritientation required for your game.
		///
		/// You can read more about it on the wikipedia page linked below.
		///
		/// See: http://en.wikipedia.org/wiki/Isometric_projection
		/// See: https://en.wikipedia.org/wiki/Isometric_graphics_in_video_games_and_pixel_art
		/// See: rotation
		///
		/// This option is only visible in the inspector if the graph shape is set to isometric or advanced.
		/// </summary>
		[JsonMember]
		public float isometricAngle;

		/// <summary>Commonly used value for <see cref="isometricAngle"/></summary>
		public static readonly float StandardIsometricAngle = 90-Mathf.Atan(1/Mathf.Sqrt(2))*Mathf.Rad2Deg;

		/// <summary>Commonly used value for <see cref="isometricAngle"/></summary>
		public static readonly float StandardDimetricAngle = Mathf.Acos(1/2f)*Mathf.Rad2Deg;

		/// <summary>
		/// If true, all edge costs will be set to the same value.
		/// If false, diagonals will cost more.
		/// This is useful for a hexagon graph where the diagonals are actually the same length as the
		/// normal edges (since the graph has been skewed)
		///
		/// If the graph is set to hexagonal in the inspector, this will be automatically set to true.
		/// </summary>
		[JsonMember]
		public bool uniformEdgeCosts;

		/// <summary>
		/// Rotation of the grid in degrees.
		///
		/// The nodes are laid out along the X and Z axes of the rotation.
		///
		/// For a 2D game, the rotation will typically be set to (-90, 270, 90).
		/// If the graph is aligned with the XY plane, the inspector will automatically switch to 2D mode.
		///
		/// See: <see cref="is2D"/>
		/// </summary>
		[JsonMember]
		public Vector3 rotation;

		/// <summary>
		/// Center point of the grid in world space.
		///
		/// The graph can be positioned anywhere in the world.
		///
		/// See: <see cref="RelocateNodes(Vector3,Quaternion,float,float,float)"/>
		/// </summary>
		[JsonMember]
		public Vector3 center;

		/// <summary>Size of the grid. Can be negative or smaller than <see cref="nodeSize"/></summary>
		[JsonMember]
		public Vector2 unclampedSize = new Vector2(10, 10);

		/// <summary>
		/// Size of one node in world units.
		///
		/// For a grid layout, this is the length of the sides of the grid squares.
		///
		/// For a hexagonal layout, this value does not correspond to any specific dimension of the hexagon.
		/// Instead you can convert it to a dimension on a hexagon using <see cref="ConvertNodeSizeToHexagonSize"/>.
		///
		/// See: <see cref="SetDimensions"/>
		/// See: <see cref="SetGridShape"/>
		/// </summary>
		[JsonMember]
		public float nodeSize = 1;

		/// <summary>Settings on how to check for walkability and height</summary>
		[JsonMember]
		public GraphCollision collision = new GraphCollision();

		/// <summary>
		/// The max y coordinate difference between two nodes to enable a connection.
		/// Set to 0 to ignore the value.
		///
		/// This affects for example how the graph is generated around ledges and stairs.
		///
		/// See: <see cref="maxStepUsesSlope"/>
		/// Version: Was previously called maxClimb
		/// </summary>
		[JsonMember]
		public float maxStepHeight = 0.4F;

		/// <summary>
		/// The max y coordinate difference between two nodes to enable a connection.
		/// Deprecated: This field has been renamed to <see cref="maxStepHeight"/>
		/// </summary>
		[System.Obsolete("This field has been renamed to maxStepHeight")]
		public float maxClimb {
			get {
				return maxStepHeight;
			}
			set {
				maxStepHeight = value;
			}
		}

		/// <summary>
		/// Take the slope into account for <see cref="maxStepHeight"/>.
		///
		/// When this is enabled the normals of the terrain will be used to make more accurate estimates of how large the steps are between adjacent nodes.
		///
		/// When this is disabled then calculated step between two nodes is their y coordinate difference. This may be inaccurate, especially at the start of steep slopes.
		///
		/// [Open online documentation to see images]
		///
		/// In the image below you can see an example of what happens near a ramp.
		/// In the topmost image the ramp is not connected with the rest of the graph which is obviously not what we want.
		/// In the middle image an attempt has been made to raise the max step height while keeping <see cref="maxStepUsesSlope"/> disabled. However this causes too many connections to be added.
		/// The agent should not be able to go up the ramp from the side.
		/// Finally in the bottommost image the <see cref="maxStepHeight"/> has been restored to the original value but <see cref="maxStepUsesSlope"/> has been enabled. This configuration handles the ramp in a much smarter way.
		/// Note that all the values in the image are just example values, they may be different for your scene.
		/// [Open online documentation to see images]
		///
		/// See: <see cref="maxStepHeight"/>
		/// </summary>
		[JsonMember]
		public bool maxStepUsesSlope = true;

		/// <summary>The max slope in degrees for a node to be walkable.</summary>
		[JsonMember]
		public float maxSlope = 90;

		/// <summary>
		/// Use heigh raycasting normal for max slope calculation.
		/// True if <see cref="maxSlope"/> is less than 90 degrees.
		/// </summary>
		protected bool useRaycastNormal { get { return Math.Abs(90-maxSlope) > float.Epsilon; } }

		/// <summary>
		/// Number of times to erode the graph.
		///
		/// The graph can be eroded to add extra margin to obstacles.
		/// It is very convenient if your graph contains ledges, and where the walkable nodes without erosion are too close to the edge.
		///
		/// Below is an image showing a graph with 0, 1 and 2 erosion iterations:
		/// [Open online documentation to see images]
		///
		/// Note: A high number of erosion iterations can slow down graph updates during runtime.
		/// This is because the region that is updated needs to be expanded by the erosion iterations times two to account for possible changes in the border nodes.
		///
		/// See: erosionUseTags
		/// </summary>
		[JsonMember]
		public int erodeIterations;

		/// <summary>
		/// Use tags instead of walkability for erosion.
		/// Tags will be used for erosion instead of marking nodes as unwalkable. The nodes will be marked with tags in an increasing order starting with the tag <see cref="erosionFirstTag"/>.
		/// Debug with the Tags mode to see the effect. With this enabled you can in effect set how close different AIs are allowed to get to walls using the Valid Tags field on the Seeker component.
		/// [Open online documentation to see images]
		/// [Open online documentation to see images]
		/// See: erosionFirstTag
		/// </summary>
		[JsonMember]
		public bool erosionUseTags;

		/// <summary>
		/// Tag to start from when using tags for erosion.
		/// See: <see cref="erosionUseTags"/>
		/// See: <see cref="erodeIterations"/>
		/// </summary>
		[JsonMember]
		public int erosionFirstTag = 1;

		/// <summary>
		/// Bitmask for which tags can be overwritten by erosion tags.
		///
		/// When <see cref="erosionUseTags"/> is enabled, nodes near unwalkable nodes will be marked with tags.
		/// However, if these nodes already have tags, you may want the custom tag to take precedence.
		/// This mask controls which tags are allowed to be replaced by the new erosion tags.
		///
		/// In the image below, erosion has applied tags which have overwritten both the base tag (tag 0) and the custom tag set on the nodes (shown in red).
		/// [Open online documentation to see images]
		///
		/// In the image below, erosion has applied tags, but it was not allowed to overwrite the custom tag set on the nodes (shown in red).
		/// [Open online documentation to see images]
		///
		/// See: <see cref="erosionUseTags"/>
		/// See: <see cref="erodeIterations"/>
		/// See: This field is a bit mask. See: bitmasks (view in online documentation for working links)
		/// </summary>
		[JsonMember]
		public int erosionTagsPrecedenceMask = -1;

		/// <summary>
		/// Number of neighbours for each node.
		/// Either four, six, eight connections per node.
		///
		/// Six connections is primarily for hexagonal graphs.
		/// </summary>
		[JsonMember]
		public NumNeighbours neighbours = NumNeighbours.Eight;

		/// <summary>
		/// If disabled, will not cut corners on obstacles.
		/// If this is true, and <see cref="neighbours"/> is set to Eight, obstacle corners are allowed to be cut by a connection.
		///
		/// [Open online documentation to see images]
		/// </summary>
		[JsonMember]
		public bool cutCorners = true;

		/// <summary>
		/// Offset for the position when calculating penalty.
		/// Deprecated: Use the RuleElevationPenalty class instead
		/// See: penaltyPosition
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleElevationPenalty class instead")]
		public float penaltyPositionOffset;

		/// <summary>
		/// Use position (y-coordinate) to calculate penalty.
		/// Deprecated: Use the RuleElevationPenalty class instead
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleElevationPenalty class instead")]
		public bool penaltyPosition;

		/// <summary>
		/// Scale factor for penalty when calculating from position.
		/// Deprecated: Use the <see cref="RuleElevationPenalty"/> class instead
		/// See: penaltyPosition
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleElevationPenalty class instead")]
		public float penaltyPositionFactor = 1F;

		/// <summary>Deprecated: Use the <see cref="RuleAnglePenalty"/> class instead</summary>
		[JsonMember]
		[System.Obsolete("Use the RuleAnglePenalty class instead")]
		public bool penaltyAngle;

		/// <summary>
		/// How much penalty is applied depending on the slope of the terrain.
		/// At a 90 degree slope (not that exactly 90 degree slopes can occur, but almost 90 degree), this penalty is applied.
		/// At a 45 degree slope, half of this is applied and so on.
		/// Note that you may require very large values, a value of 1000 is equivalent to the cost of moving 1 world unit.
		///
		/// Deprecated: Use the <see cref="RuleAnglePenalty"/> class instead
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleAnglePenalty class instead")]
		public float penaltyAngleFactor = 100F;

		/// <summary>
		/// How much extra to penalize very steep angles.
		///
		/// Deprecated: Use the <see cref="RuleAnglePenalty"/> class instead
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleAnglePenalty class instead")]
		public float penaltyAnglePower = 1;

		/// <summary>
		/// Additional rules to use when scanning the grid graph.
		///
		/// <code>
		/// // Get the first grid graph in the scene
		/// var gridGraph = AstarPath.active.data.gridGraph;
		///
		/// gridGraph.rules.AddRule(new Pathfinding.Graphs.Grid.Rules.RuleAnglePenalty {
		///     penaltyScale = 10000,
		///     curve = AnimationCurve.Linear(0, 0, 90, 1),
		/// });
		/// </code>
		///
		/// See: <see cref="GridGraphRules"/>
		/// See: <see cref="GridGraphRule"/>
		/// </summary>
		[JsonMember]
		public GridGraphRules rules = new GridGraphRules();

		/// <summary>Show an outline of the grid nodes in the Unity Editor</summary>
		[JsonMember]
		public bool showMeshOutline = true;

		/// <summary>Show the connections between the grid nodes in the Unity Editor</summary>
		[JsonMember]
		public bool showNodeConnections;

		/// <summary>Show the surface of the graph. Each node will be drawn as a square (unless e.g hexagon graph mode has been enabled).</summary>
		[JsonMember]
		public bool showMeshSurface = true;

		/// <summary>
		/// Holds settings for using a texture as source for a grid graph.
		/// Texure data can be used for fine grained control over how the graph will look.
		/// It can be used for positioning, penalty and walkability control.
		/// Below is a screenshot of a grid graph with a penalty map applied.
		/// It has the effect of the AI taking the longer path along the green (low penalty) areas.
		/// [Open online documentation to see images]
		/// Color data is got as 0...255 values.
		///
		/// Warning: Can only be used with Unity 3.4 and up
		///
		/// Deprecated: Use the RuleTexture class instead
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleTexture class instead")]
		public TextureData textureData = new TextureData();

		/// <summary>
		/// Size of the grid. Will always be positive and larger than <see cref="nodeSize"/>.
		/// See: <see cref="UpdateTransform"/>
		/// </summary>
		public Vector2 size { get; protected set; }

		/* End collision and stuff */

		/// <summary>
		/// Index offset to get neighbour nodes. Added to a node's index to get a neighbour node index.
		///
		/// <code>
		///         Z
		///         |
		///         |
		///
		///      6  2  5
		///       \ | /
		/// --  3 - X - 1  ----- X
		///       / | \
		///      7  0  4
		///
		///         |
		///         |
		/// </code>
		/// </summary>
		[System.NonSerialized]
		public readonly int[] neighbourOffsets = new int[8];

		/// <summary>
		/// Costs to neighbour nodes.
		///
		/// See <see cref="neighbourOffsets"/>.
		/// </summary>
		[System.NonSerialized]
		public readonly uint[] neighbourCosts = new uint[8];

		/// <summary>Offsets in the X direction for neighbour nodes. Only 1, 0 or -1</summary>
		public static readonly int[] neighbourXOffsets = { 0, 1, 0, -1, 1, 1, -1, -1 };

		/// <summary>Offsets in the Z direction for neighbour nodes. Only 1, 0 or -1</summary>
		public static readonly int[] neighbourZOffsets = { -1, 0, 1, 0, -1, 1, 1, -1 };

		/// <summary>Which neighbours are going to be used when <see cref="neighbours"/>=6</summary>
		internal static readonly int[] hexagonNeighbourIndices = { 0, 1, 5, 2, 3, 7 };

		/// <summary>Which neighbours are going to be used when <see cref="neighbours"/>=4</summary>
		internal static readonly int[] axisAlignedNeighbourIndices = { 0, 1, 2, 3 };

		/// <summary>Which neighbours are going to be used when <see cref="neighbours"/>=8</summary>
		internal static readonly int[] allNeighbourIndices = { 0, 1, 2, 3, 4, 5, 6, 7 };

		/// <summary>
		/// Neighbour direction indices to use depending on how many neighbours each node should have.
		///
		/// The following illustration shows the direction indices for all 8 neighbours,
		/// <code>
		///         Z
		///         |
		///         |
		///
		///      6  2  5
		///       \ | /
		/// --  3 - X - 1  ----- X
		///       / | \
		///      7  0  4
		///
		///         |
		///         |
		/// </code>
		///
		/// For other neighbour counts, a subset of these will be returned.
		///
		/// These can then be used to index into the <see cref="neighbourOffsets"/>, <see cref="neighbourCosts"/>, <see cref="neighbourXOffsets"/>, and <see cref="neighbourZOffsets"/> arrays.
		///
		/// See: <see cref="GridNodeBase.HasConnectionInDirection"/>
		/// See: <see cref="GridNodeBase.GetNeighbourAlongDirection"/>
		/// </summary>
		public static int[] GetNeighbourDirections (NumNeighbours neighbours) {
			switch (neighbours) {
			case NumNeighbours.Four:
				return axisAlignedNeighbourIndices;
			case NumNeighbours.Six:
				return hexagonNeighbourIndices;
			default:
				return allNeighbourIndices;
			}
		}

		/// <summary>
		/// Mask based on hexagonNeighbourIndices.
		/// This indicates which connections (out of the 8 standard ones) should be enabled for hexagonal graphs.
		///
		/// <code>
		/// int hexagonConnectionMask = 0;
		/// for (int i = 0; i < GridGraph.hexagonNeighbourIndices.Length; i++) hexagonConnectionMask |= 1 << GridGraph.hexagonNeighbourIndices[i];
		/// </code>
		/// </summary>
		internal const int HexagonConnectionMask = 0b010101111;

		/// <summary>
		/// All nodes in this graph.
		/// Nodes are laid out row by row.
		///
		/// The first node has grid coordinates X=0, Z=0, the second one X=1, Z=0
		/// the last one has grid coordinates X=width-1, Z=depth-1.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// int x = 5;
		/// int z = 8;
		/// GridNodeBase node = gg.nodes[z*gg.width + x];
		/// </code>
		///
		/// See: <see cref="GetNode"/>
		/// See: <see cref="GetNodes"/>
		/// </summary>
		public GridNodeBase[] nodes;

		/// <summary>
		/// Internal data for each node.
		///
		/// It also contains some data not stored in the node objects, such as normals for the surface of the graph.
		/// These normals need to be saved when the <see cref="maxStepUsesSlope"/> option is enabled for graph updates to work.
		/// </summary>
		protected GridGraphNodeData nodeData;

		internal ref GridGraphNodeData nodeDataRef => ref nodeData;

		/// <summary>
		/// Determines how the graph transforms graph space to world space.
		/// See: <see cref="UpdateTransform"/>
		/// </summary>
		public GraphTransform transform { get; private set; } = new GraphTransform(Matrix4x4.identity);

		/// <summary>
		/// Delegate which creates and returns a single instance of the node type for this graph.
		/// This may be set in the constructor for graphs inheriting from the GridGraph to change the node type of the graph.
		/// </summary>
		protected System.Func<GridNodeBase> newGridNodeDelegate = () => new GridNode();

		/// <summary>
		/// Get or set if the graph should be in 2D mode.
		///
		/// Note: This is just a convenience property, this property will actually read/modify the <see cref="rotation"/> of the graph. A rotation aligned with the 2D plane is what determines if the graph is 2D or not.
		///
		/// See: You can also set if the graph should use 2D physics using `this.collision.use2D` (<see cref="GraphCollision.use2D"/>).
		/// </summary>
		public bool is2D {
			get {
				return Quaternion.Euler(this.rotation) * Vector3.up == -Vector3.forward;
			}
			set {
				if (value != is2D) {
					this.rotation = value ? new Vector3(this.rotation.y - 90, 270, 90) : new Vector3(0, this.rotation.x + 90, 0);
				}
			}
		}

		public override bool isScanned => nodes != null;

		protected virtual GridNodeBase[] AllocateNodesJob (int size, out JobHandle dependency) {
			var newNodes = new GridNodeBase[size];

			dependency = active.AllocateNodes(newNodes, size, newGridNodeDelegate, 1);
			return newNodes;
		}

		/// <summary>Used for using a texture as a source for a grid graph.</summary>
		public class TextureData {
			public bool enabled;
			public Texture2D source;
			public float[] factors = new float[3];
			public ChannelUse[] channels = new ChannelUse[3];

			Color32[] data;

			/// <summary>Reads texture data</summary>
			public void Initialize () {
				if (enabled && source != null) {
					for (int i = 0; i < channels.Length; i++) {
						if (channels[i] != ChannelUse.None) {
							try {
								data = source.GetPixels32();
							} catch (UnityException e) {
								Debug.LogWarning(e.ToString());
								data = null;
							}
							break;
						}
					}
				}
			}

			/// <summary>Applies the texture to the node</summary>
			public void Apply (GridNode node, int x, int z) {
				if (enabled && data != null && x < source.width && z < source.height) {
					Color32 col = data[z*source.width+x];

					if (channels[0] != ChannelUse.None) {
						ApplyChannel(node, x, z, col.r, channels[0], factors[0]);
					}

					if (channels[1] != ChannelUse.None) {
						ApplyChannel(node, x, z, col.g, channels[1], factors[1]);
					}

					if (channels[2] != ChannelUse.None) {
						ApplyChannel(node, x, z, col.b, channels[2], factors[2]);
					}

					node.WalkableErosion = node.Walkable;
				}
			}

			/// <summary>Applies a value to the node using the specified ChannelUse</summary>
			void ApplyChannel (GridNode node, int x, int z, int value, ChannelUse channelUse, float factor) {
				switch (channelUse) {
				case ChannelUse.Penalty:
					node.Penalty += (uint)Mathf.RoundToInt(value*factor);
					break;
				case ChannelUse.Position:
					node.position = GridNode.GetGridGraph(node.GraphIndex).GraphPointToWorld(x, z, value);
					break;
				case ChannelUse.WalkablePenalty:
					if (value == 0) {
						node.Walkable = false;
					} else {
						node.Penalty += (uint)Mathf.RoundToInt((value-1)*factor);
					}
					break;
				}
			}

			public enum ChannelUse {
				None,
				Penalty,
				Position,
				WalkablePenalty,
			}
		}

		public override void RelocateNodes (Matrix4x4 deltaMatrix) {
			// It just makes a lot more sense to use the other overload and for that case we don't have to serialize the matrix
			throw new System.Exception("This method cannot be used for Grid Graphs. Please use the other overload of RelocateNodes instead");
		}

		/// <summary>
		/// Relocate the grid graph using new settings.
		/// This will move all nodes in the graph to new positions which matches the new settings.
		///
		/// <code>
		/// // Move the graph to the origin, with no rotation, and with a node size of 1.0
		/// var gg = AstarPath.active.data.gridGraph;
		/// gg.RelocateNodes(center: Vector3.zero, rotation: Quaternion.identity, nodeSize: 1.0f);
		/// </code>
		/// </summary>
		public void RelocateNodes (Vector3 center, Quaternion rotation, float nodeSize, float aspectRatio = 1, float isometricAngle = 0) {
			AssertSafeToUpdateGraph();
			var previousTransform = transform;

			this.center = center;
			this.rotation = rotation.eulerAngles;
			this.aspectRatio = aspectRatio;
			this.isometricAngle = isometricAngle;

			DirtyBounds(bounds);
			SetDimensions(width, depth, nodeSize);

			new JobRelocateNodes {
				previousWorldToGraph = previousTransform.inverseMatrix,
				graphToWorld = transform.matrix,
				positions = nodeData.positions,
				bounds = nodeData.bounds,
			}.Run();

			var positions = this.nodeData.positions.AsUnsafeSpan();
			for (int i = 0; i < this.nodes.Length; i++) {
				var node = this.nodes[i];
				if (node != null) node.position = (Int3)positions[i];
			}
			DirtyBounds(bounds);
		}

		/// <summary>
		/// True if the point is inside the bounding box of this graph.
		///
		/// This method may be able to use a tighter (non-axis aligned) bounding box than using the one returned by <see cref="bounds"/>.
		///
		/// For a graph that uses 2D physics, or if height testing is disabled, then the graph is treated as infinitely tall.
		/// Otherwise, the height of the graph is determined by <see cref="GraphCollision.fromHeight"/>.
		///
		/// Note: For an unscanned graph, this will always return false.
		/// </summary>
		public override bool IsInsideBounds (Vector3 point) {
			if (this.nodes == null) return false;

			var local = transform.InverseTransform(point);
			if (!(local.x >= 0 && local.z >= 0 && local.x <= width && local.z <= depth)) return false;

			if (collision.use2D || !collision.heightCheck) return true;

			const float Margin = 0.001f;
			return local.y >= -Margin && local.y <= collision.fromHeight + Margin;
		}

		/// <summary>
		/// World bounding box for the graph.
		///
		/// This always contains the whole graph.
		///
		/// Note: Since this is an axis-aligned bounding box, it may not be particularly tight if the graph is significantly rotated.
		/// </summary>
		public override Bounds bounds => transform.Transform(new Bounds(new Vector3(width*0.5f, collision.fromHeight*0.5f, depth*0.5f), new Vector3(width, collision.fromHeight, depth)));

		/// <summary>
		/// Transform a point in graph space to world space.
		/// This will give you the node position for the node at the given x and z coordinate
		/// if it is at the specified height above the base of the graph.
		/// </summary>
		public Int3 GraphPointToWorld (int x, int z, float height) {
			return (Int3)transform.Transform(new Vector3(x+0.5f, height, z+0.5f));
		}

		/// <summary>
		/// Converts a hexagon dimension to a node size.
		///
		/// A hexagon can be defined using either its diameter, or width, none of which are the same as the <see cref="nodeSize"/> used internally to define the size of a single node.
		///
		/// See: <see cref="ConvertNodeSizeToHexagonSize"/>
		/// </summary>
		public static float ConvertHexagonSizeToNodeSize (InspectorGridHexagonNodeSize mode, float value) {
			if (mode == InspectorGridHexagonNodeSize.Diameter) value *= 1.5f/(float)System.Math.Sqrt(2.0f);
			else if (mode == InspectorGridHexagonNodeSize.Width) value *= (float)System.Math.Sqrt(3.0f/2.0f);
			return value;
		}

		/// <summary>
		/// Converts an internal node size to a hexagon dimension.
		///
		/// A hexagon can be defined using either its diameter, or width, none of which are the same as the <see cref="nodeSize"/> used internally to define the size of a single node.
		///
		/// See: ConvertHexagonSizeToNodeSize
		/// </summary>
		public static float ConvertNodeSizeToHexagonSize (InspectorGridHexagonNodeSize mode, float value) {
			if (mode == InspectorGridHexagonNodeSize.Diameter) value *= (float)System.Math.Sqrt(2.0f)/1.5f;
			else if (mode == InspectorGridHexagonNodeSize.Width) value *= (float)System.Math.Sqrt(2.0f/3.0f);
			return value;
		}

		public int Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}
		public int Depth {
			get {
				return depth;
			}
			set {
				depth = value;
			}
		}

		/// <summary>
		/// Default cost of moving one node in a particular direction.
		///
		/// Note: You can only call this after the graph has been scanned. Otherwise it will return zero.
		///
		/// <code>
		///         Z
		///         |
		///         |
		///
		///      6  2  5
		///       \ | /
		/// --  3 - X - 1  ----- X
		///       / | \
		///      7  0  4
		///
		///         |
		///         |
		/// </code>
		/// </summary>
		public uint GetConnectionCost (int dir) {
			return neighbourCosts[dir];
		}

		/// <summary>
		/// Changes the grid shape.
		/// This is equivalent to changing the 'shape' dropdown in the grid graph inspector.
		///
		/// Calling this method will set <see cref="isometricAngle"/>, <see cref="aspectRatio"/>, <see cref="uniformEdgeCosts"/> and <see cref="neighbours"/>
		/// to appropriate values for that shape.
		///
		/// Note: Setting the shape to <see cref="InspectorGridMode.Advanced"/> does not do anything except set the <see cref="inspectorGridMode"/> field.
		///
		/// See: <see cref="inspectorHexagonSizeMode"/>
		/// </summary>
		public void SetGridShape (InspectorGridMode shape) {
			switch (shape) {
			case InspectorGridMode.Grid:
				isometricAngle = 0;
				aspectRatio = 1;
				uniformEdgeCosts = false;
				if (neighbours == NumNeighbours.Six) neighbours = NumNeighbours.Eight;
				break;
			case InspectorGridMode.Hexagonal:
				isometricAngle = StandardIsometricAngle;
				aspectRatio = 1;
				uniformEdgeCosts = true;
				neighbours = NumNeighbours.Six;
				break;
			case InspectorGridMode.IsometricGrid:
				uniformEdgeCosts = false;
				if (neighbours == NumNeighbours.Six) neighbours = NumNeighbours.Eight;
				isometricAngle = StandardIsometricAngle;
				break;
			case InspectorGridMode.Advanced:
			default:
				break;
			}
			inspectorGridMode = shape;
		}

		/// <summary>
		/// Aligns this grid to a given tilemap or grid layout.
		///
		/// This is very handy if your game uses a tilemap for rendering and you want to make sure the graph is laid out exactly the same.
		/// Matching grid parameters manually can be quite tricky in some cases.
		///
		/// The inspector will automatically show a button to align to a tilemap if one is detected in the scene.
		/// If no tilemap is detected, the button be hidden.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This will not change the width/height of the graph. It only aligns the graph to the closest orientation so that the grid nodes will be aligned to the cells in the tilemap.
		/// You can adjust the width/height of the graph separately using e.g. <see cref="SetDimensions"/>.
		///
		/// The following parameters will be updated:
		///
		/// - <see cref="center"/>
		/// - <see cref="nodeSize"/>
		/// - <see cref="isometricAngle"/>
		/// - <see cref="aspectRatio"/>
		/// - <see cref="rotation"/>
		/// - <see cref="uniformEdgeCosts"/>
		/// - <see cref="neighbours"/>
		/// - <see cref="inspectorGridMode"/>
		/// - <see cref="transform"/>
		///
		/// See: tilemaps (view in online documentation for working links)
		/// </summary>
		public void AlignToTilemap (UnityEngine.GridLayout grid) {
			var origin = grid.CellToWorld(new Vector3Int(0, 0, 0));
			var dx = grid.CellToWorld(new Vector3Int(1, 0, 0)) - origin;
			var dy = grid.CellToWorld(new Vector3Int(0, 1, 0)) - origin;

			switch (grid.cellLayout) {
			case GridLayout.CellLayout.Rectangle: {
				var rot = new quaternion(new float3x3(
					dx.normalized,
					-Vector3.Cross(dx, dy).normalized,
					dy.normalized
					));

				this.nodeSize = dy.magnitude;
				this.isometricAngle = 0f;
				this.aspectRatio = dx.magnitude / this.nodeSize;
				if (!float.IsFinite(this.aspectRatio)) this.aspectRatio = 1.0f;
				this.rotation = ((Quaternion)rot).eulerAngles;
				this.uniformEdgeCosts = false;
				if (this.neighbours == NumNeighbours.Six) this.neighbours = NumNeighbours.Eight;
				this.inspectorGridMode = InspectorGridMode.Grid;
				break;
			}
			case GridLayout.CellLayout.Isometric:
				var d1 = grid.CellToWorld(new Vector3Int(1, 1, 0)) - origin;
				var d2 = grid.CellToWorld(new Vector3Int(1, -1, 0)) - origin;
				if (d1.magnitude > d2.magnitude) {
					Memory.Swap(ref d1, ref d2);
				}
				var rot2 = math.mul(new quaternion(new float3x3(
					d2.normalized,
					-Vector3.Cross(d2, d1).normalized,
					d1.normalized
					)), quaternion.RotateY(-math.PI * 0.25f));

				this.isometricAngle = Mathf.Acos(d1.magnitude / d2.magnitude) * Mathf.Rad2Deg;
				this.nodeSize = d2.magnitude / Mathf.Sqrt(2.0f);
				this.rotation = ((Quaternion)rot2).eulerAngles;
				this.uniformEdgeCosts = false;
				this.aspectRatio = 1.0f;
				if (this.neighbours == NumNeighbours.Six) this.neighbours = NumNeighbours.Eight;
				this.inspectorGridMode = InspectorGridMode.IsometricGrid;
				break;
			case GridLayout.CellLayout.Hexagon:
				// Note: Unity does not use a mathematically perfect hexagonal layout by default. The cells can be squished vertically or horizontally.
				var d12 = grid.CellToWorld(new Vector3Int(1, 0, 0)) - origin;
				var d32 = grid.CellToWorld(new Vector3Int(-1, 1, 0)) - origin;
				this.aspectRatio = (d12.magnitude / Mathf.Sqrt(2f/3f)) / (Vector3.Cross(d12.normalized, d32).magnitude / (1.5f * Mathf.Sqrt(2)/3f));
				this.nodeSize = GridGraph.ConvertHexagonSizeToNodeSize(InspectorGridHexagonNodeSize.Width, d12.magnitude / aspectRatio);

				var crossAxis = -Vector3.Cross(d12, Vector3.Cross(d12, d32));

				var rot3 = new quaternion(new float3x3(
					d12.normalized,
					-Vector3.Cross(d12, crossAxis).normalized,
					crossAxis.normalized
					));

				this.rotation = ((Quaternion)rot3).eulerAngles;
				this.uniformEdgeCosts = true;
				this.neighbours = NumNeighbours.Six;
				this.inspectorGridMode = InspectorGridMode.Hexagonal;
				break;
			}

			// Snap center to the closest grid point
			UpdateTransform();
			var layoutCellPivotIsCenter = grid.cellLayout == GridLayout.CellLayout.Hexagon;
			var offset = new Vector3(((width % 2) == 0) != layoutCellPivotIsCenter ? 0 : 0.5f, 0, ((depth % 2) == 0) != layoutCellPivotIsCenter ? 0f : 0.5f);
			var worldOffset = transform.TransformVector(offset);
			var centerCell = grid.WorldToCell(center + worldOffset);
			centerCell.z = 0;
			center = grid.CellToWorld(centerCell) - worldOffset;
			if (float.IsNaN(center.x)) center = Vector3.zero;
			UpdateTransform();
		}

		/// <summary>
		/// Updates <see cref="unclampedSize"/> from <see cref="width"/>, <see cref="depth"/> and <see cref="nodeSize"/> values.
		/// Also <see cref="UpdateTransform generates a new"/>.
		/// Note: This does not rescan the graph, that must be done with Scan
		///
		/// You should use this method instead of setting the <see cref="width"/> and <see cref="depth"/> fields
		/// as the grid dimensions are not defined by the <see cref="width"/> and <see cref="depth"/> variables but by
		/// the <see cref="unclampedSize"/> and <see cref="center"/> variables.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// var width = 80;
		/// var depth = 60;
		/// var nodeSize = 1.0f;
		///
		/// gg.SetDimensions(width, depth, nodeSize);
		///
		/// // Recalculate the graph
		/// AstarPath.active.Scan();
		/// </code>
		/// </summary>
		public void SetDimensions (int width, int depth, float nodeSize) {
			if (width < 1) throw new System.ArgumentOutOfRangeException("width", "width must be at least 1");
			if (depth < 1) throw new System.ArgumentOutOfRangeException("depth", "depth must be at least 1");
			if (nodeSize <= 0) throw new System.ArgumentOutOfRangeException("nodeSize", "nodeSize must be greater than 0");

			unclampedSize = new Vector2(width, depth)*nodeSize;
			this.nodeSize = nodeSize;
			UpdateTransform();
		}

		/// <summary>
		/// Updates the <see cref="transform"/> field which transforms graph space to world space.
		/// In graph space all nodes are laid out in the XZ plane with the first node having a corner in the origin.
		/// One unit in graph space is one node so the first node in the graph is at (0.5,0) the second one at (1.5,0) etc.
		///
		/// This takes the current values of the parameters such as position and rotation into account.
		/// The transform that was used the last time the graph was scanned is stored in the <see cref="transform"/> field.
		///
		/// The <see cref="transform"/> field is calculated using this method when the graph is scanned.
		/// The width, depth variables are also updated based on the <see cref="unclampedSize"/> field.
		/// </summary>
		public void UpdateTransform () {
			CalculateDimensions(out width, out depth, out nodeSize);
			transform = CalculateTransform();
		}

		/// <summary>
		/// Returns a new transform which transforms graph space to world space.
		/// Does not update the <see cref="transform"/> field.
		/// See: <see cref="UpdateTransform"/>
		/// </summary>
		public GraphTransform CalculateTransform () {
			CalculateDimensions(out var newWidth, out var newDepth, out var newNodeSize);

			if (this.neighbours == NumNeighbours.Six) {
				var ax1 = new Vector3(newNodeSize*aspectRatio*Mathf.Sqrt(2f/3f), 0, 0);
				var ax2 = new Vector3(0, 1, 0);
				var ax3 = new Vector3(-aspectRatio * newNodeSize * 0.5f * Mathf.Sqrt(2f/3f), 0, newNodeSize * (1.5f * Mathf.Sqrt(2)/3f));
				var m = new Matrix4x4(
					(Vector4)ax1,
					(Vector4)ax2,
					(Vector4)ax3,
					new Vector4(0, 0, 0, 1)
					);

				var boundsMatrix = Matrix4x4.TRS(center, Quaternion.Euler(rotation), Vector3.one) * m;

				// Generate a matrix where Vector3.zero is the corner of the graph instead of the center
				m = Matrix4x4.TRS(boundsMatrix.MultiplyPoint3x4(-new Vector3(newWidth, 0, newDepth)*0.5F), Quaternion.Euler(rotation), Vector3.one) * m;
				return new GraphTransform(m);
			} else {
				// Generate a matrix which shrinks the graph along the main diagonal
				var squishFactor = new Vector3(Mathf.Cos(Mathf.Deg2Rad*isometricAngle), 1, 1);
				var isometricMatrix = Matrix4x4.Scale(new Vector3(newNodeSize*aspectRatio, 1, newNodeSize));
				var squishAngle = Mathf.Atan2(newNodeSize, newNodeSize*aspectRatio) * Mathf.Rad2Deg;
				isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, -squishAngle, 0)) * Matrix4x4.Scale(squishFactor) * Matrix4x4.Rotate(Quaternion.Euler(0, squishAngle, 0)) * isometricMatrix;

				// Generate a matrix for the bounds of the graph
				// This moves a point to the correct offset in the world and the correct rotation and the aspect ratio and isometric angle is taken into account
				var boundsMatrix = Matrix4x4.TRS(center, Quaternion.Euler(rotation), Vector3.one) * isometricMatrix;

				// Generate a matrix where Vector3.zero is the corner of the graph instead of the center
				// The unit is nodes here (so (0.5,0,0.5) is the position of the first node and (1.5,0,0.5) is the position of the second node)
				// 0.5 is added since this is the node center, not its corner. In graph space a node has a size of 1
				var m = Matrix4x4.TRS(boundsMatrix.MultiplyPoint3x4(-new Vector3(newWidth, 0, newDepth)*0.5F), Quaternion.Euler(rotation), Vector3.one) * isometricMatrix;

				return new GraphTransform(m);
			}
		}

		/// <summary>
		/// Calculates the width/depth of the graph from <see cref="unclampedSize"/> and <see cref="nodeSize"/>.
		/// The node size may be changed due to constraints that the width/depth is not
		/// allowed to be larger than 1024 (artificial limit).
		/// </summary>
		void CalculateDimensions (out int width, out int depth, out float nodeSize) {
			var newSize = unclampedSize;

			// Make sure size is positive
			newSize.x *= Mathf.Sign(newSize.x);
			newSize.y *= Mathf.Sign(newSize.y);

#if !ASTAR_LARGER_GRIDS
			// Clamp the nodeSize so that the graph is never larger than 1024*1024
			nodeSize = Mathf.Max(this.nodeSize, newSize.x/1024f);
			nodeSize = Mathf.Max(this.nodeSize, newSize.y/1024f);
#else
			nodeSize = Mathf.Max(this.nodeSize, newSize.x/8192f);
			nodeSize = Mathf.Max(this.nodeSize, newSize.y/8192f);
#endif

			// Prevent the graph to become smaller than a single node
			newSize.x = newSize.x < nodeSize ? nodeSize : newSize.x;
			newSize.y = newSize.y < nodeSize ? nodeSize : newSize.y;

			size = newSize;

			// Calculate the number of nodes along each side
			width = Mathf.FloorToInt(size.x / nodeSize);
			depth = Mathf.FloorToInt(size.y / nodeSize);

			// Take care of numerical edge cases
			if (Mathf.Approximately(size.x / nodeSize, Mathf.CeilToInt(size.x / nodeSize))) {
				width = Mathf.CeilToInt(size.x / nodeSize);
			}

			if (Mathf.Approximately(size.y / nodeSize, Mathf.CeilToInt(size.y / nodeSize))) {
				depth = Mathf.CeilToInt(size.y / nodeSize);
			}
		}

		public override float NearestNodeDistanceSqrLowerBound (Vector3 position, NNConstraint constraint) {
			if (nodes == null || depth*width*LayerCount != nodes.Length) {
				return float.PositiveInfinity;
			}

			position = transform.InverseTransform(position);

			float xf = position.x;
			float zf = position.z;
			float xc = Mathf.Clamp(xf, 0, width);
			float zc = Mathf.Clamp(zf, 0, depth);

			// Node y coordinates (in graph space) may range from -inf to +inf theoretically, so we only use the xz distance to calculate the lower bound
			return (xf-xc)*(xf-xc) + (zf-zc)*(zf-zc);
		}

		protected virtual GridNodeBase GetNearestFromGraphSpace (Vector3 positionGraphSpace) {
			if (nodes == null || depth*width != nodes.Length) {
				return null;
			}

			float xf = positionGraphSpace.x;
			float zf = positionGraphSpace.z;
			int x = Mathf.Clamp((int)xf, 0, width-1);
			int z = Mathf.Clamp((int)zf, 0, depth-1);
			return nodes[z*width+x];
		}

		public override NNInfo GetNearest (Vector3 position, NNConstraint constraint, float maxDistanceSqr) {
			if (nodes == null || depth*width*LayerCount != nodes.Length) {
				return NNInfo.Empty;
			}

			// Position in global space
			Vector3 globalPosition = position;

			// Position in graph space
			position = transform.InverseTransform(position);

			// Find the coordinates of the closest node
			float xf = position.x;
			float zf = position.z;
			int x = Mathf.Clamp((int)xf, 0, width-1);
			int z = Mathf.Clamp((int)zf, 0, depth-1);

			GridNodeBase minNode = null;

			// If set, we use another distance metric instead of the normal euclidean distance.
			// See constraint.projectionAxis for more info.
			// Note: The grid graph does not support any projectionAxis other than one parallel to the graph's up axis.
			// So if the constraint has a projectionAxis, we treat it as if it is transform.up
			var projectedDistance = constraint != null ? constraint.distanceMetric.isProjectedDistance : false;

			// Search up to this distance
			float minDistSqr = maxDistanceSqr;
			var layerCount = LayerCount;
			var layerStride = width*depth;
			long yOffset = 0;
			float yDistanceScale = 0;
			Int3 up = default;
			if (projectedDistance) {
				up = (Int3)transform.WorldUpAtGraphPosition(globalPosition);
				yOffset = Int3.DotLong((Int3)globalPosition, up);
				yDistanceScale = constraint.distanceMetric.distanceScaleAlongProjectionDirection * Int3.PrecisionFactor * Int3.PrecisionFactor;
			}

			// Check the closest cell
			for (int y = 0; y < layerCount; y++) {
				var node = nodes[z*width + x + layerStride*y];
				if (node != null && (constraint == null || constraint.Suitable(node))) {
					float cost;
					if (projectedDistance) {
						var distX = math.clamp(xf, x, x + 1.0f) - xf;
						var distZ = math.clamp(zf, z, z + 1.0f) - zf;
						var distSideSqr = nodeSize*nodeSize * (distX*distX + distZ*distZ);
						var distUp = (Int3.DotLong(node.position, up) - yOffset) * yDistanceScale;
						cost = Mathf.Sqrt(distSideSqr) + Mathf.Abs(distUp);
						cost = cost*cost;
					} else {
						cost = ((Vector3)node.position-globalPosition).sqrMagnitude;
					}
					if (cost <= minDistSqr) {
						// Minimum distance so far
						minDistSqr = cost;
						minNode = node;
					}
				}
			}

			// Search in a square/spiral pattern around the closest cell
			//
			//      6
			//    7 1 5
			//  8 2 X 0 4
			//    9 3 .
			//      .
			//
			// and so on...

			// Lower bound on the distance to any cell which is not the closest one
			float distanceToEdgeOfNode = Mathf.Min(Mathf.Min(xf - x, 1.0f - (xf - x)), Mathf.Min(zf - z, 1.0f - (zf - z))) * nodeSize;

			for (int w = 1;; w++) {
				// Check if the nodes are within distance limit.
				// This is an optimization to avoid calculating the distance to all nodes.
				// Since we search in a square pattern, we will have to search up to
				// sqrt(2) times further away than the closest node we have found so far (or the maximum distance).
				var distanceThreshold = math.max(0, w-2)*nodeSize + distanceToEdgeOfNode;
				if (minDistSqr - 0.00001f <= distanceThreshold*distanceThreshold) {
					break;
				}

				bool anyInside = false;

				int nx = x + w;
				int nz = z;
				int dx = -1;
				int dz = 1;
				for (int d = 0; d < 4; d++) {
					for (int i = 0; i < w; i++) {
						if (nx >= 0 && nz >= 0 && nx < width && nz < depth) {
							anyInside = true;
							var nodeIndex = nx+nz*width;
							for (int y = 0; y < layerCount; y++) {
								var node = nodes[nodeIndex + layerStride*y];
								if (node != null && (constraint == null || constraint.Suitable(node))) {
									float cost;
									if (projectedDistance) {
										var distX = math.clamp(xf, nx, nx + 1.0f) - xf;
										var distZ = math.clamp(zf, nz, nz + 1.0f) - zf;
										var distSideSqr = nodeSize*nodeSize * (distX*distX + distZ*distZ);
										var distUp = (Int3.DotLong(node.position, up) - yOffset) * yDistanceScale;
										cost = Mathf.Sqrt(distSideSqr) + Mathf.Abs(distUp);
										cost = cost*cost;
									} else {
										cost = ((Vector3)node.position-globalPosition).sqrMagnitude;
									}
									if (cost <= minDistSqr) {
										// Minimum distance so far
										minDistSqr = cost;
										minNode = node;
									}
								}
							}
						}
						nx += dx;
						nz += dz;
					}

					// Rotate direction by 90 degrees counter-clockwise
					var ndx = -dz;
					var ndz = dx;
					dx = ndx;
					dz = ndz;
				}

				// No nodes were inside grid bounds
				// We will not be able to find any more valid nodes
				// so just break
				if (!anyInside) break;
			}

			if (minNode != null) {
				if (projectedDistance) {
					// Walk towards the closest cell.
					// We do this to ensure that if projectedDistance is true, then internal edges in the graph
					// will *never* be obstructions for the agent.
					//
					// For example, if we have two nodes A and B which have different Y coordinates,
					// and we have an agent (X) which has just stepped out of A and into node B.
					// Assume that A and B are connected.
					//
					//  __A__X
					//
					//       __B__
					//
					// In this case, even though A might be closer with DistanceMetric.ClosestAsSeenFromAboveSoft,
					// we want to return node B because clamping to A would mean clamping along to an obstacle edge
					// which does not exist (A and B are connected).
					// This is very important when this is used to clamp the agent to the navmesh,
					// but it is also generally what you want in other situations as well.
					while (true) {
						var dx = x - minNode.XCoordinateInGrid;
						var dz = z - minNode.ZCoordinateInGrid;
						if (dx == 0 && dz == 0) break;
						var d1 = dx > 0 ? 1 : (dx < 0 ? 3 : -1);
						var d2 = dz > 0 ? 2 : (dz < 0 ? 0 : -1);
						if (Mathf.Abs(dx) < Mathf.Abs(dz)) Memory.Swap(ref d1, ref d2);

						// Try to walk along d1, if that does not work, try d2
						var next = minNode.GetNeighbourAlongDirection(d1);
						if (next != null && (constraint == null || constraint.Suitable(next))) minNode = next;
						else if (d2 != -1 && (next = minNode.GetNeighbourAlongDirection(d2)) != null && (constraint == null || constraint.Suitable(next))) minNode = next;
						else break;
					}
				}

				// Closest point on the node if the node is treated as a square
				var nx = minNode.XCoordinateInGrid;
				var nz = minNode.ZCoordinateInGrid;
				var closest = transform.Transform(new Vector3(Mathf.Clamp(xf, nx, nx+1f), transform.InverseTransform((Vector3)minNode.position).y, Mathf.Clamp(zf, nz, nz+1f)));
				// If projectedDistance is enabled, the distance is already accurate.
				// Otherwise, we need to calculate the distance to the closest point on the node instead of to the center
				var cost = projectedDistance ? minDistSqr : (closest-globalPosition).sqrMagnitude;
				return cost <= maxDistanceSqr ? new NNInfo(
					minNode,
					closest,
					cost
					) : NNInfo.Empty;
			} else {
				return NNInfo.Empty;
			}
		}

		public override NNInfo RandomPointOnSurface (NNConstraint nnConstraint = null, bool highQuality = true) {
			if (!isScanned || nodes.Length == 0) return NNInfo.Empty;

			// All nodes have the same surface area, so just pick a random node
			for (int i = 0; i < 10; i++) {
				var node = this.nodes[UnityEngine.Random.Range(0, this.nodes.Length)];
				if (node != null && (nnConstraint == null || nnConstraint.Suitable(node))) {
					return new NNInfo(node, node.RandomPointOnSurface(), 0);
				}
			}

			// If a valid node was not found after a few tries, the graph likely contains a lot of unwalkable/unsuitable nodes.
			// Fall back to the base method which will try to find a valid node by checking all nodes.
			return base.RandomPointOnSurface(nnConstraint, highQuality);
		}

		/// <summary>
		/// Sets up <see cref="neighbourOffsets"/> with the current settings. <see cref="neighbourOffsets"/>, <see cref="neighbourCosts"/>, <see cref="neighbourXOffsets"/> and <see cref="neighbourZOffsets"/> are set up.
		/// The cost for a non-diagonal movement between two adjacent nodes is RoundToInt (<see cref="nodeSize"/> * Int3.Precision)
		/// The cost for a diagonal movement between two adjacent nodes is RoundToInt (<see cref="nodeSize"/> * Sqrt (2) * Int3.Precision)
		/// </summary>
		public virtual void SetUpOffsetsAndCosts () {
			// First 4 are for the four directly adjacent nodes the last 4 are for the diagonals
			neighbourOffsets[0] = -width;
			neighbourOffsets[1] = 1;
			neighbourOffsets[2] = width;
			neighbourOffsets[3] = -1;
			neighbourOffsets[4] = -width+1;
			neighbourOffsets[5] = width+1;
			neighbourOffsets[6] = width-1;
			neighbourOffsets[7] = -width-1;

			// The width of a single node, and thus also the distance between two adjacent nodes (axis aligned).
			// For hexagonal graphs the node size is different from the width of a hexagon.
			float nodeWidth = neighbours == NumNeighbours.Six ? ConvertNodeSizeToHexagonSize(InspectorGridHexagonNodeSize.Width, nodeSize) : nodeSize;

			uint straightCost = (uint)Mathf.RoundToInt(nodeWidth*Int3.Precision);

			// Diagonals normally cost sqrt(2) (approx 1.41) times more
			uint diagonalCost = uniformEdgeCosts ? straightCost : (uint)Mathf.RoundToInt(nodeWidth*Mathf.Sqrt(2F)*Int3.Precision);

			neighbourCosts[0] = straightCost;
			neighbourCosts[1] = straightCost;
			neighbourCosts[2] = straightCost;
			neighbourCosts[3] = straightCost;
			neighbourCosts[4] = diagonalCost;
			neighbourCosts[5] = diagonalCost;
			neighbourCosts[6] = diagonalCost;
			neighbourCosts[7] = diagonalCost;

			/*         Z
			 *         |
			 *         |
			 *
			 *      6  2  5
			 *       \ | /
			 * --  3 - X - 1  ----- X
			 *       / | \
			 *      7  0  4
			 *
			 *         |
			 *         |
			 */
		}

		public enum RecalculationMode {
			/// <summary>Recalculates the nodes from scratch. Used when the graph is first scanned. You should have destroyed all existing nodes before updating the graph with this mode.</summary>
			RecalculateFromScratch,
			/// <summary>Recalculate the minimal number of nodes necessary to guarantee changes inside the graph update's bounding box are taken into account. Some data may be read from the existing nodes</summary>
			RecalculateMinimal,
			/// <summary>Nodes are not recalculated. Used for graph updates which only set node properties</summary>
			NoRecalculation,
		}

		/// <summary>
		/// Moves the grid by a number of nodes.
		///
		/// This is used by the <see cref="ProceduralGraphMover"/> component to efficiently move the graph.
		///
		/// All nodes that can stay in the same position will stay. The ones that would have fallen off the edge of the graph will wrap around to the other side
		/// and then be recalculated.
		///
		/// See: <see cref="ProceduralGraphMover"/>
		///
		/// Returns: An async graph update promise. See <see cref="IGraphUpdatePromise"/>.
		/// </summary>
		/// <param name="dx">Number of nodes along the graph's X axis to move by.</param>
		/// <param name="dz">Number of nodes along the graph's Z axis to move by.</param>
		public IGraphUpdatePromise TranslateInDirection(int dx, int dz) => new GridGraphMovePromise(this, dx, dz);

		class GridGraphMovePromise : IGraphUpdatePromise {
			public GridGraph graph;
			public int dx;
			public int dz;
			IGraphUpdatePromise[] promises;
			IntRect[] rects;
			int3 startingSize;

			static void DecomposeInsetsToRectangles (int width, int height, int insetLeft, int insetRight, int insetBottom, int insetTop, IntRect[] output) {
				output[0] = new IntRect(0, 0, insetLeft - 1, height - 1);
				output[1] = new IntRect(width - insetRight, 0, width - 1, height - 1);
				output[2] = new IntRect(insetLeft, 0, width - insetRight - 1, insetBottom - 1);
				output[3] = new IntRect(insetLeft, height - insetTop - 1, width - insetRight - 1, height - 1);
			}

			public GridGraphMovePromise(GridGraph graph, int dx, int dz) {
				this.graph = graph;
				this.dx = dx;
				this.dz = dz;
				var transform = graph.transform * Matrix4x4.Translate(new Vector3(dx, 0, dz));

				// If the graph is moved by more than half its width/depth, then we recalculate the whole graph instead
				startingSize = new int3(graph.width, graph.LayerCount, graph.depth);
				if (math.abs(dx) > graph.width/2 || math.abs(dz) > graph.depth/2) {
					rects = new IntRect[1] {
						new IntRect(0, 0, graph.width - 1, graph.depth - 1)
					};
				} else {
					// We recalculate nodes within some distance from each side of the (translated) grid.
					// We must always recalculate at least the nodes along the border, since they may have had
					// connections to nodes that are now outside the graph.
					// TODO: This can potentially be optimized to just clearing the out-of-bounds connections
					// on border nodes, instead of completely recalculating the border nodes.
					var insetLeft = math.max(1, -dx);
					var insetRight = math.max(1, dx);
					var insetBottom = math.max(1, -dz);
					var insetTop = math.max(1, dz);
					rects = new IntRect[4];
					DecomposeInsetsToRectangles(graph.width, graph.depth, insetLeft, insetRight, insetBottom, insetTop, rects);
				}

				promises = new GridGraphUpdatePromise[rects.Length];
				var nodes = new GridGraphUpdatePromise.NodesHolder { nodes = graph.nodes };
				for (int i = 0; i < rects.Length; i++) {
					var dependencyTracker = ObjectPool<JobDependencyTracker>.Claim();
					// TODO: Use the exact rect given, don't expand it using physics checks
					// We do need to expand the insets using erosion, though.
					promises[i] = new GridGraphUpdatePromise(
						graph: graph,
						transform: transform,
						nodes: nodes,
						nodeArrayBounds: startingSize,
						rect: rects[i],
						dependencyTracker: dependencyTracker,
						nodesDependsOn: default,
						allocationMethod: Allocator.Persistent,
						recalculationMode: RecalculationMode.RecalculateMinimal,
						graphUpdateObject: null,
						ownsJobDependencyTracker: true,
						isFinalUpdate: false
						);
				}
			}

			public IEnumerator<JobHandle> Prepare () {
				yield return graph.nodeData.Rotate2D(-dx, -dz, default);

				for (int i = 0; i < promises.Length; i++) {
					var it = promises[i].Prepare();
					while (it.MoveNext()) yield return it.Current;
				}
			}

			public void Apply (IGraphUpdateContext ctx) {
				graph.AssertSafeToUpdateGraph();
				var nodes = graph.nodes;
				if (!math.all(new int3(graph.width, graph.LayerCount, graph.depth) == startingSize)) throw new System.InvalidOperationException("The graph has been resized since the update was created. This is not allowed.");
				if (nodes == null || nodes.Length != graph.width * graph.depth * graph.LayerCount) {
					throw new System.InvalidOperationException("The Grid Graph is not scanned, cannot recalculate connections.");
				}

				Profiler.BeginSample("Rotating node array");
				Memory.Rotate3DArray(nodes, startingSize, -dx, -dz);
				Profiler.EndSample();

				Profiler.BeginSample("Recalculating node indices");
				// Recalculate the node indices for all nodes that exist before the update
				for (int y = 0; y < startingSize.y; y++) {
					var layerOffset = y * startingSize.x * startingSize.z;
					for (int z = 0; z < startingSize.z; z++) {
						var rowOffset = z * startingSize.x;
						for (int x = 0; x < startingSize.x; x++) {
							var nodeIndexXZ = rowOffset + x;
							var node = nodes[layerOffset + nodeIndexXZ];
							if (node != null) node.NodeInGridIndex = nodeIndexXZ;
						}
					}
				}
				Profiler.EndSample();

				Profiler.BeginSample("Clearing custom connections");
				var layers = graph.LayerCount;
				for (int i = 0; i < rects.Length; i++) {
					var r = rects[i];
					for (int y = 0; y < layers; y++) {
						var layerOffset = y * graph.width * graph.depth;
						for (int z = r.ymin; z <= r.ymax; z++) {
							var rowOffset = z * graph.width + layerOffset;
							for (int x = r.xmin; x <= r.xmax; x++) {
								var node = nodes[rowOffset + x];
								if (node != null) {
									// Clear connections on all nodes that are wrapped and placed on the other side of the graph.
									// This is both to clear any custom connections (which do not really make sense after moving the node)
									// and to prevent possible exceptions when the node will later (possibly) be destroyed because it was
									// not needed anymore (only for layered grid graphs).
									node.ClearCustomConnections(true);
								}
							}
						}
					}
				}
				Profiler.EndSample();
				for (int i = 0; i < promises.Length; i++) {
					promises[i].Apply(ctx);
				}
				// Move the center (this is in world units, so we need to convert it back from graph space)
				graph.center += graph.transform.TransformVector(new Vector3(dx, 0, dz));
				graph.UpdateTransform();

				if (promises.Length > 0) graph.rules.ExecuteRuleMainThread(GridGraphRule.Pass.AfterApplied, (promises[0] as GridGraphUpdatePromise).context);
			}
		}

		class GridGraphUpdatePromise : IGraphUpdatePromise {
			/// <summary>Reference to a nodes array to allow multiple serial updates to have a common reference to the nodes</summary>
			public class NodesHolder {
				public GridNodeBase[] nodes;
			}
			public GridGraph graph;
			public NodesHolder nodes;
			public JobDependencyTracker dependencyTracker;
			public int3 nodeArrayBounds;
			public IntRect rect;
			public JobHandle nodesDependsOn;
			public Allocator allocationMethod;
			public RecalculationMode recalculationMode;
			public GraphUpdateObject graphUpdateObject;
			IntBounds writeMaskBounds;
			internal GridGraphRules.Context context;
			bool emptyUpdate;
			IntBounds readBounds;
			IntBounds fullRecalculationBounds;
			public bool ownsJobDependencyTracker = false;
			bool isFinalUpdate;
			GraphTransform transform;

			public int CostEstimate => fullRecalculationBounds.volume;

			public GridGraphUpdatePromise(GridGraph graph, GraphTransform transform, NodesHolder nodes, int3 nodeArrayBounds, IntRect rect, JobDependencyTracker dependencyTracker, JobHandle nodesDependsOn, Allocator allocationMethod, RecalculationMode recalculationMode, GraphUpdateObject graphUpdateObject, bool ownsJobDependencyTracker, bool isFinalUpdate) {
				this.graph = graph;
				this.transform = transform;
				this.nodes = nodes;
				this.nodeArrayBounds = nodeArrayBounds;
				this.dependencyTracker = dependencyTracker;
				this.nodesDependsOn = nodesDependsOn;
				this.allocationMethod = allocationMethod;
				this.recalculationMode = recalculationMode;
				this.graphUpdateObject = graphUpdateObject;
				this.ownsJobDependencyTracker = ownsJobDependencyTracker;
				this.isFinalUpdate = isFinalUpdate;
				CalculateRectangles(graph, rect, out this.rect, out var fullRecalculationRect, out var writeMaskRect, out var readRect);

				if (recalculationMode == RecalculationMode.RecalculateFromScratch) {
					// If we are not allowed to read from the graph, we need to recalculate everything that we would otherwise just have read from the graph
					fullRecalculationRect = readRect;
				}

				// Check if there is anything to do. The bounds may not even overlap the graph.
				// Note that writeMaskRect may overlap the graph even though fullRecalculationRect is invalid.
				// We ignore that case however since any changes we might write can only be caused by a node that is actually recalculated.
				if (!fullRecalculationRect.IsValid()) {
					emptyUpdate = true;
				}

				// Note that IntRects are defined with inclusive (min,max) coordinates while IntBounds use an exclusive upper bounds.
				readBounds = new IntBounds(readRect.xmin, 0, readRect.ymin, readRect.xmax + 1, nodeArrayBounds.y, readRect.ymax + 1);
				fullRecalculationBounds = new IntBounds(fullRecalculationRect.xmin, 0, fullRecalculationRect.ymin, fullRecalculationRect.xmax + 1, nodeArrayBounds.y, fullRecalculationRect.ymax + 1);
				writeMaskBounds = new IntBounds(writeMaskRect.xmin, 0, writeMaskRect.ymin, writeMaskRect.xmax + 1, nodeArrayBounds.y, writeMaskRect.ymax + 1);

				// If recalculating a very small number of nodes, then disable dependency tracking and just run jobs one after the other.
				// This is faster since dependency tracking has some overhead
				if (ownsJobDependencyTracker) dependencyTracker.SetLinearDependencies(CostEstimate < 500);
			}

			/// <summary>Calculates the rectangles used for different purposes during a graph update.</summary>
			/// <param name="graph">The graph</param>
			/// <param name="rect">The rectangle to update. Anything inside this rectangle may have changed (which may affect nodes outside this rectangle as well).</param>
			/// <param name="originalRect">The original rectangle passed to the update method, clamped to the grid.</param>
			/// <param name="fullRecalculationRect">The rectangle of nodes which will be recalculated from scratch.</param>
			/// <param name="writeMaskRect">The rectangle of nodes which will have their results written back to the graph.</param>
			/// <param name="readRect">The rectangle of nodes which we need to read from in order to recalculate all nodes in writeMaskRect correctly.</param>
			public static void CalculateRectangles (GridGraph graph, IntRect rect, out IntRect originalRect, out IntRect fullRecalculationRect, out IntRect writeMaskRect, out IntRect readRect) {
				fullRecalculationRect = rect;
				var collision = graph.collision;
				if (collision.collisionCheck && collision.type != ColliderType.Ray) fullRecalculationRect = fullRecalculationRect.Expand(Mathf.FloorToInt(collision.diameter * 0.5f + 0.5f));

				// Rectangle of nodes which will have their results written back to the node class objects.
				// Due to erosion a bit more of the graph may be affected by the updates in the fullRecalculationBounds.
				writeMaskRect = fullRecalculationRect.Expand(graph.erodeIterations + 1);

				// Rectangle of nodes which we need to read from in order to recalculate all nodes in writeMaskRect correctly.
				// Due to how erosion works we need to recalculate erosion in an even larger region to make sure we
				// get the correct result inside the writeMask
				readRect = writeMaskRect.Expand(graph.erodeIterations + 1);

				// Clamp to the grid dimensions
				var gridRect = new IntRect(0, 0, graph.width - 1, graph.depth - 1);
				readRect = IntRect.Intersection(readRect, gridRect);
				fullRecalculationRect = IntRect.Intersection(fullRecalculationRect, gridRect);
				writeMaskRect = IntRect.Intersection(writeMaskRect, gridRect);
				originalRect = IntRect.Intersection(rect, gridRect);
			}


			public IEnumerator<JobHandle> Prepare () {
				if (emptyUpdate) yield break;

				var collision = graph.collision;
				var rules = graph.rules;

				if (recalculationMode != RecalculationMode.RecalculateFromScratch) {
					// In case a previous graph update has changed the number of layers in the graph
					writeMaskBounds.max.y = fullRecalculationBounds.max.y = readBounds.max.y = graph.nodeData.bounds.max.y;
				}

				// We never reduce the number of layers in an existing graph.
				// Unless we are scanning the graph (not doing an update).
				var minLayers = recalculationMode == RecalculationMode.RecalculateFromScratch ? 1 : fullRecalculationBounds.max.y;

				if (recalculationMode == RecalculationMode.RecalculateMinimal && readBounds == fullRecalculationBounds) {
					// There is no point reading from the graph since we are recalculating all those nodes anyway.
					// This happens if an update is done to the whole graph.
					// Skipping the read can improve performance quite a lot for that kind of updates.
					// This is purely an optimization and should not change the result.
					recalculationMode = RecalculationMode.RecalculateFromScratch;
				}

#if ASTAR_DEBUG
				var debugMatrix = graph.transform.matrix;
				// using (Draw.WithDuration(1)) {
				using (Draw.WithLineWidth(2)) {
					using (Draw.WithMatrix(debugMatrix)) {
						Draw.xz.WireRectangle(Rect.MinMaxRect(fullRecalculationBounds.min.x, fullRecalculationBounds.min.z, fullRecalculationBounds.max.x, fullRecalculationBounds.max.z), Color.yellow);
					}
					using (Draw.WithMatrix(debugMatrix * Matrix4x4.Translate(Vector3.up*0.1f))) {
						Draw.xz.WireRectangle(Rect.MinMaxRect(writeMaskBounds.min.x, writeMaskBounds.min.z, writeMaskBounds.max.x, writeMaskBounds.max.z), Color.magenta);
						Draw.xz.WireRectangle(Rect.MinMaxRect(readBounds.min.x, readBounds.min.z, readBounds.max.x, readBounds.max.z), Color.blue);
						Draw.xz.WireRectangle((Rect)rect, Color.green);
					}
				}
#endif

				var layeredDataLayout = graph is LayerGridGraph;
				float characterHeight = graph is LayerGridGraph lg ? lg.characterHeight : float.PositiveInfinity;

				context = new GridGraphRules.Context {
					graph = graph,
					data = new GridGraphScanData {
						dependencyTracker = dependencyTracker,
						transform = transform,
						up = transform.TransformVector(Vector3.up).normalized,
					}
				};

				if (recalculationMode == RecalculationMode.RecalculateFromScratch || recalculationMode == RecalculationMode.RecalculateMinimal) {
					var heightCheck = collision.heightCheck && !collision.use2D;
					if (heightCheck) {
						var layerCount = dependencyTracker.NewNativeArray<int>(1, allocationMethod, NativeArrayOptions.UninitializedMemory);
						float nodeWidth = graph.neighbours == NumNeighbours.Six ? ConvertNodeSizeToHexagonSize(InspectorGridHexagonNodeSize.Width, graph.nodeSize) : graph.nodeSize;
						yield return context.data.HeightCheck(collision, nodeWidth, graph.MaxLayers, fullRecalculationBounds, layerCount, characterHeight, allocationMethod);
						// The size of the buffers depend on the height check for layered grid graphs since the number of layers might change.
						// Never reduce the layer count of the graph.
						// Unless we are recalculating the whole graph: in that case we don't care about the existing layers.
						// For (not layered) grid graphs this is always 1.
						var layers = Mathf.Max(minLayers, layerCount[0]);
						readBounds.max.y = fullRecalculationBounds.max.y = writeMaskBounds.max.y = layers;
						context.data.heightHitsBounds.max.y = layerCount[0];
						context.data.nodes = new GridGraphNodeData {
							bounds = fullRecalculationBounds,
							numNodes = fullRecalculationBounds.volume,
							layeredDataLayout = layeredDataLayout,
							allocationMethod = allocationMethod,
						};
						context.data.nodes.AllocateBuffers(dependencyTracker);

						// Set the positions to be used if the height check ray didn't hit anything
						context.data.SetDefaultNodePositions(transform);
						context.data.CopyHits(context.data.heightHitsBounds);
						context.data.CalculateWalkabilityFromHeightData(graph.useRaycastNormal, collision.unwalkableWhenNoGround, graph.maxSlope, characterHeight);
					} else {
						context.data.nodes = new GridGraphNodeData {
							bounds = fullRecalculationBounds,
							numNodes = fullRecalculationBounds.volume,
							layeredDataLayout = layeredDataLayout,
							allocationMethod = allocationMethod,
						};
						context.data.nodes.AllocateBuffers(dependencyTracker);
						context.data.SetDefaultNodePositions(transform);
						// Mark all nodes as walkable to begin with
						context.data.nodes.walkable.MemSet(true).Schedule(dependencyTracker);
						// Set the normals to point straight up
						context.data.nodes.normals.MemSet(new float4(context.data.up.x, context.data.up.y, context.data.up.z, 0)).Schedule(dependencyTracker);
					}

					context.data.SetDefaultPenalties(graph.initialPenalty);

					// Kick off jobs early while we prepare the rest of them
					JobHandle.ScheduleBatchedJobs();

					rules.RebuildIfNecessary();

					{
						// Here we execute some rules and possibly wait for some dependencies to complete.
						// If main thread rules are used then we need to wait for all previous jobs to complete before the rule is actually executed.
						var wait = rules.ExecuteRule(GridGraphRule.Pass.BeforeCollision, context);
						while (wait.MoveNext()) yield return wait.Current;
					}

					if (collision.collisionCheck) {
						context.tracker.timeSlice = TimeSlice.MillisFromNow(1);
						var wait = context.data.CollisionCheck(collision, fullRecalculationBounds);
						while (wait != null && wait.MoveNext()) {
							yield return wait.Current;
							context.tracker.timeSlice = TimeSlice.MillisFromNow(2);
						}
					}

					{
						var wait = rules.ExecuteRule(GridGraphRule.Pass.BeforeConnections, context);
						while (wait.MoveNext()) yield return wait.Current;
					}

					if (recalculationMode == RecalculationMode.RecalculateMinimal) {
						// context.data.nodes = context.data.nodes.ReadFromNodesAndCopy(nodes, new Slice3D(nodeArrayBounds, readBounds), nodesDependsOn, graph.nodeData.normals, graphUpdateObject != null ? graphUpdateObject.resetPenaltyOnPhysics : true, dependencyTracker);
						var newNodes = new GridGraphNodeData {
							bounds = readBounds,
							numNodes = readBounds.volume,
							layeredDataLayout = layeredDataLayout,
							allocationMethod = allocationMethod,
						};
						newNodes.AllocateBuffers(dependencyTracker);
						// If our layer count is increased, then some nodes may end up with uninitialized normals if we didn't do this memset
						newNodes.normals.MemSet(float4.zero).Schedule(dependencyTracker);
						newNodes.walkable.MemSet(false).Schedule(dependencyTracker);
						newNodes.walkableWithErosion.MemSet(false).Schedule(dependencyTracker);
						newNodes.CopyFrom(graph.nodeData, true, dependencyTracker);
						newNodes.CopyFrom(context.data.nodes, graphUpdateObject != null ? graphUpdateObject.resetPenaltyOnPhysics : true, dependencyTracker);
						context.data.nodes = newNodes;
					}
				} else {
					// If we are not allowed to recalculate the graph then we read all the necessary info from the existing nodes
					// context.data.nodes = GridGraphNodeData.ReadFromNodes(nodes, new Slice3D(nodeArrayBounds, readBounds), nodesDependsOn, graph.nodeData.normals, allocationMethod, context.data.nodes.layeredDataLayout, dependencyTracker);

					context.data.nodes = new GridGraphNodeData {
						bounds = readBounds,
						numNodes = readBounds.volume,
						layeredDataLayout = layeredDataLayout,
						allocationMethod = allocationMethod,
					};
					UnityEngine.Assertions.Assert.IsTrue(graph.nodeData.bounds.Contains(context.data.nodes.bounds));
					context.data.nodes.AllocateBuffers(dependencyTracker);
					context.data.nodes.CopyFrom(graph.nodeData, true, dependencyTracker);
				}

				if (graphUpdateObject != null) {
					// The GraphUpdateObject has an empty implementation of WillUpdateNode,
					// so we only need to call it if we are dealing with a subclass of GraphUpdateObject.
					// The WillUpdateNode method will be deprecated in the future.
					if (graphUpdateObject.GetType() != typeof(GraphUpdateObject)) {
						// Mark nodes that might be changed
						var nodes = this.nodes.nodes;
						for (int y = writeMaskBounds.min.y; y < writeMaskBounds.max.y; y++) {
							for (int z = writeMaskBounds.min.z; z < writeMaskBounds.max.z; z++) {
								var rowOffset = y*nodeArrayBounds.x*nodeArrayBounds.z + z*nodeArrayBounds.x;
								for (int x = writeMaskBounds.min.x; x < writeMaskBounds.max.x; x++) {
									graphUpdateObject.WillUpdateNode(nodes[rowOffset + x]);
								}
							}
						}
					}

					var updateRect = rect;
					if (updateRect.IsValid()) {
						// Note that IntRects are defined with inclusive (min,max) coordinates while IntBounds use exclusive upper bounds.
						var updateBounds = new IntBounds(updateRect.xmin, 0, updateRect.ymin, updateRect.xmax + 1, context.data.nodes.layers, updateRect.ymax + 1).Offset(-context.data.nodes.bounds.min);
						var nodeIndices = dependencyTracker.NewNativeArray<int>(updateBounds.volume, context.data.nodes.allocationMethod, NativeArrayOptions.ClearMemory);
						int i = 0;
						var dataBoundsSize = context.data.nodes.bounds.size;
						for (int y = updateBounds.min.y; y < updateBounds.max.y; y++) {
							for (int z = updateBounds.min.z; z < updateBounds.max.z; z++) {
								var rowOffset = y*dataBoundsSize.x*dataBoundsSize.z + z*dataBoundsSize.x;
								for (int x = updateBounds.min.x; x < updateBounds.max.x; x++) {
									nodeIndices[i++] = rowOffset + x;
								}
							}
						}
						graphUpdateObject.ApplyJob(new GraphUpdateObject.GraphUpdateData {
							nodePositions = context.data.nodes.positions,
							nodePenalties = context.data.nodes.penalties,
							nodeWalkable = context.data.nodes.walkable,
							nodeNormals = context.data.nodes.normals,
							nodeTags = context.data.nodes.tags,
							nodeIndices = nodeIndices,
						}, dependencyTracker);
					}
				}

				// Calculate the connections between nodes and also erode the graph
				context.data.Connections(graph.maxStepHeight, graph.maxStepUsesSlope, context.data.nodes.bounds, graph.neighbours, graph.cutCorners, collision.use2D, false, characterHeight);
				{
					var wait = rules.ExecuteRule(GridGraphRule.Pass.AfterConnections, context);
					while (wait.MoveNext()) yield return wait.Current;
				}

				if (graph.erodeIterations > 0) {
					context.data.Erosion(graph.neighbours, graph.erodeIterations, writeMaskBounds, graph.erosionUseTags, graph.erosionFirstTag, graph.erosionTagsPrecedenceMask);
					{
						var wait = rules.ExecuteRule(GridGraphRule.Pass.AfterErosion, context);
						while (wait.MoveNext()) yield return wait.Current;
					}

					// After erosion is done we need to recalculate the node connections
					context.data.Connections(graph.maxStepHeight, graph.maxStepUsesSlope, context.data.nodes.bounds, graph.neighbours, graph.cutCorners, collision.use2D, true, characterHeight);
					{
						var wait = rules.ExecuteRule(GridGraphRule.Pass.AfterConnections, context);
						while (wait.MoveNext()) yield return wait.Current;
					}
				} else {
					// If erosion is disabled we can just copy nodeWalkable to nodeWalkableWithErosion
					// TODO: Can we just do an assignment of the whole array?
					context.data.nodes.walkable.CopyToJob(context.data.nodes.walkableWithErosion).Schedule(dependencyTracker);
				}

				{
					var wait = rules.ExecuteRule(GridGraphRule.Pass.PostProcess, context);
					while (wait.MoveNext()) yield return wait.Current;
				}

				// Make the graph's buffers be tracked by the dependency tracker,
				// so that they can be disposed automatically, unless we persist them.
				graph.nodeData.TrackBuffers(dependencyTracker);

				if (recalculationMode == RecalculationMode.RecalculateFromScratch) {
					UnityEngine.Assertions.Assert.AreEqual(Allocator.Persistent, context.data.nodes.allocationMethod);
					graph.nodeData = context.data.nodes;
				} else {
					// Copy node data back to the graph's buffer
					graph.nodeData.ResizeLayerCount(context.data.nodes.layers, dependencyTracker);
					graph.nodeData.CopyFrom(context.data.nodes, writeMaskBounds, true, dependencyTracker);
				}

				graph.nodeData.PersistBuffers(dependencyTracker);

				// We need to wait for the nodes array to be fully initialized before trying to resize it or reading from it
				yield return nodesDependsOn;

				yield return dependencyTracker.AllWritesDependency;

				dependencyTracker.ClearMemory();
			}

			public void Apply (IGraphUpdateContext ctx) {
				graph.AssertSafeToUpdateGraph();
				if (emptyUpdate) {
					Dispose();
					if (isFinalUpdate) graph.rules.ExecuteRuleMainThread(GridGraphRule.Pass.AfterApplied, context ?? new GridGraphRules.Context { graph = graph });
					return;
				}

				var destroyPreviousNodes = nodes.nodes != graph.nodes;
				// For layered grid graphs, we may need to allocate more nodes for the upper layers
				if (context.data.nodes.layers > 1) {
					nodeArrayBounds.y = context.data.nodes.layers;
					var newNodeCount = nodeArrayBounds.x*nodeArrayBounds.y*nodeArrayBounds.z;
					// Resize the nodes array.
					// We reference it via a shared reference, so that if any other updates will run after this one,
					// they will see the resized nodes array immediately.
					Memory.Realloc(ref nodes.nodes, newNodeCount);

					// This job needs to be executed on the main thread
					// TODO: Do we need writeMaskBounds to prevent allocating nodes outside the permitted region?
					new JobAllocateNodes {
						active = graph.active,
						nodeNormals = graph.nodeData.normals,
						dataBounds = context.data.nodes.bounds,
						nodeArrayBounds = nodeArrayBounds,
						nodes = nodes.nodes,
						newGridNodeDelegate = graph.newGridNodeDelegate,
					}.Execute();
				}

				var assignToNodesJob = graph.nodeData.AssignToNodes(this.nodes.nodes, nodeArrayBounds, writeMaskBounds, graph.graphIndex, default, dependencyTracker);
				assignToNodesJob.Complete();

				// Destroy the old nodes (if any) and assign the new nodes as an atomic operation from the main thread's perspective
				if (nodes.nodes != graph.nodes) {
					if (destroyPreviousNodes) {
						graph.DestroyAllNodes();
					}
					graph.nodes = nodes.nodes;
					graph.LayerCount = context.data.nodes.layers;
				}

				// Recalculate off mesh links in the affected area
				ctx.DirtyBounds(graph.GetBoundsFromRect(new IntRect(writeMaskBounds.min.x, writeMaskBounds.min.z, writeMaskBounds.max.x - 1, writeMaskBounds.max.z - 1)));
				Dispose();

				if (isFinalUpdate) graph.rules.ExecuteRuleMainThread(GridGraphRule.Pass.AfterApplied, context);
			}

			public void Dispose () {
				if (ownsJobDependencyTracker) {
					ObjectPool<JobDependencyTracker>.Release(ref dependencyTracker);
					if (context != null) context.data.dependencyTracker = null;
				}
			}
		}

		protected override IGraphUpdatePromise ScanInternal (bool async) {
			if (nodeSize <= 0) {
				return null;
			}

			// Make sure the matrix is up to date
			UpdateTransform();

#if !ASTAR_LARGER_GRIDS
			if (width > 1024 || depth > 1024) {
				Debug.LogError("One of the grid's sides is longer than 1024 nodes");
				return null;
			}
#endif

			SetUpOffsetsAndCosts();

			// Set a global reference to this graph so that nodes can find it
			GridNode.SetGridGraph((int)graphIndex, this);

			// Create and initialize the collision class
			collision ??= new GraphCollision();
			collision.Initialize(transform, nodeSize);


			// Used to allocate buffers for jobs
			var dependencyTracker = ObjectPool<JobDependencyTracker>.Claim();

			// Create all nodes
			var newNodes = AllocateNodesJob(width * depth, out var allocateNodesJob);

			// TODO: Set time slicing in dependency tracker
			return new GridGraphUpdatePromise(
				graph: this,
				transform: transform,
				nodes: new GridGraphUpdatePromise.NodesHolder { nodes = newNodes },
				nodeArrayBounds: new int3(width, 1, depth),
				rect: new IntRect(0, 0, width - 1, depth - 1),
				dependencyTracker: dependencyTracker,
				nodesDependsOn: allocateNodesJob,
				allocationMethod: Allocator.Persistent,
				recalculationMode: RecalculationMode.RecalculateFromScratch,
				graphUpdateObject: null,
				ownsJobDependencyTracker: true,
				isFinalUpdate: true
				);
		}

		/// <summary>
		/// Set walkability for multiple nodes at once.
		///
		/// If you are calculating your graph's walkability in some custom way, you can use this method to copy that data to the graph.
		/// In most cases you'll not use this method, but instead build your world with colliders and such, and then scan the graph.
		///
		/// Note: Any other graph updates may overwrite this data.
		///
		/// <code>
		/// // Perform the update when it is safe to do so
		/// AstarPath.active.AddWorkItem(() => {
		///     var grid = AstarPath.active.data.gridGraph;
		///     // Mark all nodes in a 10x10 square, in the top-left corner of the graph, as unwalkable.
		///     grid.SetWalkability(new bool[10*10], new IntRect(0, 0, 9, 9));
		/// });
		/// </code>
		///
		/// See: grid-rules (view in online documentation for working links) for an alternative way of modifying the graph's walkability. It is more flexible and robust, but requires a bit more code.
		/// </summary>
		public void SetWalkability (bool[] walkability, IntRect rect) {
			AssertSafeToUpdateGraph();
			var gridRect = new IntRect(0, 0, width - 1, depth - 1);
			if (!gridRect.Contains(rect)) throw new System.ArgumentException("Rect (" + rect + ") must be within the graph bounds (" + gridRect + ")");
			if (walkability.Length != rect.Width*rect.Height) throw new System.ArgumentException("Array must have the same length as rect.Width*rect.Height");
			if (LayerCount != 1) throw new System.InvalidOperationException("This method only works in single-layered grid graphs.");

			for (int z = 0; z < rect.Height; z++) {
				var offset = (z + rect.ymin) * width + rect.xmin;
				for (int x = 0; x < rect.Width; x++) {
					var w = walkability[z * rect.Width + x];
					nodes[offset + x].WalkableErosion = w;
					nodes[offset + x].Walkable = w;
				}
			}

			// Recalculate connections for all affected nodes and their neighbours
			RecalculateConnectionsInRegion(rect.Expand(1));
		}

		/// <summary>
		/// Recalculates node connections for all nodes in grid graph.
		///
		/// This is used if you have manually changed the walkability, or other parameters, of some grid nodes, and you need their connections to be recalculated.
		/// If you are changing the connections themselves, you should use the <see cref="GraphNode.Connect"/> and <see cref="GraphNode.Disconnect"/> functions instead.
		///
		/// Typically you do not change walkability manually. Instead you can use for example a <see cref="GraphUpdateObject"/>.
		///
		/// Note: This will not take into account any grid graph rules that modify connections. So if you have any of those added to the grid graph, you probably want to do a regular graph update instead.
		///
		/// See: graph-updates (view in online documentation for working links)
		/// See: <see cref="CalculateConnectionsForCellAndNeighbours"/>
		/// See: <see cref="RecalculateConnectionsInRegion"/>
		/// </summary>
		public void RecalculateAllConnections () {
			RecalculateConnectionsInRegion(new IntRect(0, 0, width - 1, depth - 1));
		}

		/// <summary>
		/// Recalculates node connections for all nodes in a given region of the grid.
		///
		/// This is used if you have manually changed the walkability, or other parameters, of some grid nodes, and you need their connections to be recalculated.
		/// If you are changing the connections themselves, you should use the <see cref="GraphNode.AddConnection"/> and <see cref="GraphNode.RemoveConnection"/> functions instead.
		///
		/// Typically you do not change walkability manually. Instead you can use for example a <see cref="GraphUpdateObject"/>.
		///
		/// Warning: This method has some constant overhead, so if you are making several changes to the graph, it is best to batch these updates and only make a single call to this method.
		///
		/// Note: This will not take into account any grid graph rules that modify connections. So if you have any of those added to the grid graph, you probably want to do a regular graph update instead.
		///
		/// See: graph-updates (view in online documentation for working links)
		/// See: <see cref="RecalculateAllConnections"/>
		/// See: <see cref="CalculateConnectionsForCellAndNeighbours"/>
		/// </summary>
		public void RecalculateConnectionsInRegion (IntRect recalculateRect) {
			AssertSafeToUpdateGraph();
			if (nodes == null || nodes.Length != width * depth * LayerCount) {
				throw new System.InvalidOperationException("The Grid Graph is not scanned, cannot recalculate connections.");
			}
			Assert.AreEqual(new int3(width, LayerCount, depth), nodeData.bounds.size);

			var gridRect = new IntRect(0, 0, width - 1, depth - 1);
			var writeRect = IntRect.Intersection(recalculateRect, gridRect);

			// Skip recalculation if the rectangle is outside the graph
			if (!writeRect.IsValid()) return;

			var dependencyTracker = ObjectPool<JobDependencyTracker>.Claim();
			// We need to read node data from the rectangle, and a 1 node border around it in order to be able to calculate connections
			// inside the rectangle properly.
			var readRect = IntRect.Intersection(writeRect.Expand(1), gridRect);
			var readBounds = new IntBounds(readRect.xmin, 0, readRect.ymin, readRect.xmax + 1, LayerCount, readRect.ymax + 1);
			if (readBounds.volume < 200) dependencyTracker.SetLinearDependencies(true);

			var layeredDataLayout = this is LayerGridGraph;
			var data = new GridGraphScanData {
				dependencyTracker = dependencyTracker,
				// We can use the temp allocator here because everything will be done before this method returns.
				// Unity will normally not let us use these allocations in jobs (presumably because it cannot guarantee that the job will complete before the end of the frame),
				// but we will trick it using the UnsafeSpan struct. This is safe because we know that the job will complete before this method returns.
				nodes = GridGraphNodeData.ReadFromNodes(nodes, new Slice3D(nodeData.bounds, readBounds), default, nodeData.normals, Allocator.TempJob, layeredDataLayout, dependencyTracker),
				transform = transform,
				up = transform.WorldUpAtGraphPosition(Vector3.zero),
			};
			float characterHeight = this is LayerGridGraph lg ? lg.characterHeight : float.PositiveInfinity;

			var writeBounds = new IntBounds(writeRect.xmin, 0, writeRect.ymin, writeRect.xmax + 1, LayerCount, writeRect.ymax + 1);
			data.Connections(maxStepHeight, maxStepUsesSlope, writeBounds, neighbours, cutCorners, collision.use2D, true, characterHeight);
			this.nodeData.CopyFrom(data.nodes, writeBounds, true, dependencyTracker);
			dependencyTracker.AllWritesDependency.Complete();
			Profiler.BeginSample("Write connections");
			data.AssignNodeConnections(nodes, new int3(width, LayerCount, depth), writeBounds);
			Profiler.EndSample();
			ObjectPool<JobDependencyTracker>.Release(ref dependencyTracker);

			// Recalculate off mesh links in the affected area
			active.DirtyBounds(GetBoundsFromRect(writeRect));
		}

		/// <summary>
		/// Calculates the grid connections for a cell as well as its neighbours.
		/// This is a useful utility function if you want to modify the walkability of a single node in the graph.
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(ctx => {
		///     var grid = AstarPath.active.data.gridGraph;
		///     int x = 5;
		///     int z = 7;
		///
		///     // Mark a single node as unwalkable
		///     grid.GetNode(x, z).Walkable = false;
		///
		///     // Recalculate the connections for that node as well as its neighbours
		///     grid.CalculateConnectionsForCellAndNeighbours(x, z);
		/// });
		/// </code>
		///
		/// Warning: If you are recalculating connections for a lot of nodes at the same time, use <see cref="RecalculateConnectionsInRegion"/> instead, since that will be much faster.
		/// </summary>
		public void CalculateConnectionsForCellAndNeighbours (int x, int z) {
			RecalculateConnectionsInRegion(new IntRect(x - 1, z - 1, x + 1, z + 1));
		}

		/// <summary>
		/// Calculates the grid connections for a single node.
		/// Convenience function, it's slightly faster to use CalculateConnections(int,int)
		/// but that will only show when calculating for a large number of nodes.
		/// This function will also work for both grid graphs and layered grid graphs.
		///
		/// Deprecated: This method is very slow since 4.3.80. Use <see cref="RecalculateConnectionsInRegion"/> or <see cref="RecalculateAllConnections"/> instead to batch connection recalculations.
		/// </summary>
		[System.Obsolete("This method is very slow since 4.3.80. Use RecalculateConnectionsInRegion or RecalculateAllConnections instead to batch connection recalculations.")]
		public virtual void CalculateConnections (GridNodeBase node) {
			int index = node.NodeInGridIndex;
			int x = index % width;
			int z = index / width;

			CalculateConnections(x, z);
		}

		/// <summary>
		/// Calculates the grid connections for a single node.
		/// Note that to ensure that connections are completely up to date after updating a node you
		/// have to calculate the connections for both the changed node and its neighbours.
		///
		/// In a layered grid graph, this will recalculate the connections for all nodes
		/// in the (x,z) cell (it may have multiple layers of nodes).
		///
		/// See: CalculateConnections(GridNodeBase)
		///
		/// Deprecated: This method is very slow since 4.3.80. Use <see cref="RecalculateConnectionsInRegion"/> instead to batch connection recalculations.
		/// </summary>
		[System.Obsolete("This method is very slow since 4.3.80. Use RecalculateConnectionsInRegion instead to batch connection recalculations.")]
		public virtual void CalculateConnections (int x, int z) {
			RecalculateConnectionsInRegion(new IntRect(x, z, x, z));
		}

		public override void OnDrawGizmos (DrawingData gizmos, bool drawNodes, RedrawScope redrawScope) {
			using (var helper = GraphGizmoHelper.GetSingleFrameGizmoHelper(gizmos, active, redrawScope)) {
				// The width and depth fields might not be up to date, so recalculate
				// them from the #unclampedSize field
				int w, d;
				float s;
				CalculateDimensions(out w, out d, out s);
				var bounds = new Bounds();
				bounds.SetMinMax(Vector3.zero, new Vector3(w, 0, d));
				using (helper.builder.WithMatrix(CalculateTransform().matrix)) {
					helper.builder.WireBox(bounds, Color.white);

					int nodeCount = nodes != null ? nodes.Length : -1;

					if (drawNodes && width*depth*LayerCount != nodeCount) {
						var color = new Color(1, 1, 1, 0.2f);
						helper.builder.WireGrid(new float3(w*0.5f, 0, d*0.5f), Quaternion.identity, new int2(w, d), new float2(w, d), color);
					}
				}
			}

			if (!drawNodes) {
				return;
			}

			// Loop through chunks of size chunkWidth*chunkWidth and create a gizmo mesh for each of those chunks.
			// This is done because rebuilding the gizmo mesh (such as when using Unity Gizmos) every frame is pretty slow
			// for large graphs. However just checking if any mesh needs to be updated is relatively fast. So we just store
			// a hash together with the mesh and rebuild the mesh when necessary.
			const int chunkWidth = 32;
			GridNodeBase[] allNodes = ArrayPool<GridNodeBase>.Claim(chunkWidth*chunkWidth*LayerCount);
			for (int cx = width/chunkWidth; cx >= 0; cx--) {
				for (int cz = depth/chunkWidth; cz >= 0; cz--) {
					Profiler.BeginSample("Hash");
					var allNodesCount = GetNodesInRegion(new IntRect(cx*chunkWidth, cz*chunkWidth, (cx+1)*chunkWidth - 1, (cz+1)*chunkWidth - 1), allNodes);
					var hasher = new NodeHasher(active);
					hasher.Add(showMeshOutline);
					hasher.Add(showMeshSurface);
					hasher.Add(showNodeConnections);
					for (int i = 0; i < allNodesCount; i++) {
						hasher.HashNode(allNodes[i]);
					}
					Profiler.EndSample();

					if (!gizmos.Draw(hasher, redrawScope)) {
						Profiler.BeginSample("Rebuild Retained Gizmo Chunk");
						using (var helper = GraphGizmoHelper.GetGizmoHelper(gizmos, active, hasher, redrawScope)) {
							if (showNodeConnections) {
								if (helper.showSearchTree) helper.builder.PushLineWidth(2);
								for (int i = 0; i < allNodesCount; i++) {
									// Don't bother drawing unwalkable nodes
									if (allNodes[i].Walkable) {
										helper.DrawConnections(allNodes[i]);
									}
								}
								if (helper.showSearchTree) helper.builder.PopLineWidth();
							}
							if (showMeshSurface || showMeshOutline) CreateNavmeshSurfaceVisualization(allNodes, allNodesCount, helper);
						}
						Profiler.EndSample();
					}
				}
			}
			ArrayPool<GridNodeBase>.Release(ref allNodes);

			if (active.showUnwalkableNodes) DrawUnwalkableNodes(gizmos, nodeSize * 0.3f, redrawScope);
		}

		/// <summary>
		/// Draw the surface as well as an outline of the grid graph.
		/// The nodes will be drawn as squares (or hexagons when using <see cref="neighbours"/> = Six).
		/// </summary>
		void CreateNavmeshSurfaceVisualization (GridNodeBase[] nodes, int nodeCount, GraphGizmoHelper helper) {
			// Count the number of nodes that we will render
			int walkable = 0;

			for (int i = 0; i < nodeCount; i++) {
				if (nodes[i].Walkable) walkable++;
			}

			var neighbourIndices = neighbours == NumNeighbours.Six ? hexagonNeighbourIndices : new [] { 0, 1, 2, 3 };
			var offsetMultiplier = neighbours == NumNeighbours.Six ? 0.333333f : 0.5f;

			// 2 for a square-ish grid, 4 for a hexagonal grid.
			var trianglesPerNode = neighbourIndices.Length-2;
			var verticesPerNode = 3*trianglesPerNode;

			// Get arrays that have room for all vertices/colors (the array might be larger)
			var vertices = ArrayPool<Vector3>.Claim(walkable*verticesPerNode);
			var colors = ArrayPool<Color>.Claim(walkable*verticesPerNode);
			int baseIndex = 0;

			for (int i = 0; i < nodeCount; i++) {
				var node = nodes[i];
				if (!node.Walkable) continue;

				var nodeColor = helper.NodeColor(node);
				// Don't bother drawing transparent nodes
				if (nodeColor.a <= 0.001f) continue;

				for (int dIndex = 0; dIndex < neighbourIndices.Length; dIndex++) {
					// For neighbours != Six
					// n2 -- n3
					// |     |
					// n  -- n1
					//
					// n = this node
					var d = neighbourIndices[dIndex];
					var nextD = neighbourIndices[(dIndex + 1) % neighbourIndices.Length];
					GridNodeBase n1, n2, n3 = null;
					n1 = node.GetNeighbourAlongDirection(d);
					if (n1 != null && neighbours != NumNeighbours.Six) {
						n3 = n1.GetNeighbourAlongDirection(nextD);
					}

					n2 = node.GetNeighbourAlongDirection(nextD);
					if (n2 != null && n3 == null && neighbours != NumNeighbours.Six) {
						n3 = n2.GetNeighbourAlongDirection(d);
					}

					// Position in graph space of the vertex
					Vector3 p = new Vector3(node.XCoordinateInGrid + 0.5f, 0, node.ZCoordinateInGrid + 0.5f);
					// Offset along diagonal to get the correct XZ coordinates
					p.x += (neighbourXOffsets[d] + neighbourXOffsets[nextD]) * offsetMultiplier;
					p.z += (neighbourZOffsets[d] + neighbourZOffsets[nextD]) * offsetMultiplier;

					// Interpolate the y coordinate of the vertex so that the mesh will be seamless (except in some very rare edge cases)
					p.y += transform.InverseTransform((Vector3)node.position).y;
					if (n1 != null) p.y += transform.InverseTransform((Vector3)n1.position).y;
					if (n2 != null) p.y += transform.InverseTransform((Vector3)n2.position).y;
					if (n3 != null) p.y += transform.InverseTransform((Vector3)n3.position).y;
					p.y /= (1f + (n1 != null ? 1f : 0f) + (n2 != null ? 1f : 0f) + (n3 != null ? 1f : 0f));

					// Convert the point from graph space to world space
					// This handles things like rotations, scale other transformations
					p = transform.Transform(p);
					vertices[baseIndex + dIndex] = p;
				}

				if (neighbours == NumNeighbours.Six) {
					// Form the two middle triangles
					vertices[baseIndex + 6] = vertices[baseIndex + 0];
					vertices[baseIndex + 7] = vertices[baseIndex + 2];
					vertices[baseIndex + 8] = vertices[baseIndex + 3];

					vertices[baseIndex + 9] = vertices[baseIndex + 0];
					vertices[baseIndex + 10] = vertices[baseIndex + 3];
					vertices[baseIndex + 11] = vertices[baseIndex + 5];
				} else {
					// Form the last triangle
					vertices[baseIndex + 4] = vertices[baseIndex + 0];
					vertices[baseIndex + 5] = vertices[baseIndex + 2];
				}

				// Set all colors for the node
				for (int j = 0; j < verticesPerNode; j++) {
					colors[baseIndex + j] = nodeColor;
				}

				// Draw the outline of the node
				for (int j = 0; j < neighbourIndices.Length; j++) {
					var other = node.GetNeighbourAlongDirection(neighbourIndices[(j+1) % neighbourIndices.Length]);
					// Just a tie breaker to make sure we don't draw the line twice.
					// Using NodeInGridIndex instead of NodeIndex to make the gizmos deterministic for a given grid layout.
					// This is important because if the graph would be re-scanned and only a small part of it would change
					// then most chunks would be cached by the gizmo system, but the node indices may have changed and
					// if NodeIndex was used then we might get incorrect gizmos at the borders between chunks.
					if (other == null || (showMeshOutline && node.NodeInGridIndex < other.NodeInGridIndex)) {
						helper.builder.Line(vertices[baseIndex + j], vertices[baseIndex + (j+1) % neighbourIndices.Length], other == null ? Color.black : nodeColor);
					}
				}

				baseIndex += verticesPerNode;
			}

			if (showMeshSurface) helper.DrawTriangles(vertices, colors, baseIndex*trianglesPerNode/verticesPerNode);

			ArrayPool<Vector3>.Release(ref vertices);
			ArrayPool<Color>.Release(ref colors);
		}

		/// <summary>
		/// Bounding box in world space which encapsulates all nodes in the given rectangle.
		///
		/// The bounding box will cover all nodes' surfaces completely. Not just their centers.
		///
		/// Note: The bounding box may not be particularly tight if the graph is not axis-aligned.
		///
		/// See: <see cref="GetRectFromBounds"/>
		/// </summary>
		/// <param name="rect">Which nodes to consider. Will be clamped to the grid's bounds. If the rectangle is outside the graph, an empty bounds will be returned.</param>
		public Bounds GetBoundsFromRect (IntRect rect) {
			rect = IntRect.Intersection(rect, new IntRect(0, 0, width-1, depth-1));
			if (!rect.IsValid()) return new Bounds();
			return transform.Transform(new Bounds(
				new Vector3(rect.xmin + rect.xmax, collision.fromHeight, rect.ymin + rect.ymax) * 0.5f,
				// Note: We add +1 to the width and height to make the bounding box cover the nodes' surfaces completely, instead
				// of just their centers.
				new Vector3(rect.Width + 1, collision.fromHeight, rect.Height + 1)
				));
		}

		/// <summary>
		/// A rect that contains all nodes that the bounds could touch.
		/// This correctly handles rotated graphs and other transformations.
		/// The returned rect is guaranteed to not extend outside the graph bounds.
		///
		/// Note: The rect may contain nodes that are not contained in the bounding box since the bounding box is aligned to the world, and the rect is aligned to the grid (which may be rotated).
		///
		/// See: <see cref="GetNodesInRegion(Bounds)"/>
		/// See: <see cref="GetNodesInRegion(IntRect)"/>
		/// </summary>
		public IntRect GetRectFromBounds (Bounds bounds) {
			// Take the bounds and transform it using the matrix
			// Then convert that to a rectangle which contains
			// all nodes that might be inside the bounds

			bounds = transform.InverseTransform(bounds);
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;

			// Allow the bounds to extend a tiny amount into adjacent nodes.
			// This is mostly to avoid requiring a much larger update region if a user
			// passes a bounding box exactly (plus/minus floating point errors) covering
			// a set of nodes.
			const float MARGIN = 0.01f;

			int minX = Mathf.FloorToInt(min.x+MARGIN);
			int maxX = Mathf.FloorToInt(max.x-MARGIN);

			int minZ = Mathf.FloorToInt(min.z+MARGIN);
			int maxZ = Mathf.FloorToInt(max.z-MARGIN);

			var originalRect = new IntRect(minX, minZ, maxX, maxZ);

			// Rect which covers the whole grid
			var gridRect = new IntRect(0, 0, width-1, depth-1);

			// Clamp the rect to the grid
			return IntRect.Intersection(originalRect, gridRect);
		}

		/// <summary>
		/// All nodes inside the bounding box.
		/// Note: Be nice to the garbage collector and pool the list when you are done with it (optional)
		/// See: Pathfinding.Pooling.ListPool
		///
		/// See: GetNodesInRegion(GraphUpdateShape)
		/// </summary>
		public List<GraphNode> GetNodesInRegion (Bounds bounds) {
			return GetNodesInRegion(bounds, null);
		}

		/// <summary>
		/// All nodes inside the shape.
		/// Note: Be nice to the garbage collector and pool the list when you are done with it (optional)
		/// See: Pathfinding.Pooling.ListPool
		///
		/// See: GetNodesInRegion(Bounds)
		/// </summary>
		public List<GraphNode> GetNodesInRegion (GraphUpdateShape shape) {
			return GetNodesInRegion(shape.GetBounds(), shape);
		}

		/// <summary>
		/// All nodes inside the shape or if null, the bounding box.
		/// If a shape is supplied, it is assumed to be contained inside the bounding box.
		/// See: GraphUpdateShape.GetBounds
		/// </summary>
		protected virtual List<GraphNode> GetNodesInRegion (Bounds bounds, GraphUpdateShape shape) {
			var rect = GetRectFromBounds(bounds);

			if (nodes == null || !rect.IsValid() || nodes.Length != width*depth*LayerCount) {
				return ListPool<GraphNode>.Claim();
			}

			// Get a buffer we can use
			var inArea = ListPool<GraphNode>.Claim(rect.Width*rect.Height);
			var rw = rect.Width;

			// Loop through all nodes in the rectangle
			for (int y = 0; y < LayerCount; y++) {
				for (int z = rect.ymin; z <= rect.ymax; z++) {
					var offset = y*width*depth + z*width + rect.xmin;
					for (int x = 0; x < rw; x++) {
						var node = nodes[offset + x];
						if (node == null) continue;

						// If it is contained in the bounds (and optionally the shape)
						// then add it to the buffer
						var pos = (Vector3)node.position;
						if (bounds.Contains(pos) && (shape == null || shape.Contains(pos))) {
							inArea.Add(node);
						}
					}
				}
			}

			return inArea;
		}

		/// <summary>
		/// Get all nodes in a rectangle.
		///
		/// See: <see cref="GetRectFromBounds"/>
		/// </summary>
		/// <param name="rect">Region in which to return nodes. It will be clamped to the grid.</param>
		public List<GraphNode> GetNodesInRegion (IntRect rect) {
			// Clamp the rect to the grid
			// Rect which covers the whole grid
			var gridRect = new IntRect(0, 0, width-1, depth-1);

			rect = IntRect.Intersection(rect, gridRect);

			if (nodes == null || !rect.IsValid() || nodes.Length != width*depth*LayerCount) return ListPool<GraphNode>.Claim(0);

			// Get a buffer we can use
			var inArea = ListPool<GraphNode>.Claim(rect.Width*rect.Height);
			var rw = rect.Width;

			for (int y = 0; y < LayerCount; y++) {
				for (int z = rect.ymin; z <= rect.ymax; z++) {
					var offset = y*width*depth + z*width + rect.xmin;
					for (int x = 0; x < rw; x++) {
						var node = nodes[offset + x];
						if (node != null) inArea.Add(node);
					}
				}
			}

			return inArea;
		}

		/// <summary>
		/// Get all nodes in a rectangle.
		/// Returns: The number of nodes written to the buffer.
		///
		/// Note: This method is much faster than GetNodesInRegion(IntRect) which returns a list because this method can make use of the highly optimized
		///  System.Array.Copy method.
		///
		/// See: <see cref="GetRectFromBounds"/>
		/// </summary>
		/// <param name="rect">Region in which to return nodes. It will be clamped to the grid.</param>
		/// <param name="buffer">Buffer in which the nodes will be stored. Should be at least as large as the number of nodes that can exist in that region.</param>
		public virtual int GetNodesInRegion (IntRect rect, GridNodeBase[] buffer) {
			// Clamp the rect to the grid
			// Rect which covers the whole grid
			var gridRect = new IntRect(0, 0, width-1, depth-1);

			rect = IntRect.Intersection(rect, gridRect);

			if (nodes == null || !rect.IsValid() || nodes.Length != width*depth) return 0;

			if (buffer.Length < rect.Width*rect.Height) throw new System.ArgumentException("Buffer is too small");

			int counter = 0;
			for (int z = rect.ymin; z <= rect.ymax; z++, counter += rect.Width) {
				System.Array.Copy(nodes, z*Width + rect.xmin, buffer, counter, rect.Width);
			}

			return counter;
		}

		/// <summary>
		/// Node in the specified cell.
		/// Returns null if the coordinate is outside the grid.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// int x = 5;
		/// int z = 8;
		/// GridNodeBase node = gg.GetNode(x, z);
		/// </code>
		///
		/// If you know the coordinate is inside the grid and you are looking to maximize performance then you
		/// can look up the node in the internal array directly which is slightly faster.
		/// See: <see cref="nodes"/>
		/// </summary>
		public virtual GridNodeBase GetNode (int x, int z) {
			if (x < 0 || z < 0 || x >= width || z >= depth) return null;
			return nodes[x + z*width];
		}

		class CombinedGridGraphUpdatePromise : IGraphUpdatePromise {
			List<IGraphUpdatePromise> promises;

			public CombinedGridGraphUpdatePromise(GridGraph graph, List<GraphUpdateObject> graphUpdates) {
				promises = ListPool<IGraphUpdatePromise>.Claim();
				var nodesHolder = new GridGraphUpdatePromise.NodesHolder { nodes = graph.nodes };

				for (int i = 0; i < graphUpdates.Count; i++) {
					var graphUpdate = graphUpdates[i];
					var promise = new GridGraphUpdatePromise(
						graph: graph,
						transform: graph.transform,
						nodes: nodesHolder,
						nodeArrayBounds: new int3(graph.width, graph.LayerCount, graph.depth),
						rect: graph.GetRectFromBounds(graphUpdate.bounds),
						dependencyTracker: ObjectPool<JobDependencyTracker>.Claim(),
						nodesDependsOn: default,
						allocationMethod: Allocator.Persistent,
						recalculationMode: graphUpdate.updatePhysics ? RecalculationMode.RecalculateMinimal : RecalculationMode.NoRecalculation,
						graphUpdateObject: graphUpdate,
						ownsJobDependencyTracker: true,
						isFinalUpdate: i == graphUpdates.Count - 1
						);
					promises.Add(promise);
				}
			}

			public IEnumerator<JobHandle> Prepare () {
				for (int i = 0; i < promises.Count; i++) {
					var it = promises[i].Prepare();
					while (it.MoveNext()) yield return it.Current;
				}
			}

			public void Apply (IGraphUpdateContext ctx) {
				for (int i = 0; i < promises.Count; i++) {
					promises[i].Apply(ctx);
				}
				ListPool<IGraphUpdatePromise>.Release(ref promises);
			}
		}

		/// <summary>Internal function to update the graph</summary>
		IGraphUpdatePromise IUpdatableGraph.ScheduleGraphUpdates (List<GraphUpdateObject> graphUpdates) {
			if (!isScanned || nodes.Length != width*depth*LayerCount) {
				Debug.LogWarning("The Grid Graph is not scanned, cannot update graph");
				return null;
			}

			collision.Initialize(transform, nodeSize);
			return new CombinedGridGraphUpdatePromise(this, graphUpdates);
		}

		class GridGraphSnapshot : IGraphSnapshot {
			internal GridGraphNodeData nodes;
			internal GridGraph graph;

			public void Dispose () {
				nodes.Dispose();
			}

			public void Restore (IGraphUpdateContext ctx) {
				graph.AssertSafeToUpdateGraph();
				if (!graph.isScanned) return;

				if (!graph.nodeData.bounds.Contains(nodes.bounds)) {
					Debug.LogError("Cannot restore snapshot because the graph dimensions have changed since the snapshot was taken");
					return;
				}

				var dependencyTracker = ObjectPool<JobDependencyTracker>.Claim();
				graph.nodeData.CopyFrom(nodes, true, dependencyTracker);

				var assignToNodesJob = nodes.AssignToNodes(graph.nodes, graph.nodeData.bounds.size, nodes.bounds, graph.graphIndex, new JobHandle(), dependencyTracker);
				assignToNodesJob.Complete();
				dependencyTracker.AllWritesDependency.Complete();
				ObjectPool<JobDependencyTracker>.Release(ref dependencyTracker);

				// Recalculate off mesh links in the affected area
				ctx.DirtyBounds(graph.GetBoundsFromRect(new IntRect(nodes.bounds.min.x, nodes.bounds.min.z, nodes.bounds.max.x - 1, nodes.bounds.max.z - 1)));
			}
		}

		public override IGraphSnapshot Snapshot (Bounds bounds) {
			if (active.isScanning || active.IsAnyWorkItemInProgress) {
				throw new System.InvalidOperationException("Trying to capture a grid graph snapshot while inside a work item. This is not supported, as the graphs may be in an inconsistent state.");
			}

			if (!isScanned || nodes.Length != width*depth*LayerCount) return null;

			GridGraphUpdatePromise.CalculateRectangles(this, GetRectFromBounds(bounds), out var _, out var _, out var writeMaskRect, out var _);
			if (!writeMaskRect.IsValid()) return null;

			var nodeBounds = new IntBounds(writeMaskRect.xmin, 0, writeMaskRect.ymin, writeMaskRect.xmax + 1, LayerCount, writeMaskRect.ymax + 1);
			var snapshotData = new GridGraphNodeData {
				allocationMethod = Allocator.Persistent,
				bounds = nodeBounds,
				numNodes = nodeBounds.volume,
			};
			snapshotData.AllocateBuffers(null);
			snapshotData.CopyFrom(this.nodeData, true, null);
			return new GridGraphSnapshot {
					   nodes = snapshotData,
					   graph = this,
			};
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// Edge cases are handled as follows:
		/// - Shared edges and corners between walkable and unwalkable nodes are treated as walkable (so for example if the linecast just touches a corner of an unwalkable node, this is allowed).
		/// - If the linecast starts outside the graph, a hit is returned at from.
		/// - If the linecast starts inside the graph, but the end is outside of it, a hit is returned at the point where it exits the graph (unless there are any other hits before that).
		/// </summary>
		public bool Linecast (Vector3 from, Vector3 to) {
			GraphHitInfo hit;

			return Linecast(from, to, out hit);
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// Deprecated: The hint parameter is deprecated
		/// </summary>
		/// <param name="from">Point to linecast from</param>
		/// <param name="to">Point to linecast to</param>
		/// <param name="hint">This parameter is deprecated. It will be ignored.</param>
		[System.Obsolete("The hint parameter is deprecated")]
		public bool Linecast (Vector3 from, Vector3 to, GraphNode hint) {
			GraphHitInfo hit;

			return Linecast(from, to, hint, out hit);
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// Deprecated: The hint parameter is deprecated
		/// </summary>
		/// <param name="from">Point to linecast from</param>
		/// <param name="to">Point to linecast to</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo</param>
		/// <param name="hint">This parameter is deprecated. It will be ignored.</param>
		[System.Obsolete("The hint parameter is deprecated")]
		public bool Linecast (Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit) {
			return Linecast(from, to, hint, out hit, null);
		}

		/// <summary>Magnitude of the cross product a x b</summary>
		protected static long CrossMagnitude (int2 a, int2 b) {
			return (long)a.x*b.y - (long)b.x*a.y;
		}

		/// <summary>
		/// Clips a line segment in graph space to the graph bounds.
		/// That is (0,0,0) is the bottom left corner of the graph and (width,0,depth) is the top right corner.
		/// The first node is placed at (0.5,y,0.5). One unit distance is the same as nodeSize.
		///
		/// Returns false if the line segment does not intersect the graph at all.
		/// </summary>
		protected bool ClipLineSegmentToBounds (Vector3 a, Vector3 b, out Vector3 outA, out Vector3 outB) {
			// If the start or end points are outside
			// the graph then clamping is needed
			if (a.x < 0 || a.z < 0 || a.x > width || a.z > depth ||
				b.x < 0 || b.z < 0 || b.x > width || b.z > depth) {
				// Boundary of the grid
				var p1 = new Vector3(0, 0,  0);
				var p2 = new Vector3(0, 0,  depth);
				var p3 = new Vector3(width, 0,  depth);
				var p4 = new Vector3(width, 0,  0);

				int intersectCount = 0;

				bool intersect;
				Vector3 intersection;

				intersection = VectorMath.SegmentIntersectionPointXZ(a, b, p1, p2, out intersect);

				if (intersect) {
					intersectCount++;
					if (!VectorMath.RightOrColinearXZ(p1, p2, a)) {
						a = intersection;
					} else {
						b = intersection;
					}
				}
				intersection = VectorMath.SegmentIntersectionPointXZ(a, b, p2, p3, out intersect);

				if (intersect) {
					intersectCount++;
					if (!VectorMath.RightOrColinearXZ(p2, p3, a)) {
						a = intersection;
					} else {
						b = intersection;
					}
				}
				intersection = VectorMath.SegmentIntersectionPointXZ(a, b, p3, p4, out intersect);

				if (intersect) {
					intersectCount++;
					if (!VectorMath.RightOrColinearXZ(p3, p4, a)) {
						a = intersection;
					} else {
						b = intersection;
					}
				}
				intersection = VectorMath.SegmentIntersectionPointXZ(a, b, p4, p1, out intersect);

				if (intersect) {
					intersectCount++;
					if (!VectorMath.RightOrColinearXZ(p4, p1, a)) {
						a = intersection;
					} else {
						b = intersection;
					}
				}

				if (intersectCount == 0) {
					// The line does not intersect with the grid
					outA = Vector3.zero;
					outB = Vector3.zero;
					return false;
				}
			}

			outA = a;
			outB = b;
			return true;
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// Deprecated: The hint parameter is deprecated
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="from">Point to linecast from</param>
		/// <param name="to">Point to linecast to</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo</param>
		/// <param name="hint">This parameter is deprecated. It will be ignored.</param>
		/// <param name="trace">If a list is passed, then it will be filled with all nodes the linecast traverses</param>
		/// <param name="filter">If not null then the delegate will be called for each node and if it returns false the node will be treated as unwalkable and a hit will be returned.
		///               Note that unwalkable nodes are always treated as unwalkable regardless of what this filter returns.</param>
		[System.Obsolete("The hint parameter is deprecated")]
		public bool Linecast (Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace, System.Func<GraphNode, bool> filter = null) {
			return Linecast(from, to, out hit, trace, filter);
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// Edge cases are handled as follows:
		/// - Shared edges and corners between walkable and unwalkable nodes are treated as walkable (so for example if the linecast just touches a corner of an unwalkable node, this is allowed).
		/// - If the linecast starts outside the graph, a hit is returned at from.
		/// - If the linecast starts inside the graph, but the end is outside of it, a hit is returned at the point where it exits the graph (unless there are any other hits before that).
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="from">Point to linecast from</param>
		/// <param name="to">Point to linecast to</param>
		/// <param name="hit">Contains info on what was hit, see \reflink{GraphHitInfo}.</param>
		/// <param name="trace">If a list is passed, then it will be filled with all nodes the linecast traverses</param>
		/// <param name="filter">If not null then the delegate will be called for each node and if it returns false the node will be treated as unwalkable and a hit will be returned.
		///               Note that unwalkable nodes are always treated as unwalkable regardless of what this filter returns.</param>
		public bool Linecast (Vector3 from, Vector3 to, out GraphHitInfo hit, List<GraphNode> trace = null, System.Func<GraphNode, bool> filter = null) {
			var res = Linecast(from, to, out GridHitInfo gridHit, trace, filter);
			hit = new GraphHitInfo {
				origin = from,
				node = gridHit.node,
			};
			if (res) {
				// Hit obstacle
				// We know from what direction we moved in
				// so we can calculate the line which we hit
				var ndir = gridHit.direction;
				if (ndir == -1 || gridHit.node == null) {
					// We didn't really hit a wall. Possibly the start node was unwalkable or we ended up at the right cell, but wrong floor (layered grid graphs only)
					hit.point = gridHit.node == null || !gridHit.node.Walkable || (filter != null && !filter(gridHit.node)) ? from : to;
					if (gridHit.node != null) hit.point = gridHit.node.ProjectOnSurface(hit.point);
					hit.tangentOrigin = Vector3.zero;
					hit.tangent = Vector3.zero;
				} else {
					Vector3 fromInGraphSpace = transform.InverseTransform(from);
					Vector3 toInGraphSpace = transform.InverseTransform(to);

					// Throw away components we don't care about (y)
					// Also subtract 0.5 because nodes have an offset of 0.5 (first node is at (0.5,0.5) not at (0,0))
					// And it's just more convenient to remove that term here.
					// The variable names #from and #to are unfortunately already taken, so let's use start and end.
					var fromInGraphSpace2D = new Vector2(fromInGraphSpace.x - 0.5f, fromInGraphSpace.z - 0.5f);
					var toInGraphSpace2D = new Vector2(toInGraphSpace.x - 0.5f, toInGraphSpace.z - 0.5f);

					// Current direction and current direction ±90 degrees
					var d1 = new Vector2(neighbourXOffsets[ndir], neighbourZOffsets[ndir]);
					var d2 = new Vector2(neighbourXOffsets[(ndir-1+4) & 0x3], neighbourZOffsets[(ndir-1+4) & 0x3]);
					Vector2 lineDirection = new Vector2(neighbourXOffsets[(ndir+1) & 0x3], neighbourZOffsets[(ndir+1) & 0x3]);
					var p = new Vector2(gridHit.node.XCoordinateInGrid, gridHit.node.ZCoordinateInGrid);
					Vector2 lineOrigin = p + (d1 + d2) * 0.5f;

					// Find the intersection
					var intersection = VectorMath.LineIntersectionPoint(lineOrigin, lineOrigin+lineDirection, fromInGraphSpace2D, toInGraphSpace2D);

					var currentNodePositionInGraphSpace = transform.InverseTransform((Vector3)gridHit.node.position);

					// The intersection is in graph space (with an offset of 0.5) so we need to transform it to world space
					var intersection3D = new Vector3(intersection.x + 0.5f, currentNodePositionInGraphSpace.y, intersection.y + 0.5f);
					var lineOrigin3D = new Vector3(lineOrigin.x + 0.5f, currentNodePositionInGraphSpace.y, lineOrigin.y + 0.5f);

					hit.point = transform.Transform(intersection3D);
					hit.tangentOrigin = transform.Transform(lineOrigin3D);
					hit.tangent = transform.TransformVector(new Vector3(lineDirection.x, 0, lineDirection.y));
				}
			} else {
				hit.point = to;
			}
			return res;
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This function is different from the other Linecast functions since it snaps the start and end positions to the centers of the closest nodes on the graph.
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// Version: Since 3.6.8 this method uses the same implementation as the other linecast methods so there is no performance boost to using it.
		/// Version: In 3.6.8 this method was rewritten and that fixed a large number of bugs.
		/// Previously it had not always followed the line exactly as it should have
		/// and the hit output was not very accurate
		/// (for example the hit point was just the node position instead of a point on the edge which was hit).
		///
		/// Deprecated: Use <see cref="Linecast"/> instead.
		/// </summary>
		/// <param name="from">Point to linecast from.</param>
		/// <param name="to">Point to linecast to.</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo.</param>
		/// <param name="hint">This parameter is deprecated. It will be ignored.</param>
		[System.Obsolete("Use Linecast instead")]
		public bool SnappedLinecast (Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit) {
			return Linecast(
				(Vector3)GetNearest(from, null).node.position,
				(Vector3)GetNearest(to, null).node.position,
				hint,
				out hit
				);
		}

		/// <summary>
		/// Returns if there is an obstacle between the two nodes on the graph.
		///
		/// This method is very similar to the other Linecast methods however it is a bit faster
		/// due to not having to look up which node is closest to a particular input point.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// var node1 = gg.GetNode(2, 3);
		/// var node2 = gg.GetNode(5, 7);
		/// bool anyObstaclesInTheWay = gg.Linecast(node1, node2);
		/// </code>
		/// </summary>
		/// <param name="fromNode">Node to start from.</param>
		/// <param name="toNode">Node to try to reach using a straight line.</param>
		/// <param name="filter">If not null then the delegate will be called for each node and if it returns false the node will be treated as unwalkable and a hit will be returned.
		///               Note that unwalkable nodes are always treated as unwalkable regardless of what this filter returns.</param>
		public bool Linecast (GridNodeBase fromNode, GridNodeBase toNode, System.Func<GraphNode, bool> filter = null) {
			var nodeCenter = new int2(FixedPrecisionScale/2, FixedPrecisionScale/2);
			return Linecast(fromNode, nodeCenter, toNode, nodeCenter, out GridHitInfo hit, null, filter);
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// Note: This overload outputs a hit of type <see cref="GridHitInfo"/> instead of <see cref="GraphHitInfo"/>. It's a bit faster to calculate this output
		/// and it can be useful for some grid-specific algorithms.
		///
		/// Edge cases are handled as follows:
		/// - Shared edges and corners between walkable and unwalkable nodes are treated as walkable (so for example if the linecast just touches a corner of an unwalkable node, this is allowed).
		/// - If the linecast starts outside the graph, a hit is returned at from.
		/// - If the linecast starts inside the graph, but the end is outside of it, a hit is returned at the point where it exits the graph (unless there are any other hits before that).
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="from">Point to linecast from</param>
		/// <param name="to">Point to linecast to</param>
		/// <param name="hit">Contains info on what was hit, see \reflink{GridHitInfo}</param>
		/// <param name="trace">If a list is passed, then it will be filled with all nodes the linecast traverses</param>
		/// <param name="filter">If not null then the delegate will be called for each node and if it returns false the node will be treated as unwalkable and a hit will be returned.
		///               Note that unwalkable nodes are always treated as unwalkable regardless of what this filter returns.</param>
		public bool Linecast (Vector3 from, Vector3 to, out GridHitInfo hit, List<GraphNode> trace = null, System.Func<GraphNode, bool> filter = null) {
			Vector3 fromInGraphSpace = transform.InverseTransform(from);
			Vector3 toInGraphSpace = transform.InverseTransform(to);

			// Clip the line so that the start and end points are on the graph
			if (!ClipLineSegmentToBounds(fromInGraphSpace, toInGraphSpace, out var fromInGraphSpaceClipped, out var toInGraphSpaceClipped)) {
				// Line does not intersect the graph
				// So there are no obstacles we can hit
				hit = new GridHitInfo {
					node = null,
					direction = -1,
				};
				return false;
			}

			// From is outside the graph, but #to is inside.
			if ((fromInGraphSpace - fromInGraphSpaceClipped).sqrMagnitude > 0.001f*0.001f) {
				hit = new GridHitInfo {
					node = null,
					direction = -1,
				};
				return true;
			}
			bool toIsOutsideGraph = (toInGraphSpace - toInGraphSpaceClipped).sqrMagnitude > 0.001f*0.001f;

			// Find the closest nodes to the start and end on the part of the segment which is on the graph
			var startNode = GetNearestFromGraphSpace(fromInGraphSpaceClipped);
			var endNode = GetNearestFromGraphSpace(toInGraphSpaceClipped);
			if (startNode == null || endNode == null) {
				hit = new GridHitInfo {
					node = null,
					direction = -1,
				};
				return false;
			}

			return Linecast(
				startNode, new Vector2(fromInGraphSpaceClipped.x - startNode.XCoordinateInGrid, fromInGraphSpaceClipped.z - startNode.ZCoordinateInGrid),
				endNode, new Vector2(toInGraphSpaceClipped.x - endNode.XCoordinateInGrid, toInGraphSpaceClipped.z - endNode.ZCoordinateInGrid),
				out hit,
				trace,
				filter,
				toIsOutsideGraph
				);
		}

		/// <summary>
		/// Scaling used for the coordinates in the Linecast methods that take normalized points using integer coordinates.
		///
		/// To convert from world space, each coordinate is multiplied by this factor and then rounded to the nearest integer.
		///
		/// Typically you do not need to use this constant yourself, instead use the Linecast overloads that do not take integer coordinates.
		/// </summary>
		public const int FixedPrecisionScale = 1024;

		/// <summary>
		/// Returns if there is an obstacle between the two nodes on the graph.
		///
		/// This method is very similar to the other Linecast methods but it gives some extra control, in particular when the start/end points are at node corners instead of inside nodes.
		///
		/// Shared edges and corners between walkable and unwalkable nodes are treated as walkable.
		/// So for example if the linecast just touches a corner of an unwalkable node, this is allowed.
		/// </summary>
		/// <param name="fromNode">Node to start from.</param>
		/// <param name="normalizedFromPoint">Where in the start node to start. This is a normalized value so each component must be in the range 0 to 1 (inclusive).</param>
		/// <param name="toNode">Node to try to reach using a straight line.</param>
		/// <param name="normalizedToPoint">Where in the end node to end. This is a normalized value so each component must be in the range 0 to 1 (inclusive).</param>
		/// <param name="hit">Contains info on what was hit, see \reflink{GridHitInfo}</param>
		/// <param name="trace">If a list is passed, then it will be filled with all nodes the linecast traverses</param>
		/// <param name="filter">If not null then the delegate will be called for each node and if it returns false the node will be treated as unwalkable and a hit will be returned.
		///               Note that unwalkable nodes are always treated as unwalkable regardless of what this filter returns.</param>
		/// <param name="continuePastEnd">If true, the linecast will continue past the end point in the same direction until it hits something.</param>
		public bool Linecast (GridNodeBase fromNode, Vector2 normalizedFromPoint, GridNodeBase toNode, Vector2 normalizedToPoint, out GridHitInfo hit, List<GraphNode> trace = null, System.Func<GraphNode, bool> filter = null, bool continuePastEnd = false) {
			var fixedNormalizedFromPoint = new int2((int)Mathf.Round(normalizedFromPoint.x*FixedPrecisionScale), (int)Mathf.Round(normalizedFromPoint.y*FixedPrecisionScale));
			var fixedNormalizedToPoint = new int2((int)Mathf.Round(normalizedToPoint.x*FixedPrecisionScale), (int)Mathf.Round(normalizedToPoint.y*FixedPrecisionScale));

			return Linecast(fromNode, fixedNormalizedFromPoint, toNode, fixedNormalizedToPoint, out hit, trace, filter, continuePastEnd);
		}

		/// <summary>
		/// Returns if there is an obstacle between the two nodes on the graph.
		/// Like <see cref="Linecast(GridNodeBase,Vector2,GridNodeBase,Vector2,GridHitInfo,List<GraphNode>,System.Func<GraphNode,bool>,bool)"/> but takes normalized points as fixed precision points normalized between 0 and FixedPrecisionScale instead of between 0 and 1.
		/// </summary>
		public bool Linecast (GridNodeBase fromNode, int2 fixedNormalizedFromPoint, GridNodeBase toNode, int2 fixedNormalizedToPoint, out GridHitInfo hit, List<GraphNode> trace = null, System.Func<GraphNode, bool> filter = null, bool continuePastEnd = false) {
			/*
			* Briefly, the algorithm used in this function can be described as:
			* 1. Determine the two axis aligned directions which will bring us closer to the target.
			* 2. In each step, check which direction out of those two that the linecast exits the current node from.
			* 3. Try to move in that direction if possible. If the linecast exits the current node through a corner, then moving along either direction is allowed.
			* 4. If that's not possible, and the line exits the current node at a corner, then try to move to the other side of line to the other row/column.
			* 5. If we still couldn't move anywhere, report a hit.
			* 6. Go back to step 2.
			*
			* Sadly the implementation is complicated by numerous edge cases, while trying to keep everything highly performant.
			* I've tried to document them as best I could.
			*
			* TODO: Maybe this could be rewritten such that instead of only being positioned at one node at a time,
			* we could be inside up to two nodes at the same time (which share either an edge or a corner).
			* This divergence would be done when the linecast line goes through a corner or right in the middle between two nodes.
			* This could potentially remove a bunch of edge cases.
			*/
			if (fixedNormalizedFromPoint.x < 0 || fixedNormalizedFromPoint.x > FixedPrecisionScale) throw new System.ArgumentOutOfRangeException(nameof(fixedNormalizedFromPoint), "must be between 0 and 1024");
			if (fixedNormalizedToPoint.x < 0 || fixedNormalizedToPoint.x > FixedPrecisionScale) throw new System.ArgumentOutOfRangeException(nameof(fixedNormalizedToPoint), "must be between 0 and 1024");

			if (fromNode == null) throw new System.ArgumentNullException(nameof(fromNode));
			if (toNode == null) throw new System.ArgumentNullException(nameof(toNode));

			// Use the filter
			if ((filter != null && !filter(fromNode)) || !fromNode.Walkable) {
				hit = new GridHitInfo {
					node = fromNode,
					direction = -1,
				};
				return true;
			}

			if (fromNode == toNode) {
				// Fast path, we don't have to do anything
				hit = new GridHitInfo {
					node = fromNode,
					direction = -1,
				};
				if (trace != null) trace.Add(fromNode);
				return false;
			}

			var fromGridCoords = new int2(fromNode.XCoordinateInGrid, fromNode.ZCoordinateInGrid);
			var toGridCoords = new int2(toNode.XCoordinateInGrid, toNode.ZCoordinateInGrid);

			var fixedFrom = new int2(fromGridCoords.x*FixedPrecisionScale, fromGridCoords.y*FixedPrecisionScale) + fixedNormalizedFromPoint;
			var fixedTo = new int2(toGridCoords.x*FixedPrecisionScale, toGridCoords.y*FixedPrecisionScale) + fixedNormalizedToPoint;
			var dir = fixedTo - fixedFrom;

			int remainingSteps = System.Math.Abs(fromGridCoords.x - toGridCoords.x) + System.Math.Abs(fromGridCoords.y - toGridCoords.y);
			if (continuePastEnd) remainingSteps = int.MaxValue;

			// If the from and to points are identical, but we start and end on different nodes, then dir will be zero
			// and the direction calculations below will get a bit messsed up.
			// So instead we don't take any steps at all, there's some code right at the end of this function which will
			// look around the corner and find the target node anyway.
			if (math.all(fixedFrom == fixedTo)) remainingSteps = 0;

			/*            Y/Z
			 *             |
			 *  quadrant   |   quadrant
			 *     1              0
			 *             2
			 *             |
			 *   ----  3 - X - 1  ----- X
			 *             |
			 *             0
			 *  quadrant       quadrant
			 *     2       |      3
			 *             |
			 */

			// Calculate the quadrant index as shown in the diagram above (the axes are part of the quadrants after them in the counter clockwise direction)
			int quadrant = 0;

			// The linecast line may be axis aligned, but we might still need to move to the side one step.
			// Like in the following two cases (starting at node S at corner X and ending at node T at corner P).
			// ┌─┬─┬─┬─┐   ┌─┬─┬─┬─┐
			// │S│ │ │ │   │S│ │#│T│
			// ├─X===P─┤   ├─X===P─┤
			// │ │ │ │T│   │ │ │ │ │
			// └─┴─┴─┴─┘   └─┴─┴─┴─┘
			//
			// We make sure that we will always be able to move to the side of the line the target is on, if we happen to be on the wrong side of the line.
			var dirBiased = dir;
			if (dirBiased.x == 0) dirBiased.x = System.Math.Sign(FixedPrecisionScale/2 - fixedNormalizedToPoint.x);
			if (dirBiased.y == 0) dirBiased.y = System.Math.Sign(FixedPrecisionScale/2 - fixedNormalizedToPoint.y);

			if (dirBiased.x <= 0 && dirBiased.y > 0) quadrant = 1;
			else if (dirBiased.x < 0 && dirBiased.y <= 0) quadrant = 2;
			else if (dirBiased.x >= 0 && dirBiased.y < 0) quadrant = 3;

			// This will be (1,2) for quadrant 0 and (2,3) for quadrant 1 etc.
			// & 0x3 is just the same thing as % 4 but it is faster
			// This is the direction which moves further to the right of the segment (when looking from the start)
			int directionToReduceError = (quadrant + 1) & 0x3;
			// This is the direction which moves further to the left of the segment (when looking from the start)
			int directionToIncreaseError = (quadrant + 2) & 0x3;

			// All errors used in this function are proportional to the signed distance.
			// They have a common multiplier which is dir.magnitude, but dividing away that would be very slow.
			// Note that almost all errors are multiplied by 2. It might seem like this could be optimized away,
			// but it cannot. The reason is that later when we use primaryDirectionError we only walk *half* a normal step.
			// But we don't want to use division, so instead we multiply all other errors by 2.
			//
			// How much further we move away from (or towards) the line when walking along the primary direction (e.g up and right or down and left).
			long primaryDirectionError = CrossMagnitude(dir,
				new int2(
					neighbourXOffsets[directionToIncreaseError]+neighbourXOffsets[directionToReduceError],
					neighbourZOffsets[directionToIncreaseError]+neighbourZOffsets[directionToReduceError]
					)
				);

			// Conceptually we start with error 0 at 'fixedFrom' (i.e. precisely on the line).
			// Imagine walking from fixedFrom to the center of the starting node.
			// This will change our "error" (signed distance to the line) correspondingly.
			int2 offset = new int2(FixedPrecisionScale/2, FixedPrecisionScale/2) - fixedNormalizedFromPoint;

			// Signed distance from the line (or at least a value proportional to that)
			long error = CrossMagnitude(dir, offset) * 2 / FixedPrecisionScale;

			// Walking one step along the X axis will increase (or decrease) our error by this amount.
			// This is equivalent to a cross product of dir with the x axis: CrossMagnitude(dir, new int2(1, 0)) * 2
			long xerror = -dir.y * 2;
			// Walking one step along the Z axis will increase our error by this amount
			long zerror = dir.x * 2;

			// When we move across a diagonal it can sometimes be important which side of the diagonal we prioritize.
			//
			// ┌───┬───┐
			// │   │ S │
			//=======P─C
			// │   │ T │
			// └───┴───┘
			//
			// Assume we are at node S and our target is node T at point P (it lies precisely between S and T).
			// Note that the linecast line (illustrated as ===) comes from the left. This means that this case will be detected as a diagonal move (because corner C lies on the line).
			// In this case we can walk either to the right from S or downwards. However walking to the right would mean that we end up in the wrong node (not the T node).
			// Therefore we make sure that, if possible, we are on the same side of the linecast line as the center of the target node is.
			int symmetryBreakingDirection1 = directionToIncreaseError;
			int symmetryBreakingDirection2 = directionToReduceError;

			var fixedCenterOfToNode = new int2(toGridCoords.x*FixedPrecisionScale, toGridCoords.y*FixedPrecisionScale) + new int2(FixedPrecisionScale/2, FixedPrecisionScale/2);
			long targetNodeError = CrossMagnitude(dir, fixedCenterOfToNode - fixedFrom);
			if (targetNodeError < 0) {
				symmetryBreakingDirection1 = directionToReduceError;
				symmetryBreakingDirection2 = directionToIncreaseError;
			}

			GridNodeBase prevNode = null;
			GridNodeBase preventBacktrackingTo = null;

			for (; remainingSteps > 0; remainingSteps--) {
				if (trace != null) trace.Add(fromNode);

				// How does the error change we take one half step in the primary direction.
				// The point which this represents is a corner of the current node.
				// Depending on which side of this point the line is (when seen from the center of the current node)
				// we know which direction we should walk from the node.
				// Since the error is just a signed distance, checking the side is equivalent to checking if its positive or negative.
				var nerror = error + primaryDirectionError;

				int ndir;
				GridNodeBase nextNode;

				if (nerror == 0) {
					// This would be a diagonal move. But we don't allow those for simplicity (we can just as well just take it in two axis aligned steps).
					// In this case we are free to choose which direction to move.
					// If one direction is blocked, we choose the other one.
					ndir = symmetryBreakingDirection1;
					nextNode = fromNode.GetNeighbourAlongDirection(ndir);
					if ((filter != null && nextNode != null && !filter(nextNode)) || nextNode == prevNode) nextNode = null;

					if (nextNode == null) {
						// Try the other one too...
						ndir = symmetryBreakingDirection2;
						nextNode = fromNode.GetNeighbourAlongDirection(ndir);
						if ((filter != null && nextNode != null && !filter(nextNode)) || nextNode == prevNode) nextNode = null;
					}
				} else {
					// This is the happy-path of the linecast. We just move in the direction of the line.
					// Check if we need to reduce or increase the error (we want to keep it near zero)
					// and pick the appropriate direction to move in
					ndir = nerror < 0 ? directionToIncreaseError : directionToReduceError;
					nextNode = fromNode.GetNeighbourAlongDirection(ndir);

					// Use the filter
					if ((filter != null && nextNode != null && !filter(nextNode)) || nextNode == prevNode) nextNode = null;
				}

				// If we cannot move forward from this node, we might still be able to by side-stepping.
				// This is a case that we need to handle if the linecast line exits this node at a corner.
				//
				// Assume we start at node S (at corner X) and linecast to node T (corner P)
				// The linecast goes exactly between two rows of nodes.
				// The code will start by going down one row, but then after a few nodes it hits an obstacle (when it's in node A).
				// We don't want to report a hit here because the linecast only touches the edge of the obstacle, which is allowed.
				// Instead we try to move to the node on the other side of the line (node B).
				// The shared corner C lies exactly on the line, and we can detect that to figure out which neighbor we should move to.
				//
				// ┌───────┬───────┬───────┬───────┐
				// │       │   B   │       │       │
				// │   S   │   ┌───┼───────┼───┐   │
				// │   │   │   │   │       │   │   │
				// X===│=======│===C=======P───┼───┤
				// │   │   │   │   │#######│   │   │
				// │   └───┼───┘   │#######│   ▼   │
				// │       │   A   │#######│   T   │
				// └───────┴───────┴───────┴───────┘
				//
				// After we have done this maneuver it is important that in the next step we don't try to move back to the node we came from.
				// We keep track of this using the prevNode variable.
				//
				if (nextNode == null) {
					// Loop over the two corners of the side of the node that we hit
					for (int i = -1; i <= 1; i += 2) {
						var d = (ndir + i + 4) & 0x3;
						if (error + xerror/2 * (neighbourXOffsets[ndir] + neighbourXOffsets[d]) + zerror/2 * (neighbourZOffsets[ndir]+neighbourZOffsets[d]) == 0) {
							// The line touches this corner precisely
							// Try to side-step in that direction.

							nextNode = fromNode.GetNeighbourAlongDirection(d);
							if ((filter != null && nextNode != null && !filter(nextNode)) || nextNode == prevNode || nextNode == preventBacktrackingTo) nextNode = null;

							if (nextNode != null) {
								// This side-stepping might add 1 additional step to the path, or not. It's hard to say.
								// We add 1 because the for loop will decrement remainingSteps after this iteration ends.
								remainingSteps = 1 + System.Math.Abs(nextNode.XCoordinateInGrid - toGridCoords.x) + System.Math.Abs(nextNode.ZCoordinateInGrid - toGridCoords.y);
								ndir = d;
								prevNode = fromNode;
								preventBacktrackingTo = nextNode;
							}
							break;
						}
					}

					// If we still have not found the next node yet, then we have hit an obstacle
					if (nextNode == null) {
						hit = new GridHitInfo {
							node = fromNode,
							direction = ndir,
						};
						return true;
					}
				}

				// Calculate how large our error will be after moving along the given direction
				error += xerror * neighbourXOffsets[ndir] + zerror * neighbourZOffsets[ndir];
				fromNode = nextNode;
			}

			hit = new GridHitInfo {
				node = fromNode,
				direction = -1,
			};

			if (fromNode != toNode) {
				// When the destination is on a corner it is sometimes possible that we end up in the wrong node.
				//
				// ┌───┬───┐
				// │ S │   │
				// ├───P───┤
				// │ T │   │
				// └───┴───┘
				//
				// Assume we are at node S and our target is node T at point P (i.e. normalizedToPoint = (1,1) so it is in the corner of the node).
				// In this case we can walk either to the right from S or downwards. However walking to the right would mean that we end up in the wrong node (not the T node).
				//
				// Similarly, if the connection from S to T was blocked for some reason (but both S and T are walkable), then we would definitely end up to the right of S, not in T.
				//
				// Therefore we check if the destination is a corner, and if so, try to reach all 4 nodes around that corner to see if any one of those is the destination.
				var dirToDestination = fixedTo - (new int2(fromNode.XCoordinateInGrid, fromNode.ZCoordinateInGrid)*FixedPrecisionScale + new int2(FixedPrecisionScale/2, FixedPrecisionScale/2));

				// Check if the destination is a corner of this node
				if (math.all(math.abs(dirToDestination) == new int2(FixedPrecisionScale/2, FixedPrecisionScale/2))) {
					var delta = dirToDestination*2/FixedPrecisionScale;
					// Figure out which directions will move us towards the target node.
					// We first try to move around the corner P in the counter-clockwise direction.
					// And if that fails, we try to move in the clockwise direction.
					//  ┌───────┬───────┐
					//  │       │       │
					//  │  ccw◄─┼───S   │
					//  │       │   │   │
					//  ├───────P───┼───┤
					//  │       │   ▼   │
					//  │   T   │   cw  │
					//  │       │       │
					//  └───────┴───────┘
					var counterClockwiseDirection = -1;
					for (int i = 0; i < 4; i++) {
						// Exactly one direction will satisfy this. It's kinda annnoying to calculate analytically.
						if (neighbourXOffsets[i]+neighbourXOffsets[(i+1)&0x3] == delta.x && neighbourZOffsets[i] + neighbourZOffsets[(i+1)&0x3] == delta.y) {
							counterClockwiseDirection = i;
							break;
						}
					}

					int traceLength = trace != null ? trace.Count : 0;
					int d = counterClockwiseDirection;
					var node = fromNode;
					for (int i = 0; i < 3 && node != toNode; i++) {
						if (trace != null) trace.Add(node);
						node = node.GetNeighbourAlongDirection(d);
						if (node == null || (filter != null && !filter(node))) {
							node = null;
							break;
						}
						d = (d + 1) & 0x3;
					}

					if (node != toNode) {
						if (trace != null) trace.RemoveRange(traceLength, trace.Count - traceLength);
						node = fromNode;
						// Try the clockwise direction instead
						d = (counterClockwiseDirection + 1) & 0x3;
						for (int i = 0; i < 3 && node != toNode; i++) {
							if (trace != null) trace.Add(node);
							node = node.GetNeighbourAlongDirection(d);
							if (node == null || (filter != null && !filter(node))) {
								node = null;
								break;
							}
							d = (d - 1 + 4) & 0x3;
						}

						if (node != toNode && trace != null) {
							trace.RemoveRange(traceLength, trace.Count - traceLength);
						}
					}

					fromNode = node;
				}
			}

			if (trace != null) trace.Add(fromNode);
			return fromNode != toNode;
		}

		protected override void SerializeExtraInfo (GraphSerializationContext ctx) {
			if (nodes == null) {
				ctx.writer.Write(-1);
				return;
			}

			ctx.writer.Write(nodes.Length);

			for (int i = 0; i < nodes.Length; i++) {
				nodes[i].SerializeNode(ctx);
			}

			SerializeNodeSurfaceNormals(ctx);
		}

		protected override void DeserializeExtraInfo (GraphSerializationContext ctx) {
			int count = ctx.reader.ReadInt32();

			if (count == -1) {
				nodes = null;
				return;
			}

			nodes = new GridNode[count];

			for (int i = 0; i < nodes.Length; i++) {
				nodes[i] = newGridNodeDelegate();
				active.InitializeNode(nodes[i]);
				nodes[i].DeserializeNode(ctx);
			}
			DeserializeNativeData(ctx, ctx.meta.version >= AstarSerializer.V4_3_6);
		}

		protected void DeserializeNativeData (GraphSerializationContext ctx, bool normalsSerialized) {
			UpdateTransform();
			var tracker = ObjectPool<JobDependencyTracker>.Claim();
			bool layeredDataLayout = this is LayerGridGraph;
			var nodeArraySize = new int3(width, LayerCount, depth);
			nodeData = GridGraphNodeData.ReadFromNodes(nodes, new Slice3D(nodeArraySize, new IntBounds(0, nodeArraySize)), default, default, Allocator.Persistent, layeredDataLayout, tracker);
			nodeData.PersistBuffers(tracker);
			DeserializeNodeSurfaceNormals(ctx, nodes, !normalsSerialized);
			tracker.AllWritesDependency.Complete();
			ObjectPool<JobDependencyTracker>.Release(ref tracker);
		}

		protected void SerializeNodeSurfaceNormals (GraphSerializationContext ctx) {
			var normals = nodeData.normals.AsUnsafeReadOnlySpan();
			for (int i = 0; i < nodes.Length; i++) {
				ctx.SerializeVector3(new Vector3(normals[i].x, normals[i].y, normals[i].z));
			}
		}

		protected void DeserializeNodeSurfaceNormals (GraphSerializationContext ctx, GridNodeBase[] nodes, bool ignoreForCompatibility) {
			if (nodeData.normals.IsCreated) nodeData.normals.Dispose();
			nodeData.normals = new NativeArray<float4>(nodes.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			if (ignoreForCompatibility) {
				// For backwards compatibility with older versions that do not have the information stored.
				// For most of these versions the #maxStepUsesSlope field will be deserialized to false anyway, so this array will not have any effect.
				for (int i = 0; i < nodes.Length; i++) {
					// If a node is null (can only happen for layered grid graphs) then the normal must be set to zero.
					// Otherwise we set it to a "reasonable" up direction.
					nodeData.normals[i] = nodes[i] != null ? new float4(0, 1, 0, 0) : float4.zero;
				}
			} else {
				for (int i = 0; i < nodes.Length; i++) {
					var v = ctx.DeserializeVector3();
					nodeData.normals[i] = new float4(v.x, v.y, v.z, 0);
				}
			}
		}

		void HandleBackwardsCompatibility (GraphSerializationContext ctx) {
			// For compatibility
			if (ctx.meta.version <= AstarSerializer.V4_3_2) maxStepUsesSlope = false;

#pragma warning disable CS0618 // Type or member is obsolete
			if (penaltyPosition) {
				penaltyPosition = false;
				// Can't convert it exactly. So assume there are no nodes with an elevation greater than 1000
				rules.AddRule(new RuleElevationPenalty {
					penaltyScale = Int3.Precision * penaltyPositionFactor * 1000.0f,
					elevationRange = new Vector2(-penaltyPositionOffset/Int3.Precision, -penaltyPositionOffset/Int3.Precision + 1000),
					curve = AnimationCurve.Linear(0, 0, 1, 1),
				});
			}

			if (penaltyAngle) {
				penaltyAngle = false;

				// Approximate the legacy behavior with an animation curve
				var curve = AnimationCurve.Linear(0, 0, 1, 1);
				var keys = new Keyframe[7];
				for (int i = 0; i < keys.Length; i++) {
					var angle = Mathf.PI*0.5f*i/(keys.Length-1);
					var penalty = (1F-Mathf.Pow(Mathf.Cos(angle), penaltyAnglePower))*penaltyAngleFactor;
					var key = new Keyframe(Mathf.Rad2Deg * angle, penalty);
					keys[i] = key;
				}
				var maxPenalty = keys.Max(k => k.value);
				if (maxPenalty > 0) for (int i = 0; i < keys.Length; i++) keys[i].value /= maxPenalty;
				curve.keys = keys;
				for (int i = 0; i < keys.Length; i++) {
					curve.SmoothTangents(i, 0.5f);
				}

				rules.AddRule(new RuleAnglePenalty {
					penaltyScale = maxPenalty,
					curve = curve,
				});
			}

			if (textureData.enabled) {
				textureData.enabled = false;
				var channelScales = textureData.factors.Select(x => x/255.0f).ToList();
				while (channelScales.Count < 4) channelScales.Add(1000);
				var channels = textureData.channels.Cast<RuleTexture.ChannelUse>().ToList();
				while (channels.Count < 4) channels.Add(RuleTexture.ChannelUse.None);

				rules.AddRule(new RuleTexture {
					texture = textureData.source,
					channels = channels.ToArray(),
					channelScales = channelScales.ToArray(),
					scalingMode = RuleTexture.ScalingMode.FixedScale,
					nodesPerPixel = 1.0f,
				});
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}

		protected override void PostDeserialization (GraphSerializationContext ctx) {
			HandleBackwardsCompatibility(ctx);

			UpdateTransform();
			SetUpOffsetsAndCosts();
			GridNode.SetGridGraph((int)graphIndex, this);

			// Deserialize all nodes
			if (nodes == null || nodes.Length == 0) return;

			if (width*depth != nodes.Length) {
				Debug.LogError("Node data did not match with bounds data. Probably a change to the bounds/width/depth data was made after scanning the graph just prior to saving it. Nodes will be discarded");
				nodes = new GridNodeBase[0];
				return;
			}

			for (int z = 0; z < depth; z++) {
				for (int x = 0; x < width; x++) {
					var node = nodes[z*width+x];

					if (node == null) {
						Debug.LogError("Deserialization Error : Couldn't cast the node to the appropriate type - GridGenerator");
						return;
					}

					node.NodeInGridIndex = z*width+x;
				}
			}
		}
	}

	/// <summary>
	/// Number of neighbours for a single grid node.
	/// Since: The 'Six' item was added in 3.6.1
	/// </summary>
	public enum NumNeighbours {
		Four,
		Eight,
		Six
	}

	/// <summary>Information about a linecast hit on a grid graph</summary>
	public struct GridHitInfo {
		/// <summary>
		/// The node which contained the edge that was hit.
		/// This may be null in case no particular edge was hit.
		/// </summary>
		public GridNodeBase node;
		/// <summary>
		/// Direction from the node to the edge that was hit.
		/// This will be in the range of 0 to 4 (exclusive) or -1 if no particular edge was hit.
		///
		/// See: <see cref="GridNodeBase.GetNeighbourAlongDirection"/>
		/// </summary>
		public int direction;
	}
}

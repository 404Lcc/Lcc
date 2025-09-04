using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

/// <summary>Local avoidance related classes</summary>
namespace Pathfinding.RVO {
	using System;
	using Pathfinding.Jobs;
	using Pathfinding.Drawing;
	using Pathfinding.Util;
	using Pathfinding.Sync;
	using Pathfinding.ECS.RVO;
	using Pathfinding.Collections;

	public interface IMovementPlaneWrapper {
		float2 ToPlane(float3 p);
		float2 ToPlane(float3 p, out float elevation);
		float3 ToWorld(float2 p, float elevation = 0);
		Bounds ToWorld(Bounds bounds);

		/// <summary>Maps from 2D (X, Y, 0) coordinates to world coordinates</summary>
		float4x4 matrix { get; }
		void Set(NativeMovementPlane plane);
	}

	public struct XYMovementPlane : IMovementPlaneWrapper {
		public float2 ToPlane(float3 p) => p.xy;
		public float2 ToPlane (float3 p, out float elevation) {
			elevation = p.z;
			return p.xy;
		}
		public float3 ToWorld(float2 p, float elevation = 0) => new float3(p.x, p.y, elevation);
		public Bounds ToWorld (Bounds bounds) {
			var center = bounds.center;
			var size = bounds.size;
			return new Bounds(new Vector3(center.x, center.z, center.y), new Vector3(size.x, size.z, size.y));
		}

		public float4x4 matrix {
			get {
				return float4x4.identity;
			}
		}
		public void Set (NativeMovementPlane plane) { }
	}

	public struct XZMovementPlane : IMovementPlaneWrapper {
		public float2 ToPlane(float3 p) => p.xz;
		public float2 ToPlane (float3 p, out float elevation) {
			elevation = p.y;
			return p.xz;
		}
		public float3 ToWorld(float2 p, float elevation = 0) => new float3(p.x, elevation, p.y);
		public Bounds ToWorld(Bounds bounds) => bounds;
		public void Set (NativeMovementPlane plane) { }
		public float4x4 matrix => float4x4.RotateX(math.radians(90));
	}

	public struct ArbitraryMovementPlane : IMovementPlaneWrapper {
		NativeMovementPlane plane;

		public float2 ToPlane(float3 p) => plane.ToPlane(p);
		public float2 ToPlane(float3 p, out float elevation) => plane.ToPlane(p, out elevation);
		public float3 ToWorld(float2 p, float elevation = 0) => plane.ToWorld(p, elevation);
		public Bounds ToWorld(Bounds bounds) => plane.ToWorld(bounds);
		public void Set (NativeMovementPlane plane) {
			this.plane = plane;
		}
		public float4x4 matrix {
			get {
				return math.mul(float4x4.TRS(0, plane.rotation, 1), new float4x4(
					new float4(1, 0, 0, 0),
					new float4(0, 0, 1, 0),
					new float4(0, 1, 0, 0),
					new float4(0, 0, 0, 1)
					));
			}
		}
	}

	[System.Flags]
	public enum AgentDebugFlags : byte {
		Nothing = 0,
		ObstacleVOs = 1 << 0,
		AgentVOs = 1 << 1,
		ReachedState = 1 << 2,
		DesiredVelocity = 1 << 3,
		ChosenVelocity = 1 << 4,
		Obstacles = 1 << 5,
		ForwardClearance = 1 << 6,
	}

	/// <summary>
	/// Exposes properties of an Agent class.
	///
	/// See: RVOController
	/// See: RVOSimulator
	/// </summary>
	public interface IAgent {
		/// <summary>
		/// Internal index of the agent.
		/// See: <see cref="Pathfinding.RVO.SimulatorBurst.simulationData"/>
		/// </summary>
		int AgentIndex { get; }

		/// <summary>
		/// Position of the agent.
		/// The agent does not move by itself, a movement script has to be responsible for
		/// reading the CalculatedTargetPoint and CalculatedSpeed properties and move towards that point with that speed.
		/// This property should ideally be set every frame.
		/// </summary>
		Vector3 Position { get; set; }

		/// <summary>
		/// Optimal point to move towards to avoid collisions.
		/// The movement script should move towards this point with a speed of <see cref="CalculatedSpeed"/>.
		///
		/// See: RVOController.CalculateMovementDelta.
		/// </summary>
		Vector3 CalculatedTargetPoint { get; }

		/// <summary>
		/// True if the agent's movement is affected by any other agents or obstacles.
		///
		/// If the agent is all alone, and can just move in a straight line to its target, this will be false.
		/// If it has to adjust its velocity, even slightly, to avoid collisions, this will be true.
		/// </summary>
		bool AvoidingAnyAgents { get; }

		/// <summary>
		/// Optimal speed of the agent to avoid collisions.
		/// The movement script should move towards <see cref="CalculatedTargetPoint"/> with this speed.
		/// </summary>
		float CalculatedSpeed { get; }

		/// <summary>
		/// Point towards which the agent should move.
		/// Usually you set this once per frame. The agent will try move as close to the target point as possible.
		/// Will take effect at the next simulation step.
		///
		/// Note: The system assumes that the agent will stop when it reaches the target point
		/// so if you just want to move the agent in a particular direction, make sure that you set the target point
		/// a good distance in front of the character as otherwise the system may not avoid colisions that well.
		/// What would happen is that the system (in simplified terms) would think that the agents would stop
		/// before the collision and thus it wouldn't slow down or change course. See the image below.
		/// In the image the desiredSpeed is the length of the blue arrow and the target point
		/// is the point where the black arrows point to.
		/// In the upper case the agent does not avoid the red agent (you can assume that the red
		/// agent has a very small velocity for simplicity) while in the lower case it does.
		/// If you are following a path a good way to pick the target point is to set it to
		/// <code>
		/// targetPoint = directionToNextWaypoint.normalized * remainingPathDistance
		/// </code>
		/// Where remainingPathDistance is the distance until the character would reach the end of the path.
		/// This works well because at the end of the path the direction to the next waypoint will just be the
		/// direction to the last point on the path and remainingPathDistance will be the distance to the last point
		/// in the path, so targetPoint will be set to simply the last point in the path. However when remainingPathDistance
		/// is large the target point will be so far away that the agent will essentially be told to move in a particular
		/// direction, which is precisely what we want.
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="targetPoint">Target point in world space.</param>
		/// <param name="desiredSpeed">Desired speed of the agent. In world units per second. The agent will try to move with this
		///      speed if possible.</param>
		/// <param name="maxSpeed">Max speed of the agent. In world units per second. If necessary (for example if another agent
		///      is on a collision trajectory towards this agent) the agent can move at this speed.
		///      Should be at least as high as desiredSpeed, but it is recommended to use a slightly
		///      higher value than desiredSpeed (for example desiredSpeed*1.2).</param>
		/// <param name="endOfPath">Point in world space which is the agent's final desired destination on the navmesh.
		/// 	This is typically the end of the path the agent is following.
		/// 	May be set to (+inf,+inf,+inf) to mark the agent as not having a well defined end of path.
		/// 	If this is set, multiple agents with roughly the same end of path will crowd more naturally around this point.
		/// 	They will be able to realize that they cannot get closer if there are many agents trying to get closer to the same destination and then stop.</param>
		void SetTarget(Vector3 targetPoint, float desiredSpeed, float maxSpeed, Vector3 endOfPath);

		/// <summary>
		/// Plane in which the agent moves.
		/// Local avoidance calculations are always done in 2D and this plane determines how to convert from 3D to 2D.
		///
		/// In a typical 3D game the agents move in the XZ plane and in a 2D game they move in the XY plane.
		/// By default this is set to the XZ plane.
		///
		/// See: <see cref="Pathfinding.Util.GraphTransform.xyPlane"/>
		/// See: <see cref="Pathfinding.Util.GraphTransform.xzPlane"/>
		/// </summary>
		Util.SimpleMovementPlane MovementPlane { get; set; }

		/// <summary>Locked agents will be assumed not to move</summary>
		bool Locked { get; set; }

		/// <summary>
		/// Radius of the agent in world units.
		/// Agents are modelled as circles/cylinders.
		/// </summary>
		float Radius { get; set; }

		/// <summary>
		/// Height of the agent in world units.
		/// Agents are modelled as circles/cylinders.
		/// </summary>
		float Height { get; set; }

		/// <summary>
		/// Max number of estimated seconds to look into the future for collisions with agents.
		/// As it turns out, this variable is also very good for controling agent avoidance priorities.
		/// Agents with lower values will avoid other agents less and thus you can make 'high priority agents' by
		/// giving them a lower value.
		/// </summary>
		float AgentTimeHorizon { get; set; }

		/// <summary>Max number of estimated seconds to look into the future for collisions with obstacles</summary>
		float ObstacleTimeHorizon { get; set; }

		/// <summary>
		/// Max number of agents to take into account.
		/// Decreasing this value can lead to better performance, increasing it can lead to better quality of the simulation.
		/// </summary>
		int MaxNeighbours { get; set; }

		/// <summary>Number of neighbours that the agent took into account during the last simulation step</summary>
		int NeighbourCount { get; }

		/// <summary>
		/// Specifies the avoidance layer for this agent.
		/// The <see cref="CollidesWith"/> mask on other agents will determine if they will avoid this agent.
		/// </summary>
		RVOLayer Layer { get; set; }

		/// <summary>
		/// Layer mask specifying which layers this agent will avoid.
		/// You can set it as CollidesWith = RVOLayer.DefaultAgent | RVOLayer.Layer3 | RVOLayer.Layer6 ...
		///
		/// See: http://en.wikipedia.org/wiki/Mask_(computing)
		/// See: bitmasks (view in online documentation for working links)
		/// </summary>
		RVOLayer CollidesWith { get; set; }

		/// <summary>
		/// Determines how strongly this agent just follows the flow instead of making other agents avoid it.
		/// The default value is 0, if it is greater than zero (up to the maximum value of 1) other agents will
		/// not avoid this character as much. However it works in a different way to <see cref="Priority"/>.
		///
		/// A group of agents with FlowFollowingStrength set to a high value that all try to reach the same point
		/// will end up just settling to stationary positions around that point, none will push the others away to any significant extent.
		/// This is tricky to achieve with priorities as priorities are all relative, so setting all agents to a low priority is the same thing
		/// as not changing priorities at all.
		///
		/// Should be a value in the range [0, 1].
		///
		/// TODO: Add video
		/// </summary>
		float FlowFollowingStrength { get; set; }

		/// <summary>Draw debug information in the scene view</summary>
		AgentDebugFlags DebugFlags { get; set; }

		/// <summary>
		/// How strongly other agents will avoid this agent.
		/// Usually a value between 0 and 1.
		/// Agents with similar priorities will avoid each other with an equal strength.
		/// If an agent sees another agent with a higher priority than itself it will avoid that agent more strongly.
		/// In the extreme case (e.g this agent has a priority of 0 and the other agent has a priority of 1) it will treat the other agent as being a moving obstacle.
		/// Similarly if an agent sees another agent with a lower priority than itself it will avoid that agent less.
		///
		/// In general the avoidance strength for this agent is:
		/// <code>
		/// if this.priority > 0 or other.priority > 0:
		///     avoidanceStrength = other.priority / (this.priority + other.priority);
		/// else:
		///     avoidanceStrength = 0.5
		/// </code>
		/// </summary>
		float Priority { get; set; }

		int HierarchicalNodeIndex { get; set; }

		/// <summary>
		/// Callback which will be called right before avoidance calculations are started.
		/// Used to update the other properties with the most up to date values
		/// </summary>
		System.Action PreCalculationCallback { set; }

		/// <summary>
		/// Callback which will be called right the agent is removed from the simulation.
		/// This agent should not be used anymore after this callback has been called.
		/// </summary>
		System.Action DestroyedCallback { set; }

		/// <summary>
		/// Set the normal of a wall (or something else) the agent is currently colliding with.
		/// This is used to make the RVO system aware of things like physics or an agent being clamped to the navmesh.
		/// The velocity of this agent that other agents observe will be modified so that there is no component
		/// into the wall. The agent will however not start to avoid the wall, for that you will need to add RVO obstacles.
		///
		/// This value will be cleared after the next simulation step, normally it should be set every frame
		/// when the collision is still happening.
		/// </summary>
		void SetCollisionNormal(Vector3 normal);

		/// <summary>
		/// Set the current velocity of the agent.
		/// This will override the local avoidance input completely.
		/// It is useful if you have a player controlled character and want other agents to avoid it.
		///
		/// Calling this method will mark the agent as being externally controlled for 1 simulation step.
		/// Local avoidance calculations will be skipped for the next simulation step but will be resumed
		/// after that unless this method is called again.
		/// </summary>
		void ForceSetVelocity(Vector3 velocity);

		public ReachedEndOfPath CalculatedEffectivelyReachedDestination { get; }

		/// <summary>
		/// Add obstacles to avoid for this agent.
		///
		/// The obstacles are based on nearby borders of the navmesh.
		/// You should call this method every frame.
		/// </summary>
		/// <param name="sourceNode">The node to start the obstacle search at. This is typically the node the agent is standing on.</param>
		public void SetObstacleQuery(GraphNode sourceNode);
	}

	/// <summary>
	/// Type of obstacle shape.
	/// See: <see cref="ObstacleVertexGroup"/>
	/// </summary>
	public enum ObstacleType {
		/// <summary>A chain of vertices, the first and last segments end at a point</summary>
		Chain,
		/// <summary>A loop of vertices, the last vertex connects back to the first one</summary>
		Loop,
	}

	public struct ObstacleVertexGroup {
		/// <summary>Type of obstacle shape</summary>
		public ObstacleType type;
		/// <summary>Number of vertices that this group consists of</summary>
		public int vertexCount;
		public float3 boundsMn;
		public float3 boundsMx;
	}

	/// <summary>Represents a set of obstacles</summary>
	public struct UnmanagedObstacle {
		/// <summary>The allocation in <see cref="ObstacleData.obstacleVertices"/> which represents all vertices used for these obstacles</summary>
		public int verticesAllocation;
		/// <summary>The allocation in <see cref="ObstacleData.obstacles"/> which represents the obstacle groups</summary>
		public int groupsAllocation;
	}

	// TODO: Change to byte?

	/// <summary>
	/// Indicates if the agent has reached the end of its path, or been blocked by other agents.
	///
	/// In the video below, the agents will get a red ring around them for the Reached state,
	/// and a brown ring for the ReachedSoon state.
	///
	/// [Open online documentation to see videos]
	///
	/// See: <see cref="IAgent.SetTarget"/>
	/// </summary>
	public enum ReachedEndOfPath {
		/// <summary>The agent has no reached the end of its path yet</summary>
		NotReached,
		/// <summary>
		/// The agent will soon reached the end of the path, or be blocked by other agents such that it cannot get closer.
		/// Typically the agent can only move forward for a fraction of a second before it will become blocked.
		/// </summary>
		ReachedSoon,
		/// <summary>
		/// The agent has reached the end of the path, or it is blocked by other agents such that it cannot get closer right now.
		/// If multiple have roughly the same end of path they will end up crowding around that point and all agents in the crowd will get this status.
		/// </summary>
		Reached,
	}

	// TODO: Change to byte?
	/// <summary>Plane which movement is primarily happening in</summary>
	public enum MovementPlane {
		/// <summary>Movement happens primarily in the XZ plane (3D)</summary>
		XZ,
		/// <summary>Movement happens primarily in the XY plane (2D)</summary>
		XY,
		/// <summary>For curved worlds. See: spherical (view in online documentation for working links)</summary>
		Arbitrary,
	}

	// Note: RVOLayer must not be marked with the [System.Flags] attribute because then Unity will show all RVOLayer fields as mask fields
	// which we do not want
	public enum RVOLayer {
		DefaultAgent = 1 << 0,
		DefaultObstacle = 1 << 1,
		Layer2 = 1 << 2,
		Layer3 = 1 << 3,
		Layer4 = 1 << 4,
		Layer5 = 1 << 5,
		Layer6 = 1 << 6,
		Layer7 = 1 << 7,
		Layer8 = 1 << 8,
		Layer9 = 1 << 9,
		Layer10 = 1 << 10,
		Layer11 = 1 << 11,
		Layer12 = 1 << 12,
		Layer13 = 1 << 13,
		Layer14 = 1 << 14,
		Layer15 = 1 << 15,
		Layer16 = 1 << 16,
		Layer17 = 1 << 17,
		Layer18 = 1 << 18,
		Layer19 = 1 << 19,
		Layer20 = 1 << 20,
		Layer21 = 1 << 21,
		Layer22 = 1 << 22,
		Layer23 = 1 << 23,
		Layer24 = 1 << 24,
		Layer25 = 1 << 25,
		Layer26 = 1 << 26,
		Layer27 = 1 << 27,
		Layer28 = 1 << 28,
		Layer29 = 1 << 29,
		Layer30 = 1 << 30
	}

	/// <summary>
	/// Local Avoidance Simulator.
	/// This class handles local avoidance simulation for a number of agents using
	/// Reciprocal Velocity Obstacles (RVO) and Optimal Reciprocal Collision Avoidance (ORCA).
	///
	/// This class will handle calculation of velocities from desired velocities supplied by a script.
	/// It is, however, not responsible for moving any objects in a Unity Scene. For that there are other scripts (see below).
	///
	/// Agents be added and removed at any time.
	///
	/// See: RVOSimulator
	/// See: RVOAgentBurst
	/// See: Pathfinding.RVO.IAgent
	///
	/// You will most likely mostly use the wrapper class <see cref="RVOSimulator"/>.
	/// </summary>
	public class SimulatorBurst {
		/// <summary>
		/// Inverse desired simulation fps.
		/// See: DesiredDeltaTime
		/// </summary>
		private float desiredDeltaTime = 0.05f;

		/// <summary>Number of agents in this simulation</summary>
		int numAgents = 0;

		/// <summary>
		/// Scope for drawing gizmos even on frames during which the simulation is not running.
		/// This is used to draw the obstacles, quadtree and agent debug lines.
		/// </summary>
		Drawing.RedrawScope debugDrawingScope;

		/// <summary>
		/// Quadtree for this simulation.
		/// Used internally by the simulation to perform fast neighbour lookups for each agent.
		/// Please only read from this tree, do not rebuild it since that can interfere with the simulation.
		/// It is rebuilt when necessary.
		///
		/// Warning: Before accessing this, you should call <see cref="LockSimulationDataReadOnly"/> or <see cref="LockSimulationDataReadWrite"/>.
		/// </summary>
		public RVOQuadtreeBurst quadtree;

		public bool drawQuadtree;

		Action[] agentPreCalculationCallbacks = new Action[0];
		Action[] agentDestroyCallbacks = new Action[0];

		Stack<int> freeAgentIndices = new Stack<int>();
		TemporaryAgentData temporaryAgentData;
		HorizonAgentData horizonAgentData;

		/// <summary>
		/// Internal simulation data.
		/// Can be used if you need very high performance access to the agent data.
		/// Normally you would use the SimulatorBurst.Agent class instead (implements the IAgent interface).
		///
		/// Warning: Before accessing this, you should call <see cref="LockSimulationDataReadOnly"/> or <see cref="LockSimulationDataReadWrite"/>.
		/// </summary>
		public AgentData simulationData;

		/// <summary>
		/// Internal simulation data.
		/// Can be used if you need very high performance access to the agent data.
		/// Normally you would use the SimulatorBurst.Agent class instead (implements the IAgent interface).
		///
		/// Warning: Before accessing this, you should call <see cref="LockSimulationDataReadOnly"/> or <see cref="LockSimulationDataReadWrite"/>.
		/// </summary>
		public AgentOutputData outputData;

		public const int MaxNeighbourCount = 50;
		public const int MaxBlockingAgentCount = 7;

		public const int MaxObstacleVertices = 256;

		public struct AgentNeighbourLookup {
			[ReadOnly]
			[NativeDisableParallelForRestriction]
			NativeArray<int> neighbours;

			public AgentNeighbourLookup(NativeArray<int> neighbours) {
				this.neighbours = neighbours;
			}

			/// <summary>Read-only span with all agent indices that a given agent took into account during its last simulation step</summary>
			public UnsafeSpan<int> GetNeighbours (int agentIndex) {
				var startIndex = agentIndex * MaxNeighbourCount;
				var endIndex = startIndex;
				while (neighbours[endIndex] != -1) endIndex++;
				return neighbours.AsUnsafeReadOnlySpan().Slice(startIndex, endIndex - startIndex);
			}
		}

		/// <summary>
		/// Lookup to find neighbours of a agents.
		///
		/// Warning: Before accessing this, you should call <see cref="LockSimulationDataReadOnly"/> or <see cref="LockSimulationDataReadWrite"/>.
		/// </summary>
		public AgentNeighbourLookup GetAgentNeighbourLookup () {
			return new AgentNeighbourLookup(temporaryAgentData.neighbours);
		}

		struct Agent : IAgent {
			public SimulatorBurst simulator;
			public AgentIndex agentIndex;

			public int AgentIndex => agentIndex.Index;
			public Vector3 Position { get => simulator.simulationData.position[AgentIndex]; set => simulator.simulationData.position[AgentIndex] = value; }
			public bool Locked { get => simulator.simulationData.locked[AgentIndex]; set => simulator.simulationData.locked[AgentIndex] = value; }
			public float Radius { get => simulator.simulationData.radius[AgentIndex]; set => simulator.simulationData.radius[AgentIndex] = value; }
			public float Height { get => simulator.simulationData.height[AgentIndex]; set => simulator.simulationData.height[AgentIndex] = value; }
			public float AgentTimeHorizon { get => simulator.simulationData.agentTimeHorizon[AgentIndex]; set => simulator.simulationData.agentTimeHorizon[AgentIndex] = value; }
			public float ObstacleTimeHorizon { get => simulator.simulationData.obstacleTimeHorizon[AgentIndex]; set => simulator.simulationData.obstacleTimeHorizon[AgentIndex] = value; }
			public int MaxNeighbours { get => simulator.simulationData.maxNeighbours[AgentIndex]; set => simulator.simulationData.maxNeighbours[AgentIndex] = value; }
			public RVOLayer Layer { get => simulator.simulationData.layer[AgentIndex]; set => simulator.simulationData.layer[AgentIndex] = value; }
			public RVOLayer CollidesWith { get => simulator.simulationData.collidesWith[AgentIndex]; set => simulator.simulationData.collidesWith[AgentIndex] = value; }
			public float FlowFollowingStrength { get => simulator.simulationData.flowFollowingStrength[AgentIndex]; set => simulator.simulationData.flowFollowingStrength[AgentIndex] = value; }
			public AgentDebugFlags DebugFlags { get => simulator.simulationData.debugFlags[AgentIndex]; set => simulator.simulationData.debugFlags[AgentIndex] = value; }
			public float Priority { get => simulator.simulationData.priority[AgentIndex]; set => simulator.simulationData.priority[AgentIndex] = value; }
			public int HierarchicalNodeIndex { get => simulator.simulationData.hierarchicalNodeIndex[AgentIndex]; set => simulator.simulationData.hierarchicalNodeIndex[AgentIndex] = value; }
			public SimpleMovementPlane MovementPlane { get => new SimpleMovementPlane(simulator.simulationData.movementPlane[AgentIndex].rotation); set => simulator.simulationData.movementPlane[AgentIndex] = new NativeMovementPlane(value); }
			public Action PreCalculationCallback { set => simulator.agentPreCalculationCallbacks[AgentIndex] = value; }
			public Action DestroyedCallback { set => simulator.agentDestroyCallbacks[AgentIndex] = value; }

			public Vector3 CalculatedTargetPoint {
				get {
					simulator.BlockUntilSimulationStepDone();
					return simulator.outputData.targetPoint[AgentIndex];
				}
			}

			public float CalculatedSpeed {
				get {
					simulator.BlockUntilSimulationStepDone();
					return simulator.outputData.speed[AgentIndex];
				}
			}

			public ReachedEndOfPath CalculatedEffectivelyReachedDestination {
				get {
					simulator.BlockUntilSimulationStepDone();
					return simulator.outputData.effectivelyReachedDestination[AgentIndex];
				}
			}

			public int NeighbourCount {
				get {
					simulator.BlockUntilSimulationStepDone();
					return simulator.outputData.numNeighbours[AgentIndex];
				}
			}

			public bool AvoidingAnyAgents {
				get {
					simulator.BlockUntilSimulationStepDone();
					return simulator.outputData.blockedByAgents[AgentIndex*SimulatorBurst.MaxBlockingAgentCount] != -1;
				}
			}

			public void SetObstacleQuery (GraphNode sourceNode) {
				HierarchicalNodeIndex = sourceNode != null && !sourceNode.Destroyed && sourceNode.Walkable ? sourceNode.HierarchicalNodeIndex : -1;
			}

			public void SetTarget (Vector3 targetPoint, float desiredSpeed, float maxSpeed, Vector3 endOfPath) {
				simulator.simulationData.SetTarget(AgentIndex, targetPoint, desiredSpeed, maxSpeed, endOfPath);
			}

			public void SetCollisionNormal (Vector3 normal) {
				simulator.simulationData.collisionNormal[AgentIndex] = normal;
			}

			public void ForceSetVelocity (Vector3 velocity) {
				// A bit hacky, but it is approximately correct
				// assuming the agent does not move significantly
				simulator.simulationData.targetPoint[AgentIndex] = simulator.simulationData.position[AgentIndex] + (float3)velocity * 1000;
				simulator.simulationData.desiredSpeed[AgentIndex] = velocity.magnitude;
				simulator.simulationData.allowedVelocityDeviationAngles[AgentIndex] = float2.zero;
				simulator.simulationData.manuallyControlled[AgentIndex] = true;
			}
		}

		/// <summary>Holds internal obstacle data for the local avoidance simulation</summary>
		public struct ObstacleData {
			/// <summary>
			/// Groups of vertices representing obstacles.
			/// An obstacle is either a cycle or a chain of vertices
			/// </summary>
			public SlabAllocator<ObstacleVertexGroup> obstacleVertexGroups;
			/// <summary>Vertices of all obstacles</summary>
			public SlabAllocator<float3> obstacleVertices;
			/// <summary>Obstacle sets, each one is represented as a set of obstacle vertex groups</summary>
			public NativeList<UnmanagedObstacle> obstacles;

			public void Init (Allocator allocator) {
				if (!obstacles.IsCreated) obstacles = new NativeList<UnmanagedObstacle>(0, allocator);
				if (!obstacleVertexGroups.IsCreated) obstacleVertexGroups = new SlabAllocator<ObstacleVertexGroup>(4, allocator);
				if (!obstacleVertices.IsCreated) obstacleVertices = new SlabAllocator<float3>(16, allocator);
			}

			public void Dispose () {
				if (obstacleVertexGroups.IsCreated) {
					obstacleVertexGroups.Dispose();
					obstacleVertices.Dispose();
					obstacles.Dispose();
				}
			}
		}

		/// <summary>Holds internal agent data for the local avoidance simulation</summary>
		public struct AgentData {
			// Note: All 3D vectors are in world space
			public NativeArray<AgentIndex> version;
			public NativeArray<float> radius;
			public NativeArray<float> height;
			public NativeArray<float> desiredSpeed;
			public NativeArray<float> maxSpeed;
			public NativeArray<float> agentTimeHorizon;
			public NativeArray<float> obstacleTimeHorizon;
			public NativeArray<bool> locked;
			public NativeArray<int> maxNeighbours;
			public NativeArray<RVOLayer> layer;
			public NativeArray<RVOLayer> collidesWith;
			public NativeArray<float> flowFollowingStrength;
			public NativeArray<float3> position;
			public NativeArray<float3> collisionNormal;
			public NativeArray<bool> manuallyControlled;
			public NativeArray<float> priority;
			public NativeArray<AgentDebugFlags> debugFlags;
			public NativeArray<float3> targetPoint;
			/// <summary>x = signed left angle in radians, y = signed right angle in radians (should be greater than x)</summary>
			public NativeArray<float2> allowedVelocityDeviationAngles;
			public NativeArray<NativeMovementPlane> movementPlane;
			public NativeArray<float3> endOfPath;
			/// <summary>Which obstacle data in the <see cref="ObstacleData.obstacles"/> array the agent should use for avoidance</summary>
			public NativeArray<int> agentObstacleMapping;
			public NativeArray<int> hierarchicalNodeIndex;

			public void Realloc (int size, Allocator allocator) {
				Util.Memory.Realloc(ref version, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref radius, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref height, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref desiredSpeed, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref maxSpeed, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref agentTimeHorizon, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref obstacleTimeHorizon, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref locked, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref maxNeighbours, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref layer, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref collidesWith, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref flowFollowingStrength, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref position, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref collisionNormal, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref manuallyControlled, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref priority, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref debugFlags, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref targetPoint, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref movementPlane, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref allowedVelocityDeviationAngles, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref endOfPath, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref agentObstacleMapping, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref hierarchicalNodeIndex, size, allocator, NativeArrayOptions.UninitializedMemory);
			}

			public void SetTarget (int agentIndex, float3 targetPoint, float desiredSpeed, float maxSpeed, float3 endOfPath) {
				maxSpeed = math.max(maxSpeed, 0);
				desiredSpeed = math.clamp(desiredSpeed, 0, maxSpeed);

				this.targetPoint[agentIndex] = targetPoint;
				this.desiredSpeed[agentIndex] = desiredSpeed;
				this.maxSpeed[agentIndex] = maxSpeed;
				this.endOfPath[agentIndex] = endOfPath;
				// TODO: Set allowedVelocityDeviationAngles here
			}

			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			public bool HasDebugFlag(int agentIndex, AgentDebugFlags flag) => Unity.Burst.CompilerServices.Hint.Unlikely((debugFlags[agentIndex] & flag) != 0);

			public void Dispose () {
				version.Dispose();
				radius.Dispose();
				height.Dispose();
				desiredSpeed.Dispose();
				maxSpeed.Dispose();
				agentTimeHorizon.Dispose();
				obstacleTimeHorizon.Dispose();
				locked.Dispose();
				maxNeighbours.Dispose();
				layer.Dispose();
				collidesWith.Dispose();
				flowFollowingStrength.Dispose();
				position.Dispose();
				collisionNormal.Dispose();
				manuallyControlled.Dispose();
				priority.Dispose();
				debugFlags.Dispose();
				targetPoint.Dispose();
				movementPlane.Dispose();
				allowedVelocityDeviationAngles.Dispose();
				endOfPath.Dispose();
				agentObstacleMapping.Dispose();
				hierarchicalNodeIndex.Dispose();
			}
		};

		public struct AgentOutputData {
			public NativeArray<float3> targetPoint;
			public NativeArray<float> speed;
			public NativeArray<int> numNeighbours;
			[NativeDisableParallelForRestrictionAttribute]
			public NativeArray<int> blockedByAgents;
			public NativeArray<ReachedEndOfPath> effectivelyReachedDestination;
			public NativeArray<float> forwardClearance;

			public void Realloc (int size, Allocator allocator) {
				Util.Memory.Realloc(ref targetPoint, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref speed, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref numNeighbours, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref blockedByAgents, size * MaxBlockingAgentCount, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref effectivelyReachedDestination, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref forwardClearance, size, allocator, NativeArrayOptions.UninitializedMemory);
			}

			public void Move (int fromIndex, int toIndex) {
				targetPoint[toIndex] = targetPoint[fromIndex];
				speed[toIndex] = speed[fromIndex];
				numNeighbours[toIndex] = numNeighbours[fromIndex];
				effectivelyReachedDestination[toIndex] = effectivelyReachedDestination[fromIndex];
				for (int i = 0; i < MaxBlockingAgentCount; i++) {
					blockedByAgents[toIndex * MaxBlockingAgentCount + i] = blockedByAgents[fromIndex * MaxBlockingAgentCount + i];
				}
				forwardClearance[toIndex] = forwardClearance[fromIndex];
			}

			public void Dispose () {
				targetPoint.Dispose();
				speed.Dispose();
				numNeighbours.Dispose();
				blockedByAgents.Dispose();
				effectivelyReachedDestination.Dispose();
				forwardClearance.Dispose();
			}
		};

		public struct HorizonAgentData {
			public NativeArray<int> horizonSide;
			public NativeArray<float> horizonMinAngle;
			public NativeArray<float> horizonMaxAngle;

			public void Realloc (int size, Allocator allocator) {
				Util.Memory.Realloc(ref horizonSide, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref horizonMinAngle, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref horizonMaxAngle, size, allocator, NativeArrayOptions.UninitializedMemory);
			}

			public void Move (int fromIndex, int toIndex) {
				horizonSide[toIndex] = horizonSide[fromIndex];
				// The other values are temporary values that don't have to be moved
			}

			public void Dispose () {
				horizonSide.Dispose();
				horizonMinAngle.Dispose();
				horizonMaxAngle.Dispose();
			}
		}

		public struct TemporaryAgentData {
			public NativeArray<float2> desiredTargetPointInVelocitySpace;
			public NativeArray<float3> desiredVelocity;
			public NativeArray<float3> currentVelocity;
			public NativeArray<float2> collisionVelocityOffsets;
			public NativeArray<int> neighbours;

			public void Realloc (int size, Allocator allocator) {
				Util.Memory.Realloc(ref desiredTargetPointInVelocitySpace, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref desiredVelocity, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref currentVelocity, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref collisionVelocityOffsets, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref neighbours, size * MaxNeighbourCount, allocator, NativeArrayOptions.UninitializedMemory);
			}

			public void Dispose () {
				desiredTargetPointInVelocitySpace.Dispose();
				desiredVelocity.Dispose();
				currentVelocity.Dispose();
				neighbours.Dispose();
				collisionVelocityOffsets.Dispose();
			}
		}

		/// <summary>
		/// Time in seconds between each simulation step.
		/// This is the desired delta time, the simulation will never run at a higher fps than
		/// the rate at which the Update function is called.
		/// </summary>
		public float DesiredDeltaTime { get { return desiredDeltaTime; } set { desiredDeltaTime = System.Math.Max(value, 0.0f); } }

		/// <summary>
		/// Bias agents to pass each other on the right side.
		/// If the desired velocity of an agent puts it on a collision course with another agent or an obstacle
		/// its desired velocity will be rotated this number of radians (1 radian is approximately 57°) to the right.
		/// This helps to break up symmetries and makes it possible to resolve some situations much faster.
		///
		/// When many agents have the same goal this can however have the side effect that the group
		/// clustered around the target point may as a whole start to spin around the target point.
		///
		/// Recommended values are in the range of 0 to 0.2.
		///
		/// If this value is negative, the agents will be biased towards passing each other on the left side instead.
		/// </summary>
		public float SymmetryBreakingBias { get; set; }

		/// <summary>Use hard collisions</summary>
		public bool HardCollisions { get; set; }

		public bool UseNavmeshAsObstacle { get; set; }

		public Rect AgentBounds {
			get {
				rwLock.ReadSync().Unlock();
				return quadtree.bounds;
			}
		}

		/// <summary>Number of agents in the simulation</summary>
		public int AgentCount => numAgents;

		public MovementPlane MovementPlane => movementPlane;

		/// <summary>Determines if the XY (2D) or XZ (3D) plane is used for movement</summary>
		public readonly MovementPlane movementPlane = MovementPlane.XZ;

		/// <summary>Used to synchronize access to the simulation data</summary>
		RWLock rwLock = new RWLock();

		public void BlockUntilSimulationStepDone () {
			rwLock.WriteSync().Unlock();
		}

		/// <summary>Create a new simulator.</summary>
		/// <param name="movementPlane">The plane that the movement happens in. XZ for 3D games, XY for 2D games.</param>
		public SimulatorBurst (MovementPlane movementPlane) {
			this.DesiredDeltaTime = 1;
			this.movementPlane = movementPlane;

			AllocateAgentSpace();

			// Just to make sure the quadtree is in a valid state
			quadtree.BuildJob(simulationData.position, simulationData.version, simulationData.desiredSpeed, simulationData.radius, 0, movementPlane).Run();
		}

		/// <summary>Removes all agents from the simulation</summary>
		public void ClearAgents () {
			BlockUntilSimulationStepDone();
			for (int i = 0; i < agentDestroyCallbacks.Length; i++) agentDestroyCallbacks[i]?.Invoke();
			numAgents = 0;
		}

		/// <summary>
		/// Frees all used memory.
		/// Warning: You must call this when you are done with the simulator, otherwise some resources can linger and lead to memory leaks.
		/// </summary>
		public void OnDestroy () {
			debugDrawingScope.Dispose();
			BlockUntilSimulationStepDone();
			ClearAgents();
			simulationData.Dispose();
			temporaryAgentData.Dispose();
			outputData.Dispose();
			quadtree.Dispose();
			horizonAgentData.Dispose();
		}

		void AllocateAgentSpace () {
			if (numAgents > agentPreCalculationCallbacks.Length || agentPreCalculationCallbacks.Length == 0) {
				var prevSize = simulationData.version.Length;
				int newSize = Mathf.Max(64, Mathf.Max(numAgents, agentPreCalculationCallbacks.Length * 2));
				simulationData.Realloc(newSize, Allocator.Persistent);
				temporaryAgentData.Realloc(newSize, Allocator.Persistent);
				outputData.Realloc(newSize, Allocator.Persistent);
				horizonAgentData.Realloc(newSize, Allocator.Persistent);
				Memory.Realloc(ref agentPreCalculationCallbacks, newSize);
				Memory.Realloc(ref agentDestroyCallbacks, newSize);
				for (int i = prevSize; i < newSize; i++) simulationData.version[i] = new AgentIndex(0, i);
			}
		}

		public bool anyAgentsInSimulation => numAgents > freeAgentIndices.Count;

		/// <summary>
		/// Add an agent at the specified position.
		/// You can use the returned interface to read and write parameters
		/// and set for example radius and desired point to move to.
		///
		/// See: <see cref="RemoveAgent"/>
		/// </summary>
		/// <param name="position">See \reflink{IAgent.Position}</param>
		public IAgent AddAgent (Vector3 position) {
			var agentIndex = AddAgentBurst(position);
			return new Agent { simulator = this, agentIndex = agentIndex };
		}

		/// <summary>
		/// Add an agent at the specified position.
		/// You can use the returned index to read and write parameters
		/// and set for example radius and desired point to move to.
		///
		/// See: <see cref="RemoveAgent"/>
		/// </summary>
		public AgentIndex AddAgentBurst (float3 position) {
			BlockUntilSimulationStepDone();

			int agentIndex;
			if (freeAgentIndices.Count > 0) {
				agentIndex = freeAgentIndices.Pop();
			} else {
				agentIndex = numAgents++;
				AllocateAgentSpace();
			}

			var packedAgentIndex = simulationData.version[agentIndex].WithIncrementedVersion();
			UnityEngine.Assertions.Assert.AreEqual(packedAgentIndex.Index, agentIndex);

			simulationData.version[agentIndex] = packedAgentIndex;
			simulationData.radius[agentIndex] = 5;
			simulationData.height[agentIndex] = 5;
			simulationData.desiredSpeed[agentIndex] = 0;
			simulationData.maxSpeed[agentIndex] = 1;
			simulationData.agentTimeHorizon[agentIndex] = 2;
			simulationData.obstacleTimeHorizon[agentIndex] = 2;
			simulationData.locked[agentIndex] = false;
			simulationData.maxNeighbours[agentIndex] = 10;
			simulationData.layer[agentIndex] = RVOLayer.DefaultAgent;
			simulationData.collidesWith[agentIndex] = (RVOLayer)(-1);
			simulationData.flowFollowingStrength[agentIndex] = 0;
			simulationData.position[agentIndex] = position;
			simulationData.collisionNormal[agentIndex] = float3.zero;
			simulationData.manuallyControlled[agentIndex] = false;
			simulationData.priority[agentIndex] = 0.5f;
			simulationData.debugFlags[agentIndex] = AgentDebugFlags.Nothing;
			simulationData.targetPoint[agentIndex] = position;
			// Set the default movement plane. Default to the XZ plane even if movement plane is arbitrary (the user will have to set a custom one later)
			simulationData.movementPlane[agentIndex] = new NativeMovementPlane((movementPlane == MovementPlane.XY ? SimpleMovementPlane.XYPlane : SimpleMovementPlane.XZPlane).rotation);
			simulationData.allowedVelocityDeviationAngles[agentIndex] = float2.zero;
			simulationData.endOfPath[agentIndex] = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			simulationData.agentObstacleMapping[agentIndex] = -1;
			simulationData.hierarchicalNodeIndex[agentIndex] = -1;

			outputData.speed[agentIndex] = 0;
			outputData.numNeighbours[agentIndex] = 0;
			outputData.targetPoint[agentIndex] = position;
			outputData.blockedByAgents[agentIndex * MaxBlockingAgentCount] = -1;
			outputData.effectivelyReachedDestination[agentIndex] = ReachedEndOfPath.NotReached;

			temporaryAgentData.neighbours[agentIndex * MaxNeighbourCount] = -1;

			horizonAgentData.horizonSide[agentIndex] = 0;
			agentPreCalculationCallbacks[agentIndex] = null;
			agentDestroyCallbacks[agentIndex] = null;

			return packedAgentIndex;
		}

		/// <summary>Deprecated: Use AddAgent(Vector3) instead</summary>
		[System.Obsolete("Use AddAgent(Vector3) instead", true)]
		public IAgent AddAgent (IAgent agent) { return null; }

		/// <summary>
		/// Removes a specified agent from this simulation.
		/// The agent can be added again later by using AddAgent.
		///
		/// See: AddAgent(IAgent)
		/// See: ClearAgents
		/// </summary>
		public void RemoveAgent (IAgent agent) {
			if (agent == null) throw new System.ArgumentNullException(nameof(agent));
			Agent realAgent = (Agent)agent;
			RemoveAgent(realAgent.agentIndex);
		}

		public void RemoveAgent (AgentIndex agent) {
			BlockUntilSimulationStepDone();

			if (!agent.TryGetIndex(ref simulationData, out var index)) throw new System.InvalidOperationException("Trying to remove agent which does not exist");

			// Increment version and set deleted bit
			simulationData.version[index] = simulationData.version[index].WithIncrementedVersion().WithDeleted();
			// Avoid memory leaks
			agentPreCalculationCallbacks[index] = null;
			try {
				if (agentDestroyCallbacks[index] != null) agentDestroyCallbacks[index]();
			} catch (System.Exception e) {
				Debug.LogException(e);
			}
			agentDestroyCallbacks[index] = null;
			freeAgentIndices.Push(index);
		}

		void PreCalculation (JobHandle dependency) {
			bool blocked = false;
			for (int i = 0; i < numAgents; i++) {
				var callback = agentPreCalculationCallbacks[i];
				if (callback != null) {
					if (!blocked) {
						dependency.Complete();
						// The pre-calculation callback may want to read the simulation data
						rwLock.ReadSync().Unlock();
						blocked = true;
					}
					callback.Invoke();
				}
			}
		}

		/// <summary>Should be called once per frame.</summary>
		/// <param name="dependency">Jobs that need to complete before local avoidance runs.</param>
		/// <param name="dt">Length of timestep in seconds.</param>
		/// <param name="drawGizmos">If true, debug gizmos will be allowed to render (they never render in standalone games, though).</param>
		/// <param name="allocator">Allocator to use for some temporary allocations. Should be a rewindable allocator since no disposal will be done.</param>
		public JobHandle Update (JobHandle dependency, float dt, bool drawGizmos, Allocator allocator) {
			var x = 0;
			if (x != 0) {
				// We need to specify these types somewhere in their concrete form.
				// Otherwise the burst compiler doesn't understand that it has to compile them.
				// This code will never run.
				new JobRVO<XYMovementPlane>().ScheduleBatch(0, 0);
				new JobRVO<XZMovementPlane>().ScheduleBatch(0, 0);
				new JobRVO<ArbitraryMovementPlane>().ScheduleBatch(0, 0);

				new JobRVOPreprocess<XYMovementPlane>().Schedule();
				new JobRVOPreprocess<XZMovementPlane>().Schedule();
				new JobRVOPreprocess<ArbitraryMovementPlane>().Schedule();

				new JobHorizonAvoidancePhase1<XYMovementPlane>().ScheduleBatch(0, 0);
				new JobHorizonAvoidancePhase1<XZMovementPlane>().ScheduleBatch(0, 0);
				new JobHorizonAvoidancePhase1<ArbitraryMovementPlane>().ScheduleBatch(0, 0);

				new JobHorizonAvoidancePhase2<XYMovementPlane>().ScheduleBatch(0, 0);
				new JobHorizonAvoidancePhase2<XZMovementPlane>().ScheduleBatch(0, 0);
				new JobHorizonAvoidancePhase2<ArbitraryMovementPlane>().ScheduleBatch(0, 0);

				new JobRVOCalculateNeighbours<XYMovementPlane>().ScheduleBatch(0, 0);
				new JobRVOCalculateNeighbours<XZMovementPlane>().ScheduleBatch(0, 0);
				new JobRVOCalculateNeighbours<ArbitraryMovementPlane>().ScheduleBatch(0, 0);

				new JobHardCollisions<XYMovementPlane>().ScheduleBatch(0, 0);
				new JobHardCollisions<XZMovementPlane>().ScheduleBatch(0, 0);
				new JobHardCollisions<ArbitraryMovementPlane>().ScheduleBatch(0, 0);

				new JobDestinationReached<XYMovementPlane>().Schedule();
				new JobDestinationReached<XZMovementPlane>().Schedule();
				new JobDestinationReached<ArbitraryMovementPlane>().Schedule();
			}

			// The burst jobs are specialized for the type of movement plane used. This improves performance for the XY and XZ movement planes quite a lot.
			// Note: The agents' own movement planes could be colinear with e.g. the XY plane, but may add an additional rotation,
			// so we must ensure that we always use the movement plane wrappers for all conversions.
			// Otherwise some conversions may add a rotation, and some may not.
			// All external communication with the rest of the world happens in world space, so we just need to be consistent internally.
			if (movementPlane == MovementPlane.XY) return UpdateInternal<XYMovementPlane>(dependency, dt, drawGizmos, allocator);
			else if (movementPlane == MovementPlane.XZ) return UpdateInternal<XZMovementPlane>(dependency, dt, drawGizmos, allocator);
			else return UpdateInternal<ArbitraryMovementPlane>(dependency, dt, drawGizmos, allocator);
		}

		/// <summary>
		/// Takes an async read-only lock on the simulation data.
		///
		/// This can be used to access <see cref="simulationData"/>, <see cref="outputData"/>, <see cref="quadtree"/>, and <see cref="GetAgentNeighbourLookup"/> in a job.
		///
		/// Use the <see cref="ReadLockAsync.dependency"/> field when you schedule the job using the simulation data,
		/// and then call <see cref="ReadLockAsync.UnlockAfter"/> with the job handle of that job.
		/// </summary>
		public RWLock.ReadLockAsync LockSimulationDataReadOnly () {
			return this.rwLock.Read();
		}

		/// <summary>
		/// Takes an async read/write lock on the simulation data.
		///
		/// This can be used to access <see cref="simulationData"/>, <see cref="outputData"/>, <see cref="quadtree"/>, and <see cref="GetAgentNeighbourLookup"/> in a job.
		///
		/// Use the <see cref="WriteLockAsync.dependency"/> field when you schedule the job using the simulation data,
		/// and then call <see cref="WriteLockAsync.UnlockAfter"/> with the job handle of that job.
		/// </summary>
		public RWLock.WriteLockAsync LockSimulationDataReadWrite () {
			return this.rwLock.Write();
		}

		JobHandle UpdateInternal<T>(JobHandle dependency, float deltaTime, bool drawGizmos, Allocator allocator) where T : struct, IMovementPlaneWrapper {
			if (!anyAgentsInSimulation) {
				// No agents, nothing to do
				// This saves some performance, since scheduling jobs has some overhead
				return dependency;
			}

			// Prevent a zero delta time
			deltaTime = math.max(deltaTime, 1.0f/2000f);

			UnityEngine.Profiling.Profiler.BeginSample("Read agent data");

			// Read agent data from RVOController components on the main thread.
			// We cannot do this in a job because RVOController data may be changed at any time
			// on the main thread.
			PreCalculation(dependency);

			UnityEngine.Profiling.Profiler.EndSample();

			var writeLock = rwLock.Write();
			dependency = JobHandle.CombineDependencies(dependency, writeLock.dependency);

			var quadtreeJob = quadtree.BuildJob(simulationData.position, simulationData.version, outputData.speed, simulationData.radius, numAgents, movementPlane).Schedule(dependency);

			var preprocessJob = new JobRVOPreprocess<T> {
				agentData = simulationData,
				previousOutput = outputData,
				temporaryAgentData = temporaryAgentData,
				startIndex = 0,
				endIndex = numAgents,
			}.Schedule(dependency);

			int batchSize = math.max(numAgents / 64, 8);
			var neighboursJob = new JobRVOCalculateNeighbours<T> {
				agentData = simulationData,
				quadtree = quadtree,
				outNeighbours = temporaryAgentData.neighbours,
				output = outputData,
			}.ScheduleBatch(numAgents, batchSize, JobHandle.CombineDependencies(preprocessJob, quadtreeJob));

			// Make the threads start working now, we have enough work scheduled that they have stuff to do.
			JobHandle.ScheduleBatchedJobs();

			var combinedJob = JobHandle.CombineDependencies(preprocessJob, neighboursJob);

			debugDrawingScope.Rewind();
			var draw = DrawingManager.GetBuilder(debugDrawingScope);

			var horizonJob1 = new JobHorizonAvoidancePhase1<T> {
				agentData = simulationData,
				neighbours = temporaryAgentData.neighbours,
				desiredTargetPointInVelocitySpace = temporaryAgentData.desiredTargetPointInVelocitySpace,
				horizonAgentData = horizonAgentData,
				draw = draw,
			}.ScheduleBatch(numAgents, batchSize, combinedJob);

			var horizonJob2 = new JobHorizonAvoidancePhase2<T> {
				neighbours = temporaryAgentData.neighbours,
				versions = simulationData.version,
				desiredVelocity = temporaryAgentData.desiredVelocity,
				desiredTargetPointInVelocitySpace = temporaryAgentData.desiredTargetPointInVelocitySpace,
				horizonAgentData = horizonAgentData,
				movementPlane = simulationData.movementPlane,
			}.ScheduleBatch(numAgents, batchSize, horizonJob1);

			var hardCollisionsJob1 = new JobHardCollisions<T> {
				agentData = simulationData,
				neighbours = temporaryAgentData.neighbours,
				collisionVelocityOffsets = temporaryAgentData.collisionVelocityOffsets,
				deltaTime = deltaTime,
				enabled = HardCollisions,
			}.ScheduleBatch(numAgents, batchSize, combinedJob);

			RWLock.CombinedReadLockAsync navmeshEdgeDataLock;
			NavmeshEdges.NavmeshBorderData navmeshEdgeData;
			bool hasAstar = AstarPath.active != null;
			if (hasAstar) {
				navmeshEdgeData = AstarPath.active.GetNavmeshBorderData(out navmeshEdgeDataLock);
			} else {
				navmeshEdgeData = NavmeshEdges.NavmeshBorderData.CreateEmpty(allocator);
				navmeshEdgeDataLock = default;
			}
			var rvoJobData = new JobRVO<T> {
				agentData = simulationData,
				temporaryAgentData = temporaryAgentData,
				navmeshEdgeData = navmeshEdgeData,
				output = outputData,
				deltaTime = deltaTime,
				symmetryBreakingBias = Mathf.Max(0, SymmetryBreakingBias),
				draw = draw,
				useNavmeshAsObstacle = UseNavmeshAsObstacle,
				priorityMultiplier = 1f,
				// priorityMultiplier = 0.1f,
			};

			combinedJob = JobHandle.CombineDependencies(horizonJob2, hardCollisionsJob1, navmeshEdgeDataLock.dependency);

			// JobHandle rvoJob = combinedJob;
			// for (int k = 0; k < 3; k++) {
			// 	var preprocessJob2 = new JobRVOPreprocess {
			// 		agentData = simulationData,
			// 		previousOutput = outputData,
			// 		temporaryAgentData = temporaryAgentData,
			// 		startIndex = 0,
			// 		endIndex = numAgents,
			// 	}.Schedule(rvoJob);
			// 	rvoJob = new JobRVO<T> {
			// 		agentData = simulationData,
			// 		temporaryAgentData = temporaryAgentData,
			// 		navmeshEdgeData = navmeshEdgeData,
			// 		output = outputData,
			// 		deltaTime = deltaTime,
			// 		symmetryBreakingBias = Mathf.Max(0, SymmetryBreakingBias),
			// 		draw = draw,
			// 		priorityMultiplier = (k+1) * (1.0f/3.0f),
			// 	}.ScheduleBatch(numAgents, batchSize, preprocessJob2);
			// }
			var rvoJob = rvoJobData.ScheduleBatch(numAgents, batchSize, combinedJob);
			if (hasAstar) {
				navmeshEdgeDataLock.UnlockAfter(rvoJob);
			} else {
				navmeshEdgeData.DisposeEmpty(rvoJob);
			}

			var reachedJob = new JobDestinationReached<T> {
				agentData = simulationData,
				temporaryAgentData = temporaryAgentData,
				output = outputData,
				draw = draw,
				numAgents = numAgents,
			}.Schedule(rvoJob);

			// Clear some fields that are reset every simulation tick
			var clearJob = simulationData.collisionNormal.MemSet(float3.zero).Schedule(reachedJob);
			var clearJob2 = simulationData.manuallyControlled.MemSet(false).Schedule(reachedJob);
			var clearJob3 = simulationData.hierarchicalNodeIndex.MemSet(-1).Schedule(reachedJob);

			dependency = JobHandle.CombineDependencies(reachedJob, clearJob, clearJob2);
			dependency = JobHandle.CombineDependencies(dependency, clearJob3);

			if (drawQuadtree && drawGizmos) {
				dependency = JobHandle.CombineDependencies(dependency, new RVOQuadtreeBurst.DebugDrawJob {
					draw = draw,
					quadtree = quadtree,
				}.Schedule(quadtreeJob));
			}

			draw.DisposeAfter(dependency);

			writeLock.UnlockAfter(dependency);
			return dependency;
		}
	}
}

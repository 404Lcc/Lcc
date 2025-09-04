using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Pathfinding.RVO {
	using Pathfinding.Util;
	using Pathfinding.Drawing;

	/// <summary>
	/// RVO Character Controller.
	/// Similar to Unity's CharacterController. It handles movement calculations and takes other agents into account.
	/// It does not handle movement itself, but allows the calling script to get the calculated velocity and
	/// use that to move the object using a method it sees fit (for example using a CharacterController, using
	/// transform.Translate or using a rigidbody).
	///
	/// \inspectorField{Radius, RVOController.radius}
	/// \inspectorField{Height, RVOController.height}
	/// \inspectorField{Center, RVOController.center}
	/// \inspectorField{Agent Time Horizon, RVOController.agentTimeHorizon}
	/// \inspectorField{Obstacle Time Horizon, RVOController.obstacleTimeHorizon}
	/// \inspectorField{Max Neighbours, RVOController.maxNeighbours}
	/// \inspectorField{Layer, RVOController.layer}
	/// \inspectorField{Collides with, RVOController.collidesWith}
	/// \inspectorField{Priority, RVOController.priority}
	/// \inspectorField{Lock When Not Moving, RVOController.lockWhenNotMoving}
	/// \inspectorField{Locked, RVOController.locked}
	/// \inspectorField{Debug, RVOController.debug}
	///
	/// <code>
	/// public void Update () {
	///     // Just some point far away
	///     var targetPoint = transform.position + transform.forward * 100;
	///
	///     // Set the desired point to move towards using a desired speed of 10 and a max speed of 12
	///     controller.SetTarget(targetPoint, 10, 12, targetPoint);
	///
	///     // Calculate how much to move during this frame
	///     // This information is based on movement commands from earlier frames
	///     // as local avoidance is calculated globally at regular intervals by the RVOSimulator component
	///     var delta = controller.CalculateMovementDelta(transform.position, Time.deltaTime);
	///     transform.position = transform.position + delta;
	/// }
	/// </code>
	///
	/// For documentation of many of the variables of this class: refer to the Pathfinding.RVO.IAgent interface.
	///
	/// Note: Requires a single <see cref="RVOSimulator"/> component in the scene
	///
	/// See: Pathfinding.RVO.IAgent
	/// See: RVOSimulator
	/// See: local-avoidance (view in online documentation for working links)
	/// </summary>
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Controller")]
	[UniqueComponent(tag = "rvo")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rvocontroller.html")]
	public class RVOController : VersionedMonoBehaviour {
		[SerializeField][FormerlySerializedAs("radius")]
		internal float radiusBackingField = 0.5f;

		[SerializeField][FormerlySerializedAs("height")]
		float heightBackingField = 2;

		[SerializeField][FormerlySerializedAs("center")]
		float centerBackingField = 1;

		/// <summary>
		/// Radius of the agent in world units.
		/// Note: If a movement script (AIPath/RichAI/AILerp, anything implementing the IAstarAI interface) is attached to the same GameObject, this value will be driven by that script.
		/// </summary>
		public float radius {
			get {
				if (ai != null) return ai.radius;
				return radiusBackingField;
			}
			set {
				if (ai != null) ai.radius = value;
				radiusBackingField = value;
			}
		}

		/// <summary>
		/// Height of the agent in world units.
		/// Note: If a movement script (AIPath/RichAI/AILerp, anything implementing the IAstarAI interface) is attached to the same GameObject, this value will be driven by that script.
		/// </summary>
		public float height {
			get {
				if (ai != null) return ai.height;
				return heightBackingField;
			}
			set {
				if (ai != null) ai.height = value;
				heightBackingField = value;
			}
		}

		/// <summary>A locked unit cannot move. Other units will still avoid it but avoidance quality is not the best.</summary>
		[Tooltip("A locked unit cannot move. Other units will still avoid it. But avoidance quality is not the best")]
		public bool locked;

		/// <summary>
		/// Automatically set <see cref="locked"/> to true when desired velocity is approximately zero.
		/// This prevents other units from pushing them away when they are supposed to e.g block a choke point.
		///
		/// When this is true every call to <see cref="SetTarget"/> or <see cref="Move"/> will set the <see cref="locked"/> field to true if the desired velocity
		/// was non-zero or false if it was zero.
		/// </summary>
		[Tooltip("Automatically set #locked to true when desired velocity is approximately zero")]
		public bool lockWhenNotMoving = false;

		/// <summary>How far into the future to look for collisions with other agents (in seconds)</summary>
		[Tooltip("How far into the future to look for collisions with other agents (in seconds)")]
		public float agentTimeHorizon = 2;

		/// <summary>How far into the future to look for collisions with obstacles (in seconds)</summary>
		[Tooltip("How far into the future to look for collisions with obstacles (in seconds)")]
		public float obstacleTimeHorizon = 0.5f;

		/// <summary>
		/// Max number of other agents to take into account.
		/// A smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.
		/// </summary>
		[Tooltip("Max number of other agents to take into account.\n" +
			"A smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.")]
		public int maxNeighbours = 10;

		/// <summary>
		/// Specifies the avoidance layer for this agent.
		/// The <see cref="collidesWith"/> mask on other agents will determine if they will avoid this agent.
		/// </summary>
		public RVOLayer layer = RVOLayer.DefaultAgent;

		/// <summary>
		/// Layer mask specifying which layers this agent will avoid.
		/// You can set it as CollidesWith = RVOLayer.DefaultAgent | RVOLayer.Layer3 | RVOLayer.Layer6 ...
		///
		/// This can be very useful in games which have multiple teams of some sort. For example you usually
		/// want the agents in one team to avoid each other, but you do not want them to avoid the enemies.
		///
		/// This field only affects which other agents that this agent will avoid, it does not affect how other agents
		/// react to this agent.
		///
		/// See: bitmasks (view in online documentation for working links)
		/// See: http://en.wikipedia.org/wiki/Mask_(computing)
		/// </summary>
		[Pathfinding.EnumFlag]
		public RVOLayer collidesWith = (RVOLayer)(-1);

		/// <summary>
		/// An extra force to avoid walls.
		/// This can be good way to reduce "wall hugging" behaviour.
		///
		/// Deprecated: This feature is currently disabled as it didn't work that well and was tricky to support after some changes to the RVO system. It may be enabled again in a future version.
		/// </summary>
		[HideInInspector]
		[System.Obsolete]
		public float wallAvoidForce = 1;

		/// <summary>
		/// How much the wallAvoidForce decreases with distance.
		/// The strenght of avoidance is:
		/// <code> str = 1/dist*wallAvoidFalloff </code>
		///
		/// See: wallAvoidForce
		///
		/// Deprecated: This feature is currently disabled as it didn't work that well and was tricky to support after some changes to the RVO system. It may be enabled again in a future version.
		/// </summary>
		[HideInInspector]
		[System.Obsolete]
		public float wallAvoidFalloff = 1;

		/// <summary>\copydocref{Pathfinding.RVO.IAgent.Priority}</summary>
		[Tooltip("How strongly other agents will avoid this agent")]
		[UnityEngine.Range(0, 1)]
		public float priority = 0.5f;

		/// <summary>
		/// Priority multiplier.
		/// This functions identically to the <see cref="priority"/>, however it is not exposed in the Unity inspector.
		/// It is primarily used by the <see cref="Pathfinding.RVO.RVODestinationCrowdedBehavior"/>.
		/// </summary>
		[System.NonSerialized]
		public float priorityMultiplier = 1.0f;

		/// <summary>\copydocref{Pathfinding.RVO.IAgent.FlowFollowingStrength}</summary>
		[System.NonSerialized]
		public float flowFollowingStrength = 0.0f;

		GraphNode obstacleQuery;

		/// <summary>
		/// Center of the agent relative to the pivot point of this game object.
		/// Note: If a movement script (AIPath/RichAI/AILerp, anything implementing the IAstarAI interface) is attached to the same GameObject, this value will be driven by that script.
		/// </summary>
		public float center {
			get {
				// With an AI attached, this will always be driven to height/2 because the movement script expects the object position to be at its feet
				if (ai != null) return ai.height/2;
				return centerBackingField;
			}
			set {
				centerBackingField = value;
			}
		}

		public MovementPlane movementPlaneMode => simulator?.MovementPlane ?? RVOSimulator.active?.movementPlane ?? MovementPlane.XZ;

		/// <summary>Determines if the XY (2D) or XZ (3D) plane is used for movement</summary>
		public SimpleMovementPlane movementPlane {
			get {
				var mode = simulator?.MovementPlane ?? RVOSimulator.active?.movementPlane;
				if (mode != null) {
					if (mode.Value == MovementPlane.Arbitrary) return movementPlaneBackingField;
					if (mode.Value == MovementPlane.XY) return SimpleMovementPlane.XYPlane;
				}
				return SimpleMovementPlane.XZPlane;
			}
			set {
				var mode = simulator?.MovementPlane ?? RVOSimulator.active?.movementPlane;
				if (mode != null && mode.Value != MovementPlane.Arbitrary) throw new System.InvalidOperationException("Cannot set the movement plane unless the RVOSimulator's movement plane setting is set to Arbitrary.");
				movementPlaneBackingField = value;
			}
		}

		/// <summary>Reference to the internal agent</summary>
		public IAgent rvoAgent { get; private set; }

		/// <summary>Reference to the rvo simulator</summary>
		SimulatorBurst simulator { get; set; }

		/// <summary>Cached tranform component</summary>
		protected Transform tr;

		[SerializeField]
		[FormerlySerializedAs("ai")]
		IAstarAI aiBackingField;

		internal SimpleMovementPlane movementPlaneBackingField = GraphTransform.xzPlane.ToSimpleMovementPlane();

		/// <summary>Cached reference to a movement script (if one is used)</summary>
		protected IAstarAI ai {
			get {
#if UNITY_EDITOR
				if (aiBackingField == null && !Application.isPlaying) aiBackingField = GetComponent<IAstarAI>();
#endif
				// Note: have to cast to MonoBehaviour to get Unity's special overloaded == operator.
				// If we didn't do this then this property could return a non-null value that pointed to a destroyed component.
				if ((aiBackingField as MonoBehaviour) == null) aiBackingField = null;
				return aiBackingField;
			}
			set {
				aiBackingField = value;
			}
		}

		/// <summary>Enables drawing debug information in the scene view</summary>
		public AgentDebugFlags debug;

		/// <summary>
		/// Current position of the agent.
		/// Note that this is only updated every local avoidance simulation step, not every frame.
		/// </summary>
		public Vector3 position {
			get {
				simulator.BlockUntilSimulationStepDone();
				return rvoAgent.Position;
			}
		}

		/// <summary>
		/// Current calculated velocity of the agent.
		/// This is not necessarily the velocity the agent is actually moving with
		/// (that is up to the movement script to decide) but it is the velocity
		/// that the RVO system has calculated is best for avoiding obstacles and
		/// reaching the target.
		///
		/// See: CalculateMovementDelta
		///
		/// You can also set the velocity of the agent. This will override the local avoidance input completely.
		/// It is useful if you have a player controlled character and want other agents to avoid it.
		///
		/// Setting the velocity using this property will mark the agent as being externally controlled for 1 simulation step.
		/// Local avoidance calculations will be skipped for the next simulation step but will be resumed
		/// after that unless this property is set again.
		///
		/// Note that if you set the velocity the value that can be read from this property will not change until
		/// the next simulation step.
		///
		/// <code>
		/// void Update () {
		///     var x = Input.GetAxis("Horizontal");
		///     var y = Input.GetAxis("Vertical");
		///
		///     var v = new Vector3(x, 0, y) * speed;
		///
		///     // Override the RVOController's velocity. This will disable local avoidance calculations for one simulation step.
		///     rvo.velocity = v;
		///     transform.position += v * Time.deltaTime;
		/// }
		/// </code>
		///
		/// See: <see cref="Pathfinding::RVO::IAgent::ForceSetVelocity"/>
		/// </summary>
		public Vector3 velocity {
			get {
				// For best accuracy and to allow other code to do things like Move(agent.velocity * Time.deltaTime)
				// the code bases the velocity on how far the agent should move during this frame.
				// Unless the game is paused (timescale is zero) then just use a very small dt.
				var dt = Time.deltaTime > 0.0001f ? Time.deltaTime : 0.02f;
				return CalculateMovementDelta(dt) / dt;
			}
			set {
				simulator.BlockUntilSimulationStepDone();
				rvoAgent.ForceSetVelocity(value);
			}
		}

		/// <summary>
		/// Direction and distance to move in a single frame to avoid obstacles.
		///
		/// The position of the agent is taken from the attached movement script's position (see <see cref="Pathfinding.IAstarAI.position)"/> or if none is attached then transform.position.
		/// </summary>
		/// <param name="deltaTime">How far to move [seconds].
		///      Usually set to Time.deltaTime.</param>
		public Vector3 CalculateMovementDelta(float deltaTime) => CalculateMovementDelta(ai != null ? ai.position : tr.position, deltaTime);

		/// <summary>
		/// Direction and distance to move in a single frame to avoid obstacles.
		///
		/// <code>
		/// public void Update () {
		///     // Just some point far away
		///     var targetPoint = transform.position + transform.forward * 100;
		///
		///     // Set the desired point to move towards using a desired speed of 10 and a max speed of 12
		///     controller.SetTarget(targetPoint, 10, 12, targetPoint);
		///
		///     // Calculate how much to move during this frame
		///     // This information is based on movement commands from earlier frames
		///     // as local avoidance is calculated globally at regular intervals by the RVOSimulator component
		///     var delta = controller.CalculateMovementDelta(transform.position, Time.deltaTime);
		///     transform.position = transform.position + delta;
		/// }
		/// </code>
		/// </summary>
		/// <param name="position">Position of the agent.</param>
		/// <param name="deltaTime">How far to move [seconds].
		///      Usually set to Time.deltaTime.</param>
		public Vector3 CalculateMovementDelta (Vector3 position, float deltaTime) {
			if (rvoAgent == null) return Vector3.zero;
			var delta = movementPlane.ToPlane(rvoAgent.CalculatedTargetPoint - position);
			return movementPlane.ToWorld(Vector2.ClampMagnitude(delta, rvoAgent.CalculatedSpeed * deltaTime), 0);
		}

		/// <summary>\copydocref{Pathfinding.RVO.IAgent.AvoidingAnyAgents}</summary>
		public bool AvoidingAnyAgents {
			get {
				if (rvoAgent == null) return false;
				return rvoAgent.AvoidingAnyAgents;
			}
		}

		/// <summary>\copydocref{Pathfinding.RVO.IAgent.SetCollisionNormal}</summary>
		public void SetCollisionNormal (Vector3 normal) {
			simulator.BlockUntilSimulationStepDone();
			rvoAgent.SetCollisionNormal(normal);
		}

		/// <summary>\copydocref{Pathfinding.RVO.IAgent.SetObstacleQuery}</summary>
		public void SetObstacleQuery (GraphNode sourceNode) {
			obstacleQuery = sourceNode;
		}

		/// <summary>
		/// Converts a 3D vector to a 2D vector in the movement plane.
		/// If movementPlane is XZ it will be projected onto the XZ plane
		/// otherwise it will be projected onto the XY plane.
		/// </summary>
		public Vector2 To2D (Vector3 p) {
			return movementPlane.ToPlane(p);
		}

		/// <summary>
		/// Converts a 3D vector to a 2D vector in the movement plane.
		/// If movementPlane is XZ it will be projected onto the XZ plane
		/// and the elevation coordinate will be the Y coordinate
		/// otherwise it will be projected onto the XY plane and elevation
		/// will be the Z coordinate.
		/// </summary>
		public Vector2 To2D (Vector3 p, out float elevation) {
			return movementPlane.ToPlane(p, out elevation);
		}

		/// <summary>
		/// Converts a 2D vector in the movement plane as well as an elevation to a 3D coordinate.
		/// See: To2D
		/// See: movementPlane
		/// </summary>
		public Vector3 To3D (Vector2 p, float elevationCoordinate) {
			return movementPlane.ToWorld(p, elevationCoordinate);
		}

		void OnDisable () {
			if (simulator == null) return;

			// Remove the agent from the simulation but keep the reference
			// this component might get enabled and then we can simply
			// add it to the simulation again
			simulator.RemoveAgent(rvoAgent);
			simulator = null;
			rvoAgent = null;
		}

		void OnEnable () {
			tr = transform;
			ai = GetComponent<IAstarAI>();

			// Make sure the AI finds this component
			// This is useful if the RVOController was added during runtime.
			if (ai is AIBase aiBase) aiBase.FindComponents();

			if (RVOSimulator.active == null) {
				Debug.LogError("No RVOSimulator component found in the scene. Please add one.");
				enabled = false;
			} else {
				simulator = RVOSimulator.active.GetSimulator();
				rvoAgent = simulator.AddAgent(Vector3.zero);
				rvoAgent.PreCalculationCallback = UpdateAgentProperties;
				rvoAgent.DestroyedCallback = OnAgentDestroyed;
			}
		}

		void OnAgentDestroyed () {
			// This can happen if the RVOSimulator component is destroyed.
			if (gameObject.activeInHierarchy) {
				// We clear the fields to avoid calling simulator.RemoveAgent during OnDisable
				simulator = null;
				rvoAgent = null;
				enabled = false;
			} else {
				// If the gameObject is not active, we do not want to set this.enabled to false
				// as that will also disable this component.
				// If the user used e.g. a pooling system, they might inactivate the gameObject,
				// but would expect the RVOController component to be enabled when they activate it again.
			}
		}

		protected void UpdateAgentProperties () {
			var scale = tr.localScale;

			rvoAgent.Radius = Mathf.Max(0.001f, radius * Mathf.Abs(scale.x));
			rvoAgent.AgentTimeHorizon = agentTimeHorizon;
			rvoAgent.ObstacleTimeHorizon = obstacleTimeHorizon;
			rvoAgent.Locked = locked;
			rvoAgent.MaxNeighbours = maxNeighbours;
			rvoAgent.DebugFlags = debug;
			rvoAgent.Layer = layer;
			rvoAgent.CollidesWith = collidesWith;

			var plane = movementPlane;
			rvoAgent.MovementPlane = plane;

			// Use the position from the movement script if one is attached
			// as the movement script's position may not be the same as the transform's position
			// (in particular if IAstarAI.updatePosition is false).
			var pos = plane.ToPlane(ai != null ? ai.position : tr.position, out float elevation);

			if (movementPlaneMode == MovementPlane.XY) {
				// In 2D it is assumed the Z coordinate differences of agents is ignored.
				rvoAgent.Height = 1;
				rvoAgent.Position = plane.ToWorld(pos, 0);
			} else {
				rvoAgent.Height = height * scale.y;
				rvoAgent.Position = plane.ToWorld(pos, elevation + (center - 0.5f * height) * scale.y);
			}

			// TODO: Move this to a separate file
			var reached = rvoAgent.CalculatedEffectivelyReachedDestination;
			var prio = priority * priorityMultiplier;
			var flow = flowFollowingStrength;
			if (reached == ReachedEndOfPath.Reached) {
				flow = 1.0f;
				prio *= 0.3f;
			} else if (reached == ReachedEndOfPath.ReachedSoon) {
				flow = 1.0f;
				prio *= 0.45f;
			}
			rvoAgent.Priority = prio;
			rvoAgent.FlowFollowingStrength = flow;

			// Note: We need to set this during UpdateAgentProperties to avoid a race condition.
			// The HierarchicalNodeIndex, which is what is stored in the rvoAgent, can be invalidated by graph updates.
			// So we must ensure that there cannot be any graph updates between when we set this, and when the simulation step happens.
			// So setting it here, which is right before the simulation step, is a good option.
			rvoAgent.SetObstacleQuery(obstacleQuery);
			obstacleQuery = null;
		}

		/// <summary>
		/// Set the target point for the agent to move towards.
		/// Similar to the <see cref="Move"/> method but this is more flexible.
		/// It is also better to use near the end of the path as when using the Move
		/// method the agent does not know where to stop, so it may overshoot the target.
		/// When using this method the agent will not overshoot the target.
		/// The agent will assume that it will stop when it reaches the target so make sure that
		/// you don't place the point too close to the agent if you actually just want to move in a
		/// particular direction.
		///
		/// The target point is assumed to stay the same until something else is requested (as opposed to being reset every frame).
		///
		/// See: Also take a look at the documentation for <see cref="IAgent.SetTarget"/> which has a few more details.
		/// See: <see cref="Move"/>
		/// </summary>
		/// <param name="pos">Point in world space to move towards.</param>
		/// <param name="speed">Desired speed in world units per second.</param>
		/// <param name="maxSpeed">Maximum speed in world units per second.
		/// 	The agent will use this speed if it is necessary to avoid collisions with other agents.
		/// 	Should be at least as high as speed, but it is recommended to use a slightly higher value than speed (for example speed*1.2).</param>
		/// <param name="endOfPath">Point in world space which is the agent's final desired destination on the navmesh.
		/// 	This is typically the end of the path the agent is following.
		/// 	May be set to (+inf,+inf,+inf) to mark the agent as not having a well defined end of path.
		/// 	If this is set, multiple agents with roughly the same end of path will crowd more naturally around this point.
		/// 	They will be able to realize that they cannot get closer if there are many agents trying to get closer to the same destination and then stop.</param>
		public void SetTarget (Vector3 pos, float speed, float maxSpeed, Vector3 endOfPath) {
			if (rvoAgent == null) return;

			simulator.BlockUntilSimulationStepDone();
			rvoAgent.SetTarget(pos, speed, maxSpeed, endOfPath);

			if (lockWhenNotMoving) {
				locked = speed < 0.001f;
			}
		}

		/// <summary>
		/// Set the desired velocity for the agent.
		///
		/// This is assumed to stay the same until something else is requested (as opposed to being reset every frame).
		///
		/// Note: In most cases the <see cref="SetTarget"/> method is better to use.
		///  What this will actually do is call <see cref="SetTarget"/> with (position + velocity).
		///  See the note in the documentation for IAgent.SetTarget about the potential
		///  issues that this can cause (in particular that it might be hard to get the agent
		///  to stop at a precise point).
		///
		/// See: <see cref="SetTarget"/>
		/// </summary>
		/// <param name="velocity">Velocity in units/second that you want the agent to move with.</param>
		public void Move (Vector3 velocity) {
			if (rvoAgent == null) return;

			simulator.BlockUntilSimulationStepDone();
			var speed = movementPlane.ToPlane(velocity).magnitude;
			rvoAgent.SetTarget((ai != null ? ai.position : tr.position) + velocity, speed, speed, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));

			if (lockWhenNotMoving) {
				locked = speed < 0.001f;
			}
		}

		public override void DrawGizmos () {
			tr = transform;
			// The AI script will draw similar gizmos
			if (ai == null) {
				var color = AIBase.ShapeGizmoColor * (locked ? 0.5f : 1.0f);
				var pos = transform.position;

				var scale = tr.localScale;
				if (movementPlaneMode == MovementPlane.XY) {
					Draw.WireCylinder(pos, Vector3.forward, 0, radius * scale.x, color);
				} else {
					Draw.WireCylinder(pos + To3D(Vector2.zero, center - height * 0.5f) * scale.y, To3D(Vector2.zero, 1), height * scale.y, radius * scale.x, color);
				}
			}
		}

		[System.Flags]
		enum RVOControllerMigrations {
			MigrateScale,
		}

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			if (migrations.TryMigrateFromLegacyFormat(out var legacyVersion)) {
				if (legacyVersion > 1) migrations.MarkMigrationFinished((int)RVOControllerMigrations.MigrateScale);
			}
			if (migrations.AddAndMaybeRunMigration((int)RVOControllerMigrations.MigrateScale, unityThread)) {
				if (transform.localScale.y != 0) centerBackingField /= Mathf.Abs(transform.localScale.y);
				if (transform.localScale.y != 0) heightBackingField /= Mathf.Abs(transform.localScale.y);
				if (transform.localScale.x != 0) radiusBackingField /= Mathf.Abs(transform.localScale.x);
			}
		}
	}
}

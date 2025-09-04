using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.Util;
	using Pathfinding.Drawing;

	/// <summary>
	/// AI for following paths.
	///
	/// This AI is the default movement script which comes with the A* Pathfinding Project.
	/// It is in no way required by the rest of the system, so feel free to write your own. But I hope this script will make it easier
	/// to set up movement for the characters in your game.
	/// This script works well for many types of units, but if you need the highest performance (for example if you are moving hundreds of characters) you
	/// may want to customize this script or write a custom movement script to be able to optimize it specifically for your game.
	///
	/// This script will try to move to a given <see cref="destination"/>. At <see cref="Pathfinding.AutoRepathPolicy.period;regular intervals"/>, the path to the destination will be recalculated.
	/// If you want to make the AI to follow a particular object you can attach the <see cref="Pathfinding.AIDestinationSetter"/> component.
	/// Take a look at the getstarted (view in online documentation for working links) tutorial for more instructions on how to configure this script.
	///
	/// Here is a video of this script being used move an agent around (it also uses the <see cref="MineBotAnimation"/> component to drive the animations):
	/// [Open online documentation to see videos]
	///
	/// \section variables Quick overview of the variables
	/// In the inspector in Unity, you will see a bunch of variables. You can view detailed information further down, but here's a quick overview.
	///
	/// The <see cref="Pathfinding.AutoRepathPolicy.period;period setting"/> determines how often it will search for new paths, if you have fast moving targets, you might want to set it to a lower value.
	/// The <see cref="destination"/> field is where the AI will try to move, it can be a point on the ground where the player has clicked in an RTS for example.
	/// Or it can be the player object in a zombie game.
	/// The <see cref="maxSpeed"/> is self-explanatory, as is <see cref="rotationSpeed"/>. however <see cref="slowdownDistance"/> might require some explanation:
	/// It is the approximate distance from the target where the AI will start to slow down. Setting it to a large value will make the AI slow down very gradually.
	/// <see cref="pickNextWaypointDist"/> determines the distance to the point the AI will move to (see image below).
	///
	/// Below is an image illustrating several variables that are exposed by this class (<see cref="pickNextWaypointDist"/>, <see cref="steeringTarget"/>, <see cref="desiredVelocity)"/>
	/// [Open online documentation to see images]
	///
	/// This script has many movement fallbacks.
	/// If it finds an RVOController attached to the same GameObject as this component, it will use that. If it finds a character controller it will also use that.
	/// If it finds a rigidbody it will use that. Lastly it will fall back to simply modifying Transform.position which is guaranteed to always work and is also the most performant option.
	///
	/// \section how-aipath-works How it works
	/// In this section I'm going to go over how this script is structured and how information flows.
	/// This is useful if you want to make changes to this script or if you just want to understand how it works a bit more deeply.
	/// However you do not need to read this section if you are just going to use the script as-is.
	///
	/// This script inherits from the <see cref="AIBase"/> class. The movement happens either in Unity's standard Update or FixedUpdate method.
	/// They are both defined in the AIBase class. Which one is actually used depends on if a rigidbody is used for movement or not.
	/// Rigidbody movement has to be done inside the FixedUpdate method while otherwise it is better to do it in Update.
	///
	/// From there a call is made to the <see cref="MovementUpdate"/> method (which in turn calls <see cref="MovementUpdateInternal)"/>.
	/// This method contains the main bulk of the code and calculates how the AI *wants* to move. However it doesn't do any movement itself.
	/// Instead it returns the position and rotation it wants the AI to move to have at the end of the frame.
	/// The Update (or FixedUpdate) method then passes these values to the <see cref="FinalizeMovement"/> method which is responsible for actually moving the character.
	/// That method also handles things like making sure the AI doesn't fall through the ground using raycasting.
	///
	/// The AI recalculates its path regularly. This happens in the Update method which checks <see cref="shouldRecalculatePath"/>, and if that returns true it will call <see cref="SearchPath"/>.
	/// The <see cref="SearchPath"/> method will prepare a path request and send it to the <see cref="Seeker"/> component, which should be attached to the same GameObject as this script.
	/// </summary>
	[AddComponentMenu("Pathfinding/AI/AIPath (2D,3D)")]
	[UniqueComponent(tag = "ai")]
	[DisallowMultipleComponent]
	public partial class AIPath : AIBase, IAstarAI {
		/// <summary>
		/// How quickly the agent accelerates.
		/// Positive values represent an acceleration in world units per second squared.
		/// Negative values are interpreted as an inverse time of how long it should take for the agent to reach its max speed.
		/// For example if it should take roughly 0.4 seconds for the agent to reach its max speed then this field should be set to -1/0.4 = -2.5.
		/// For a negative value the final acceleration will be: -acceleration*maxSpeed.
		/// This behaviour exists mostly for compatibility reasons.
		///
		/// In the Unity inspector there are two modes: Default and Custom. In the Default mode this field is set to -2.5 which means that it takes about 0.4 seconds for the agent to reach its top speed.
		/// In the Custom mode you can set the acceleration to any positive value.
		/// </summary>
		public float maxAcceleration = -2.5f;

		/// <summary>
		/// Rotation speed in degrees per second.
		/// Rotation is calculated using Quaternion.RotateTowards. This variable represents the rotation speed in degrees per second.
		/// The higher it is, the faster the character will be able to rotate.
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("turningSpeed")]
		public float rotationSpeed = 360;

		/// <summary>Distance from the end of the path where the AI will start to slow down</summary>
		public float slowdownDistance = 0.6F;

		/// <summary>
		/// How far the AI looks ahead along the path to determine the point it moves to.
		/// In world units.
		/// If you enable the <see cref="alwaysDrawGizmos"/> toggle this value will be visualized in the scene view as a blue circle around the agent.
		/// [Open online documentation to see images]
		///
		/// Here are a few example videos showing some typical outcomes with good values as well as how it looks when this value is too low and too high.
		/// <table>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-danger">Too low</span><br/></verbatim>\endxmlonly A too low value and a too low acceleration will result in the agent overshooting a lot and not managing to follow the path well.</td></tr>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-warning">Ok</span><br/></verbatim>\endxmlonly A low value but a high acceleration works decently to make the AI follow the path more closely. Note that the <see cref="Pathfinding.AILerp"/> component is better suited if you want the agent to follow the path without any deviations.</td></tr>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-success">Ok</span><br/></verbatim>\endxmlonly A reasonable value in this example.</td></tr>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-success">Ok</span><br/></verbatim>\endxmlonly A reasonable value in this example, but the path is followed slightly more loosely than in the previous video.</td></tr>
		/// <tr><td>[Open online documentation to see videos]</td><td>\xmlonly <verbatim><span class="label label-danger">Too high</span><br/></verbatim>\endxmlonly A too high value will make the agent follow the path too loosely and may cause it to try to move through obstacles.</td></tr>
		/// </table>
		/// </summary>
		public float pickNextWaypointDist = 2;

		/// <summary>Draws detailed gizmos constantly in the scene view instead of only when the agent is selected and settings are being modified</summary>
		public bool alwaysDrawGizmos;

		/// <summary>
		/// Slow down when not facing the target direction.
		/// Incurs at a small performance overhead.
		///
		/// This setting only has an effect if <see cref="enableRotation"/> is enabled.
		/// </summary>
		public bool slowWhenNotFacingTarget = true;

		/// <summary>
		/// Prevent the velocity from being too far away from the forward direction of the character.
		/// If the character is ordered to move in the opposite direction from where it is facing
		/// then enabling this will cause it to make a small loop instead of turning on the spot.
		///
		/// This setting only has an effect if <see cref="slowWhenNotFacingTarget"/> is enabled.
		/// </summary>
		public bool preventMovingBackwards = false;

		/// <summary>
		/// Ensure that the character is always on the traversable surface of the navmesh.
		/// When this option is enabled a <see cref="AstarPath.GetNearest"/> query will be done every frame to find the closest node that the agent can walk on
		/// and if the agent is not inside that node, then the agent will be moved to it.
		///
		/// This is especially useful together with local avoidance in order to avoid agents pushing each other into walls.
		/// See: local-avoidance (view in online documentation for working links) for more info about this.
		///
		/// This option also integrates with local avoidance so that if the agent is say forced into a wall by other agents the local avoidance
		/// system will be informed about that wall and can take that into account.
		///
		/// Enabling this has some performance impact depending on the graph type (pretty fast for grid graphs, slightly slower for navmesh/recast graphs).
		/// If you are using a navmesh/recast graph you may want to switch to the <see cref="Pathfinding.RichAI"/> movement script which is specifically written for navmesh/recast graphs and
		/// does this kind of clamping out of the box. In many cases it can also follow the path more smoothly around sharp bends in the path.
		///
		/// It is not recommended that you use this option together with the funnel modifier on grid graphs because the funnel modifier will make the path
		/// go very close to the border of the graph and this script has a tendency to try to cut corners a bit. This may cause it to try to go slightly outside the
		/// traversable surface near corners and that will look bad if this option is enabled.
		///
		/// Warning: This option makes no sense to use on point graphs because point graphs do not have a surface.
		/// Enabling this option when using a point graph will lead to the agent being snapped to the closest node every frame which is likely not what you want.
		///
		/// Below you can see an image where several agents using local avoidance were ordered to go to the same point in a corner.
		/// When not constraining the agents to the graph they are easily pushed inside obstacles.
		/// [Open online documentation to see images]
		/// </summary>
		public bool constrainInsideGraph = false;

		/// <summary>Current path which is followed</summary>
		protected Path path;

		/// <summary>Represents the current steering target for the agent</summary>
		protected PathInterpolator.Cursor interpolator;
		/// <summary>Helper which calculates points along the current path</summary>
		protected PathInterpolator interpolatorPath = new PathInterpolator();

		#region IAstarAI implementation

		/// <summary>\copydoc Pathfinding::IAstarAI::Teleport</summary>
		public override void Teleport (Vector3 newPosition, bool clearPath = true) {
			reachedEndOfPath = false;
			base.Teleport(newPosition, clearPath);
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::remainingDistance</summary>
		public float remainingDistance => interpolator.valid ? interpolator.remainingDistance + movementPlane.ToPlane(interpolator.position - position).magnitude : float.PositiveInfinity;

		/// <summary>\copydoc Pathfinding::IAstarAI::reachedDestination</summary>
		public override bool reachedDestination {
			get {
				if (!reachedEndOfPath) return false;
				if (!interpolator.valid || remainingDistance + movementPlane.ToPlane(destination - interpolator.endPoint).magnitude > endReachedDistance) return false;

				// Don't do height checks in 2D mode
				if (orientation != OrientationMode.YAxisForward) {
					// Check if the destination is above the head of the character or far below the feet of it
					movementPlane.ToPlane(destination - position, out float yDifference);
					var h = tr.localScale.y * height;
					if (yDifference > h || yDifference < -h*0.5) return false;
				}

				return true;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::reachedEndOfPath</summary>
		public bool reachedEndOfPath { get; protected set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::hasPath</summary>
		public bool hasPath => interpolator.valid;

		/// <summary>\copydoc Pathfinding::IAstarAI::pathPending</summary>
		public bool pathPending => waitingForPathCalculation;

		/// <summary>\copydoc Pathfinding::IAstarAI::steeringTarget</summary>
		public Vector3 steeringTarget => interpolator.valid ? interpolator.position : position;

		/// <summary>\copydoc Pathfinding::IAstarAI::endOfPath</summary>
		public override Vector3 endOfPath {
			get {
				if (interpolator.valid) return interpolator.endPoint;
				if (float.IsFinite(destination.x)) return destination;
				return position;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		float IAstarAI.radius { get => radius; set => radius = value; }

		/// <summary>\copydoc Pathfinding::IAstarAI::height</summary>
		float IAstarAI.height { get => height; set => height = value; }

		/// <summary>\copydoc Pathfinding::IAstarAI::maxSpeed</summary>
		float IAstarAI.maxSpeed { get => maxSpeed; set => maxSpeed = value; }

		/// <summary>\copydoc Pathfinding::IAstarAI::canSearch</summary>
		bool IAstarAI.canSearch { get => canSearch; set => canSearch = value; }

		/// <summary>\copydoc Pathfinding::IAstarAI::canMove</summary>
		bool IAstarAI.canMove { get => canMove; set => canMove = value; }

		/// <summary>\copydoc Pathfinding::IAstarAI::movementPlane</summary>
		NativeMovementPlane IAstarAI.movementPlane => new NativeMovementPlane(movementPlane);

		#endregion

		/// <summary>\copydocref{IAstarAI.GetRemainingPath(List<Vector3>,bool)}</summary>
		public void GetRemainingPath (List<Vector3> buffer, out bool stale) {
			buffer.Clear();
			buffer.Add(position);
			if (!interpolator.valid) {
				stale = true;
				return;
			}

			stale = false;
			interpolator.GetRemainingPath(buffer);
		}

		/// <summary>\copydocref{IAstarAI.GetRemainingPath(List<Vector3>,List<PathPartWithLinkInfo>,bool)}</summary>
		public void GetRemainingPath (List<Vector3> buffer, List<PathPartWithLinkInfo> partsBuffer, out bool stale) {
			GetRemainingPath(buffer, out stale);
			// This movement script doesn't keep track of path parts, so we just add the whole path as a single part
			if (partsBuffer != null) {
				partsBuffer.Clear();
				partsBuffer.Add(new PathPartWithLinkInfo { startIndex = 0, endIndex = buffer.Count - 1 });
			}
		}

		protected override void OnDisable () {
			// This will, among other things call ClearPath
			base.OnDisable();
			rotationFilterState = Vector2.zero;
			rotationFilterState2 = Vector2.zero;
		}

		/// <summary>
		/// The end of the path has been reached.
		/// If you want custom logic for when the AI has reached it's destination add it here. You can
		/// also create a new script which inherits from this one and override the function in that script.
		///
		/// This method will be called again if a new path is calculated as the destination may have changed.
		/// So when the agent is close to the destination this method will typically be called every <see cref="Pathfinding.AutoRepathPolicy.period;period"/> seconds.
		///
		/// Deprecated: Avoid overriding this method. Instead poll the <see cref="reachedDestination"/> or <see cref="reachedEndOfPath"/> properties.
		/// </summary>
		public virtual void OnTargetReached () {
		}

		protected virtual void UpdateMovementPlane () {
			if (path.path == null || path.path.Count == 0) return;
			var graph = AstarData.GetGraph(path.path[0]) as ITransformedGraph;
			IMovementPlane graphTransform = graph != null ? graph.transform : (orientation == OrientationMode.YAxisForward ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 270, 90), Vector3.one)) : GraphTransform.identityTransform);

			movementPlane = graphTransform.ToSimpleMovementPlane();
		}

		/// <summary>
		/// Called when a requested path has been calculated.
		/// A path is first requested by <see cref="SearchPath"/>, it is then calculated, probably in the same or the next frame.
		/// Finally it is returned to the seeker which forwards it to this function.
		/// </summary>
		protected override void OnPathComplete (Path newPath) {
			ABPath p = newPath as ABPath;

			if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

			waitingForPathCalculation = false;

			// Increase the reference count on the new path.
			// This is used for object pooling to reduce allocations.
			p.Claim(this);

			// Path couldn't be calculated of some reason.
			// More info in p.errorLog (debug string)
			if (p.error) {
				p.Release(this);
				SetPath(null);
				return;
			}

			// Release the previous path.
			if (path != null) path.Release(this);

			// Replace the old path
			path = p;

			// The RandomPath and MultiTargetPath do not have a well defined destination that could have been
			// set before the paths were calculated. So we instead set the destination here so that some properties
			// like #reachedDestination and #remainingDistance work correctly.
			if (!p.endPointKnownBeforeCalculation) {
				destination = p.originalEndPoint;
			}

			// Make sure the path contains at least 2 points
			if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
			interpolatorPath.SetPath(path.vectorPath);
			interpolator = interpolatorPath.start;

			UpdateMovementPlane();

			// Reset some variables
			reachedEndOfPath = false;

			// Simulate movement from the point where the path was requested
			// to where we are right now. This reduces the risk that the agent
			// gets confused because the first point in the path is far away
			// from the current position (possibly behind it which could cause
			// the agent to turn around, and that looks pretty bad).
			interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint) * 0.5f);
			interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

			// Update which point we are moving towards.
			// Note that we need to do this here because otherwise the remainingDistance field might be incorrect for 1 frame.
			// (due to interpolator.remainingDistance being incorrect).
			interpolator.MoveToCircleIntersection2D(position, pickNextWaypointDist, movementPlane);

			var distanceToEnd = remainingDistance;

			if (distanceToEnd <= endReachedDistance) {
				reachedEndOfPath = true;
				OnTargetReached();
			}
		}

		protected override void ClearPath () {
			CancelCurrentPathRequest();
			// Release current path so that it can be pooled
			if (path != null) path.Release(this);
			path = null;
			interpolatorPath.SetPath(null);
			reachedEndOfPath = false;
		}

		/// <summary>Called during either Update or FixedUpdate depending on if rigidbodies are used for movement or not</summary>
		protected override void MovementUpdateInternal (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			float currentAcceleration = maxAcceleration;

			// If negative, calculate the acceleration from the max speed
			if (currentAcceleration < 0) currentAcceleration *= -maxSpeed;

			if (updatePosition) {
				// Get our current position. We read from transform.position as few times as possible as it is relatively slow
				// (at least compared to a local variable)
				simulatedPosition = tr.position;
			}
			if (updateRotation) simulatedRotation = tr.rotation;

			var currentPosition = simulatedPosition;

			// Normalized direction of where the agent is looking
			var forwards = movementPlane.ToPlane(simulatedRotation * (orientation == OrientationMode.YAxisForward ? Vector3.up : Vector3.forward));

			// Check if we have a valid path to follow and some other script has not stopped the character
			bool stopped = isStopped || (reachedDestination && whenCloseToDestination == CloseToDestinationMode.Stop);

			if (rvoController != null) rvoDensityBehavior.Update(rvoController.enabled, reachedDestination, ref stopped, ref rvoController.priorityMultiplier, ref rvoController.flowFollowingStrength, currentPosition);

			float speedLimitFactor = 0;
			float distanceToEnd;
			// Check if we have a path to follow
			if (interpolator.valid) {
				// Update which point we are moving towards
				interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
				var dir = movementPlane.ToPlane(steeringTarget - currentPosition);

				// Calculate the distance to the end of the path
				distanceToEnd = dir.magnitude + Mathf.Max(0, interpolator.remainingDistance);

				// Check if we have reached the target
				var prevTargetReached = reachedEndOfPath;
				reachedEndOfPath = distanceToEnd <= endReachedDistance;
				if (!prevTargetReached && reachedEndOfPath) OnTargetReached();

				if (!stopped) {
					// How fast to move depending on the distance to the destination.
					// Move slower as the character gets closer to the destination.
					// This is always a value between 0 and 1.
					speedLimitFactor = distanceToEnd < slowdownDistance? Mathf.Sqrt(distanceToEnd / slowdownDistance) : 1;
					velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir, dir.normalized*maxSpeed, velocity2D, currentAcceleration, rotationSpeed, maxSpeed, forwards) * deltaTime;
				}
			} else {
				reachedEndOfPath = false;
				distanceToEnd = float.PositiveInfinity;
			}

			if (!interpolator.valid || stopped) {
				// Slow down as quickly as possible
				velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
				// We are already slowing down as quickly as possible. Avoid limiting the speed in other ways.
				speedLimitFactor = 1;
			}

			velocity2D = MovementUtilities.ClampVelocity(velocity2D, maxSpeed, speedLimitFactor, slowWhenNotFacingTarget && enableRotation, preventMovingBackwards, forwards);

			ApplyGravity(deltaTime);
			bool avoidingOtherAgents = false;

			if (rvoController != null && rvoController.enabled) {
				// Send a message to the RVOController that we want to move
				// with this velocity. In the next simulation step, this
				// velocity will be processed and it will be fed back to the
				// rvo controller and finally it will be used by this script
				// when calling the CalculateMovementDelta method below

				// Make sure that we don't move further than to the end point
				// of the path. If the RVO simulation FPS is low and we did
				// not do this, the agent might overshoot the target a lot.
				var rvoTarget = currentPosition + movementPlane.ToWorld(Vector2.ClampMagnitude(velocity2D, distanceToEnd), 0f);
				rvoController.SetTarget(rvoTarget, velocity2D.magnitude, maxSpeed, endOfPath);
				avoidingOtherAgents = rvoController.AvoidingAnyAgents;
			}

			// Set how much the agent wants to move during this frame
			var delta2D = lastDeltaPosition = CalculateDeltaToMoveThisFrame(currentPosition, distanceToEnd, deltaTime);
			nextPosition = currentPosition + movementPlane.ToWorld(delta2D, verticalVelocity * deltaTime);
			CalculateNextRotation(speedLimitFactor, avoidingOtherAgents, out nextRotation);
		}

		Vector2 rotationFilterState, rotationFilterState2;

		protected virtual void CalculateNextRotation (float slowdown, bool avoidingOtherAgents, out Quaternion nextRotation) {
			if (lastDeltaTime > 0.00001f && enableRotation) {
				// Rotate towards the direction we are moving in
				// Filter out noise in the movement direction
				// This is especially important when the agent is almost standing still and when using local avoidance
				float noiseThreshold = radius * tr.localScale.x * 0.2f;
				float rotationSpeedFactor = MovementUtilities.FilterRotationDirection(ref rotationFilterState, ref rotationFilterState2, lastDeltaPosition, noiseThreshold, lastDeltaTime, avoidingOtherAgents);
				nextRotation = SimulateRotationTowards(rotationFilterState, rotationSpeed * lastDeltaTime * rotationSpeedFactor, rotationSpeed * lastDeltaTime);
			} else {
				// TODO: simulatedRotation
				nextRotation = rotation;
			}
		}

		static NNConstraint cachedNNConstraint = NNConstraint.Walkable;
		protected override Vector3 ClampToNavmesh (Vector3 position, out bool positionChanged) {
			if (constrainInsideGraph) {
				cachedNNConstraint.tags = seeker.traversableTags;
				cachedNNConstraint.graphMask = seeker.graphMask;
				cachedNNConstraint.distanceMetric = DistanceMetric.ClosestAsSeenFromAboveSoft();
				// Note: We don't want to set nn.constrainDistance = false (i.e. allow finding nodes arbitrarily far away), because that can lead to harsh
				// performance cliffs if agents for example fall through the ground or get thrown off the map, or something like that (it's bound to happen in some games).
				var nearestOnNavmesh = AstarPath.active.GetNearest(position, cachedNNConstraint);

				if (nearestOnNavmesh.node == null) {
					// Found no valid node to constrain to. This can happen if there are no valid nodes close enough to the agent.
					positionChanged = false;
					return position;
				}

				var clampedPosition = nearestOnNavmesh.position;

				if (rvoController != null && rvoController.enabled) {
					// Inform the RVO system about the edges of the navmesh which will allow
					// it to better keep inside the navmesh in the first place.
					rvoController.SetObstacleQuery(nearestOnNavmesh.node);
				}

				// We cannot simply check for equality because some precision may be lost
				// if any coordinate transformations are used.
				var difference = movementPlane.ToPlane(clampedPosition - position);
				float sqrDifference = difference.sqrMagnitude;
				if (sqrDifference > 0.001f*0.001f) {
					// The agent was outside the navmesh. Remove that component of the velocity
					// so that the velocity only goes along the direction of the wall, not into it
					velocity2D -= difference * Vector2.Dot(difference, velocity2D) / sqrDifference;

					positionChanged = true;
					// Return the new position, but ignore any changes in the y coordinate from the ClampToNavmesh method as the y coordinates in the navmesh are rarely very accurate
					return position + movementPlane.ToWorld(difference);
				}
			}

			positionChanged = false;
			return position;
		}

#if UNITY_EDITOR
		[System.NonSerialized]
		int gizmoHash = 0;

		[System.NonSerialized]
		float lastChangedTime = float.NegativeInfinity;

		protected static readonly Color GizmoColor = new Color(46.0f/255, 104.0f/255, 201.0f/255);

		public override void DrawGizmos () {
			base.DrawGizmos();

			// If alwaysDrawGizmos is false, gizmos are only visible for a short while after the user changes any settings on this component
			var newGizmoHash = pickNextWaypointDist.GetHashCode() ^ slowdownDistance.GetHashCode() ^ endReachedDistance.GetHashCode();

			if (newGizmoHash != gizmoHash && gizmoHash != 0) lastChangedTime = Time.realtimeSinceStartup;
			gizmoHash = newGizmoHash;
			float alpha = alwaysDrawGizmos ? 1 : Mathf.SmoothStep(1, 0, (Time.realtimeSinceStartup - lastChangedTime - 5f)/0.5f) * (GizmoContext.selectionSize == 1 ? 1 : 0);

			if (alpha > 0) {
				// Make sure the scene view is repainted while the gizmos are visible
				if (!alwaysDrawGizmos) UnityEditor.SceneView.RepaintAll();
				Draw.Line(position, steeringTarget, GizmoColor * new Color(1, 1, 1, alpha));
				using (Draw.WithMatrix(Matrix4x4.TRS(position, transform.rotation * (orientation == OrientationMode.YAxisForward ? Quaternion.Euler(-90, 0, 0) : Quaternion.identity), Vector3.one))) {
					Draw.xz.Circle(Vector3.zero, pickNextWaypointDist, GizmoColor * new Color(1, 1, 1, alpha));
					Draw.xz.Circle(Vector3.zero, slowdownDistance, Color.Lerp(GizmoColor, Color.red, 0.5f) * new Color(1, 1, 1, alpha));
					Draw.xz.Circle(Vector3.zero, endReachedDistance, Color.Lerp(GizmoColor, Color.red, 0.8f) * new Color(1, 1, 1, alpha));
				}
			}
		}
#endif

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			if (migrations.IsLegacyFormat) {
				// Approximately convert from a damping value to a degrees per second value.
				if (migrations.LegacyVersion < 1) rotationSpeed *= 90;
				// The base call will migrate the legacy format further
			}
			base.OnUpgradeSerializedData(ref migrations, unityThread);
		}
	}
}

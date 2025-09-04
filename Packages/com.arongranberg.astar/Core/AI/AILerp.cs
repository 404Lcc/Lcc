using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.Util;

	/// <summary>
	/// Linearly interpolating movement script.
	///
	/// This movement script will follow the path exactly, it uses linear interpolation to move between the waypoints in the path.
	/// This is desirable for some types of games.
	/// It also works in 2D.
	///
	/// See: You can see an example of this script in action in the example scene called Example15_2D.
	///
	/// \section rec Configuration
	/// \subsection rec-snapped Recommended setup for movement along connections
	///
	/// This depends on what type of movement you are aiming for.
	/// If you are aiming for movement where the unit follows the path exactly and move only along the graph connections on a grid/point graph.
	/// I recommend that you adjust the StartEndModifier on the Seeker component: set the 'Start Point Snapping' field to 'NodeConnection' and the 'End Point Snapping' field to 'SnapToNode'.
	/// [Open online documentation to see images]
	/// [Open online documentation to see images]
	///
	/// \subsection rec-smooth Recommended setup for smooth movement
	/// If you on the other hand want smoother movement I recommend setting 'Start Point Snapping' and 'End Point Snapping' to 'ClosestOnNode' and to add the Simple Smooth Modifier to the GameObject as well.
	/// Alternatively you can use the <see cref="Pathfinding.FunnelModifier Funnel"/> which works better on navmesh/recast graphs or the <see cref="Pathfinding.RaycastModifier"/>.
	///
	/// You should not combine the Simple Smooth Modifier or the Funnel Modifier with the NodeConnection snapping mode. This may lead to very odd behavior.
	///
	/// [Open online documentation to see images]
	/// [Open online documentation to see images]
	/// You may also want to tweak the <see cref="rotationSpeed"/>.
	/// </summary>
	[RequireComponent(typeof(Seeker))]
	[AddComponentMenu("Pathfinding/AI/AILerp (2D,3D)")]
	[UniqueComponent(tag = "ai")]
	[DisallowMultipleComponent]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/ailerp.html")]
	public class AILerp : VersionedMonoBehaviour, IAstarAI {
		/// <summary>
		/// Determines how often it will search for new paths.
		/// If you have fast moving targets or AIs, you might want to set it to a lower value.
		/// The value is in seconds between path requests.
		///
		/// Deprecated: This has been renamed to <see cref="autoRepath.period"/>.
		/// See: <see cref="AutoRepathPolicy"/>
		/// </summary>
		public float repathRate {
			get {
				return this.autoRepath.period;
			}
			set {
				this.autoRepath.period = value;
			}
		}

		/// <summary>
		/// \copydoc Pathfinding::IAstarAI::canSearch
		/// Deprecated: This has been superseded by <see cref="autoRepath.mode"/>.
		/// </summary>
		public bool canSearch {
			get {
				return this.autoRepath.mode != AutoRepathPolicy.Mode.Never;
			}
			set {
				this.autoRepath.mode = value ? AutoRepathPolicy.Mode.EveryNSeconds : AutoRepathPolicy.Mode.Never;
			}
		}

		/// <summary>
		/// Determines how the agent recalculates its path automatically.
		/// This corresponds to the settings under the "Recalculate Paths Automatically" field in the inspector.
		/// </summary>
		public AutoRepathPolicy autoRepath = new AutoRepathPolicy();

		/// <summary>\copydoc Pathfinding::IAstarAI::canMove</summary>
		public bool canMove = true;

		/// <summary>Speed in world units</summary>
		public float speed = 3;

		/// <summary>
		/// Determines which direction the agent moves in.
		/// For 3D games you most likely want the ZAxisIsForward option as that is the convention for 3D games.
		/// For 2D games you most likely want the YAxisIsForward option as that is the convention for 2D games.
		///
		/// Using the YAxisForward option will also allow the agent to assume that the movement will happen in the 2D (XY) plane instead of the XZ plane
		/// if it does not know. This is important only for the point graph which does not have a well defined up direction. The other built-in graphs (e.g the grid graph)
		/// will all tell the agent which movement plane it is supposed to use.
		///
		/// [Open online documentation to see images]
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("rotationIn2D")]
		public OrientationMode orientation = OrientationMode.ZAxisForward;

		/// <summary>
		/// If true, the AI will rotate to face the movement direction.
		/// See: <see cref="orientation"/>
		/// </summary>
		public bool enableRotation = true;

		/// <summary>How quickly to rotate</summary>
		public float rotationSpeed = 10;

		/// <summary>
		/// If true, some interpolation will be done when a new path has been calculated.
		/// This is used to avoid short distance teleportation.
		/// See: <see cref="switchPathInterpolationSpeed"/>
		/// </summary>
		public bool interpolatePathSwitches = true;

		/// <summary>
		/// How quickly to interpolate to the new path.
		/// See: <see cref="interpolatePathSwitches"/>
		/// </summary>
		public float switchPathInterpolationSpeed = 5;

		/// <summary>True if the end of the current path has been reached</summary>
		public bool reachedEndOfPath { get; private set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::reachedDestination</summary>
		public bool reachedDestination {
			get {
				if (!reachedEndOfPath || !interpolator.valid) return false;
				// Note: distanceToSteeringTarget is the distance to the end of the path when approachingPathEndpoint is true
				var dir = destination - interpolator.endPoint;
				// Ignore either the y or z coordinate depending on if we are using 2D mode or not
				if (orientation == OrientationMode.YAxisForward) dir.z = 0;
				else dir.y = 0;

				// Check against using a very small margin
				// In theory a check against 0 should be done, but this will be a bit more resilient against targets that move slowly or maybe jitter around due to floating point errors.
				if (remainingDistance + dir.magnitude >= 0.05f) return false;

				return true;
			}
		}

		public Vector3 destination { get; set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::movementPlane</summary>
		public NativeMovementPlane movementPlane {
			get {
				if (path != null && path.path.Count > 0) {
					var graph = path.path[0].Graph;
					if (graph is NavmeshBase navmeshBase) {
						return new NativeMovementPlane(navmeshBase.transform.ToSimpleMovementPlane());
					} else if (graph is GridGraph gg) {
						return new NativeMovementPlane(gg.transform.ToSimpleMovementPlane());
					}
				}
				return new NativeMovementPlane(Unity.Mathematics.quaternion.identity);
			}
		}

		/// <summary>
		/// Determines if the character's position should be coupled to the Transform's position.
		/// If false then all movement calculations will happen as usual, but the object that this component is attached to will not move
		/// instead only the <see cref="position"/> property will change.
		///
		/// See: <see cref="canMove"/> which in contrast to this field will disable all movement calculations.
		/// See: <see cref="updateRotation"/>
		/// </summary>
		public bool updatePosition { get; set; } = true;

		/// <summary>
		/// Determines if the character's rotation should be coupled to the Transform's rotation.
		/// If false then all movement calculations will happen as usual, but the object that this component is attached to will not rotate
		/// instead only the <see cref="rotation"/> property will change.
		///
		/// See: <see cref="updatePosition"/>
		/// </summary>
		public bool updateRotation { get; set; } = true;

		/// <summary>
		/// Cached delegate for the <see cref="OnPathComplete"/> method.
		///
		/// Caching this avoids allocating a new one every time a path is calculated, which reduces GC pressure.
		/// </summary>
		protected OnPathDelegate onPathComplete;

		/// <summary>\copydoc Pathfinding::IAstarAI::position</summary>
		public Vector3 position { get { return updatePosition ? tr.position : simulatedPosition; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::rotation</summary>
		public Quaternion rotation {
			get { return updateRotation ? tr.rotation : simulatedRotation; }
			set {
				if (updateRotation) {
					tr.rotation = value;
				} else {
					simulatedRotation = value;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::endOfPath</summary>
		public Vector3 endOfPath {
			get {
				if (interpolator.valid) return interpolator.endPoint;
				if (float.IsFinite(destination.x)) return destination;
				return position;
			}
		}

		#region IAstarAI implementation

		/// <summary>\copydoc Pathfinding::IAstarAI::Move</summary>
		void IAstarAI.Move (Vector3 deltaPosition) {
			// This script does not know the concept of being away from the path that it is following
			// so this call will be ignored (as is also mentioned in the documentation).
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		float IAstarAI.radius { get { return 0; } set {} }

		/// <summary>\copydoc Pathfinding::IAstarAI::height</summary>
		float IAstarAI.height { get { return 0; } set {} }

		/// <summary>\copydoc Pathfinding::IAstarAI::maxSpeed</summary>
		float IAstarAI.maxSpeed { get { return speed; } set { speed = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::canSearch</summary>
		bool IAstarAI.canSearch { get { return canSearch; } set { canSearch = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::canMove</summary>
		bool IAstarAI.canMove { get { return canMove; } set { canMove = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::velocity</summary>
		public Vector3 velocity {
			get {
				return Time.deltaTime > 0.00001f ? (previousPosition1 - previousPosition2) / Time.deltaTime : Vector3.zero;
			}
		}

		Vector3 IAstarAI.desiredVelocity {
			get {
				// The AILerp script sets the position every frame. It does not take into account physics
				// or other things. So the velocity should always be the same as the desired velocity.
				return (this as IAstarAI).velocity;
			}
		}

		Vector3 IAstarAI.desiredVelocityWithoutLocalAvoidance {
			get {
				// The AILerp script sets the position every frame. It does not take into account physics
				// or other things. So the velocity should always be the same as the desired velocity.
				return (this as IAstarAI).velocity;
			}
			set {
				throw new System.InvalidOperationException("The AILerp component does not support setting the desiredVelocityWithoutLocalAvoidance property since it does not make sense for how its movement works.");
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::steeringTarget</summary>
		Vector3 IAstarAI.steeringTarget {
			get {
				// AILerp doesn't use steering at all, so we will just return a point ahead of the agent in the direction it is moving.
				return interpolator.valid ? interpolator.position + interpolator.tangent : simulatedPosition;
			}
		}

		#endregion

		/// <summary>\copydoc Pathfinding::IAstarAI::remainingDistance</summary>
		public float remainingDistance {
			get {
				return interpolator.valid ? Mathf.Max(interpolator.remainingDistance, 0) : float.PositiveInfinity;
			}
			set {
				if (!interpolator.valid) throw new System.InvalidOperationException("Cannot set the remaining distance on the AILerp component because it doesn't have a path to follow.");
				interpolator.remainingDistance = Mathf.Max(value, 0);
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::hasPath</summary>
		public bool hasPath {
			get {
				return interpolator.valid;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::pathPending</summary>
		public bool pathPending {
			get {
				return !canSearchAgain;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::isStopped</summary>
		public bool isStopped { get; set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::onSearchPath</summary>
		public System.Action onSearchPath { get; set; }

		/// <summary>Cached Seeker component</summary>
		protected Seeker seeker;

		/// <summary>Cached Transform component</summary>
		protected Transform tr;

		/// <summary>Current path which is followed</summary>
		protected ABPath path;

		/// <summary>Only when the previous path has been returned should a search for a new path be done</summary>
		protected bool canSearchAgain = true;

		/// <summary>
		/// When a new path was returned, the AI was moving along this ray.
		/// Used to smoothly interpolate between the previous movement and the movement along the new path.
		/// The speed is equal to movement direction.
		/// </summary>
		protected Vector3 previousMovementOrigin;
		protected Vector3 previousMovementDirection;

		/// <summary>
		/// Time since the path was replaced by a new path.
		/// See: <see cref="interpolatePathSwitches"/>
		/// </summary>
		protected float pathSwitchInterpolationTime = 0;

		protected PathInterpolator.Cursor interpolator;
		protected PathInterpolator interpolatorPath = new PathInterpolator();


		/// <summary>
		/// Holds if the Start function has been run.
		/// Used to test if coroutines should be started in OnEnable to prevent calculating paths
		/// in the awake stage (or rather before start on frame 0).
		/// </summary>
		bool startHasRun = false;

		Vector3 previousPosition1, previousPosition2, simulatedPosition;
		Quaternion simulatedRotation;

		[SerializeField]
		[HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs("repathRate")]
		float repathRateCompatibility = float.NaN;

		[SerializeField]
		[HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs("canSearch")]
		bool canSearchCompability = false;

		protected AILerp () {
			// Note that this needs to be set here in the constructor and not in e.g Awake
			// because it is possible that other code runs and sets the destination property
			// before the Awake method on this script runs.
			destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		/// <summary>
		/// Initializes reference variables.
		/// If you override this function you should in most cases call base.Awake () at the start of it.
		/// </summary>
		protected override void Awake () {
			base.Awake();
			//This is a simple optimization, cache the transform component lookup
			tr = transform;

			seeker = GetComponent<Seeker>();

			// Tell the StartEndModifier to ask for our exact position when post processing the path This
			// is important if we are using prediction and requesting a path from some point slightly ahead
			// of us since then the start point in the path request may be far from our position when the
			// path has been calculated. This is also good because if a long path is requested, it may take
			// a few frames for it to be calculated so we could have moved some distance during that time
			seeker.startEndModifier.adjustStartPoint = () => simulatedPosition;
		}

		/// <summary>
		/// Starts searching for paths.
		/// If you override this function you should in most cases call base.Start () at the start of it.
		/// See: <see cref="Init"/>
		/// </summary>
		protected virtual void Start () {
			startHasRun = true;
			Init();
		}

		/// <summary>Called when the component is enabled</summary>
		protected virtual void OnEnable () {
			onPathComplete = OnPathComplete;
			Init();
		}

		void Init () {
			if (startHasRun) {
				// The Teleport call will make sure some variables are properly initialized (like #prevPosition1 and #prevPosition2)
				Teleport(position, false);
				autoRepath.Reset();
				if (shouldRecalculatePath) SearchPath();
			}
		}

		public void OnDisable () {
			ClearPath();
		}

		/// <summary>\copydocref{IAstarAI.GetRemainingPath(List<Vector3>,bool)}</summary>
		public void GetRemainingPath (List<Vector3> buffer, out bool stale) {
			buffer.Clear();
			if (!interpolator.valid) {
				buffer.Add(position);
				stale = true;
				return;
			}

			stale = false;
			interpolator.GetRemainingPath(buffer);
			// The agent is almost always at interpolation.position (which is buffer[0])
			// but sometimes - in particular when interpolating between two paths - the agent might at a slightly different position.
			// So we replace the first point with the actual position of the agent.
			buffer[0] = position;
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

		public void Teleport (Vector3 position, bool clearPath = true) {
			if (clearPath) ClearPath();
			simulatedPosition = previousPosition1 = previousPosition2 = position;
			if (updatePosition) tr.position = position;
			reachedEndOfPath = false;
			if (clearPath) SearchPath();
		}

		/// <summary>True if the path should be automatically recalculated as soon as possible</summary>
		protected virtual bool shouldRecalculatePath {
			get {
				return canSearchAgain && autoRepath.ShouldRecalculatePath(position, 0.0f, destination, Time.time);
			}
		}

		/// <summary>Requests a path to the target.</summary>
		public virtual void SearchPath () {
			if (float.IsPositiveInfinity(destination.x)) return;
			if (onSearchPath != null) onSearchPath();

			// This is where the path should start to search from
			var currentPosition = GetFeetPosition();

			// If we are following a path, start searching from the node we will
			// reach next this can prevent odd turns right at the start of the path
			/*if (interpolator.valid) {
			    var prevDist = interpolator.distance;
			    // Move to the end of the current segment
			    interpolator.MoveToSegment(interpolator.segmentIndex, 1);
			    currentPosition = interpolator.position;
			    // Move back to the original position
			    interpolator.distance = prevDist;
			}*/

			canSearchAgain = false;

			// Create a new path request
			// The OnPathComplete method will later be called with the result
			SetPath(ABPath.Construct(currentPosition, destination, null), false);
		}

		/// <summary>
		/// The end of the path has been reached.
		/// If you want custom logic for when the AI has reached it's destination
		/// add it here.
		/// You can also create a new script which inherits from this one
		/// and override the function in that script.
		///
		/// Deprecated: Avoid overriding this method. Instead poll the <see cref="reachedDestination"/> or <see cref="reachedEndOfPath"/> properties.
		/// </summary>
		public virtual void OnTargetReached () {
		}

		/// <summary>
		/// Called when a requested path has finished calculation.
		/// A path is first requested by <see cref="SearchPath"/>, it is then calculated, probably in the same or the next frame.
		/// Finally it is returned to the seeker which forwards it to this function.
		/// </summary>
		protected virtual void OnPathComplete (Path _p) {
			ABPath p = _p as ABPath;

			if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

			canSearchAgain = true;

			// Increase the reference count on the path.
			// This is used for path pooling
			p.Claim(this);

			// Path couldn't be calculated of some reason.
			// More info in p.errorLog (debug string)
			if (p.error) {
				p.Release(this);
				return;
			}

			if (interpolatePathSwitches) {
				ConfigurePathSwitchInterpolation();
			}


			// Replace the old path
			var oldPath = path;

			path = p;
			reachedEndOfPath = false;

			// The RandomPath and MultiTargetPath do not have a well defined destination that could have been
			// set before the paths were calculated. So we instead set the destination here so that some properties
			// like #reachedDestination and #remainingDistance work correctly.
			if (path is RandomPath rpath) {
				destination = rpath.originalEndPoint;
			} else if (path is MultiTargetPath mpath) {
				destination = mpath.originalEndPoint;
			}

			// Just for the rest of the code to work, if there
			// is only one waypoint in the path add another one
			if (path.vectorPath != null && path.vectorPath.Count == 1) {
				path.vectorPath.Insert(0, GetFeetPosition());
			}

			// Reset some variables
			ConfigureNewPath();

			// Release the previous path
			// This is used for path pooling.
			// This is done after the interpolator has been configured in the ConfigureNewPath method
			// as this method would otherwise invalidate the interpolator
			// since the vectorPath list (which the interpolator uses) will be pooled.
			if (oldPath != null) oldPath.Release(this);

			if (interpolator.remainingDistance < 0.0001f && !reachedEndOfPath) {
				reachedEndOfPath = true;
				OnTargetReached();
			}
		}

		/// <summary>
		/// Clears the current path of the agent.
		///
		/// Usually invoked using <see cref="SetPath"/>(null)
		///
		/// See: <see cref="SetPath"/>
		/// See: <see cref="isStopped"/>
		/// </summary>
		protected virtual void ClearPath () {
			// Abort any calculations in progress
			if (seeker != null) seeker.CancelCurrentPathRequest();
			canSearchAgain = true;
			reachedEndOfPath = false;

			// Release current path so that it can be pooled
			if (path != null) path.Release(this);
			path = null;
			interpolatorPath.SetPath(null);
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::SetPath</summary>
		public void SetPath (Path path, bool updateDestinationFromPath = true) {
			if (updateDestinationFromPath && path is ABPath abPath && !(path is RandomPath)) {
				this.destination = abPath.originalEndPoint;
			}

			if (path == null) {
				ClearPath();
			} else if (path.PipelineState == PathState.Created) {
				// Path has not started calculation yet
				canSearchAgain = false;
				seeker.CancelCurrentPathRequest();
				seeker.StartPath(path, onPathComplete);
				autoRepath.DidRecalculatePath(destination, Time.time);
			} else if (path.PipelineState >= PathState.Returning) {
				// Path has already been calculated

				// We might be calculating another path at the same time, and we don't want that path to override this one. So cancel it.
				if (seeker.GetCurrentPath() != path) seeker.CancelCurrentPathRequest();

				OnPathComplete(path);
			} else {
				// Path calculation has been started, but it is not yet complete. Cannot really handle this.
				throw new System.ArgumentException("You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
			}
		}

		protected virtual void ConfigurePathSwitchInterpolation () {
			bool reachedEndOfPreviousPath = interpolator.valid && interpolator.remainingDistance < 0.0001f;

			if (interpolator.valid && !reachedEndOfPreviousPath) {
				previousMovementOrigin = interpolator.position;
				previousMovementDirection = interpolator.tangent.normalized * interpolator.remainingDistance;
				pathSwitchInterpolationTime = 0;
			} else {
				previousMovementOrigin = Vector3.zero;
				previousMovementDirection = Vector3.zero;
				pathSwitchInterpolationTime = float.PositiveInfinity;
			}
		}

		public virtual Vector3 GetFeetPosition () {
			return position;
		}

		/// <summary>Finds the closest point on the current path and configures the <see cref="interpolator"/></summary>
		protected virtual void ConfigureNewPath () {
			var hadValidPath = interpolator.valid;
			var prevTangent = hadValidPath ? interpolator.tangent : Vector3.zero;

			interpolatorPath.SetPath(path.vectorPath);
			interpolator = interpolatorPath.start;
			interpolator.MoveToClosestPoint(GetFeetPosition());

			if (interpolatePathSwitches && switchPathInterpolationSpeed > 0.01f && hadValidPath) {
				var correctionFactor = Mathf.Max(-Vector3.Dot(prevTangent.normalized, interpolator.tangent.normalized), 0);
				interpolator.distance -= speed*correctionFactor*(1f/switchPathInterpolationSpeed);
			}
		}

		protected virtual void Update () {
			if (shouldRecalculatePath) SearchPath();
			if (canMove) {
				Vector3 nextPosition;
				Quaternion nextRotation;
				MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);
				FinalizeMovement(nextPosition, nextRotation);
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::MovementUpdate</summary>
		public void MovementUpdate (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			if (updatePosition) simulatedPosition = tr.position;
			if (updateRotation) simulatedRotation = tr.rotation;

			Vector3 direction;

			nextPosition = CalculateNextPosition(out direction, isStopped ? 0f : deltaTime);

			if (enableRotation) nextRotation = SimulateRotationTowards(direction, deltaTime);
			else nextRotation = simulatedRotation;
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::FinalizeMovement</summary>
		public void FinalizeMovement (Vector3 nextPosition, Quaternion nextRotation) {
			previousPosition2 = previousPosition1;
			previousPosition1 = simulatedPosition = nextPosition;
			simulatedRotation = nextRotation;
			if (updatePosition) tr.position = nextPosition;
			if (updateRotation) tr.rotation = nextRotation;
		}

		Quaternion SimulateRotationTowards (Vector3 direction, float deltaTime) {
			// Rotate unless we are really close to the target
			if (direction != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation(direction, orientation == OrientationMode.YAxisForward ? Vector3.back : Vector3.up);
				// This causes the character to only rotate around the Z axis
				if (orientation == OrientationMode.YAxisForward) targetRotation *= Quaternion.Euler(90, 0, 0);
				return Quaternion.Slerp(simulatedRotation, targetRotation, deltaTime * rotationSpeed);
			}
			return simulatedRotation;
		}

		/// <summary>Calculate the AI's next position (one frame in the future).</summary>
		/// <param name="direction">The tangent of the segment the AI is currently traversing. Not normalized.</param>
		/// <param name="deltaTime">The time to simulate into the future.</param>
		protected virtual Vector3 CalculateNextPosition (out Vector3 direction, float deltaTime) {
			if (!interpolator.valid) {
				direction = Vector3.zero;
				return simulatedPosition;
			}

			interpolator.distance += deltaTime * speed;

			if (interpolator.remainingDistance < 0.0001f && !reachedEndOfPath) {
				reachedEndOfPath = true;
				OnTargetReached();
			}

			direction = interpolator.tangent;
			pathSwitchInterpolationTime += deltaTime;
			var alpha = switchPathInterpolationSpeed * pathSwitchInterpolationTime;

			if (interpolatePathSwitches && alpha < 1f) {
				// Find the approximate position we would be at if we
				// would have continued to follow the previous path
				Vector3 positionAlongPreviousPath = previousMovementOrigin + Vector3.ClampMagnitude(previousMovementDirection, speed * pathSwitchInterpolationTime);

				// Interpolate between the position on the current path and the position
				// we would have had if we would have continued along the previous path.
				return Vector3.Lerp(positionAlongPreviousPath, interpolator.position, alpha);
			} else {
				return interpolator.position;
			}
		}

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			if (migrations.TryMigrateFromLegacyFormat(out var legacyVersion)) {
				if (legacyVersion <= 3) {
					repathRate = repathRateCompatibility;
					canSearch = canSearchCompability;
				}
			}
		}

		public override void DrawGizmos () {
			tr = transform;
			autoRepath.DrawGizmos(Pathfinding.Drawing.Draw.editor, this.position, 0.0f, new NativeMovementPlane(orientation == OrientationMode.YAxisForward ? Quaternion.Euler(-90, 0, 0) : Quaternion.identity));
		}
	}
}

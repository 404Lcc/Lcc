using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;

namespace Pathfinding {
	/// <summary>
	/// Common interface for all movement scripts in the A* Pathfinding Project.
	/// See: <see cref="Pathfinding.AIPath"/>
	/// See: <see cref="Pathfinding.RichAI"/>
	/// See: <see cref="Pathfinding.AILerp"/>
	/// </summary>
	public interface IAstarAI {
		/// <summary>
		/// Radius of the agent in world units.
		/// This is visualized in the scene view as a yellow cylinder around the character.
		///
		/// Note that this does not affect pathfinding in any way.
		/// The graph used completely determines where the agent can move.
		///
		/// Note: The <see cref="Pathfinding.AILerp"/> script doesn't really have any use of knowing the radius or the height of the character, so this property will always return 0 in that script.
		/// </summary>
		float radius { get; set; }

		/// <summary>
		/// Height of the agent in world units.
		/// This is visualized in the scene view as a yellow cylinder around the character.
		///
		/// This value is currently only used if an RVOController is attached to the same GameObject, otherwise it is only used for drawing nice gizmos in the scene view.
		/// However since the height value is used for some things, the radius field is always visible for consistency and easier visualization of the character.
		/// That said, it may be used for something in a future release.
		///
		/// Note: The <see cref="Pathfinding.AILerp"/> script doesn't really have any use of knowing the radius or the height of the character, so this property will always return 0 in that script.
		/// </summary>
		float height { get; set; }

		/// <summary>
		/// Position of the agent.
		/// In world space.
		/// See: <see cref="rotation"/>
		///
		/// If you want to move the agent you may use <see cref="Teleport"/> or <see cref="Move"/>.
		/// </summary>
		Vector3 position { get; }

		/// <summary>
		/// Rotation of the agent.
		/// In world space.
		/// See: <see cref="position"/>
		/// </summary>
		Quaternion rotation { get; set; }

		/// <summary>Max speed in world units per second</summary>
		float maxSpeed { get; set; }

		/// <summary>
		/// Actual velocity that the agent is moving with.
		/// In world units per second.
		///
		/// See: <see cref="desiredVelocity"/>
		/// </summary>
		Vector3 velocity { get; }

		/// <summary>
		/// Velocity that this agent wants to move with.
		/// Includes gravity and local avoidance if applicable.
		/// In world units per second.
		///
		/// See: <see cref="velocity"/>
		///
		/// Note: The <see cref="Pathfinding.AILerp"/> movement script doesn't use local avoidance or gravity so this property will always be identical to <see cref="velocity"/> on that component.
		/// </summary>
		Vector3 desiredVelocity { get; }

		/// <summary>
		/// Velocity that this agent wants to move with before taking local avoidance into account.
		///
		/// Includes gravity.
		/// In world units per second.
		///
		/// Setting this property will set the current velocity that the agent is trying to move with, including gravity.
		/// This can be useful if you want to make the agent come to a complete stop in a single frame or if you want to modify the velocity in some way.
		///
		/// <code>
		/// // Set the velocity to zero, but keep the current gravity
		/// var newVelocity = new Vector3(0, ai.desiredVelocityWithoutLocalAvoidance.y, 0);
		///
		/// ai.desiredVelocityWithoutLocalAvoidance = newVelocity;
		/// </code>
		///
		/// Note: The <see cref="Pathfinding.AILerp"/> movement script doesn't use local avoidance so this property will always be identical to <see cref="velocity"/> on that component.
		///
		/// Warning: Trying to set this property on an AILerp component will throw an exception since its velocity cannot meaningfully be changed abitrarily.
		///
		/// If you are not using local avoidance then this property will in almost all cases be identical to <see cref="desiredVelocity"/> plus some noise due to floating point math.
		///
		/// See: <see cref="velocity"/>
		/// See: <see cref="desiredVelocity"/>
		/// See: <see cref="Move"/>
		/// See: <see cref="MovementUpdate"/>
		/// </summary>
		Vector3 desiredVelocityWithoutLocalAvoidance { get; set; }

		/// <summary>
		/// Approximate remaining distance along the current path to the end of the path.
		/// The RichAI movement script approximates this distance since it is quite expensive to calculate the real distance.
		/// However it will be accurate when the agent is within 1 corner of the destination.
		/// You can use <see cref="GetRemainingPath"/> to calculate the actual remaining path more precisely.
		///
		/// The AIPath and AILerp scripts use a more accurate distance calculation at all times.
		///
		/// If the agent does not currently have a path, then positive infinity will be returned.
		///
		/// Note: This is the distance to the end of the path, which may or may not be at the <see cref="destination"/>. If the character cannot reach the destination it will try to move as close as possible to it.
		///
		/// Warning: Since path requests are asynchronous, there is a small delay between a path request being sent and this value being updated with the new calculated path.
		///
		/// See: <see cref="reachedDestination"/>
		/// See: <see cref="reachedEndOfPath"/>
		/// See: <see cref="pathPending"/>
		/// </summary>
		float remainingDistance { get; }

		/// <summary>
		/// True if the ai has reached the <see cref="destination"/>.
		/// This is a best effort calculation to see if the <see cref="destination"/> has been reached.
		/// For the AIPath/RichAI scripts, this is when the character is within <see cref="AIPath.endReachedDistance"/> world units from the <see cref="destination"/>.
		/// For the AILerp script it is when the character is at the destination (±a very small margin).
		///
		/// This value will be updated immediately when the <see cref="destination"/> is changed (in contrast to <see cref="reachedEndOfPath)"/>, however since path requests are asynchronous
		/// it will use an approximation until it sees the real path result. What this property does is to check the distance to the end of the current path, and add to that the distance
		/// from the end of the path to the <see cref="destination"/> (i.e. is assumes it is possible to move in a straight line between the end of the current path to the destination) and then checks if that total
		/// distance is less than <see cref="AIPath.endReachedDistance"/>. This property is therefore only a best effort, but it will work well for almost all use cases.
		///
		/// Furthermore it will not report that the destination is reached if the destination is above the head of the character or more than half the <see cref="height"/> of the character below its feet
		/// (so if you have a multilevel building, it is important that you configure the <see cref="height"/> of the character correctly).
		///
		/// The cases which could be problematic are if an agent is standing next to a very thin wall and the destination suddenly changes to the other side of that thin wall.
		/// During the time that it takes for the path to be calculated the agent may see itself as alredy having reached the destination because the destination only moved a very small distance (the wall was thin),
		/// even though it may actually be quite a long way around the wall to the other side.
		///
		/// In contrast to <see cref="reachedEndOfPath"/>, this property is immediately updated when the <see cref="destination"/> is changed.
		///
		/// <code>
		/// IEnumerator Start () {
		///     ai.destination = somePoint;
		///     // Start to search for a path to the destination immediately
		///     ai.SearchPath();
		///     // Wait until the agent has reached the destination
		///     while (!ai.reachedDestination) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		///
		/// See: <see cref="AIPath.endReachedDistance"/>
		/// See: <see cref="remainingDistance"/>
		/// See: <see cref="reachedEndOfPath"/>
		/// </summary>
		bool reachedDestination { get; }

		/// <summary>
		/// True if the agent has reached the end of the current path.
		///
		/// Note that setting the <see cref="destination"/> does not immediately update the path, nor is there any guarantee that the
		/// AI will actually be able to reach the destination that you set. The AI will try to get as close as possible.
		/// Often you want to use <see cref="reachedDestination"/> instead which is easier to work with.
		///
		/// It is very hard to provide a method for detecting if the AI has reached the <see cref="destination"/> that works across all different games
		/// because the destination may not even lie on the navmesh and how that is handled differs from game to game (see also the code snippet in the docs for <see cref="destination"/>).
		///
		/// See: <see cref="remainingDistance"/>
		/// See: <see cref="reachedDestination"/>
		/// </summary>
		bool reachedEndOfPath { get; }

		/// <summary>
		/// End point of path the agent is currently following.
		/// If the agent has no path (or it might not be calculated yet), this will return the <see cref="destination"/> instead.
		/// If the agent has no destination it will return the agent's current position.
		///
		/// The end of the path is usually identical or very close to the <see cref="destination"/>, but it may differ
		/// if the path for example was blocked by a wall so that the agent couldn't get any closer.
		///
		/// This is only updated when the path is recalculated.
		/// </summary>
		Vector3 endOfPath { get; }

		/// <summary>
		/// Position in the world that this agent should move to.
		///
		/// If no destination has been set yet, then (+infinity, +infinity, +infinity) will be returned.
		///
		/// Note that setting this property does not immediately cause the agent to recalculate its path.
		/// So it may take some time before the agent starts to move towards this point.
		/// Most movement scripts have a repathRate field which indicates how often the agent looks
		/// for a new path. You can also call the <see cref="SearchPath"/> method to immediately
		/// start to search for a new path. Paths are calculated asynchronously so when an agent starts to
		/// search for path it may take a few frames (usually 1 or 2) until the result is available.
		/// During this time the <see cref="pathPending"/> property will return true.
		///
		/// If you are setting a destination and then want to know when the agent has reached that destination
		/// then you could either use <see cref="reachedDestination"/> (recommended) or check both <see cref="pathPending"/> and <see cref="reachedEndOfPath"/>.
		/// Check the documentation for the respective fields to learn about their differences.
		///
		/// <code>
		/// IEnumerator Start () {
		///     ai.destination = somePoint;
		///     // Start to search for a path to the destination immediately
		///     ai.SearchPath();
		///     // Wait until the agent has reached the destination
		///     while (!ai.reachedDestination) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		/// <code>
		/// IEnumerator Start () {
		///     ai.destination = somePoint;
		///     // Start to search for a path to the destination immediately
		///     // Note that the result may not become available until after a few frames
		///     // ai.pathPending will be true while the path is being calculated
		///     ai.SearchPath();
		///     // Wait until we know for sure that the agent has calculated a path to the destination we set above
		///     while (ai.pathPending || !ai.reachedEndOfPath) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		/// </summary>
		Vector3 destination { get; set; }

		/// <summary>
		/// Enables or disables recalculating the path at regular intervals.
		/// Setting this to false does not stop any active path requests from being calculated or stop it from continuing to follow the current path.
		///
		/// Note that this only disables automatic path recalculations. If you call the <see cref="SearchPath()"/> method a path will still be calculated.
		///
		/// See: <see cref="canMove"/>
		/// See: <see cref="isStopped"/>
		/// </summary>
		bool canSearch { get; set; }

		/// <summary>
		/// Enables or disables movement completely.
		/// If you want the agent to stand still, but still react to local avoidance and use gravity: use <see cref="isStopped"/> instead.
		///
		/// This is also useful if you want to have full control over when the movement calculations run.
		/// Take a look at <see cref="MovementUpdate"/>
		///
		/// See: <see cref="canSearch"/>
		/// See: <see cref="isStopped"/>
		/// </summary>
		bool canMove { get; set; }

		/// <summary>True if this agent currently has a path that it follows</summary>
		bool hasPath { get; }

		/// <summary>True if a path is currently being calculated</summary>
		bool pathPending { get; }

		/// <summary>
		/// Determines if the character's position should be coupled to the Transform's position.
		/// If false then all movement calculations will happen as usual, but the GameObject that this component is attached to will not move.
		/// Instead, only the <see cref="position"/> property will change.
		///
		/// This is useful if you want to control the movement of the character using some other means, such
		/// as root motion, but still want the AI to move freely.
		///
		/// See: <see cref="canMove"/> which in contrast to this field will disable all movement calculations.
		/// See: <see cref="updateRotation"/>
		/// </summary>
		bool updatePosition { get; set; }

		/// <summary>
		/// Determines if the character's rotation should be coupled to the Transform's rotation.
		/// If false then all movement calculations will happen as usual, but the GameObject that this component is attached to will not rotate.
		/// Instead, only the <see cref="rotation"/> property will change.
		///
		/// This is particularly useful for 2D games where you want the Transform to stay in the same orientation, and instead swap out the displayed
		/// sprite to indicate the direction the character is facing.
		///
		/// See: <see cref="updatePosition"/>
		/// See: <see cref="rotation"/>
		/// </summary>
		bool updateRotation { get; set; }

		/// <summary>
		/// Gets or sets if the agent should stop moving.
		/// If this is set to true the agent will immediately start to slow down as quickly as it can to come to a full stop.
		/// The agent will still react to local avoidance and gravity (if applicable), but it will not try to move in any particular direction.
		///
		/// The current path of the agent will not be cleared, so when this is set
		/// to false again the agent will continue moving along the previous path.
		///
		/// This is a purely user-controlled parameter, so for example it is not set automatically when the agent stops
		/// moving because it has reached the target. Use <see cref="reachedEndOfPath"/> for that.
		///
		/// If this property is set to true while the agent is traversing an off-mesh link (RichAI script only), then the agent will
		/// continue traversing the link and stop once it has completed it.
		///
		/// Note: This is not the same as the <see cref="canMove"/> setting which some movement scripts have. The <see cref="canMove"/> setting
		/// disables movement calculations completely (which among other things makes it not be affected by local avoidance or gravity).
		/// For the AILerp movement script which doesn't use gravity or local avoidance anyway changing this property is very similar to
		/// changing <see cref="canMove"/>.
		///
		/// The <see cref="steeringTarget"/> property will continue to indicate the point which the agent would move towards if it would not be stopped.
		/// </summary>
		bool isStopped { get; set; }

		/// <summary>
		/// Point on the path which the agent is currently moving towards.
		/// This is usually a point a small distance ahead of the agent
		/// or the end of the path.
		///
		/// If the agent does not have a path at the moment, then the agent's current position will be returned.
		/// </summary>
		Vector3 steeringTarget { get; }

		/// <summary>
		/// Called when the agent recalculates its path.
		/// This is called both for automatic path recalculations (see <see cref="canSearch)"/> and manual ones (see <see cref="SearchPath)"/>.
		///
		/// See: Take a look at the <see cref="Pathfinding.AIDestinationSetter"/> source code for an example of how it can be used.
		/// </summary>
		System.Action onSearchPath { get; set; }

		/// <summary>
		/// The plane the agent is moving in.
		///
		/// This is typically the ground plane, which will be the XZ plane in a 3D game, and the XY plane in a 2D game.
		/// Ultimately it depends on the graph orientation.
		///
		/// If you are doing pathfinding on a spherical world (see spherical) (view in online documentation for working links), the the movement plane will be the tangent plane of the sphere at the agent's position.
		/// </summary>
		NativeMovementPlane movementPlane { get; }
		/// <summary>
		/// Fills buffer with the remaining path.
		///
		/// <code>
		/// var buffer = new List<Vector3>();
		///
		/// ai.GetRemainingPath(buffer, out bool stale);
		/// for (int i = 0; i < buffer.Count - 1; i++) {
		///     Debug.DrawLine(buffer[i], buffer[i+1], Color.red);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="buffer">The buffer will be cleared and replaced with the path. The first point is the current position of the agent.</param>
		/// <param name="stale">May be true if the path is invalid in some way. For example if the agent has no path or (for the RichAI/FollowerEntity components only) if the agent has detected that some nodes in the path have been destroyed.</param>
		void GetRemainingPath(List<Vector3> buffer, out bool stale);

		/// <summary>
		/// Fills buffer with the remaining path.
		///
		/// <code>
		/// var buffer = new List<Vector3>();
		/// var parts = new List<PathPartWithLinkInfo>();
		///
		/// ai.GetRemainingPath(buffer, parts, out bool stale);
		/// foreach (var part in parts) {
		///     for (int i = part.startIndex; i < part.endIndex; i++) {
		///         Debug.DrawLine(buffer[i], buffer[i+1], part.type == Funnel.PartType.NodeSequence ? Color.red : Color.green);
		///     }
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: The <see cref="AIPath"/> and <see cref="AILerp"/> movement scripts do not know about off-mesh links, so the partsBuffer will always be filled with a single node-sequence part.
		/// </summary>
		/// <param name="buffer">The buffer will be cleared and replaced with the path. The first point is the current position of the agent.</param>
		/// <param name="partsBuffer">If not null, this list will be cleared and filled with information about the different parts of the path. A part is a sequence of nodes or an off-mesh link.</param>
		/// <param name="stale">May be true if the path is invalid in some way. For example if the agent has no path or (for the RichAI/FollowerEntity components only) if the agent has detected that some nodes in the path have been destroyed.</param>
		void GetRemainingPath(List<Vector3> buffer, List<PathPartWithLinkInfo> partsBuffer, out bool stale);

		/// <summary>
		/// Recalculate the current path.
		/// You can for example use this if you want very quick reaction times when you have changed the <see cref="destination"/>
		/// so that the agent does not have to wait until the next automatic path recalculation (see <see cref="canSearch)"/>.
		///
		/// If there is an ongoing path calculation, it will be canceled, so make sure you leave time for the paths to get calculated before calling this function again.
		/// A canceled path will show up in the log with the message "Canceled by script" (see <see cref="Seeker.CancelCurrentPathRequest"/>).
		///
		/// If no <see cref="destination"/> has been set yet then nothing will be done.
		///
		/// Note: The path result may not become available until after a few frames.
		/// During the calculation time the <see cref="pathPending"/> property will return true.
		///
		/// See: <see cref="pathPending"/>
		/// </summary>
		void SearchPath();

		/// <summary>
		/// Make the AI follow the specified path.
		///
		/// In case the path has not been calculated, the script will call seeker.StartPath to calculate it.
		/// This means the AI may not actually start to follow the path until in a few frames when the path has been calculated.
		/// The <see cref="pathPending"/> field will as usual return true while the path is being calculated.
		///
		/// In case the path has already been calculated it will immediately replace the current path the AI is following.
		/// This is useful if you want to replace how the AI calculates its paths.
		///
		/// If you pass null as a parameter then the current path will be cleared and the agent will stop moving.
		/// Note than unless you have also disabled <see cref="canSearch"/> then the agent will soon recalculate its path and start moving again.
		///
		/// You can disable the automatic path recalculation by setting the <see cref="canSearch"/> field to false.
		///
		/// Note: This call will be ignored if the agent is currently traversing an off-mesh link. Furthermore, if the agent starts traversing an off-mesh link, the current path request will be canceled (if one is currently in progress).
		///
		/// <code>
		/// // Disable the automatic path recalculation
		/// ai.canSearch = false;
		/// var pointToAvoid = enemy.position;
		/// // Make the AI flee from the enemy.
		/// // The path will be about 20 world units long (the default cost of moving 1 world unit is 1000).
		/// var path = FleePath.Construct(ai.position, pointToAvoid, 1000 * 20);
		/// ai.SetPath(path);
		/// </code>
		/// </summary>
		/// <param name="path">The path to follow.</param>
		/// <param name="updateDestinationFromPath">If true, the \reflink{destination} property will be set to the end point of the path. If false, the previous destination value will be kept.</param>
		void SetPath(Path path, bool updateDestinationFromPath = true);

		/// <summary>
		/// Instantly move the agent to a new position.
		/// This will trigger a path recalculation (if clearPath is true, which is the default) so if you want to teleport the agent and change its <see cref="destination"/>
		/// it is recommended that you set the <see cref="destination"/> before calling this method.
		///
		/// The current path will be cleared by default.
		///
		/// This method is preferred for long distance teleports. If you only move the agent a very small distance (so that it is reasonable that it can keep its current path),
		/// then setting the <see cref="position"/> property is preferred.
		/// When using the <see cref="FollowerEntity"/> movement script, setting the <see cref="position"/> property when using a long distance teleport could cause the agent to fail to move the full distance, as it can get blocked by the navmesh.
		///
		/// See: Works similarly to Unity's NavmeshAgent.Warp.
		/// See: <see cref="SearchPath"/>
		/// </summary>
		void Teleport(Vector3 newPosition, bool clearPath = true);

		/// <summary>
		/// Move the agent.
		///
		/// This is intended for external movement forces such as those applied by wind, conveyor belts, knockbacks etc.
		///
		/// Some movement scripts may ignore this completely (notably the <see cref="AILerp"/> script) if it does not have
		/// any concept of being moved externally.
		///
		/// For the <see cref="AIPath"/> and <see cref="RichAI"/> movement scripts, the agent will not be moved immediately when calling this method. Instead this offset will be stored and then
		/// applied the next time the agent runs its movement calculations (which is usually later this frame or the next frame).
		/// If you want to move the agent immediately then call:
		/// <code>
		/// ai.Move(someVector);
		/// ai.FinalizeMovement(ai.position, ai.rotation);
		/// </code>
		///
		/// The <see cref="FollowerEntity"/> movement script will, on the other hand, move the agent immediately.
		/// </summary>
		/// <param name="deltaPosition">Direction and distance to move the agent in world space.</param>
		void Move(Vector3 deltaPosition);

		/// <summary>
		/// Calculate how the character wants to move during this frame.
		///
		/// Note that this does not actually move the character. You need to call <see cref="FinalizeMovement"/> for that.
		/// This is called automatically unless <see cref="canMove"/> is false.
		///
		/// To handle movement yourself you can disable <see cref="canMove"/> and call this method manually.
		/// This code will replicate the normal behavior of the component:
		/// <code>
		/// void Update () {
		///     // Disable the AIs own movement code
		///     ai.canMove = false;
		///     Vector3 nextPosition;
		///     Quaternion nextRotation;
		///     // Calculate how the AI wants to move
		///     ai.MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);
		///     // Modify nextPosition and nextRotation in any way you wish
		///     // Actually move the AI
		///     ai.FinalizeMovement(nextPosition, nextRotation);
		/// }
		/// </code>
		/// </summary>
		/// <param name="deltaTime">time to simulate movement for. Usually set to Time.deltaTime.</param>
		/// <param name="nextPosition">the position that the agent wants to move to during this frame.</param>
		/// <param name="nextRotation">the rotation that the agent wants to rotate to during this frame.</param>
		void MovementUpdate(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);

		/// <summary>
		/// Move the agent.
		/// To be called as the last step when you are handling movement manually.
		///
		/// The movement will be clamped to the navmesh if applicable (this is done for the RichAI movement script).
		///
		/// See: <see cref="MovementUpdate"/> for a code example.
		/// </summary>
		void FinalizeMovement(Vector3 nextPosition, Quaternion nextRotation);
	}
}

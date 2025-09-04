using UnityEngine;
using UnityEngine.Profiling;

namespace Pathfinding.RVO {
	using Pathfinding.Util;
	using Pathfinding.Drawing;

	/// <summary>
	/// Unity front end for an RVO simulator.
	/// Attached to any GameObject in a scene, scripts such as the RVOController will use the
	/// simulator exposed by this class to handle their movement.
	/// In pretty much all cases you should only have a single RVOSimulator in the scene.
	///
	/// You can have more than one of these, however most scripts which make use of the RVOSimulator
	/// will use the <see cref="active"/> property which just returns the first simulator in the scene.
	///
	/// This is only a wrapper class for a Pathfinding.RVO.Simulator which simplifies exposing it
	/// for a unity scene.
	///
	/// See: Pathfinding.RVO.Simulator
	/// See: local-avoidance (view in online documentation for working links)
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Simulator")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rvosimulator.html")]
	public class RVOSimulator : VersionedMonoBehaviour {
		/// <summary>First RVOSimulator in the scene (usually there is only one)</summary>
		public static RVOSimulator active { get; private set; }

		/// <summary>
		/// Desired FPS for rvo simulation.
		/// It is usually not necessary to run a crowd simulation at a very high fps.
		/// Usually 10-30 fps is enough, but it can be increased for better quality.
		/// The rvo simulation will never run at a higher fps than the game
		/// </summary>
		[Tooltip("Desired FPS for rvo simulation. It is usually not necessary to run a crowd simulation at a very high fps.\n" +
			"Usually 10-30 fps is enough, but can be increased for better quality.\n"+
			"The rvo simulation will never run at a higher fps than the game")]
		public int desiredSimulationFPS = 20;

		/// <summary>
		/// Number of RVO worker threads.
		/// If set to None, no multithreading will be used.
		/// Using multithreading can significantly improve performance by offloading work to other CPU cores.
		///
		/// Deprecated: The number of worker threads is now set by the unity job system.
		/// </summary>
		[Tooltip("Number of RVO worker threads. If set to None, no multithreading will be used.")]
		[System.Obsolete("The number of worker threads is now set by the unity job system", true)]
		public ThreadCount workerThreads = ThreadCount.Two;

		/// <summary>
		/// Calculate local avoidance in between frames.
		/// If this is enabled and multithreading is used, the local avoidance calculations will continue to run
		/// until the next frame instead of waiting for them to be done the same frame. This can increase the performance
		/// but it can make the agents seem a little less responsive.
		///
		/// This will only be read at Awake.
		/// See: Pathfinding.RVO.Simulator.DoubleBuffering
		///
		/// Deprecated: Double buffering has been removed
		/// </summary>
		[Tooltip("Calculate local avoidance in between frames.\nThis can increase jitter in the agents' movement so use it only if you really need the performance boost. " +
			"It will also reduce the responsiveness of the agents to the commands you send to them.")]
		[System.Obsolete("Double buffering has been removed")]
		public bool doubleBuffering;

		/// <summary>
		/// Prevent agent overlap more aggressively.
		/// This will it much harder for agents to overlap, even in crowded scenarios.
		/// It is particularly noticable when running at a low simulation fps.
		/// This does not influence agent avoidance when the agents are not overlapping.
		///
		/// Enabling this has a small performance penalty, usually not high enough to care about.
		///
		/// Disabling this may be beneficial if you want softer behaviour when larger groups of agents collide.
		/// </summary>
		public bool hardCollisions = true;

		/// <summary>\copydoc Pathfinding::RVO::SimulatorBurst::SymmetryBreakingBias</summary>
		[Tooltip("Bias agents to pass each other on the right side.\n" +
			"If the desired velocity of an agent puts it on a collision course with another agent or an obstacle " +
			"its desired velocity will be rotated this number of radians (1 radian is approximately 57°) to the right. " +
			"This helps to break up symmetries and makes it possible to resolve some situations much faster.\n\n" +
			"When many agents have the same goal this can however have the side effect that the group " +
			"clustered around the target point may as a whole start to spin around the target point.")]
		[Range(0, 0.2f)]
		public float symmetryBreakingBias = 0.1f;

		/// <summary>
		/// Determines if the XY (2D) or XZ (3D) plane is used for movement.
		/// For 2D games you would set this to XY and for 3D games you would usually set it to XZ.
		/// </summary>
		[Tooltip("Determines if the XY (2D) or XZ (3D) plane is used for movement")]
		public MovementPlane movementPlane = MovementPlane.XZ;

		/// <summary>
		/// Allows the local avoidance system to take the edges of the navmesh into account.
		///
		/// This will make agents try to avoid moving into, and getting pushed into the borders of the navmesh.
		///
		/// This works best on navmesh/recast graphs, but can also be used on grid graphs.
		///
		/// Enabling this has a performance impact.
		///
		/// Note: For the <see cref="AIPath"/> movement script, this only has an effect if the <see cref="AIPath.constrainInsideGraph"/> field is enabled.
		///
		/// If you are writing your own movement script, you must call <see cref="RVOController.SetObstacleQuery"/> every frame for the navmesh obstacle detection to work.
		/// </summary>
		public bool useNavmeshAsObstacle = false;
		public bool drawQuadtree;

		/// <summary>Reference to the internal simulator</summary>
		Pathfinding.RVO.SimulatorBurst simulatorBurst;

		/// <summary>
		/// Get the internal simulator.
		/// Will never be null when the game is running
		/// </summary>
		public SimulatorBurst GetSimulator () {
			if (simulatorBurst == null && Application.isPlaying) {
				simulatorBurst = new Pathfinding.RVO.SimulatorBurst(movementPlane);
			}
			return simulatorBurst;
		}

		void OnEnable () {
			if (active != null) {
				if (active != this && Application.isPlaying) {
					if (enabled) Debug.LogWarning("Another RVOSimulator component is already in the scene. More than one RVOSimulator component cannot be active at the same time. Disabling this one.", this);
					enabled = false;
				}
				return;
			}

			active = this;
		}

		/// <summary>Update the simulation</summary>
		void Update () {
			if (!Application.isPlaying) return;

			if (desiredSimulationFPS < 1) desiredSimulationFPS = 1;

			var sim = GetSimulator();
			sim.DesiredDeltaTime = 1.0f / desiredSimulationFPS;
			sim.SymmetryBreakingBias = symmetryBreakingBias;
			sim.HardCollisions = hardCollisions;
			sim.drawQuadtree = drawQuadtree;
			sim.UseNavmeshAsObstacle = useNavmeshAsObstacle;
#if !MODULE_ENTITIES
			sim.Update(default, Time.deltaTime, true, Unity.Collections.Allocator.TempJob).Complete();
#endif
		}

		void OnDisable () {
			if (active == this) {
				active = null;
			}
			if (simulatorBurst != null) {
				simulatorBurst.OnDestroy();
				simulatorBurst = null;
			}
		}

		// static Color ObstacleColor = new Color(255/255f, 60/255f, 15/255f, 1.0f);
		public override void DrawGizmos () {
			// Prevent interfering with scene view picking
			//if (Event.current.type != EventType.Repaint) return;
		}
	}
}

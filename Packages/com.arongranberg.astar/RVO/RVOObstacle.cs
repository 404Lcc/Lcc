using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding.RVO {
	using Pathfinding.Drawing;

	/// <summary>
	/// Base class for simple RVO colliders.
	///
	/// This is a helper base class for RVO colliders. It provides automatic gizmos
	/// and helps with the winding order of the vertices as well as automatically updating the obstacle when moved.
	///
	/// Extend this class to create custom RVO obstacles.
	///
	/// See: RVOSquareObstacle
	///
	/// Deprecated: This component is deprecated. Local avoidance colliders never worked particularly well and the performance was poor. Modify the graphs instead so that pathfinding takes obstacles into account.
	/// </summary>
	public abstract class RVOObstacle : VersionedMonoBehaviour {
		/// <summary>
		/// Mode of the obstacle.
		/// Determines winding order of the vertices
		/// </summary>
		public ObstacleVertexWinding obstacleMode;

		public RVOLayer layer = RVOLayer.DefaultObstacle;

		/// <summary>
		/// RVO Obstacle Modes.
		/// Determines winding order of obstacle vertices
		/// </summary>
		public enum ObstacleVertexWinding {
			/// <summary>Keeps agents from entering the obstacle</summary>
			KeepOut,
			/// <summary>Keeps agents inside the obstacle</summary>
			KeepIn,
		}

		/// <summary>
		/// Enable executing in editor to draw gizmos.
		/// If enabled, the CreateObstacles function will be executed in the editor as well
		/// in order to draw gizmos.
		/// </summary>
		protected abstract bool ExecuteInEditor { get; }

		/// <summary>If enabled, all coordinates are handled as local.</summary>
		protected abstract bool LocalCoordinates { get; }

		/// <summary>
		/// Static or dynamic.
		/// This determines if the obstacle can be updated by e.g moving the transform
		/// around in the scene.
		/// </summary>
		protected abstract bool StaticObstacle { get; }

		protected abstract float Height { get; }
	}
}

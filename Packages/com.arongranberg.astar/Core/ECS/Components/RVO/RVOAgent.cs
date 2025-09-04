#if MODULE_ENTITIES
using Pathfinding.RVO;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

namespace Pathfinding.ECS.RVO {
	using Pathfinding.RVO;

	/// <summary>
	/// Agent data for the local avoidance system.
	///
	/// See: local-avoidance (view in online documentation for working links)
	/// </summary>
	[System.Serializable]
	public struct RVOAgent : IComponentData {
		/// <summary>How far into the future to look for collisions with other agents (in seconds)</summary>
		[Tooltip("How far into the future to look for collisions with other agents (in seconds)")]
		public float agentTimeHorizon;

		/// <summary>How far into the future to look for collisions with obstacles (in seconds)</summary>
		[Tooltip("How far into the future to look for collisions with obstacles (in seconds)")]
		public float obstacleTimeHorizon;

		/// <summary>
		/// Max number of other agents to take into account.
		/// A smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.
		/// </summary>
		[Tooltip("Max number of other agents to take into account.\n" +
			"A smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.")]
		public int maxNeighbours;

		/// <summary>
		/// Specifies the avoidance layer for this agent.
		/// The <see cref="collidesWith"/> mask on other agents will determine if they will avoid this agent.
		/// </summary>
		public RVOLayer layer;

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
		public RVOLayer collidesWith;

		/// <summary>\copydocref{Pathfinding.RVO.IAgent.Priority}</summary>
		[Tooltip("How strongly other agents will avoid this agent")]
		[UnityEngine.Range(0, 1)]
		public float priority;

		/// <summary>
		/// Priority multiplier.
		/// This functions identically to the <see cref="priority"/>, however it is not exposed in the Unity inspector.
		/// It is primarily used by the <see cref="Pathfinding.RVO.RVODestinationCrowdedBehavior"/>.
		/// </summary>
		[System.NonSerialized]
		public float priorityMultiplier;

		[System.NonSerialized]
		public float flowFollowingStrength;

		/// <summary>Enables drawing debug information in the scene view</summary>
		public AgentDebugFlags debug;

		/// <summary>A locked unit cannot move. Other units will still avoid it but avoidance quality is not the best.</summary>
		[Tooltip("A locked unit cannot move. Other units will still avoid it. But avoidance quality is not the best")]
		public bool locked;

		/// <summary>Good default settings for an RVO agent</summary>
		public static readonly RVOAgent Default = new RVOAgent {
			locked = false,
			agentTimeHorizon = 1.0f,
			obstacleTimeHorizon = 0.5f,
			maxNeighbours = 10,
			layer = RVOLayer.DefaultAgent,
			collidesWith = (RVOLayer)(-1),
			priority = 0.5f,
			priorityMultiplier = 1.0f,
			flowFollowingStrength = 0.0f,
			debug = AgentDebugFlags.Nothing,
		};
	}
}
#endif

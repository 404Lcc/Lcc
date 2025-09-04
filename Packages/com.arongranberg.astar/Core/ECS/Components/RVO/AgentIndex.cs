#if MODULE_ENTITIES
using Unity.Entities;
#endif

namespace Pathfinding.ECS.RVO {
	using Pathfinding.RVO;
	using Unity.Collections;

	/// <summary>
	/// Index of an RVO agent in the local avoidance simulation.
	///
	/// If this component is present, that indicates that the agent is part of a local avoidance simulation.
	/// The <see cref="RVOSystem"/> is responsible for adding and removing this component as necessary.
	/// Any other systems should only concern themselves with the <see cref="RVOAgent"/> component.
	///
	/// Warning: This component does not support cloning. You must not clone entities that use this component.
	/// There doesn't seem to be any way to make this work with the Unity.Entities API at the moment.
	/// </summary>
#if MODULE_ENTITIES
	[WriteGroup(typeof(ResolvedMovement))]
#endif
	public readonly struct AgentIndex
#if MODULE_ENTITIES
		: Unity.Entities.ICleanupComponentData
#endif
	{
		const int DeletedBit = 1 << 31;
		const int IndexMask = (1 << 24) - 1;
		const int VersionOffset = 24;
		const int VersionMask = 0b1111_111 << VersionOffset;

		readonly int packedAgentIndex;

		/// <summary>
		/// Index of the agent in the simulation's data arrays.
		///
		/// See: <see cref="TryGetIndex"/>
		/// </summary>
		internal int Index => packedAgentIndex & IndexMask;
		int Version => packedAgentIndex & VersionMask;
		internal bool Valid => (packedAgentIndex & DeletedBit) == 0;

		internal AgentIndex(int packedAgentIndex) {
			this.packedAgentIndex = packedAgentIndex;
		}

		internal AgentIndex(int version, int index) {
			version <<= VersionOffset;
			UnityEngine.Assertions.Assert.IsTrue((index & IndexMask) == index);
			packedAgentIndex = (version & VersionMask) | (index & IndexMask);
		}

		internal readonly AgentIndex WithIncrementedVersion () {
			return new AgentIndex((((packedAgentIndex & VersionMask) + (1 << VersionOffset)) & VersionMask) | Index);
		}

		internal readonly AgentIndex WithDeleted () {
			return new AgentIndex(packedAgentIndex | DeletedBit);
		}

		/// <summary>True if the agent exists in the simulation</summary>
		public readonly bool Exists (ref SimulatorBurst.AgentData agentData) {
			return TryGetIndex(ref agentData, out _);
		}

		/// <summary>
		/// Returns the index of the agent in the simulation's data arrays, if the agent exists.
		///
		/// If the agent does not exist, the index will be set to -1 and the method returns false.
		/// </summary>
		public readonly bool TryGetIndex (ref SimulatorBurst.AgentData agentData, out int index) {
			return TryGetIndex(ref agentData.version, out index);
		}

		/// <summary>
		/// Returns the index of the agent in the simulation's data arrays, if the agent exists.
		///
		/// If the agent does not exist, the index will be set to -1 and the method returns false.
		/// </summary>
		public readonly bool TryGetIndex (ref NativeArray<AgentIndex> agentDataVersions, out int index) {
			var tmpIndex = Index;
			index = -1;
			if (!agentDataVersions.IsCreated) return false;
			if (tmpIndex >= agentDataVersions.Length) return false;
			if (agentDataVersions[tmpIndex].Version != Version) return false;

			index = tmpIndex;
			return true;
		}
	}
}

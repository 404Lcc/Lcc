using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Pathfinding.Jobs;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Calculates the default node positions for a grid graph.
	///
	/// The node positions will lie on the base plane of the grid graph.
	///
	/// See: <see cref="GridGraph.CalculateTransform"/>
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobNodeGridLayout : IJob, GridIterationUtilities.ICellAction {
		public Matrix4x4 graphToWorld;
		public IntBounds bounds;

		[WriteOnly]
		public NativeArray<Vector3> nodePositions;

		public static Vector3 NodePosition (Matrix4x4 graphToWorld, int x, int z, float height = 0) {
			return graphToWorld.MultiplyPoint3x4(new Vector3(x + 0.5f, height, z + 0.5f));
		}

		public void Execute () {
			GridIterationUtilities.ForEachCellIn3DArray(bounds.size, ref this);
		}

		public void Execute (uint innerIndex, int x, int y, int z) {
			nodePositions[(int)innerIndex] = NodePosition(graphToWorld, x + bounds.min.x, z + bounds.min.z);
		}
	}
}

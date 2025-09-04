namespace Pathfinding.Graphs.Grid.Rules {
	using Pathfinding.Jobs;
	using Unity.Jobs;
	using Unity.Collections;
	using Unity.Burst;
	using UnityEngine;
	using Unity.Mathematics;

	/// <summary>
	/// Applies penalty based on the elevation of the node.
	///
	/// This is useful if you for example want to discourage agents from walking high up in mountain regions.
	///
	/// The penalty applied is equivalent to:
	///
	/// <code>
	/// penalty = curve.evaluate(Mathf.Clamp01(Mathf.InverseLerp(lower elevation range, upper elevation range, elevation))) * penaltyScale
	/// </code>
	///
	/// [Open online documentation to see images]
	///
	/// See: grid-rules (view in online documentation for working links)
	/// </summary>
	[Pathfinding.Util.Preserve]
	public class RuleElevationPenalty : GridGraphRule {
		public float penaltyScale = 10000;
		public Vector2 elevationRange = new Vector2(0, 100);
		public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
		NativeArray<float> elevationToPenalty;

		public override void Register (GridGraphRules rules) {
			if (!elevationToPenalty.IsCreated) elevationToPenalty = new NativeArray<float>(64, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < elevationToPenalty.Length; i++) {
				elevationToPenalty[i] = Mathf.Max(0, penaltyScale * curve.Evaluate(i * 1.0f / (elevationToPenalty.Length - 1)));
			}

			var clampedElevationRange = new Vector2(math.max(0, elevationRange.x), math.max(1, elevationRange.y));
			rules.AddJobSystemPass(Pass.BeforeConnections, context => {
				//var elevationRangeScale = Matrix4x4.TRS(new Vector3(0, -clampedElevationRange.x, 0), Quaternion.identity, new Vector3(1, 1/(clampedElevationRange.y - clampedElevationRange.x), 1));
				var elevationRangeScale = Matrix4x4.Scale(new Vector3(1, 1/(clampedElevationRange.y - clampedElevationRange.x), 1)) * Matrix4x4.Translate(new Vector3(0, -clampedElevationRange.x, 0));
				new JobElevationPenalty {
					elevationToPenalty = elevationToPenalty,
					nodePositions = context.data.nodes.positions,
					worldToGraph = elevationRangeScale * context.data.transform.matrix.inverse,
					penalty = context.data.nodes.penalties,
				}.Schedule(context.tracker);
			});
		}

		public override void DisposeUnmanagedData () {
			if (elevationToPenalty.IsCreated) elevationToPenalty.Dispose();
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		public struct JobElevationPenalty : IJob {
			[ReadOnly]
			public NativeArray<float> elevationToPenalty;

			[ReadOnly]
			public NativeArray<Vector3> nodePositions;

			public Matrix4x4 worldToGraph;
			public NativeArray<uint> penalty;

			public void Execute () {
				for (int i = 0; i < penalty.Length; i++) {
					float y = math.clamp(worldToGraph.MultiplyPoint3x4(nodePositions[i]).y, 0, 1) * (elevationToPenalty.Length - 1);
					int iy = (int)y;
					float p1 = elevationToPenalty[iy];
					float p2 = elevationToPenalty[math.min(iy + 1, elevationToPenalty.Length - 1)];
					penalty[i] += (uint)math.lerp(p1, p2, y - iy);
				}
			}
		}
	}
}

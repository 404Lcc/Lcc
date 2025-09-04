namespace Pathfinding.Graphs.Grid.Rules {
	using Pathfinding.Jobs;
	using Unity.Jobs;
	using Unity.Collections;
	using Unity.Burst;
	using UnityEngine;
	using Unity.Mathematics;

	/// <summary>
	/// Applies penalty based on the slope of the surface below the node.
	///
	/// This is useful if you for example want to discourage agents from walking on steep slopes.
	///
	/// The penalty applied is equivalent to:
	///
	/// <code>
	/// penalty = curve.evaluate(slope angle in degrees) * penaltyScale
	/// </code>
	///
	/// [Open online documentation to see images]
	///
	/// See: grid-rules (view in online documentation for working links)
	/// </summary>
	[Pathfinding.Util.Preserve]
	public class RuleAnglePenalty : GridGraphRule {
		public float penaltyScale = 10000;
		public AnimationCurve curve = AnimationCurve.Linear(0, 0, 90, 1);
		NativeArray<float> angleToPenalty;

		public override void Register (GridGraphRules rules) {
			if (!angleToPenalty.IsCreated) angleToPenalty = new NativeArray<float>(32, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < angleToPenalty.Length; i++) {
				angleToPenalty[i] = Mathf.Max(0, curve.Evaluate(90.0f * i / (angleToPenalty.Length - 1)) * penaltyScale);
			}

			rules.AddJobSystemPass(Pass.BeforeConnections, context => {
				new JobPenaltyAngle {
					angleToPenalty = angleToPenalty,
					up = context.data.up,
					nodeNormals = context.data.nodes.normals,
					penalty = context.data.nodes.penalties,
				}.Schedule(context.tracker);
			});
		}

		public override void DisposeUnmanagedData () {
			if (angleToPenalty.IsCreated) angleToPenalty.Dispose();
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		public struct JobPenaltyAngle : IJob {
			public Vector3 up;

			[ReadOnly]
			public NativeArray<float> angleToPenalty;

			[ReadOnly]
			public NativeArray<float4> nodeNormals;

			public NativeArray<uint> penalty;

			public void Execute () {
				float4 up = new float4(this.up.x, this.up.y, this.up.z, 0);

				for (int i = 0; i < penalty.Length; i++) {
					float4 normal = nodeNormals[i];
					if (math.any(normal)) {
						float angle = math.acos(math.dot(normal, up));
						// Take the dot product to find out the cosinus of the angle it has
						// Add penalty based on the angle from a precalculated array
						float x = angle*(angleToPenalty.Length - 1)/math.PI;
						int ix = (int)x;
						float p1 = angleToPenalty[math.max(ix, 0)];
						float p2 = angleToPenalty[math.min(ix + 1, angleToPenalty.Length - 1)];
						penalty[i] += (uint)math.lerp(p1, p2, x - ix);
					}
				}
			}
		}
	}
}

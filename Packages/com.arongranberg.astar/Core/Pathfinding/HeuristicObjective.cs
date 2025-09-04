using Unity.Mathematics;
using Unity.Burst;
using Pathfinding.Util;
using Pathfinding.Graphs.Util;
using Pathfinding.Collections;

namespace Pathfinding {
	/// <summary>
	/// Calculates an estimated cost from the specified point to the target.
	///
	/// See: https://en.wikipedia.org/wiki/A*_search_algorithm
	/// </summary>
	[BurstCompile]
	public readonly struct HeuristicObjective {
		readonly int3 mn;
		readonly int3 mx;
		readonly Heuristic heuristic;
		readonly float heuristicScale;
		readonly UnsafeSpan<uint> euclideanEmbeddingCosts;
		readonly uint euclideanEmbeddingPivots;
		readonly uint targetNodeIndex;

		public bool hasHeuristic => heuristic != Heuristic.None;

		public HeuristicObjective (int3 point, Heuristic heuristic, float heuristicScale) {
			this.mn = this.mx = point;
			this.heuristic = heuristic;
			this.heuristicScale = heuristicScale;
			this.euclideanEmbeddingCosts = default;
			this.euclideanEmbeddingPivots = 0;
			this.targetNodeIndex = 0;
		}

		public HeuristicObjective (int3 point, Heuristic heuristic, float heuristicScale, uint targetNodeIndex, EuclideanEmbedding euclideanEmbedding) {
			this.mn = this.mx = point;
			this.heuristic = heuristic;
			this.heuristicScale = heuristicScale;
			// The euclidean embedding costs are guaranteed to be valid for the duration of the pathfinding request.
			// We cannot perform checks here, because we may be running in another thread, and Unity does not like that.
			this.euclideanEmbeddingCosts = euclideanEmbedding != null? euclideanEmbedding.costs.AsUnsafeSpanNoChecks() : default;
			this.euclideanEmbeddingPivots = euclideanEmbedding != null ? (uint)euclideanEmbedding.pivotCount : 0;
			this.targetNodeIndex = targetNodeIndex;
		}

		public HeuristicObjective (int3 mn, int3 mx, Heuristic heuristic, float heuristicScale, uint targetNodeIndex, EuclideanEmbedding euclideanEmbedding) {
			this.mn = mn;
			this.mx = mx;
			this.heuristic = heuristic;
			this.heuristicScale = heuristicScale;
			// The euclidean embedding costs are guaranteed to be valid for the duration of the pathfinding request.
			// We cannot perform checks here, because we may be running in another thread, and Unity does not like that.
			this.euclideanEmbeddingCosts = euclideanEmbedding != null? euclideanEmbedding.costs.AsUnsafeSpanNoChecks() : default;
			this.euclideanEmbeddingPivots = euclideanEmbedding != null ? (uint)euclideanEmbedding.pivotCount : 0;
			this.targetNodeIndex = targetNodeIndex;
		}

		public int Calculate (int3 point, uint nodeIndex) {
			return Calculate(in this, ref point, nodeIndex);
		}

		[BurstCompile]
		public static int Calculate (in HeuristicObjective objective, ref int3 point, uint nodeIndex) {
			var closest = math.clamp(point, objective.mn, objective.mx);
			var diff = point - closest;

			int h;
			switch (objective.heuristic) {
			case Heuristic.Euclidean:
				h = (int)(math.length((float3)diff) * objective.heuristicScale);
				break;
			case Heuristic.Manhattan:
				h = (int)(math.csum(math.abs(diff)) * objective.heuristicScale);
				break;
			case Heuristic.DiagonalManhattan:
				// Octile distance extended to 3D
				diff = math.abs(diff);
				var a = diff.x;
				var b = diff.y;
				var c = diff.z;
				// Sort the values so that a <= b <= c
				if (a > b) Memory.Swap(ref a, ref b);
				if (b > c) Memory.Swap(ref b, ref c);
				if (a > b) Memory.Swap(ref a, ref b);

				// This is the same as the Manhattan distance, but with a different weight for the diagonal moves.
				const float SQRT_3 = 1.7321f;
				const float SQRT_2 = 1.4142f;
				h = (int)(objective.heuristicScale * (SQRT_3 * a + SQRT_2 * (b-a) + (c-b-a)));
				break;
			case Heuristic.None:
			default:
				h = 0;
				break;
			}

			if (objective.euclideanEmbeddingPivots > 0) {
				h = math.max(h, (int)EuclideanEmbedding.GetHeuristic(objective.euclideanEmbeddingCosts, objective.euclideanEmbeddingPivots, nodeIndex, objective.targetNodeIndex));
			}
			return h;
		}
	}
}

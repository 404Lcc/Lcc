using Pathfinding.Pooling;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Writes connections to each node in each tile.
	///
	/// It also calculates the connection costs between nodes.
	///
	/// This job is run after all tiles have been built and the connections have been calculated.
	///
	/// See: <see cref="JobCalculateTriangleConnections"/>
	/// </summary>
	public struct JobWriteNodeConnections : IJob {
		/// <summary>Connections for each tile</summary>
		[ReadOnly]
		public NativeArray<JobCalculateTriangleConnections.TileNodeConnectionsUnsafe> nodeConnections;
		/// <summary>Array of <see cref="NavmeshTile"/></summary>
		public System.Runtime.InteropServices.GCHandle tiles;

		public void Execute () {
			var tiles = (NavmeshTile[])this.tiles.Target;
			Assert.AreEqual(nodeConnections.Length, tiles.Length);

			for (int i = 0; i < tiles.Length; i++) {
				Profiler.BeginSample("CreateConnections");
				var connections = nodeConnections[i];
				Apply(tiles[i].nodes, connections);
				connections.neighbourCounts.Dispose();
				connections.neighbours.Dispose();
				Profiler.EndSample();
			}
		}

		void Apply (TriangleMeshNode[] nodes, JobCalculateTriangleConnections.TileNodeConnectionsUnsafe connections) {
			var neighbourCountsReader = connections.neighbourCounts.AsReader();
			var neighboursReader = connections.neighbours.AsReader();

			for (int i = 0; i < nodes.Length; i++) {
				var node = nodes[i];
				var neighbourCount = neighbourCountsReader.ReadNext<int>();
				var conns = node.connections = ArrayPool<Connection>.ClaimWithExactLength(neighbourCount);
				for (int j = 0; j < neighbourCount; j++) {
					var otherIndex = neighboursReader.ReadNext<int>();
					var shapeEdgeInfo = (byte)neighboursReader.ReadNext<int>();
					var other = nodes[otherIndex];
					var cost = (node.position - other.position).costMagnitude;
					conns[j] = new Connection(
						other,
						(uint)cost,
						shapeEdgeInfo
						);
				}
			}
		}
	}
}

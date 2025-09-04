using UnityEngine;
using System.Collections.Generic;
using Pathfinding;
using System.Linq;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsbuildingmanager.html")]
	public class RTSBuildingManager : MonoBehaviour {
		public static bool IsValidBuildingPlacement (GameObject building) {
			var onNavmesh = AstarPath.active.data.recastGraph.PointOnNavmesh(building.transform.position, NNConstraint.None);

			// Check if the center of the building is on the navmesh
			if (onNavmesh == null) return false;

			var cuts = building.GetComponentsInChildren<NavmeshCut>(true);
			if (cuts.Length == 0) return true;

			var graph = AstarPath.active.data.recastGraph;
			var characterRadius = graph.characterRadius;
			var boundingBox = cuts[0].GetBounds(graph.transform, characterRadius);
			List<NavmeshCut.Contour> contours = new List<NavmeshCut.Contour>();
			for (int i = 0; i < cuts.Length; i++) {
				boundingBox = RectUnion(boundingBox, cuts[i].GetBounds(graph.transform, characterRadius));
				cuts[i].GetContour(contours, Matrix4x4.identity, characterRadius);
			}

			Vector2[][] contourArrays = contours.Select(c => c.contour.ToArray()).ToArray();

			// Loop over all tiles in the vincinity of the building
			var touchingTiles = graph.GetTouchingTilesInGraphSpace(boundingBox);
			for (int x = touchingTiles.xmin; x <= touchingTiles.xmax; x++) {
				for (int z = touchingTiles.ymin; z <= touchingTiles.ymax; z++) {
					// Loop over all nodes in the tile
					var tile = graph.GetTile(x, z);
					for (int i = 0; i < tile.nodes.Length; i++) {
						// Figure out which sides of the node are shared with adjacent nodes
						// and which ones are graph borders.
						var node = tile.nodes[i];
						var usedSides = new bool[3];
						for (int j = 0; j < node.connections.Length; j++) {
							var conn = node.connections[j];
							if (conn.isEdgeShared && conn.shapeEdge >= 0 && conn.shapeEdge < 3) usedSides[conn.shapeEdge] = true;
						}

						for (int j = 0; j < 3; j++) {
							if (!usedSides[j]) {
								// This is a graph border as it is not shared with any neighbour nodes.
								// Check if this segment intersects with any navmesh cuts of the building

								var segmentStart = (Vector3)node.GetVertex(j);
								var segmentEnd = (Vector3)node.GetVertex((j+1) % 3);

								for (int k = 0; k < contourArrays.Length; k++) {
									if (Polygon.ContainsPoint(contourArrays[k], new Vector2(segmentStart.x, segmentStart.z)) || Polygon.ContainsPoint(contourArrays[k], new Vector2(segmentEnd.x, segmentEnd.z))) {
										// One of the vertices of the segment was inside a navmesh cut! This means the building placement is invalid
										return false;
									}

									var contour = contourArrays[k];
									for (int v = 0; v < contour.Length; v++) {
										var a = new Vector3(contour[v].x, 0.0f, contour[v].y);
										var b = new Vector3(contour[(v+1) % contour.Length].x, 0.0f, contour[(v+1) % contour.Length].y);
										if (VectorMath.SegmentsIntersectXZ(a, b, segmentStart, segmentEnd)) {
											// This segment intersects one of the segments of a navmesh cut! This means the building placement is invalid
											return false;
										}
									}
								}
							}
						}
					}
				}
			}

			return true;
		}

		static Rect RectUnion (Rect a, Rect b) {
			return Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));
		}
	}
}

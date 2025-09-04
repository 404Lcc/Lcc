using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Pathfinding.Graphs.Navmesh.Voxelization.Burst {
	using System;
	using Pathfinding.Jobs;
	using Pathfinding.Collections;
#if MODULE_COLLECTIONS_2_1_0_OR_NEWER
	using NativeHashMapInt3Int = Unity.Collections.NativeHashMap<Int3, int>;
#else
	using NativeHashMapInt3Int = Unity.Collections.NativeParallelHashMap<Int3, int>;
#endif

	/// <summary>VoxelMesh used for recast graphs.</summary>
	public struct VoxelMesh : IArenaDisposable {
		/// <summary>Vertices of the mesh</summary>
		public NativeList<Int3> verts;

		/// <summary>
		/// Triangles of the mesh.
		/// Each element points to a vertex in the <see cref="verts"/> array
		/// </summary>
		public NativeList<int> tris;

		/// <summary>Area index for each triangle</summary>
		public NativeList<int> areas;

		void IArenaDisposable.DisposeWith (DisposeArena arena) {
			arena.Add(verts);
			arena.Add(tris);
			arena.Add(areas);
		}
	}

	/// <summary>Builds a polygon mesh from a contour set.</summary>
	[BurstCompile]
	public struct JobBuildMesh : IJob {
		public NativeList<int> contourVertices;
		/// <summary>contour set to build a mesh from.</summary>
		public NativeList<VoxelContour> contours;
		/// <summary>Results will be written to this mesh.</summary>
		public VoxelMesh mesh;
		public CompactVoxelField field;

		/// <summary>
		/// Returns T iff (v_i, v_j) is a proper internal
		/// diagonal of P.
		/// </summary>
		static bool Diagonal (int i, int j, int n, NativeArray<int> verts, NativeArray<int> indices) {
			return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
		}

		static bool InCone (int i, int j, int n, NativeArray<int> verts, NativeArray<int> indices) {
			int pi = (indices[i] & 0x0fffffff) * 3;
			int pj = (indices[j] & 0x0fffffff) * 3;
			int pi1 = (indices[Next(i, n)] & 0x0fffffff) * 3;
			int pin1 = (indices[Prev(i, n)] & 0x0fffffff) * 3;

			// If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
			if (LeftOn(pin1, pi, pi1, verts))
				return Left(pi, pj, pin1, verts) && Left(pj, pi, pi1, verts);
			// Assume (i-1,i,i+1) not collinear.
			// else P[i] is reflex.
			return !(LeftOn(pi, pj, pi1, verts) && LeftOn(pj, pi, pin1, verts));
		}

		/// <summary>
		/// Returns true iff c is strictly to the left of the directed
		/// line through a to b.
		/// </summary>
		static bool Left (int a, int b, int c, NativeArray<int> verts) {
			return Area2(a, b, c, verts) < 0;
		}

		static bool LeftOn (int a, int b, int c, NativeArray<int> verts) {
			return Area2(a, b, c, verts) <= 0;
		}

		static bool Collinear (int a, int b, int c, NativeArray<int> verts) {
			return Area2(a, b, c, verts) == 0;
		}

		public static int Area2 (int a, int b, int c, NativeArray<int> verts) {
			return (verts[b] - verts[a]) * (verts[c+2] - verts[a+2]) - (verts[c+0] - verts[a+0]) * (verts[b+2] - verts[a+2]);
		}

		/// <summary>
		/// Returns T iff (v_i, v_j) is a proper internal *or* external
		/// diagonal of P, *ignoring edges incident to v_i and v_j*.
		/// </summary>
		static bool Diagonalie (int i, int j, int n, NativeArray<int> verts, NativeArray<int> indices) {
			int d0 = (indices[i] & 0x0fffffff) * 3;
			int d1 = (indices[j] & 0x0fffffff) * 3;

			/*int a = (i+1) % indices.Length;
			 * if (a == j) a = (i-1 + indices.Length) % indices.Length;
			 * int a_v = (indices[a] & 0x0fffffff) * 4;
			 *
			 * if (a != j && Collinear (d0,a_v,d1,verts)) {
			 *  return false;
			 * }*/

			// For each edge (k,k+1) of P
			for (int k = 0; k < n; k++) {
				int k1 = Next(k, n);
				// Skip edges incident to i or j
				if (!((k == i) || (k1 == i) || (k == j) || (k1 == j))) {
					int p0 = (indices[k] & 0x0fffffff) * 3;
					int p1 = (indices[k1] & 0x0fffffff) * 3;

					if (Vequal(d0, p0, verts) || Vequal(d1, p0, verts) || Vequal(d0, p1, verts) || Vequal(d1, p1, verts))
						continue;

					if (Intersect(d0, d1, p0, p1, verts))
						return false;
				}
			}


			return true;
		}

		//	Exclusive or: true iff exactly one argument is true.
		//	The arguments are negated to ensure that they are 0/1
		//	values.  Then the bitwise Xor operator may apply.
		//	(This idea is due to Michael Baldwin.)
		static bool Xorb (bool x, bool y) {
			return !x ^ !y;
		}

		//	Returns true iff ab properly intersects cd: they share
		//	a point interior to both segments.  The properness of the
		//	intersection is ensured by using strict leftness.
		static bool IntersectProp (int a, int b, int c, int d, NativeArray<int> verts) {
			// Eliminate improper cases.
			if (Collinear(a, b, c, verts) || Collinear(a, b, d, verts) ||
				Collinear(c, d, a, verts) || Collinear(c, d, b, verts))
				return false;

			return Xorb(Left(a, b, c, verts), Left(a, b, d, verts)) && Xorb(Left(c, d, a, verts), Left(c, d, b, verts));
		}

		// Returns T iff (a,b,c) are collinear and point c lies
		// on the closed segement ab.
		static bool Between (int a, int b, int c, NativeArray<int> verts) {
			if (!Collinear(a, b, c, verts))
				return false;
			// If ab not vertical, check betweenness on x; else on y.
			if (verts[a+0] != verts[b+0])
				return ((verts[a+0] <= verts[c+0]) && (verts[c+0] <= verts[b+0])) || ((verts[a+0] >= verts[c+0]) && (verts[c+0] >= verts[b+0]));
			else
				return ((verts[a+2] <= verts[c+2]) && (verts[c+2] <= verts[b+2])) || ((verts[a+2] >= verts[c+2]) && (verts[c+2] >= verts[b+2]));
		}

		// Returns true iff segments ab and cd intersect, properly or improperly.
		static bool Intersect (int a, int b, int c, int d, NativeArray<int> verts) {
			if (IntersectProp(a, b, c, d, verts))
				return true;
			else if (Between(a, b, c, verts) || Between(a, b, d, verts) ||
					 Between(c, d, a, verts) || Between(c, d, b, verts))
				return true;
			else
				return false;
		}

		static bool Vequal (int a, int b, NativeArray<int> verts) {
			return verts[a+0] == verts[b+0] && verts[a+2] == verts[b+2];
		}

		/// <summary>(i-1+n) % n assuming 0 <= i < n</summary>
		static int Prev (int i, int n) { return i-1 >= 0 ? i-1 : n-1; }
		/// <summary>(i+1) % n assuming 0 <= i < n</summary>
		static int Next (int i, int n) { return i+1 < n ? i+1 : 0; }

		static int AddVertex (NativeList<Int3> vertices, NativeHashMapInt3Int vertexMap, Int3 vertex) {
			if (vertexMap.TryGetValue(vertex, out var index)) {
				return index;
			}
			vertices.AddNoResize(vertex);
			vertexMap.Add(vertex, vertices.Length-1);
			return vertices.Length-1;
		}

		public void Execute () {
			// Maximum allowed vertices per polygon. Currently locked to 3.
			var nvp = 3;

			int maxVertices = 0;
			int maxTris = 0;
			int maxVertsPerCont = 0;

			for (int i = 0; i < contours.Length; i++) {
				// Skip null contours.
				if (contours[i].nverts < 3) continue;

				maxVertices += contours[i].nverts;
				maxTris += contours[i].nverts - 2;
				maxVertsPerCont = System.Math.Max(maxVertsPerCont, contours[i].nverts);
			}

			mesh.verts.Clear();
			if (maxVertices > mesh.verts.Capacity) mesh.verts.SetCapacity(maxVertices);
			mesh.tris.ResizeUninitialized(maxTris*nvp);
			mesh.areas.ResizeUninitialized(maxTris);
			var verts = mesh.verts;
			var polys = mesh.tris;
			var areas = mesh.areas;

			var indices = new NativeArray<int>(maxVertsPerCont, Allocator.Temp);
			var tris = new NativeArray<int>(maxVertsPerCont*3, Allocator.Temp);
			var verticesToRemove = new NativeArray<bool>(maxVertices, Allocator.Temp);
			var vertexPointers = new NativeHashMapInt3Int(maxVertices, Allocator.Temp);

			int polyIndex = 0;
			int areaIndex = 0;

			for (int i = 0; i < contours.Length; i++) {
				VoxelContour cont = contours[i];

				// Skip degenerate contours
				if (cont.nverts < 3) {
					continue;
				}

				for (int j = 0; j < cont.nverts; j++) {
					// Convert the z coordinate from the form z*voxelArea.width which is used in other places for performance
					contourVertices[cont.vertexStartIndex + j*4+2] /= field.width;
				}

				// Copy the vertex positions
				for (int j = 0; j < cont.nverts; j++) {
					// Try to remove all border vertices
					// See https://digestingduck.blogspot.com/2009/08/navmesh-height-accuracy-pt-5.html
					var vertexRegion = contourVertices[cont.vertexStartIndex + j*4+3];

					// Add a new vertex, or reuse an existing one if it has already been added to the mesh
					var idx = AddVertex(verts, vertexPointers, new Int3(
						contourVertices[cont.vertexStartIndex + j*4],
						contourVertices[cont.vertexStartIndex + j*4+1],
						contourVertices[cont.vertexStartIndex + j*4+2]
						));
					indices[j] = idx;
					verticesToRemove[idx] = (vertexRegion & VoxelUtilityBurst.RC_BORDER_VERTEX) != 0;
				}

				// Triangulate the contour
				int ntris = Triangulate(cont.nverts, verts.AsArray().Reinterpret<int>(12), indices, tris);

				if (ntris < 0) {
					// Degenerate triangles. This may lead to a hole in the navmesh.
					// We add the triangles that the triangulation generated before it failed.
					ntris = -ntris;
				}

				// Copy the resulting triangles to the mesh
				for (int j = 0; j < ntris*3; polyIndex++, j++) {
					polys[polyIndex] = tris[j];
				}

				// Mark all triangles generated by this contour
				// as having the area cont.area
				for (int j = 0; j < ntris; areaIndex++, j++) {
					areas[areaIndex] = cont.area;
				}
			}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (areaIndex > mesh.areas.Length) throw new System.Exception("Ended up at an unexpected area index");
			if (polyIndex > mesh.tris.Length) throw new System.Exception("Ended up at an unexpected poly index");
#endif

			// polyIndex might in rare cases not be equal to mesh.tris.Length.
			// This can happen if degenerate triangles were generated.
			// So we make sure the list is truncated to the right size here.
			mesh.tris.ResizeUninitialized(polyIndex);
			// Same thing for area index
			mesh.areas.ResizeUninitialized(areaIndex);

			RemoveTileBorderVertices(ref mesh, verticesToRemove);
		}

		void RemoveTileBorderVertices (ref VoxelMesh mesh, NativeArray<bool> verticesToRemove) {
			// Iterate in reverse to avoid having to update the verticesToRemove array as we remove vertices
			var vertexScratch = new NativeArray<byte>(mesh.verts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			for (int i = mesh.verts.Length - 1; i >= 0; i--) {
				if (verticesToRemove[i] && CanRemoveVertex(ref mesh, i, vertexScratch.AsUnsafeSpan())) {
					RemoveVertex(ref mesh, i);
				}
			}
		}

		bool CanRemoveVertex (ref VoxelMesh mesh, int vertexToRemove, UnsafeSpan<byte> vertexScratch) {
			UnityEngine.Assertions.Assert.IsTrue(vertexScratch.Length >= mesh.verts.Length);

			int remainingEdges = 0;
			for (int i = 0; i < mesh.tris.Length; i += 3) {
				int touched = 0;
				for (int j = 0; j < 3; j++) {
					if (mesh.tris[i+j] == vertexToRemove) {
						// This vertex is used by a triangle
						touched++;
					}
				}

				if (touched > 0) {
					if (touched > 1) throw new Exception("Degenerate triangle. This should have already been removed.");
					// If one vertex is removed from a triangle, 1 edge remains
					remainingEdges++;
				}
			}

			if (remainingEdges <= 2) {
				// There would be too few edges remaining to create a polygon.
				// This can happen for example when a tip of a triangle is marked
				// as deletion, but there are no other polys that share the vertex.
				// In this case, the vertex should not be removed.
				return false;
			}

			vertexScratch.FillZeros();

			for (int i = 0; i < mesh.tris.Length; i += 3) {
				for (int a = 0, b = 2; a < 3; b = a++) {
					if (mesh.tris[i+a] == vertexToRemove || mesh.tris[i+b] == vertexToRemove) {
						// This edge is used by a triangle
						int v1 = mesh.tris[i+a];
						int v2 = mesh.tris[i+b];

						// Update the shared count for the edge.
						// We identify the edge by the vertex index which is not the vertex to remove.
						vertexScratch[v2 == vertexToRemove ? v1 : v2]++;
					}
				}
			}

			int openEdges = 0;
			int multiSharedEdges = 0;
			for (int i = 0; i < vertexScratch.Length; i++) {
				if (vertexScratch[i] == 1) openEdges++;
				else if (vertexScratch[i] > 2) multiSharedEdges++;
			}

			if (multiSharedEdges > 0) {
				// This should not happen in valid navmeshes. But if the navmesh for some reason has overlapping triangles due to some other bug,
				// we should return false here, as otherwise we might end up in an infinite loop when trying to remove the vertex.
				Debug.LogError($"Vertex has multiple shared edges. This should not happen. Navmesh must be corrupt. Trying to not make it worse.");
				return false;
			}

			// There should be no more than 2 open edges.
			// This catches the case that two non-adjacent polygons
			// share the removed vertex. In that case, do not remove the vertex.
			return openEdges <= 2;
		}

		void RemoveVertex (ref VoxelMesh mesh, int vertexToRemove) {
			// Note: Assumes CanRemoveVertex has been called and returned true

			var remainingEdges = new NativeList<int>(16, Allocator.Temp);
			var area = -1;
			// Find all triangles that use this vertex
			for (int i = 0; i < mesh.tris.Length; i += 3) {
				int touched = -1;
				for (int j = 0; j < 3; j++) {
					if (mesh.tris[i+j] == vertexToRemove) {
						// This vertex is used by a triangle
						touched = j;
						break;
					}
				}
				if (touched != -1) {
					// Note: Only vertices that are not on an area border will be chosen (see GetCornerHeight),
					// so it is safe to assume that all triangles that share this vertex also share an area.
					area = mesh.areas[i/3];
					// If one vertex is removed from a triangle, 1 edge remains
					remainingEdges.Add(mesh.tris[i+((touched+1) % 3)]);
					remainingEdges.Add(mesh.tris[i+((touched+2) % 3)]);

					mesh.tris[i+0] = mesh.tris[mesh.tris.Length-3+0];
					mesh.tris[i+1] = mesh.tris[mesh.tris.Length-3+1];
					mesh.tris[i+2] = mesh.tris[mesh.tris.Length-3+2];

					mesh.tris.Length -= 3;
					mesh.areas.RemoveAtSwapBack(i/3);
					i -= 3;
				}
			}

			UnityEngine.Assertions.Assert.AreNotEqual(-1, area);

			// Build a sorted list of all vertices in the contour for the hole
			var sortedVertices = new NativeList<int>(remainingEdges.Length/2 + 1, Allocator.Temp);
			sortedVertices.Add(remainingEdges[remainingEdges.Length-2]);
			sortedVertices.Add(remainingEdges[remainingEdges.Length-1]);
			remainingEdges.Length -= 2;

			while (remainingEdges.Length > 0) {
				for (int i = remainingEdges.Length - 2; i >= 0; i -= 2) {
					var a = remainingEdges[i];
					var b = remainingEdges[i+1];
					bool added = false;
					if (sortedVertices[0] == b) {
						sortedVertices.InsertRange(0, 1);
						sortedVertices[0] = a;
						added = true;
					}
					if (sortedVertices[sortedVertices.Length-1] == a) {
						sortedVertices.AddNoResize(b);
						added = true;
					}
					if (added) {
						// Remove the edge and swap with the last one
						remainingEdges[i] = remainingEdges[remainingEdges.Length-2];
						remainingEdges[i+1] = remainingEdges[remainingEdges.Length-1];
						remainingEdges.Length -= 2;
					}
				}
			}

			// Remove the vertex
			mesh.verts.RemoveAt(vertexToRemove);

			// Patch indices to account for the removed vertex
			for (int i = 0; i < mesh.tris.Length; i++) {
				if (mesh.tris[i] > vertexToRemove) mesh.tris[i]--;
			}
			for (int i = 0; i < sortedVertices.Length; i++) {
				if (sortedVertices[i] > vertexToRemove) sortedVertices[i]--;
			}

			var maxIndices = (sortedVertices.Length - 2) * 3;
			var trisBeforeResize = mesh.tris.Length;
			mesh.tris.Length += maxIndices;
			int newTriCount = Triangulate(
				sortedVertices.Length,
				mesh.verts.AsArray().Reinterpret<int>(12),
				sortedVertices.AsArray(),
				// Insert the new triangles at the end of the array
				mesh.tris.AsArray().GetSubArray(trisBeforeResize, maxIndices)
				);

			if (newTriCount < 0) {
				// Degenerate triangles. This may lead to a hole in the navmesh.
				// We add the triangles that the triangulation generated before it failed.
				newTriCount = -newTriCount;
			}

			// Resize the triangle array to the correct size
			mesh.tris.ResizeUninitialized(trisBeforeResize + newTriCount*3);
			mesh.areas.AddReplicate(area, newTriCount);

			UnityEngine.Assertions.Assert.AreEqual(mesh.areas.Length, mesh.tris.Length/3);
		}

		static int Triangulate (int n, NativeArray<int> verts, NativeArray<int> indices, NativeArray<int> tris) {
			int ntris = 0;
			var dst = tris;
			int dstIndex = 0;

			// The last bit of the index is used to indicate if the vertex can be removed
			// in an ear-cutting operation.
			const int CanBeRemovedBit = 0x40000000;
			// Used to get only the index value, without any flag bits.
			const int IndexMask = 0x0fffffff;

			for (int i = 0; i < n; i++) {
				int i1 = Next(i, n);
				int i2 = Next(i1, n);
				if (Diagonal(i, i2, n, verts, indices)) {
					indices[i1] |= CanBeRemovedBit;
				}
			}

			while (n > 3) {
				int minLen = int.MaxValue;
				int mini = -1;

				for (int q = 0; q < n; q++) {
					int q1 = Next(q, n);
					if ((indices[q1] & CanBeRemovedBit) != 0) {
						int p0 = (indices[q] & IndexMask) * 3;
						int p2 = (indices[Next(q1, n)] & IndexMask) * 3;

						int dx = verts[p2+0] - verts[p0+0];
						int dz = verts[p2+2] - verts[p0+2];


						//Squared distance
						int len = dx*dx + dz*dz;

						if (len < minLen) {
							minLen = len;
							mini = q;
						}
					}
				}

				if (mini == -1) {
					Debug.LogWarning("Degenerate triangles might have been generated.\n" +
						"Usually this is not a problem, but if you have a static level, try to modify the graph settings slightly to avoid this edge case.");
					return -ntris;
				}

				int i = mini;
				int i1 = Next(i, n);
				int i2 = Next(i1, n);


				dst[dstIndex] = indices[i] & IndexMask;
				dstIndex++;
				dst[dstIndex] = indices[i1] & IndexMask;
				dstIndex++;
				dst[dstIndex] = indices[i2] & IndexMask;
				dstIndex++;
				ntris++;

				// Removes P[i1] by copying P[i+1]...P[n-1] left one index.
				n--;
				for (int k = i1; k < n; k++) {
					indices[k] = indices[k+1];
				}

				if (i1 >= n) i1 = 0;
				i = Prev(i1, n);
				// Update diagonal flags.
				if (Diagonal(Prev(i, n), i1, n, verts, indices)) {
					indices[i] |= CanBeRemovedBit;
				} else {
					indices[i] &= IndexMask;
				}
				if (Diagonal(i, Next(i1, n), n, verts, indices)) {
					indices[i1] |= CanBeRemovedBit;
				} else {
					indices[i1] &= IndexMask;
				}
			}

			dst[dstIndex] = indices[0] & IndexMask;
			dstIndex++;
			dst[dstIndex] = indices[1] & IndexMask;
			dstIndex++;
			dst[dstIndex] = indices[2] & IndexMask;
			dstIndex++;
			ntris++;

			return ntris;
		}
	}
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace Pathfinding {
	using Pathfinding.Pooling;
	using Unity.Collections.LowLevel.Unsafe;

	/// <summary>
	/// Movement script for curved worlds.
	/// This script inherits from AIPath, but adjusts its movement plane every frame using the ground normal.
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/aipathalignedtosurface.html")]
	public class AIPathAlignedToSurface : AIPath {
		/// <summary>Scratch dictionary used to avoid allocations every frame</summary>
		static readonly Dictionary<Mesh, int> scratchDictionary = new Dictionary<Mesh, int>();

		protected override void OnEnable () {
			base.OnEnable();
			movementPlane = new Util.SimpleMovementPlane(rotation);
		}

		protected override void ApplyGravity (float deltaTime) {
			// Apply gravity
			if (usingGravity) {
				// Gravity is relative to the current surface.
				// Only the normal direction is well defined however so x and z are ignored.
				verticalVelocity += deltaTime * (float.IsNaN(gravity.x) ? Physics.gravity.y : gravity.y);
			} else {
				verticalVelocity = 0;
			}
		}

		/// <summary>
		/// Calculates smoothly interpolated normals for all raycast hits and uses that to set the movement planes of the agents.
		///
		/// To support meshes that change at any time, we use Mesh.AcquireReadOnlyMeshData to get a read-only view of the mesh data.
		/// This is only efficient if we batch all updates and make a single call to Mesh.AcquireReadOnlyMeshData.
		///
		/// This method is quite convoluted due to having to read the raw vertex data streams from unity meshes to avoid allocations.
		/// </summary>
		public static void UpdateMovementPlanes (AIPathAlignedToSurface[] components, int count) {
			Profiler.BeginSample("UpdateMovementPlanes");
			var meshes = ListPool<Mesh>.Claim();
			var componentsByMesh = new List<List<AIPathAlignedToSurface> >();
			var meshToIndex = scratchDictionary;
			for (int i = 0; i < count; i++) {
				var c = components[i].lastRaycastHit.collider;
				// triangleIndex can be -1 if the mesh collider is convex, and the raycast started inside it.
				// This is not a documented behavior, but it seems to happen in practice.
				if (c is MeshCollider mc && components[i].lastRaycastHit.triangleIndex != -1) {
					var sharedMesh = mc.sharedMesh;
					if (meshToIndex.TryGetValue(sharedMesh, out var meshIndex)) {
						componentsByMesh[meshIndex].Add(components[i]);
					} else if (sharedMesh != null && sharedMesh.isReadable) {
						meshToIndex[sharedMesh] = meshes.Count;
						meshes.Add(sharedMesh);
						componentsByMesh.Add(ListPool<AIPathAlignedToSurface>.Claim());
						componentsByMesh[meshes.Count-1].Add(components[i]);
					} else {
						// Unreadable mesh
						components[i].SetInterpolatedNormal(components[i].lastRaycastHit.normal);
					}
				} else {
					// Not a mesh collider, or the triangle index was -1
					components[i].SetInterpolatedNormal(components[i].lastRaycastHit.normal);
				}
			}
			var meshDatas = Mesh.AcquireReadOnlyMeshData(meshes);
			for (int i = 0; i < meshes.Count; i++) {
				var m = meshes[i];
				var meshIndex = meshToIndex[m];
				var meshData = meshDatas[meshIndex];
				var componentsForMesh = componentsByMesh[meshIndex];

				var stream = meshData.GetVertexAttributeStream(UnityEngine.Rendering.VertexAttribute.Normal);

				if (stream == -1) {
					// Mesh does not have normals
					for (int j = 0; j < componentsForMesh.Count; j++) componentsForMesh[j].SetInterpolatedNormal(componentsForMesh[j].lastRaycastHit.normal);
					continue;
				}
				var vertexData = meshData.GetVertexData<byte>(stream);
				var stride = meshData.GetVertexBufferStride(stream);
				var normalOffset = meshData.GetVertexAttributeOffset(UnityEngine.Rendering.VertexAttribute.Normal);
				unsafe {
					var normals = (byte*)vertexData.GetUnsafeReadOnlyPtr() + normalOffset;

					for (int j = 0; j < componentsForMesh.Count; j++) {
						var comp = componentsForMesh[j];
						var hit = comp.lastRaycastHit;
						int t0, t1, t2;

						// Get the vertex indices corresponding to the triangle that was hit
						if (meshData.indexFormat == UnityEngine.Rendering.IndexFormat.UInt16) {
							var indices = meshData.GetIndexData<ushort>();
							t0 = indices[hit.triangleIndex * 3 + 0];
							t1 = indices[hit.triangleIndex * 3 + 1];
							t2 = indices[hit.triangleIndex * 3 + 2];
						} else {
							var indices = meshData.GetIndexData<int>();
							t0 = indices[hit.triangleIndex * 3 + 0];
							t1 = indices[hit.triangleIndex * 3 + 1];
							t2 = indices[hit.triangleIndex * 3 + 2];
						}

						// Get the normals corresponding to the 3 vertices
						var n0 = *((Vector3*)(normals + t0 * stride));
						var n1 = *((Vector3*)(normals + t1 * stride));
						var n2 = *((Vector3*)(normals + t2 * stride));

						// Interpolate the normal using the barycentric coordinates
						Vector3 baryCenter = hit.barycentricCoordinate;
						Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
						interpolatedNormal = interpolatedNormal.normalized;
						Transform hitTransform = hit.collider.transform;
						interpolatedNormal = hitTransform.TransformDirection(interpolatedNormal);
						comp.SetInterpolatedNormal(interpolatedNormal);
					}
				}
			}
			meshDatas.Dispose();
			for (int i = 0; i < componentsByMesh.Count; i++) ListPool<AIPathAlignedToSurface>.Release(componentsByMesh[i]);
			ListPool<Mesh>.Release(ref meshes);
			scratchDictionary.Clear();
			Profiler.EndSample();
		}

		void SetInterpolatedNormal (Vector3 normal) {
			if (normal != Vector3.zero) {
				var fwd = Vector3.Cross(movementPlane.rotation * Vector3.right, normal);
				movementPlane = new Util.SimpleMovementPlane(Quaternion.LookRotation(fwd, normal));
			}
			if (rvoController != null) rvoController.movementPlane = movementPlane;
		}

		protected override void UpdateMovementPlane () {
			// The UpdateMovementPlanes method will take care of this
		}
	}
}
